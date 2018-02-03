using UnityEngine;
using UnityEngine.UI;

public class InteractableItem : MonoBehaviour
{
	public enum ItemType {weapon, bullets}
	public ItemType itemType;
	public Vector3 interactDisplayOffset;
	public string itemName = "New Item";
	public int bullets = 10;
	public WeaponAsset weapon;

	public void Interact(GameObject playerObject)
	{
		//Get the player information script.
		PlayerInformation info = playerObject.GetComponent<PlayerInformation>();

		//Check if the item is a weapon.
		if(itemType == ItemType.weapon)
		{
			//Equip the new weapon.
			info.EquipWeapon(weapon);
		}

		//Check if the item is a bullet.
		if(itemType == ItemType.bullets)
		{
			//Add the bullets to the inventory.
			info.bullets += bullets;
		}

		//Destroy the object after pickup.
		Destroy(gameObject);
	}
}
