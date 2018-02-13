using UnityEngine;

public class WeaponBullet : MonoBehaviour
{
	public GameObject surfaceHitEffect;
	public GameObject enemyHitEffect;
	public LayerMask collisionLayer;
	private float bulletSpeed;

	private void Start()
	{
		//In case the bullet doesn't hit anything, destroy it a few seconds after it spawned.
		Destroy(gameObject, 0.4f);

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
		if(Physics.Raycast(ray, out hit, moveDistance, collisionLayer, QueryTriggerInteraction.Collide))
		{
			//Check if we hit an enemy and apply damage on him.
			if(hit.transform.CompareTag("Enemy"))
			{
				hit.transform.SendMessageUpwards("TakeDamage", 1);
				Instantiate(enemyHitEffect, hit.point, Quaternion.identity);
			}

			//Spawn the hit effect.
			else
			{
				GameObject effect = Instantiate(surfaceHitEffect, hit.point, Quaternion.LookRotation(hit.normal));
				Destroy(effect, 0.1f);
			}

			//Destroy the bullet.
			Destroy(gameObject);
		}
	}

	private void CheckInitialHit()
	{
		//Get an array of collisions the bullet is intersecting with.
		Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionLayer, QueryTriggerInteraction.Collide);

		//Check the length of the array.
		if(initialCollisions.Length > 0)
		{
			//Apply the damage on the first enemy's collider and destroy the bullet.
			initialCollisions[0].transform.SendMessageUpwards("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
			Destroy(gameObject);
		}
	}

	public void SetSpeed(float speed)
	{
		//Gets called from <WeaponManager>, sets the bullet's speed.
		bulletSpeed = speed;
	}
}
