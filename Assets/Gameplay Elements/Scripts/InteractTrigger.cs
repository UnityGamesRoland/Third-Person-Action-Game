using UnityEngine;

public class InteractTrigger : MonoBehaviour
{
	private InteractManager manager;

	private void Start()
	{
		//Initialization.
		manager = FindObjectOfType<InteractManager>();
	}

	private void OnTriggerEnter(Collider col)
	{
		//Make sure that the collider which entered the area is the player's collider.
		if(!col.CompareTag("Player")) return;

		//Create the pickup effect.
		Instantiate(manager.pickupEffect, transform.position, manager.pickupEffect.transform.rotation);

		//Send the player object through and execute the interaction with the object.
		GetComponent<InteractableItem>().Interact();
	}
}
