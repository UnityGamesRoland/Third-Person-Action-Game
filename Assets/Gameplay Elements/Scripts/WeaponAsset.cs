using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon Asset")]
public class WeaponAsset : ScriptableObject
{
	//This is a modifiable and instancable unity asset, which holds a bunch of data that define a weapon.

	public string weaponName = "New Weapon";
	public WeaponBullet bulletPrefab;
	public int clipSize = 30;
	public int bulletDamage = 1;
	public int bulletHealth = 1;
	public float bulletSpread = 1.2f;
	public int bulletSpeed = 40;
	public float fireRate = 0.1f;
	public float reloadTime = 1f;
	public Vector3 recoil = new Vector3(0.25f, 0.2f, 0);
	public Vector3 positionOffsetInHand;
	public Vector3 rotationOffsetInHand;
	public Vector3 positionOffsetOnBack;
	public Vector3 rotationOffsetOnBack;
	public AudioClip shootSound;
	public AudioClip reloadSound;

	[Range(0, 1)] public float shotVolume = 0.4f;
	[Range(0, 1)] public float reloadVolume = 0.5f;

	[HideInInspector] public int bulletsInClip;
}
