using UnityEngine;
using UnityEngine.UI;

public class InteractManager : MonoBehaviour
{
	public CanvasGroup interactUI;
	public GameObject pickupEffect;
	public Text interactText;
	public LayerMask interactLayer;
	public bool canInteract = true;

	[HideInInspector] public InteractableItem closestInteractable;
	[HideInInspector] public bool hasAvailableInteraction;

	private string closestItemName;

	#region Singleton
	public static InteractManager Instance {get; private set;}
	private void Awake()
	{
		if(Instance == null) Instance = this;
	}
	#endregion

	private void Start()
	{
		//Set the starting interact UI alpha.
		interactUI.alpha = hasAvailableInteraction ? 1 : 0;
	}

	private void Update()
	{
		//Update the alpha of the interact UI based on the available interactions.
		interactUI.alpha = hasAvailableInteraction ? 1 : 0;

		//Check if there is an available interaction.
		if(hasAvailableInteraction && closestInteractable != null)
		{
			//Get the interact object's screen position and update the interact UI's position.
			Vector3 screenPoint = Camera.main.WorldToScreenPoint(closestInteractable.transform.position + closestInteractable.interactDisplayOffset);
			interactUI.transform.position = screenPoint;

			//Update the interact UI's text.
			interactText.text = "<color=orange>[E]</color> " + closestInteractable.itemName;
		}
	}
}
