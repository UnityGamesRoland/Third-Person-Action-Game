using UnityEngine;
using System.Collections;

public class TP_Motor : MonoBehaviour
{
	public float moveSpeed = 5.9f;
	public float gravity = -20f;
	public LayerMask collisionLayer;

	public bool canMove = true;
	public bool canDash = true;

	public MovementInfo movement;
	public PassiveStates passive;
	private RayOrigins rayOrigin;

	private Vector3[] originArray = new Vector3[4];
	private Vector3 speedChangeVelocity;

	private float maxSlopeAngle;
	private float rotationChangeVelocity;

	private TP_Animations anim;
	private PlayerInformation info;
	private CharacterController controller;

	private void Start()
	{
		//Initialization.
		anim = GetComponent<TP_Animations>();
		info = GetComponent<PlayerInformation>();
		controller = GetComponent<CharacterController>();

		//Set the max slope angle.
		maxSlopeAngle = controller.slopeLimit;
	}

	private void Update()
	{
		//Get the input direction.
		Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

		//Move the character in the input direction.
		Move(inputDir);

		//Rotate the character's body to face the mouse position.
		RotateBody(inputDir);

		//Animate the character.
		anim.AnimateCharacter(inputDir, new Vector2(controller.velocity.x, controller.velocity.z).magnitude / moveSpeed, info.combatMode);
	}

	private void Move(Vector2 direction)
	{
		//Reset the passive states.
		SetCharacterStates(direction);

		//Set the target movement speed.
		SetTargetMoveSpeed(direction);

		//Smooth out the movement speed.
		movement.currentSpeed.x = Mathf.SmoothDamp(movement.currentSpeed.x, movement.targetSpeed.x, ref speedChangeVelocity.x, 0.05f);
		movement.currentSpeed.z = Mathf.SmoothDamp(movement.currentSpeed.z, movement.targetSpeed.z, ref speedChangeVelocity.z, 0.05f);

		//Apply gravity.
		movement.currentSpeed.y += gravity * Time.deltaTime;

		//Set the move direction.
		movement.moveVelocity.Set(movement.currentSpeed.x, movement.currentSpeed.y, movement.currentSpeed.z);

		//Adjust the move direction if we are descending a slope.
		DescendSlope(ref movement.moveVelocity);

		//Move the character.
		controller.Move(movement.moveVelocity * Time.deltaTime);

		//Reset the gravity force.
		if(passive.isGrounded && !passive.isSliding) movement.currentSpeed.y = 0;
	}

	private void DescendSlope(ref Vector3 direction)
	{
		//Update the ray origin array.
		AssignVerticalRays();

		//Loop through the vertical rays to find out if we are on a slope or not.
		for(int i = 0; i < originArray.Length; i++)
		{
			//Shoot a ray downwards from the current origin point.
			Ray descendRay = new Ray(originArray[i], Vector3.down);
			RaycastHit descendHit;

			//Check if the ray hit something.
			if(Physics.Raycast(descendRay, out descendHit, 100, collisionLayer))
			{
				//Get the angle of the surface that we are standing on.
				float slopeAngle = Vector3.Angle(descendHit.normal, Vector3.up);

				//Check if we are on a slope.
				if(slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
				{
					//Check if we are close enough to the slope to move on it.
					if(descendHit.distance - controller.skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(new Vector3(direction.x, 0, direction.z).magnitude))
					{
						//Get the amount of movement required to descend the slope properly.
						float moveDistanceX = Mathf.Abs(direction.x);
						float moveDistanceZ = Mathf.Abs(direction.z);
						float descendVelocity = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * new Vector3(moveDistanceX, 0, moveDistanceZ).magnitude;

						//Descend slope.
						direction.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistanceX * Mathf.Sign(direction.x);
						direction.z = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistanceZ * Mathf.Sign(direction.z);
						direction.y -= descendVelocity;

						//Update the grounded state.
						passive.isGrounded = true;
					}
				}
			}
		}
	}

	private void RotateBody(Vector2 direction)
	{
		//Body rotation: To Cursor
		if(info.combatMode)
		{
			//Create a virtual plane for the rotation.
			Plane rotationPlane = new Plane(Vector3.up, Vector3.up * transform.position.y);

			//Shoot a ray from the camera to the mouse position.
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			float rayDistance;

			//Rotate the character.
			if(rotationPlane.Raycast(ray, out rayDistance))
			{
				//Get the mouse point.
				Vector3 mousePoint = ray.GetPoint(rayDistance);

				//Get the look point and rotate the player.
				Vector3 lookPoint = new Vector3(mousePoint.x, transform.position.y, mousePoint.z);
				transform.LookAt(lookPoint);
			}
		}

		//Body rotation: To Direction
		else if(!info.combatMode && direction != Vector2.zero)
		{
			float targetRotation = Mathf.Atan2(passive.isDashing ? movement.dashDirection.x : direction.x, passive.isDashing ? movement.dashDirection.y : direction.y) * Mathf.Rad2Deg;
			transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationChangeVelocity, 0.07f);
		}
	}

	private void SetCharacterStates(Vector2 input)
	{
		//Reset the passive states.
		passive.isGrounded = controller.isGrounded;
		passive.isMoving = (input.magnitude > 0) ? true : false;
		passive.isSliding = false;
	}

	private void SetTargetMoveSpeed(Vector2 input)
	{
		//Check the dashing input.
		bool dash = Input.GetKeyDown(KeyCode.LeftShift);

		//Start dashing coroutine if every statement is true.
		if(dash && input != Vector2.zero && canDash && !passive.isDashing && canMove)
		{
			StartCoroutine(Dash(input, 0.25f));
		}

		//Set the target speed to the move speed.
		if(canMove && !passive.isDashing)
		{
			movement.targetSpeed.x = input.x * moveSpeed;
			movement.targetSpeed.z = input.y * moveSpeed;
		}

		//Set the target speed to zero.
		else if(!canMove && !passive.isDashing)
		{
			movement.targetSpeed.x = 0;
			movement.targetSpeed.z = 0;
		}
	}

	private IEnumerator Dash(Vector2 direction, float dashTime)
	{
		//Get the dash velocity based on the input direction and move speed.
		Vector2 dashVelocity = direction * moveSpeed * 2.7f;

		//Set the target speed to the dash velocity.
		movement.targetSpeed.x = dashVelocity.x;
		movement.targetSpeed.z = dashVelocity.y;

		//Set the dash direction.
		movement.dashDirection = direction;

		//Update the dashing states.
		passive.isDashing = true;
		canDash = false;

		//Wait for the dashing to complete.
		yield return new WaitForSeconds(dashTime);

		//Update the dashing states.
		passive.isDashing = false;
		canDash = true;
	}

	private void AssignVerticalRays()
	{
		//Update the ray origins.
		rayOrigin.SetRayOrigins(controller);

		//Assign the origins to an array.
		originArray[0] = rayOrigin.leftRay;
		originArray[1] = rayOrigin.rightRay;
		originArray[2] = rayOrigin.topRay;
		originArray[3] = rayOrigin.bottomRay;
	}

	public struct MovementInfo
	{
		public Vector3 moveVelocity;
		public Vector3 currentSpeed;
		public Vector3 targetSpeed;
		public Vector2 dashDirection;
	}

	public struct PassiveStates
	{
		public bool isGrounded;
		public bool isSliding;
		public bool isMoving;
		public bool isDashing;
	}

	private struct RayOrigins
	{
		public Vector3 topRay;
		public Vector3 bottomRay;
		public Vector3 rightRay;
		public Vector3 leftRay;

		public void SetRayOrigins(CharacterController controller)
		{
			Bounds bounds = controller.bounds;
			bounds.Expand(-controller.skinWidth);
			topRay = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z + bounds.extents.z);
			bottomRay = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z - bounds.extents.z);
			rightRay = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y, bounds.center.z);
			leftRay = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y, bounds.center.z);
		}
	}
}
