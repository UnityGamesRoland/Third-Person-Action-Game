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

		float targetVolume = PlayerInformation.Instance.combatMode ? 0f : -80f;
		mixer.SetFloat("combatMusicVolume", targetVolume);
	}

	private void Update()
	{
		if(!source.isPlaying)
		{
			int randomIndex = Random.Range(0, combatMusics.Length);
			source.clip = combatMusics[randomIndex];
			source.Play();
		}

		float currentVolume;
		mixer.GetFloat("combatMusicVolume", out currentVolume);

		float targetVolume = Mathf.Lerp(currentVolume, PlayerInformation.Instance.combatMode ? 0f : -80f, Time.deltaTime * (PlayerInformation.Instance.combatMode ? 3.5f : 0.2f));
		mixer.SetFloat("combatMusicVolume", targetVolume);
	}
}
