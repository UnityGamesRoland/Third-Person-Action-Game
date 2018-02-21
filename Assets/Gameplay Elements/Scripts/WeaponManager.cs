using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
	public Transform muzzlePosition;
	public CanvasGroup weaponHUD;
	public Text bulletsInClipText;
	public Text bulletsInInventoryText;

	private float actionTimer;
	private float shootTimer;

	private float chargeProgress;
	private bool isCharging;

	private PlayerInformation info;
	private AudioSource source;

	private void Start()
	{
		//Initialization.
		info = PlayerInformation.Instance;
		source = GetComponent<AudioSource>();

		//Set the default alpha of the weapon display.
		weaponHUD.alpha = info.combatMode ? 1 : 0;
	}

	private void Update()
	{
		//Check if we are in combat and that we have a weapon.
		if(info.combatMode && info.weapon != null)
		{
			//Update the bullet display's text.
			bulletsInClipText.text = info.weapon.bulletsInClip.ToString();
			bulletsInInventoryText.text = info.bullets.ToString();

			//Update the weapon HUD.
			weaponHUD.alpha = Mathf.Lerp(weaponHUD.alpha, 1, Time.deltaTime * 10);

			if(Time.time > actionTimer && !TP_Motor.Instance.passive.isDashing)
			{
				//Check for shooting input.
				if(Input.GetMouseButton(0) && info.weapon.bulletsInClip > 0 && Time.time > shootTimer && !isCharging)
				{
					//Launch a projectile from the muzzle.
					Shoot();
				}

				if(Input.GetMouseButton(1))
				{
					//Update the charging state.
					isCharging = true;

					//Update the charge progress.
					chargeProgress += Time.deltaTime * info.weapon.chargeSpeed;
					chargeProgress = Mathf.Clamp01(chargeProgress);
				}

				if(Input.GetMouseButtonUp(1))
				{
					//Update the charging state.
					isCharging = false;

					//Check if the ultimate is charged.
					if(chargeProgress >= 1) UnleashUltimate();

					//Reset the charge progress.
					chargeProgress = 0f;
				}

				//Check for reloading input.
				if(Input.GetKeyDown(KeyCode.R) && info.weapon.bulletsInClip < info.weapon.clipSize && info.bullets > 0)
				{
					//Start the reloading process.
					StartCoroutine(Reload());
				}
			}
		}

		else
		{
			//Update the weapon HUD.
			weaponHUD.alpha = Mathf.Lerp(weaponHUD.alpha, 0, Time.deltaTime * 8);

			//Reset the charge progress.
			chargeProgress = 0f;
		}
	}

	private void Shoot()
	{
		//Generate the spread vector.
		Vector3 spreadVector = new Vector3(Random.Range(-info.weapon.bulletSpread, info.weapon.bulletSpread), Random.Range(-info.weapon.bulletSpread, info.weapon.bulletSpread), 0);

		//Store the instantiated bullet and set its damage and speed.
		WeaponBullet bullet = Instantiate(info.weapon.mainBulletPrefab, muzzlePosition.position, Quaternion.Euler(muzzlePosition.localEulerAngles + spreadVector)) as WeaponBullet;
		bullet.InitializeBullet(info.weapon.bulletDamage, info.weapon.bulletSpeed, false);

		//Update the bullet count.
		info.weapon.bulletsInClip --;

		//Shake the camera.
		TP_Camera.Instance.Shake(0.25f, 0.2f, 0);

		//Play the shooting sound.
		source.PlayOneShot(info.weapon.shootSound, info.weapon.shotVolume);

		//Update the action timer.
		shootTimer = Time.time + info.weapon.fireRate;

		//After the shot check if we should reload or not.
		if(info.weapon.bulletsInClip == 0 && Time.time > actionTimer && info.bullets > 0) StartCoroutine(Reload());
	}

	private void UnleashUltimate()
	{
		//Store the instantiated bullet and set its damage and speed.
		WeaponBullet bullet = Instantiate(info.weapon.ultimateBulletPrefab, muzzlePosition.position, muzzlePosition.rotation) as WeaponBullet;
		bullet.InitializeBullet(info.weapon.ultimateDamage, info.weapon.ultimateSpeed, true);

		//Shake the camera.
		TP_Camera.Instance.Shake(0.6f, 0.2f, 1);

		//Play the shooting sound.
		source.PlayOneShot(info.weapon.ultimateSound, info.weapon.shotVolume);

		//Delay the shooting.
		shootTimer = Time.time + (info.weapon.fireRate * 3);
	}

	private IEnumerator Reload()
	{
		//Update the action timer.
		actionTimer = Time.time + info.weapon.reloadTime;

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
