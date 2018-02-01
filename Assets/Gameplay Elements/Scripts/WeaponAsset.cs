using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon Asset")]
public class WeaponAsset : ScriptableObject
{
	//This is a modifiable and instancable unity asset.
	//It holds a bunch of data which define a weapon in the game.

	public string weaponName = "New Weapon";
	public int clipSize = 30;
	public int bulletSpeed = 40;
	public float fireRate = 0.1f;
	public float reloadTime = 1f;

	[HideInInspector] public int bulletsInClip;
}
