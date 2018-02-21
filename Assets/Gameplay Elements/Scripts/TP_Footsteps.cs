using UnityEngine;

public class TP_Footsteps : MonoBehaviour
{
	public AudioClip[] footstepSounds;
	public float footstepVolume = 0.4f;
	public float timeBetweenFootsteps = 0.5f;

	private float footstepTimer;
	private TP_Motor motor;
	private AudioSource source;

	private void Start()
	{
		//Initialization.
		source = GetComponent<AudioSource>();
		motor = TP_Motor.Instance;
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//Check several conditions to see if a footstep sound should be played.
		if(motor.passive.isMoving && !motor.passive.isDashing && Time.time > footstepTimer && (hit.controller.velocity.magnitude / motor.moveSpeed) > 0.15f)
		{
			//Get a random sound from the array and play it.
			int randomIndex = Random.Range(0, footstepSounds.Length);
			source.PlayOneShot(footstepSounds[randomIndex], footstepVolume);

			//Update the footstep timer.
			footstepTimer = Time.time + timeBetweenFootsteps;
		}
	}
}
