using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon Asset")]
public class WeaponAsset : ScriptableObject
{
	//This is a modifiable and instancable unity asset, which holds a bunch of data that define a weapon.

	public string weaponName = "New Weapon";
	public WeaponBullet mainBulletPrefab;
	public WeaponBullet ultimateBulletPrefab;
	public int bulletDamage = 1;
	public int ultimateDamage = 4;
	public int clipSize = 30;
	public float bulletSpread = 1.2f;
	public int bulletSpeed = 40;
	public int ultimateSpeed = 65;
	public float fireRate = 0.1f;
	public float reloadTime = 1f;
	public float chargeSpeed = 5f;
	public AudioClip shootSound;
	public AudioClip ultimateSound;
	public AudioClip reloadSound;
	public float shotVolume = 0.4f;
	public float reloadVolume = 0.5f;

	[HideInInspector] public int bulletsInClip;
}
