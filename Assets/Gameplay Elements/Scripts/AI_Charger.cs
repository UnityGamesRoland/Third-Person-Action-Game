using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AI_Charger : MonoBehaviour
{
	public Animator animator;
	public ParticleSystem chargeParticle;
	public GameObject dieEffect;
	public Renderer body;
	public LayerMask collisionLayer;
	public int health = 2;
	public float attackSpeed = 2f;
	public float maxChargeDistance = 140f;
	public float minChargeDistance = 30f;
	public bool prewarmDissolving;
	public bool isDead;
	public bool isCharging;

	private float dissolveAmount = 0f;
	private float distanceToTarget;
	private float attackTimer;

	private NavMeshAgent agent;
	private PlayerInformation target;
	private CharacterController controller;

	private void Start()
	{
		//Initialization.
		agent = GetComponent<NavMeshAgent>();
		target = PlayerInformation.Instance;
		controller = target.GetComponent<CharacterController>();

		//Start the basic walking movement.
		StartCoroutine(UpdateDestination());

		//Make sure that the charging effect only plays when it has to.
		chargeParticle.Stop();

		//Set the default dissolve amount.
		body.material.SetFloat("_DissolveAmount", prewarmDissolving ? 0 : 1);
		dissolveAmount = prewarmDissolving ? 0 : 1;
	}

	private void Update()
	{
		//Calculate the distance between the enemy and the player, and try charging.
		distanceToTarget = (target.transform.position - transform.position).sqrMagnitude;

		//Check if the player is in explosion range.
		if(distanceToTarget <= 1.4f && target.canTakeDamage)
		{
			//Apply the damage on the player and destroy the enemy.
			target.TakeHit(1);
			TakeDamage(100);
		}

		//Check if the enemy is charging.
		if(!isCharging)
		{
			//Handle the enemy's movement animations.
			float moveVelocity = agent.velocity.normalized.magnitude;
			animator.SetFloat("Velocity", moveVelocity);
		}

		//Check if we have to update the dissolve amount.
		if(dissolveAmount != 0)
		{
			//Update the dissolve amount.
			dissolveAmount = Mathf.Lerp(dissolveAmount, 0, Time.deltaTime * 1.3f);
			body.material.SetFloat("_DissolveAmount", dissolveAmount);
		}
	}

	public void TakeDamage(int damage)
	{
		//Check if the enemy is dead.
		if(isDead) return;

		//Calculate the new health amount.
		health -= damage;

		//Check if the amount of damage was enough to kill this enemy.
		if(health <= 0)
		{
			//Set the dying state of the enemy.
			isDead = true;

			//Spawn the destroy effect at the enemy's position.
			GameObject destroyEffect = Instantiate(dieEffect, transform.position, transform.rotation);
			Destroy(destroyEffect, 5);

			//Destroy the enemy.
			StopAllCoroutines();
			Destroy(gameObject);
		}
	}

	private IEnumerator UpdateDestination()
	{
		//Check if the enemy has a target.
		while(target != null && !isDead && !isCharging)
		{
			//Check if the enemy can charge.
			if(Time.time > attackTimer)
			{
				//Check if the enemy is in the right range to charge.
				if(distanceToTarget < maxChargeDistance && distanceToTarget > minChargeDistance)
				{
					//Try charging at the player.
					TryCharging();
				}
			}

			//Recheck that the enemy is not charging. (Could have changed if path was found this iteration.)
			if(!isCharging)
			{
				//Update the destination and delay the next update.
				agent.SetDestination(target.transform.position);
				yield return new WaitForSeconds(0.25f);
			}
		}
	}

	private NavMeshPath GetChargePath(Vector3 chargePosition)
	{
		//Calculate a path to the player's next frame position.
		NavMeshPath simplePath = new NavMeshPath();
		NavMesh.CalculatePath(transform.position, target.transform.position, NavMesh.AllAreas, simplePath);

		//Check if the path is straight.
		if(simplePath.corners.Length == 2 && simplePath.status == NavMeshPathStatus.PathComplete)
		{
			//Calculate a path to the player's next frame position.
			NavMeshPath chargePath = new NavMeshPath();
			NavMesh.CalculatePath(transform.position, chargePosition, NavMesh.AllAreas, chargePath);

			//Get the target path based on the charge path.
			NavMeshPath finalPath = (chargePath.corners.Length == 2 && chargePath.status == NavMeshPathStatus.PathComplete) ? chargePath : simplePath;

			//Return the calculated path.
			return finalPath;
		}

		//Couldn't get path to the given position, returning null.
		else return null;
	}

	private void TryCharging()
	{
		//Calculate the charge position and get the path.
		Vector3 chargePosition = target.transform.position + controller.velocity * controller.velocity.normalized.magnitude;
		NavMeshPath chargePath = GetChargePath(chargePosition);

		//Check if there is an available path.
		if(chargePath != null)
		{
			//Set the charging state and visualize the charge path.
			isCharging = true;

			Debug.DrawLine(transform.position, (chargePath.corners.Length == 2 && chargePath.status == NavMeshPathStatus.PathComplete) ? chargePosition : target.transform.position, Color.red, 3);

			//Start charging.
			StopCoroutine(UpdateDestination());
			StartCoroutine(ChargeTarget(chargePath));
		}
	}

	private IEnumerator ChargeTarget(NavMeshPath path)
	{
		//Store information about the secondary charge.
		bool hasChargedAgain = false;

		//Update the enemy's speed and set it's path.
		agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
		agent.speed = 16f;
		agent.SetPath(path);

		//Get the distance of the charge.
		float overallDistance = agent.remainingDistance;

		//Play the charging effect.
		chargeParticle.Play();

		//Start the charging animation.
		animator.SetBool("Charging", true);

		//Wait for the end of the charge.
		while(agent.remainingDistance > 0.1f)
		{
			//Calculate the percentage of the charge's progress.
			float distancePercent = agent.remainingDistance / overallDistance;
			double formatedDistance = System.Math.Round(distancePercent, 1);

			//Round the percentage and check the progress.
			if(formatedDistance <= 0.5f && distanceToTarget > 5 && !hasChargedAgain)
			{
				//Calculate the charge position and get the path.
				Vector3 chargePosition = target.transform.position + (controller.velocity + ((target.transform.position - transform.position).normalized * 2.3f)) * controller.velocity.normalized.magnitude;
				NavMeshPath chargePath = GetChargePath(chargePosition);

				//Check if there is an available path and charge.
				if(chargePath != null) agent.SetPath(chargePath);

				//Update the charging state.
				hasChargedAgain = true;
			}

			//Wait a frame before the next update.
			yield return null;
		}

		//Update the enemy's speed and set it's path.
		isCharging = false;
		agent.speed = 3.7f;
		agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

		//Stop the charging effect.
		chargeParticle.Stop();

		//Stop the charging animation.
		animator.SetBool("Charging", false);

		//Update the attack timer.
		attackTimer = Time.time + attackSpeed;

		//Return to the basic walking movement.
		StartCoroutine(UpdateDestination());
	}
}
