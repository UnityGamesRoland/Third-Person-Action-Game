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

	[HideInInspector] public GameObject weaponObject;

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
		Cursor.visible = PauseManager.Instance.isPaused ? true : false;

		//Temporary feature... Switch combat state on key press. (Extra delay to allow animation exit time to finish)
		if(Input.GetKeyDown(KeyCode.X) && !PauseManager.Instance.isPaused && Time.time > WeaponManager.Instance.actionTimer + 0.22f)
		{
			//Switch the combat mode.
			combatMode = !combatMode;

			//Play the draw/holster animation based on the combat mode.
			if(combatMode) TP_Animations.Instance.PlayDrawAnimation();
			else TP_Animations.Instance.PlayHolsterAnimation();
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
		//Set the current weapon and its bullet amount.
		weapon = newWeapon;
		weapon.bulletsInClip = newWeapon.clipSize;

		//Set the weapon object and the muzzle transform.
		weaponObject = GameObject.Find(newWeapon.weaponName);
		if(weaponObject != null) WeaponManager.Instance.muzzleTransform = weaponObject.transform.Find("C_Muzzle");

	}

	private IEnumerator Die()
	{
		//Set the dying state.
		isDead = true;

		//Update the time scale.
		Time.timeScale = 0.05f;
		Time.fixedDeltaTime = 0.02f * Time.timeScale;

		//Temporary feature... Resurrect the player.
		yield return new WaitForSecondsRealtime(1.5f);
		isDead = false;
		Time.timeScale = 1f;
		Time.fixedDeltaTime = 0.02f * Time.timeScale;
	}
}
