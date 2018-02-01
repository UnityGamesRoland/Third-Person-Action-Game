using UnityEngine;

public class PlayerInformation : MonoBehaviour
{
	public WeaponAsset weapon;
	public int health = 5;
	public int bullets = 120;
	public bool combatMode;

	private void Start()
	{
		//Equip the initial weapon.
		if(weapon != null) EquipWeapon(weapon);
	}

	private void Update()
	{
		//Temporary feature... Switch combat state on key press.
		if(Input.GetKeyDown(KeyCode.X)) combatMode = !combatMode;
	}

	public void TakeHit(int damage)
	{
		//Calculate the new health amount.
		health -= damage;

		//Check if the amount of damage was enough to kill this enemy.
		if(health <= 0)
		{
			//Temporary feature... Send a notification to the console.
			Debug.Log("Player died!");
		}
	}

	public void EquipWeapon(WeaponAsset newWeapon)
	{
		//Gets called from <PickupManager> or <PlayerInformation>, sets the current weapon and its bullet amount.
		weapon = newWeapon;
		weapon.bulletsInClip = newWeapon.clipSize;
	}
}
