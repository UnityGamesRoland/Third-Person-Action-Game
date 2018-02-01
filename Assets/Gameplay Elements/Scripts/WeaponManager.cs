using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
	public WeaponBullet bulletPrefab;
	public Transform muzzlePosition;
	public CanvasGroup weaponHUD;
	public Text weaponNameText;
	public Text bulletsInClipText;
	public Text bulletsInInventoryText;

	private float actionTimer;
	private PlayerInformation info;

	private void Start()
	{
		//Initialization.
		info = FindObjectOfType<PlayerInformation>();

		//Set the default alpha of the weapon display.
		weaponHUD.alpha = info.combatMode ? 1 : 0;
	}

	private void Update()
	{
		//Check if we are in combat and that we have a weapon.
		if(info.combatMode && info.weapon != null)
		{
			//Update the weapon name display;
			weaponNameText.text = info.weapon.weaponName;

			//Update the weapon HUD.
			weaponHUD.alpha = Mathf.Lerp(weaponHUD.alpha, 1, Time.deltaTime * 10);

			//Check if the timer is ready.
			if(Time.time > actionTimer)
			{
				//Check for shooting input.
				if(Input.GetMouseButton(0) && info.weapon.bulletsInClip > 0)
				{
					//Launch a projectile from the muzzle.
					Shoot();
				}

				//Check for reloading input.
				if(Input.GetKeyDown(KeyCode.R) && info.weapon.bulletsInClip < info.weapon.clipSize)
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
		}
	}

	private void Shoot()
	{
		//Generate the spread vector.
		Vector3 spreadVector = new Vector3(Random.Range(-1.8f, 1.8f), Random.Range(-1.8f, 1.8f), 0);

		//Store the instantiated bullet and set its damage and speed.
		WeaponBullet bullet = Instantiate(bulletPrefab, muzzlePosition.position, Quaternion.Euler(muzzlePosition.localEulerAngles + spreadVector)) as WeaponBullet;
		bullet.SetSpeed(info.weapon.bulletSpeed);

		//Update the bullet count.
		info.weapon.bulletsInClip --;

		//Update the bullet display.
		UpdateBulletDisplay();

		//Update the action timer.
		actionTimer = Time.time + info.weapon.fireRate;
	}

	private IEnumerator Reload()
	{
		//Update the action timer.
		actionTimer = Time.time + info.weapon.reloadTime;

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

		//Update the bullet display.
		UpdateBulletDisplay();
	}

	private void UpdateBulletDisplay()
	{
		//Update the bullet display's text.
		bulletsInClipText.text = info.weapon.bulletsInClip.ToString();
		bulletsInInventoryText.text = info.bullets.ToString();
	}
}
