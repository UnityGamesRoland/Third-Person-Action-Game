using UnityEngine;
using System.Collections.Generic;

public class InteractArea : MonoBehaviour
{
	private List<GameObject> interactableObjects;
	private InteractManager manager;

	private void Start()
	{
		//Initialization.
		manager = FindObjectOfType<InteractManager>();

		//Get every interactable object inside the area.
		GetInteractableObjectsInArea();
	}

	private void GetInteractableObjectsInArea()
	{
		//Get all objects in area with a specific layer.
		Collider[] objectsInArea = Physics.OverlapBox(transform.position, transform.localScale, Quaternion.identity, manager.interactLayer, QueryTriggerInteraction.Collide);

		//Reset the array.
		interactableObjects = new List<GameObject>();

		//Loop through all the colliders that are inside the area.
		for(int i = 0; i < objectsInArea.Length; i++)
		{
			//Assign the colliders to gameObjects.
			interactableObjects.Add(objectsInArea[i].gameObject);
		}
	}

	private GameObject GetClosestInteractable()
	{
		//Create the holders for the closest object and the distance.
		GameObject closestInteractable = null;
		float closestDistance = Mathf.Infinity;

		//Loop through every interactable inside the area.
		foreach(GameObject interactable in interactableObjects)
		{
			//Get the direction and the distance from the target.
			Vector3 directionToTarget = interactable.transform.position - manager.transform.position;
			float distanceToTarget = directionToTarget.sqrMagnitude;

			//Check if the distance is less than the current closest distance.
			if(distanceToTarget < closestDistance)
			{
				//Update the distance and the object.
				closestDistance = distanceToTarget;
				closestInteractable = interactable;
			}
		}

		//Send the closest interactable back.
		return closestInteractable;
	}

	private void OnTriggerStay(Collider col)
	{
		//Make sure that the collider which entered the area is the player's collider.
		if(!col.CompareTag("Player") && interactableObjects.Count > 0) return;

		//Get the closest interactable to the player.
		GameObject closestInteractable = GetClosestInteractable();

		//Check for interact input.
		if(Input.GetKeyDown(KeyCode.E) && closestInteractable != null && manager.canInteract)
		{
			//Send the player object through and execute the interaction with the object.
			interactableObjects.Remove(closestInteractable);
			closestInteractable.SendMessage("Interact", manager.gameObject);
		}
	}

	private void OnDrawGizmosSelected()
	{
		//Draw the fill of the area.
		Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
		Gizmos.DrawCube(transform.position, transform.localScale);

		//Draw the outline of the area.
		Gizmos.color = new Color(0f, 0f, 1f);
		Gizmos.DrawWireCube(transform.position, transform.localScale);
	}
}
