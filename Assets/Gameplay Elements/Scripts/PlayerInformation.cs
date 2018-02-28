using UnityEngine;
using System.Collections;

public class PlayerInformation : MonoBehaviour
{
	public ParticleSystem explodeParticle;
	public ParticleSystem dashParticle;
	public WeaponAsset weapon;
	public int health = 1;
	public int bullets = 120;
	public bool combatMode;
	public bool canTakeDamage = true;
	public bool isDead;

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
	}

	private void Update()
	{
		//Temporary feature... Hide mouse cursor.
		Cursor.visible = false;

		//Temporary feature... Switch combat state on key press.
		if(Input.GetKeyDown(KeyCode.X))
		{
			combatMode = !combatMode;
		}
	}

	public void TakeHit(int damage)
	{
		//Check if the player can take damage.
		if(!canTakeDamage || isDead) return;

		//Play the explosion effect.
		explodeParticle.Play();

		//Calculate the new health amount.
		health -= damage;

		//Check if the player should be dead.
		if(health <= 0) StartCoroutine(Die());
	}

	public void EquipWeapon(WeaponAsset newWeapon)
	{
		//Gets called from <PickupManager> or <PlayerInformation>, sets the current weapon and its bullet amount.
		weapon = newWeapon;
		weapon.bulletsInClip = newWeapon.clipSize;
	}

	private IEnumerator Die()
	{
		//Set the dying state.
		isDead = true;

		//Update the time scale.
		Time.timeScale = 0.05f;
		Time.fixedDeltaTime = 0.02f * Time.timeScale;

		//Temporary feature... Resurrect the player.
		Debug.Log("Player is dead!");
		yield return new WaitForSecondsRealtime(4);
		isDead = false;
		Time.timeScale = 1f;
		Time.fixedDeltaTime = 0.02f * Time.timeScale;
	}
}
