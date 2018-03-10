using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class MusicManager : MonoBehaviour
{
	public AudioClip[] combatMusics;
	public AudioMixer mixer;

	private AudioSource source;

	private void Start()
	{
		//Initialization
		source = GetComponent<AudioSource>();
		source.ignoreListenerPause = true;

		//Set the initial volume.
		float targetVolume = PlayerInformation.Instance.combatMode ? 0f : -80f;
		mixer.SetFloat("combatMusicVolume", targetVolume);
	}

	private void Update()
	{
		//Check if there is a music playing.
		if(!source.isPlaying)
		{
			//Get a random clip from the music array and start playing it.
			int randomIndex = Random.Range(0, combatMusics.Length);
			source.clip = combatMusics[randomIndex];
			source.Play();
		}

		//Get the current volume of the mixer.
		float currentVolume;
		mixer.GetFloat("combatMusicVolume", out currentVolume);

		//Set the volume of the mixer based on the combat mode and the pause state.
		float targetVolume = Mathf.Lerp(currentVolume, GetTargetVolume(), Time.unscaledDeltaTime * GetBlendSpeed());
		mixer.SetFloat("combatMusicVolume", targetVolume);
	}

	private float GetTargetVolume()
	{
		//Store the final outcome of the method.
		float volume = 0f;

		//Calculate the volume.
		if(PlayerInformation.Instance.combatMode) volume = PauseManager.Instance.isPaused ? -22f : 0f;
		else volume = -80f;

		//Return the volume.
		return volume;
	}

	private float GetBlendSpeed()
	{
		//Store the final outcome of the method.
		float speed = 0f;

		//Calculate the volume.
		if(PlayerInformation.Instance.combatMode) speed = PauseManager.Instance.isPaused ? 4.8f : 3.5f;
		else speed = 0.35f;

		//Return the volume.
		return speed;
	}
}
