using UnityEngine;

public class InteractTrigger : MonoBehaviour
{
	private InteractManager manager;

	private void Start()
	{
		//Initialization.
		manager = InteractManager.Instance;
	}

	private void OnTriggerEnter(Collider col)
	{
		//Make sure that the collider which entered the area is the player's collider and that we can interact.
		if(!col.CompareTag("Player") || !manager.canInteract) return;

		//Create the pickup effect. This will play the pickup sound as well.
		GameObject pickupEffect = Instantiate(manager.pickupEffect, transform.position, manager.pickupEffect.transform.rotation);
		Destroy(pickupEffect, 1f);

		//Send the player object through and execute the interaction with the object.
		GetComponent<InteractableItem>().Interact();
	}
}
