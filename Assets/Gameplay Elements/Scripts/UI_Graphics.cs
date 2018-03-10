using UnityEngine;
using UnityEngine.PostProcessing;

public class UI_Graphics : MonoBehaviour
{
	public PostProcessingProfile graphicsProfile;

	#region Singleton
	public static UI_Graphics Instance {get; private set;}
	private void Awake()
	{
		if(Instance == null) Instance = this;
	}
	#endregion
}
