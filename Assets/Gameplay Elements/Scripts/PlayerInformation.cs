using UnityEngine;
using System.Collections;

public class PlayerInformation : MonoBehaviour
{
	public ParticleSystem explodeParticle;
	public ParticleSystem dashParticle;
	public GameObject crosshairObject;
	public WeaponAsset weapon;
	public int health = 1;
	public int bullets = 120;
	public bool combatMode;
	public bool canTakeDamage = true;

	#region Singleton
	public static PlayerInformation Instance {get; private set;}
	private void Awake()
	{
		if(Instance == null) Instance = this;

		//Stop the dashing particle.
		dashParticle.Stop();

		//Make sure that the explosion effect only plays when it supposed to.
		explodeParticle.Stop();
	}
	#endregion

	private void Start()
	{
		//Equip the initial weapon.
		if(weapon != null) EquipWeapon(weapon);

		//Make sure that the crosshair only shows when it supposed to.
		crosshairObject.SetActive(combatMode);
	}

	private void Update()
	{
		//Temporary feature... Hide mouse cursor.
		Cursor.visible = false;

		//Temporary feature... Switch combat state on key press.
		if(Input.GetKeyDown(KeyCode.X))
		{
			combatMode = !combatMode;
			crosshairObject.SetActive(combatMode);
		}
	}

	public void TakeHit(int damage)
	{
		//Check if the player can take damage.
		if(!canTakeDamage) return;

		//Play the explosion effect.
		explodeParticle.Play();

		//Calculate the new health amount.
		health -= damage;
	}

	public void EquipWeapon(WeaponAsset newWeapon)
	{
		//Gets called from <PickupManager> or <PlayerInformation>, sets the current weapon and its bullet amount.
		weapon = newWeapon;
		weapon.bulletsInClip = newWeapon.clipSize;
	}
}
