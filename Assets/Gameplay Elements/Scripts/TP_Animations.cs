using UnityEngine;
using System.Collections;

public class TP_Animations : MonoBehaviour
{
	public Animator animator;

	#region Singleton
	public static TP_Animations Instance {get; private set;}
	private void Awake()
	{
		if(Instance == null) Instance = this;
	}
	#endregion

	public void AnimateCharacter(Vector2 moveDirection, float moveMagnitude)
	{
		//Get the combat mode and set the animator's paramater.
		bool combatMode = PlayerInformation.Instance.combatMode;
		animator.SetBool("CombatMode", combatMode);

		//Get the grounded state and set the animator's paramater.
		bool grounded = TP_Motor.Instance.passive.isGrounded;
		animator.SetBool("Grounded", grounded);

		//Calculate the move magnitude and set the animator's paramater.
		float magnitude = TP_Motor.Instance.passive.isMoving ? Mathf.Clamp01(moveMagnitude) : 1f;
		animator.SetFloat("PlaybackSpeed", magnitude);

		//Body animation: For Combat Mode
		if(combatMode)
		{
			//Get the relative move direction.
			Vector3 cameraForward = Vector3.Scale(Camera.main.transform.up, new Vector3(1, 0, 1)).normalized;
			Vector3 move = moveDirection.y * cameraForward + moveDirection.x * Camera.main.transform.right;

			//Normalize the move direction.
			move.Normalize();

			//Transform the move direction to local space.
			Vector3 localMove = transform.InverseTransformDirection(move);

			//Clamp the animation blend amount.
			float forward = Mathf.Clamp(localMove.z * moveMagnitude, -1, 1);
			float turn = Mathf.Clamp(localMove.x * moveMagnitude, -1, 1);

			//Set the animator paramaters.
			animator.SetFloat("Forward", Mathf.Round(forward), 0.07f, Time.deltaTime);
			animator.SetFloat("Turn", Mathf.Round(turn), 0.07f, Time.deltaTime);
		}

		//Body animation: For Non-Combat Mode
		else if(!combatMode)
		{
			//Calculate the blend amount.
			float forward = Mathf.Clamp01(moveMagnitude);
			if(forward >= 0.2f) forward = 1f;

			//Set the animator paramaters.
			animator.SetFloat("Forward", forward, 0.04f, Time.deltaTime);
			animator.SetFloat("Turn", 0f, 0.04f, Time.deltaTime);
		}
	}

	public void PlayReloadAnimation()
	{
		//Start the reload animation process.
		StopCoroutine(AnimateReload());
		StartCoroutine(AnimateReload());
	}

	private IEnumerator AnimateReload()
	{
		//Enable the action layer and start the reloading animation.
		animator.SetLayerWeight(1, 1);
		animator.CrossFade("Rifle Reload", 0.1f, 1);

		//Wait for the reloading to finish then disable the action layer.
		yield return new WaitForSeconds(PlayerInformation.Instance.weapon.reloadTime);
		animator.SetLayerWeight(1, 0);
	}
}
