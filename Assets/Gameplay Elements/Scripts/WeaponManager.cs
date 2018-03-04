using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class WeaponManager : MonoBehaviour
{
	public Transform muzzlePosition;
	public CanvasGroup weaponHUD;
	public TMP_Text bulletsInClipText;
	public TMP_Text bulletsInInventoryText;

	private float actionTimer;
	private float shootTimer;

	private PlayerInformation info;
	private AudioSource source;

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
			//Update the bullet display's text.
			bulletsInClipText.text = info.weapon.bulletsInClip.ToString();
			bulletsInInventoryText.text = info.bullets.ToString();

			//Check if we can perform any actions.
			if(info.combatMode && Time.time > actionTimer && !TP_Motor.Instance.passive.isDashing)
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

		//Update the weapon HUD.
		weaponHUD.alpha = Mathf.Lerp(weaponHUD.alpha, info.combatMode ? 1f : 0.2f, Time.deltaTime * 10);
	}

	private void Shoot()
	{
		//Generate the spread vector.
		Vector3 spreadVector = new Vector3(Random.Range(-info.weapon.bulletSpread, info.weapon.bulletSpread), Random.Range(-info.weapon.bulletSpread, info.weapon.bulletSpread), 0);

		//Store the instantiated bullet and set its damage and speed.
		WeaponBullet bullet = Instantiate(info.weapon.bulletPrefab, muzzlePosition.position, Quaternion.Euler(muzzlePosition.localEulerAngles + spreadVector)) as WeaponBullet;
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
	}
}
