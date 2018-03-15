using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UI_SceneManager : MonoBehaviour
{
	public GameObject loadingScreenUI;
	public GameObject continueText;
	public bool isLoading;

	#region Singleton
	public static UI_SceneManager Instance {get; private set;}
	private void Awake()
	{
		if(Instance == null) Instance = this;
	}
	#endregion

	public void LoadLevel(int index)
	{
		//Check if the game is loading something.
		if(isLoading) return;

		//Start the loading process.
		StartCoroutine(LoadLevelAsync(index));
	}

	public void LoadNextLevel()
	{
		//Check if the game is loading something.
		if(isLoading) return;

		//Get the target level index.
		int targetLevel = SceneManager.GetActiveScene().buildIndex + 1;

		//Start the loading process.
		StartCoroutine(LoadLevelAsync(targetLevel));
	}

	private IEnumerator LoadLevelAsync(int index)
	{
		//Make sure that no other loading will start while the current async operation is running.
		UI_Manager.Instance.interactable = false;
		isLoading = true;

		//Enable the loading screen.
		continueText.SetActive(false);
		loadingScreenUI.SetActive(true);

		//Start the loading process.
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(index);
		asyncLoad.allowSceneActivation = false;

		//Check if the loading is still going.
		while(!asyncLoad.isDone)
		{
			//Check if the loading has completed.
			if(asyncLoad.progress == 0.9f)
			{
				//Enable the continue text.
				continueText.SetActive(true);

				//Look for input and enable scene actiavtion to fully load the target scene.
				if(Input.anyKeyDown) asyncLoad.allowSceneActivation = true;
			}

			//Wait for the next frame.
			yield return null;
		}
	}
}
