using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PauseManager : MonoBehaviour
{
	public CanvasGroup pauseUI;
	public bool isPaused;

	private bool unpausedCanMove = true;
	private bool unpausedCanInteract = true;

	private InteractManager interact;
	private PlayerInformation info;
	private TP_Motor motor;

	#region Singleton
	public static PauseManager Instance {get; private set;}
	private void Awake()
	{
		if(Instance == null) Instance = this;
	}
	#endregion

	private void Start()
	{
		//Initialization.
		interact = InteractManager.Instance;
		info = PlayerInformation.Instance;
		motor = TP_Motor.Instance;

		//Set the initial pause UI group's variables.
		pauseUI.alpha = isPaused ? 1 : 0;
		pauseUI.interactable = isPaused;
		pauseUI.blocksRaycasts = isPaused;

		//Initialize the pause menu.
		ContinueGame();
	}

	private void Update()
	{
		//Fade the pause UI.
		pauseUI.alpha = Mathf.Lerp(pauseUI.alpha, isPaused ? 1 : 0, Time.unscaledDeltaTime * (isPaused ? 12f : 16f));
		pauseUI.interactable = isPaused;
		pauseUI.blocksRaycasts = isPaused;
	}

	public void PauseGame()
	{
		//IMPORTANT! Since dash particle is fucked, it has to be paused before time scale gets set to prevent memory leak.
		info.dashParticle.Pause();

		//Update the paused state.
		isPaused = true;

		//Enable the screen blur.
		UI_Graphics.Instance.graphicsProfile.depthOfField.enabled = true;

		//Adjust the time scale.
		Time.timeScale = 0f;
		Time.fixedDeltaTime = 0.02f * Time.timeScale;

		//Pause the sounds.
		AudioListener.pause = true;

		//Get the unpaused states.
		unpausedCanMove = motor.canMove;
		unpausedCanInteract = interact.canInteract;

		//Set the affected states.
		motor.canMove = false;
		interact.canInteract = false;

		//Initializes the pause menu's UI elements and enables interaction with them.
		UI_Manager.Instance.interactable = true;
	}

	public void ContinueGame()
	{
		//Update the paused state and delay the next pause by a few seconds.
		isPaused = false;

		//Disable the screen blur.
		UI_Graphics.Instance.graphicsProfile.depthOfField.enabled = false;

		//Adjust the time scale.
		Time.timeScale = 1f;
		Time.fixedDeltaTime = 0.02f * Time.timeScale;

		//Resume the sounds.
		AudioListener.pause = false;

		//Set the affected states.
		motor.canMove = unpausedCanMove;
		interact.canInteract = unpausedCanInteract;

		//Disable the interaction with the UI.
		UI_Manager.Instance.interactable = false;

		//IMPORTANT! Since the dashing particle is paused, check if we are dashing and restart/clear the particles.
		if(motor.passive.isDashing) info.dashParticle.Play();
		else info.dashParticle.Clear();
	}
}
