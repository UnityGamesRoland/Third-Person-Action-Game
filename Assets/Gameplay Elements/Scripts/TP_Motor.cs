using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TP_Motor : MonoBehaviour
{
	public float moveSpeed = 5.9f;
	public LayerMask collisionLayer;
	public SkinnedMeshRenderer bodyRenderer;
	public CanvasGroup dashHUD;
	public CanvasGroup crosshairHUD;

	public bool canMove = true;
	public bool canDash = true;

	public MovementInfo movement;
	public PassiveStates passive;

	private Vector3[] originArray = new Vector3[4];
	private Vector3 mousePoint;
	private Vector3 speedChangeVelocity;

	private float gravity = -20f;
	private float maxSlopeAngle;
	private float rotationChangeVelocity;

	private Slider dashMeter;
	private RayOrigins rayOrigin;

	private TP_Animations anim;
	private PlayerInformation info;
	private CharacterController controller;

	#region Singleton
	public static TP_Motor Instance {get; private set;}
	private void Awake()
	{
		if(Instance == null) Instance = this;
	}
	#endregion

	private void Start()
	{
		//Initialization.
		anim = GetComponent<TP_Animations>();
		info = PlayerInformation.Instance;
		controller = GetComponent<CharacterController>();
		dashMeter = dashHUD.gameObject.GetComponentInChildren<Slider>();

		//Set the max slope angle.
		maxSlopeAngle = controller.slopeLimit;

		//Set the default alpha of the HUD elements.
		crosshairHUD.alpha = info.combatMode ? 1 : 0;
		dashHUD.alpha = (dashMeter.value > 0.98f) ? 0.2f : 1;
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
		anim.AnimateCharacter(inputDir, new Vector2(controller.velocity.x, controller.velocity.z).magnitude / moveSpeed);
	}

	private void LateUpdate()
	{
		//Update the crosshair's position.
		Vector3 screenPoint = Camera.main.WorldToScreenPoint(mousePoint);
		crosshairHUD.transform.position = screenPoint;

		//Update the alpha value of the HUD elements.
		crosshairHUD.alpha = info.combatMode ? 1 : 0;
		dashHUD.alpha = Mathf.Lerp(dashHUD.alpha, (dashMeter.value > 0.98f) ? 0.2f : 1, Time.deltaTime * ((dashMeter.value > 0.98f) ? 10 : 40));
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
		if(passive.isGrounded) movement.currentSpeed.y = 0f;
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
			if(Physics.Raycast(descendRay, out descendHit, 100f, collisionLayer))
			{
				//Get the angle of the surface that we are standing on.
				float slopeAngle = Vector3.Angle(descendHit.normal, Vector3.up);

				//Check if we are on a slope.
				if(slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
				{
					//Update the slope state.
					passive.isOnSlope = true;

					//Check if we are close enough to the slope to move on it.
					if(descendHit.distance - controller.skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(new Vector3(direction.x, 0, direction.z).magnitude) && !passive.isDescendingSlope)
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
						passive.isDescendingSlope = true;
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
				mousePoint = ray.GetPoint(rayDistance);

				//Check if we have a weapon in hand and correct the mouse point's height.
				if(info.weapon != null) mousePoint += Vector3.up * 0.4f;

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
		//Update the ray origin array.
		AssignVerticalRays();

		//Reset the passive states.
		passive.isGrounded = CheckGroundCollision();
		passive.isMoving = (input.magnitude > 0) ? true : false;
		passive.isOnSlope = false;
		passive.isDescendingSlope = false;
	}

	private void SetTargetMoveSpeed(Vector2 input)
	{
		//Check the dashing input.
		bool dash = Input.GetKeyDown(KeyCode.LeftShift);

		//Start dashing coroutine if every statement is true.
		if(dash && input != Vector2.zero && canDash && !passive.isDashing && canMove)
		{
			StartCoroutine(Dash(input, 0.31f));
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
		Vector2 dashVelocity = direction * moveSpeed * 3f;

		//Set the target speed to the dash velocity.
		movement.targetSpeed.x = dashVelocity.x;
		movement.targetSpeed.z = dashVelocity.y;

		//Set the dash direction.
		movement.dashDirection = direction;

		//Update the dashing and damage taking states.
		info.canTakeDamage = false;
		passive.isDashing = true;
		canDash = false;

		//Play the dashing particle and set the body color.
		info.dashParticle.Play();
		bodyRenderer.material.SetColor("_Color", Color.white);
		bodyRenderer.material.SetColor("_EmissionColor", new Color(0f, 0.65f, 1f));

		//Update the dash meter.
		StopCoroutine(SetDashMeter(0f, 0f));
		StartCoroutine(SetDashMeter(0, dashTime));

		//Wait for the dashing to complete.
		yield return new WaitForSeconds(dashTime);

		//Update the dashing and damage taking state.
		info.canTakeDamage = true;
		passive.isDashing = false;

		//Stop the dashing particle.
		info.dashParticle.Stop();

		//Update the dash meter.
		StopCoroutine(SetDashMeter(0f, 0f));
		StartCoroutine(SetDashMeter(1f, 1.1f));

		//Fade the body color after a bit of delay.
		yield return new WaitForSeconds(0.12f);
		StopCoroutine(FadeBodyEmission());
		StartCoroutine(FadeBodyEmission());

		//Delay the next dash.
		yield return new WaitForSeconds(0.78f);
		canDash = true;
	}

	private IEnumerator FadeBodyEmission()
	{
		//Reset the progress and get the current colors.
		float progress = 0f;
		Color startBaseColor = bodyRenderer.material.GetColor("_Color");
		Color startEmissionColor = bodyRenderer.material.GetColor("_EmissionColor");

		while(progress < 1)
		{
			//Update the body color.
			Color newBaseColor = Color.Lerp(startBaseColor, Color.black, progress);
			Color newEmissionColor = Color.Lerp(startEmissionColor, Color.black, progress);
			bodyRenderer.material.SetColor("_Color", newBaseColor);
			bodyRenderer.material.SetColor("_EmissionColor", newEmissionColor);

			//Update the progress.
			progress += Time.deltaTime / 0.4f;
			yield return null;
		}
	}

	private IEnumerator SetDashMeter(float targetValue, float time)
	{
		//Reset the progress and get the start value.
		float progress = 0f;
		float startValue = dashMeter.value;

		while(progress < 1)
		{
			//Smoothly reach the target value over time.
			dashMeter.value = Mathf.Lerp(startValue, targetValue, progress);

			//Update the progress.
			progress += Time.deltaTime / time;
			yield return null;
		}
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

	private bool CheckGroundCollision()
	{
		//Stores the final outcome of the method.
		bool grounded = false;

		//Loop through the vertical rays to find out if we are on a slope or not.
		for(int i = 0; i < originArray.Length; i++)
		{
			//Shoot a ray downwards from the current origin point.
			Ray verticalRay = new Ray(originArray[i], Vector3.down);
			RaycastHit verticalHit;

			//Check if the ray hit something.
			if(Physics.Raycast(verticalRay, out verticalHit, 1f, collisionLayer))
			{
				//Set the grounded state.
				grounded = true;
				Debug.DrawRay(verticalRay.origin, verticalRay.direction * 1f);
			}
		}

		//Return the grounded state.
		return grounded;
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
		public bool isOnSlope;
		public bool isDescendingSlope;
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

