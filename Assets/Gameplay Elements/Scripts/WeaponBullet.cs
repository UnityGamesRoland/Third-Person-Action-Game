using UnityEngine;

public class WeaponBullet : MonoBehaviour
{
	public GameObject surfaceHitEffect;
	public GameObject enemyHitEffect;
	public LayerMask collisionLayer;
	public float bulletRadius = 0.5f;

	private float bulletDamage;
	private float bulletSpeed;
	private bool isUltimate;

	private void Start()
	{
		//In case the bullet doesn't hit anything, destroy it a few seconds after it spawned.
		Destroy(gameObject, 0.8f);

		//Check if the bullet spawns inside an enemy.
		CheckInitialHit();
	}

	private void Update()
	{
		//Get the amount of distance the bullet will move this frame.
		float moveDistance = bulletSpeed * Time.deltaTime;

		//Check if the bullet will hit something.
		CheckHit(moveDistance);

		//Move the bullet forward every frame.
		transform.Translate(Vector3.forward * moveDistance);
	}

	private void CheckHit(float moveDistance)
	{
		//Setup a forward ray.
		Ray ray = new Ray(transform.position, transform.forward);
		RaycastHit hit;

		//Check if the ray hits something.
		if(Physics.SphereCast(ray, bulletRadius, out hit, moveDistance, collisionLayer, QueryTriggerInteraction.Collide))
		{
			//On Hit: Enemy
			if(hit.transform.CompareTag("Enemy"))
			{
				//Apply damage and spawn hit effect.
				hit.transform.SendMessageUpwards("TakeDamage", bulletDamage);
				Instantiate(enemyHitEffect, hit.point, Quaternion.identity);

				//Destroy the bullet.
				if(!isUltimate) Destroy(gameObject);
			}

			//On Hit: Other
			else
			{
				//Spawn hit effect.
				GameObject effect = Instantiate(surfaceHitEffect, hit.point, Quaternion.LookRotation(hit.normal));
				Destroy(effect, 0.1f);

				//Destroy the bullet.
				Destroy(gameObject);
			}
		}
	}

	private void CheckInitialHit()
	{
		//Get an array of collisions the bullet is intersecting with.
		Collider[] initialCollisions = Physics.OverlapSphere(transform.position, bulletRadius, collisionLayer, QueryTriggerInteraction.Collide);

		//Check the length of the array.
		if(initialCollisions.Length > 0)
		{
			//Check if this is a simple bullet.
			if(!isUltimate)
			{
				//Apply damage on the first enemy's collider and destroy the bullet.
				initialCollisions[0].transform.SendMessageUpwards("TakeDamage", bulletDamage, SendMessageOptions.DontRequireReceiver);
				Destroy(gameObject);
			}

			//Check if this is a ultimate bullet.
			else
			{
				//Apply damage on each enemy inside the bullet radius.
				foreach(Collider col in initialCollisions) col.transform.SendMessageUpwards("TakeDamage", bulletDamage, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void InitializeBullet(int damage, float speed, bool ultimate)
	{
		//Gets called from <WeaponManager>, sets the bullet's damage and speed.
		bulletDamage = damage;
		bulletSpeed = speed;
		isUltimate = ultimate;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, bulletRadius);
	}
}
