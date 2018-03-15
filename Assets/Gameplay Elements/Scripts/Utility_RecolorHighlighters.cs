using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class Utility_RecolorHighlighters : MonoBehaviour
{
	public Color outlineColor;
	public Color backgroundColor;

	public void RecolorHighlightElements()
	{
		//Get all gameObjects with Image component.
		Image[] backgrounds = Resources.FindObjectsOfTypeAll<Image>();

		//Loop through the elements.
		foreach(Image image in backgrounds)
		{
			//Check if the current image component's parent is a hover object.
			if(image.transform.parent.name == "Highlight")
			{
				//Recolorize.
				if(image.name == "Outline") image.color = outlineColor;
				if(image.name == "Background") image.color = backgroundColor;
			}
		}
	}
}

[CustomEditor(typeof(Utility_RecolorHighlighters))]
public class RecolorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		//Draw the default inspector.
		DrawDefaultInspector();

		//Get the script in the inspector's target and draw the button.
		Utility_RecolorHighlighters myScript = (Utility_RecolorHighlighters)target;
		if(GUILayout.Button("Recolor UI Highlighters"))
		{
			//Execute the recoloring method on button press.
			myScript.RecolorHighlightElements();
		}
	}
}