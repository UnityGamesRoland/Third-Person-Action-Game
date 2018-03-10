using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UI_Layer
{
	public string layerName;
	public int layerIndex;
	public GameObject layerObject;
	public UI_Element firstElement;
}

public class UI_Manager : MonoBehaviour
{
	public UI_Layer[] layers;
	public bool interactable;

	[HideInInspector] public UI_Layer selectedLayer;
	[HideInInspector] public UI_Element selectedElement;

	private UI_Layer previousLayer;
	private PauseManager pause;
	private bool controlsGamePause = false;

	#region Singleton
	public static UI_Manager Instance {get; private set;}
	private void Awake()
	{
		if(Instance == null) Instance = this;
	}
	#endregion

	private void Start()
	{
		//Move to the base layer at game start.
		MoveToLayer(0);

		//Check if the UI manager should control the pause manager as well.
		pause = PauseManager.Instance;
		if(pause != null) controlsGamePause = true;
	}

	private void Update()
	{
		//First check for navigation input, then execution input.
		GetNavigationInput();
		GetExecutionInput();
	}

	private void GetNavigationInput()
	{
		//Moving to next layer: Below <selectedLayer>
		if(Input.GetKeyDown(KeyCode.Escape) && !PlayerInformation.Instance.isDead) HandleEscapeInput();

		//Check if we can interact with the UI.
		if(!interactable) return;

		//Moving to next UI element: Below <selectedElement>
		if(Input.GetKeyDown(KeyCode.DownArrow)) SetSelectedElement(false);

		//Moving to next UI element: Above <selectedElement>
		if(Input.GetKeyDown(KeyCode.UpArrow)) SetSelectedElement(true);
	}

	private void GetExecutionInput()
	{
		//Check if we can interact with the UI.
		if(!interactable) return;

		//Check if the selected UI is a single channel one.
		if(selectedElement.elementType == UI_Element_Type.buttonElement)
		{
			//Execute UI element: Single Channel Click Event.
			if(Input.GetKeyDown(KeyCode.Return)) selectedElement.ClickEvent.Invoke();
		}

		else
		{
			//Execute UI element: Negative (Left) Direction Event.
			if(Input.GetKeyDown(KeyCode.LeftArrow)) selectedElement.NegativeEvent.Invoke();

			//Execute UI element: Positive (Right) Direction Event.
			if(Input.GetKeyDown(KeyCode.RightArrow)) selectedElement.PositiveEvent.Invoke();
		}
	}

	private void HandleEscapeInput()
	{
		//Get the selected layer's index.
		int selectedLayerIndex = GetSelectedLayerIndex();

		//Check if the UI manager controls the pause manager.
		if(controlsGamePause)
		{
			//Check if the game is paused.
			if(pause.isPaused)
			{
				//If the selected layer is not the base layer, descend one layer.
				if(selectedLayerIndex > 0)
				{
					MoveToPreviousLayer();
					return;
				}

				//If the selected layer is the base layer, continue the game.
				else if(selectedLayerIndex == 0)
				{
					pause.ContinueGame();
					return;
				}
			}

			//Check if the game is not paused.
			else
			{
				//Pause the game and move to base layer.
				pause.PauseGame();
				MoveToLayer(0);
			}
		}

		//The UI manager doesn't control the pause manager.
		else
		{
			//If the selected layer is not the base layer, descend one layer.
			if(selectedLayerIndex > 0)
			{
				MoveToPreviousLayer();
				return;
			}
		}
	}

	public void MoveToLayer(int index)
	{
		//Disable the current layer's element holder object.
		if(selectedLayer.layerName != "")
		{
			previousLayer = selectedLayer;
			selectedElement.outlineObject.SetActive(false);
			selectedLayer.layerObject.SetActive(false);
		}

		//Loop through the layer array.
		for(int i = 0; i < layers.Length; i++)
		{
			//Check if the layer index is matching.
			if(layers[i].layerIndex == index)
			{
				//Update the selected layer and its corresponding elements.
				selectedLayer = layers[i];
				selectedLayer.layerObject.SetActive(true);
				selectedElement = layers[i].firstElement;
				selectedElement.outlineObject.SetActive(true);
				break;
			}
		}
	}

	public void MoveToPreviousLayer()
	{
		//Check if there was a previous layer and move to it.
		if(previousLayer.layerName != "") MoveToLayer(previousLayer.layerIndex);

		//No previous layer. Returning to base layer.
		else MoveToLayer(0);
	}

	public int GetSelectedLayerIndex()
	{
		//Store the final outcome of the method.
		int index = -1;

		//Check if there is a selected layer and get it's index.
		if(selectedLayer.layerName != "") index = selectedLayer.layerIndex;

		//Return the layer index.
		return index;
	}

	private void SetSelectedElement(bool upMovement)
	{
		//Check if there is a selected element that we can start from.
		if(selectedElement != null)
		{
			//Get the next UI element based on the given direction.
			UI_Element nextElement = upMovement ? selectedElement.elementAbove : selectedElement.elementBelow;

			//Check if there is a new element.
			if(nextElement != null)
			{
				//Disable the currently selected element's outline object, update the selected element and enable it's outline object in this exact order.
				selectedElement.outlineObject.SetActive(false);
				selectedElement = nextElement;
				selectedElement.outlineObject.SetActive(true);
			}
		}

		//Selected element not specified.
		else
		{
			//Selected layer not specified. Returning to base layer.
			if(selectedLayer == null) MoveToLayer(0);

			//Update the selected element and it's outline object.
			selectedElement = selectedLayer.firstElement;
			selectedElement.outlineObject.SetActive(true);
		}
	}

	public void SetSelectedElement(UI_Element newElement)
	{
		//Disable the currently selected element's outline object.
		if(selectedElement != null) selectedElement.outlineObject.SetActive(false);

		//Update the selected element and it's outline object.
		selectedElement = newElement;
		selectedElement.outlineObject.SetActive(true);
	}
}
