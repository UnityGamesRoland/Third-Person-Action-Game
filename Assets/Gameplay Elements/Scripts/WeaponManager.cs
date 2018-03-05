using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class WeaponManager : MonoBehaviour
{
	public Transform handTransform;
	public Transform backTransform;
	public CanvasGroup weaponHUD;
	public TMP_Text bulletsInClipText;
	public TMP_Text bulletsInInventoryText;
	public LayerMask muzzleCollisionLayer;

	[HideInInspector] public Transform muzzleTransform;

	private bool isReloading;
	private float actionTimer;
	private float shootTimer;

	private PlayerInformation info;
	private AudioSource source;

	#region Singleton
	public static WeaponManager Instance {get; private set;}
	private void Awake()
	{
		if(Instance == null) Instance = this;
	}
	#endregion

	private void Start()
	{
		//Initialization.
		info = PlayerInformation.Instance;
		source = GetComponent<AudioSource>();

		//Set the default alpha of the weapon display.
		weaponHUD.alpha = info.combatMode ? 1 : 0.2f;
	}

	private void Update()
	{
		//Check if we have a weapon.
		if(info.weapon != null)
		{
			//Update the weapon's position and rotation.
			UpdateWeaponPosition();

			//Update the bullet display's text.
			bulletsInClipText.text = info.weapon.bulletsInClip.ToString();
			bulletsInInventoryText.text = info.bullets.ToString();

			//Check if we are in combat mode.
			if(info.combatMode)
			{
				//Check if we can perform any actions.
				if(Time.time > actionTimer && !TP_Motor.Instance.passive.isDashing)
				{
					//Check for shooting input.
					if(Input.GetMouseButton(0) && info.weapon.bulletsInClip > 0 && Time.time > shootTimer)
					{
						//Launch a projectile from the muzzle.
						Shoot();
					}

					//Check for reloading input.
					if(Input.GetKeyDown(KeyCode.R) && info.weapon.bulletsInClip < info.weapon.clipSize && info.bullets > 0)
					{
						//Start the reloading process.
						StartCoroutine(Reload());
					}
				}
			}
		}

		//Update the weapon HUD.
		weaponHUD.alpha = Mathf.Lerp(weaponHUD.alpha, info.combatMode ? 1f : 0.2f, Time.deltaTime * 10);
	}

	private void UpdateWeaponPosition()
	{
		//Check if we have a weapon object.
		if(info.weaponObject == null) return;

		//Weapon placing: In Hand
		if(info.combatMode || isReloading)
		{
			info.weaponObject.transform.SetParent(handTransform);
			info.weaponObject.transform.localPosition = info.weapon.positionOffsetInHand;
			info.weaponObject.transform.localRotation = Quaternion.Euler(info.weapon.rotationOffsetInHand);
		}

		//Weapon placing: On Back
		else
		{
			info.weaponObject.transform.SetParent(backTransform);
			info.weaponObject.transform.localPosition = info.weapon.positionOffsetOnBack;
			info.weaponObject.transform.localRotation = Quaternion.Euler(info.weapon.rotationOffsetOnBack);
		}
	}

	private bool GetMuzzleCollision()
	{
		//Store the outcome of the collision check.
		bool muzzleCollision = false;

		//Setup the ray.
		Ray ray = new Ray(info.transform.position, info.transform.forward);
		RaycastHit hit;

		//Check if the ray hit something.
		if(Physics.Raycast(ray, out hit, 0.78f, muzzleCollisionLayer, QueryTriggerInteraction.Collide)) muzzleCollision = true;

		//Return the outcome of the collision check.
		return muzzleCollision;
	}

	private void Shoot()
	{
		//Check muzzle collision.
		if(GetMuzzleCollision()) return;

		//Generate the spread vector.
		Vector3 spreadVector = new Vector3(Random.Range(-info.weapon.bulletSpread, info.weapon.bulletSpread), Random.Range(-info.weapon.bulletSpread, info.weapon.bulletSpread), 0);

		//Store the instantiated bullet and set its damage and speed.
		WeaponBullet bullet = Instantiate(info.weapon.bulletPrefab, muzzleTransform.position, Quaternion.Euler(info.transform.eulerAngles + spreadVector)) as WeaponBullet;
		bullet.InitializeBullet(info.weapon.bulletDamage, info.weapon.bulletSpeed, info.weapon.bulletHealth);

		//Update the bullet count.
		info.weapon.bulletsInClip --;

		//Shake the camera.
		TP_Camera.Instance.Shake(info.weapon.recoil.x, info.weapon.recoil.y, info.weapon.recoil.z);

		//Play the shooting sound.
		source.PlayOneShot(info.weapon.shootSound, info.weapon.shotVolume);

		//Update the action timer.
		shootTimer = Time.time + info.weapon.fireRate;

		//After the shot check if we should reload or not.
		if(info.weapon.bulletsInClip == 0 && Time.time > actionTimer && info.bullets > 0) StartCoroutine(Reload());
	}

	private IEnumerator Reload()
	{
		//Update the action timer and add a bit of delay to ensure that the reload process is finished before shooting.
		isReloading = true;
		actionTimer = Time.time + info.weapon.reloadTime + 0.1f;

		//Play the reload animation.
		TP_Animations.Instance.PlayReloadAnimation();

		//Play the shooting sound.
		source.PlayOneShot(info.weapon.reloadSound, info.weapon.reloadVolume);

		//Delay the actual reloading mechanics.
		yield return new WaitForSeconds(info.weapon.reloadTime);

		//Get the amount of bullets we have to load.
		int bulletsToLoad = info.weapon.clipSize - info.weapon.bulletsInClip;

		//Check if the amount of bullets in the inventory is less then the amount we will load.
		if(info.bullets <= bulletsToLoad)
		{
			//Load the bullets.
			info.weapon.bulletsInClip += info.bullets;
			info.bullets = 0;
		}

		//Check if the amount of bullets in the inventory is more then the amount we will load.
		else
		{
			//Load the bullets.
			info.weapon.bulletsInClip += bulletsToLoad;
			info.bullets -= bulletsToLoad;
		}

		//Update the reloading state.
		isReloading = false;
	}
}
