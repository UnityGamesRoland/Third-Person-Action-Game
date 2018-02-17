using UnityEngine;

public class TP_Animations : MonoBehaviour
{
	public Animator animator;
	private TP_Camera cam;

	private void Start()
	{
		//Initialization.
		cam = GetComponent<TP_Camera>();
	}

	public void AnimateCharacter(Vector2 moveDirection, float moveMagnitude)
	{
		//Body animation: For Combat Mode
		if(PlayerInformation.Instance.combatMode)
		{
			//Get the relative move direction.
			Vector3 cameraForward = Vector3.Scale(cam.playerCamera.transform.up, new Vector3(1, 0, 1)).normalized;
			Vector3 move = moveDirection.y * cameraForward + moveDirection.x * cam.playerCamera.transform.right;

			//Normalize the move direction.
			move.Normalize();

			//Transform the move direction to local space.
			Vector3 localMove = transform.InverseTransformDirection(move);

			//Clamp the animation blend amount.
			float forward = Mathf.Clamp(localMove.z * moveMagnitude, -1, 1);
			float turn = Mathf.Clamp(localMove.x * moveMagnitude, -1, 1);

			//Set the animator paramaters.
			animator.SetFloat("Forward", forward, 0.08f, Time.deltaTime);
			animator.SetFloat("Turn", turn, 0.08f, Time.deltaTime);
		}

		//Body animation: For Non-Combat Mode
		else if(!PlayerInformation.Instance.combatMode)
		{
			//Set the animator paramaters.
			animator.SetFloat("Forward", Mathf.Clamp(moveMagnitude, -1, 1), 0.08f, Time.deltaTime);
			animator.SetFloat("Turn", 0f, 0.08f, Time.deltaTime);
		}
	}
}
