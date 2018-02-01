using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AI_Charger : MonoBehaviour
{
	public Animator animator;
	public ParticleSystem chargeParticle;
	public LayerMask collisionLayer;
	public int health = 3;
	public float attackSpeed = 2f;
	public float maxChargeDistance = 140f;
	public float minChargeDistance = 30f;
	public bool isDead;
	public bool isCharging;

	private float attackTimer;

	private NavMeshAgent agent;
	private PlayerInformation target;
	private CharacterController controller;

	private void Start()
	{
		//Initialization.
		agent = GetComponent<NavMeshAgent>();
		target = FindObjectOfType<PlayerInformation>();
		controller = target.GetComponent<CharacterController>();

		//Start the basic walking movement.
		StartCoroutine(UpdateDestination());

		//Make sure that the charging effect only plays when it has to.
		chargeParticle.Stop();
	}

	private void Update()
	{
		//Check if the enemy is charging.
		if(!isCharging)
		{
			//Handle the enemy's movement animations.
			float moveVelocity = agent.velocity.normalized.magnitude;
			animator.SetFloat("Velocity", moveVelocity);
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
			//Set the state to dead and destroy the enemy object.
			isDead = true;
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
				//Calculate the distance between the enemy and the player, and try charging.
				float distanceToTarget = (target.transform.position - transform.position).sqrMagnitude;
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
		agent.speed = 15f;
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
			if(formatedDistance <= 0.4f && !hasChargedAgain)
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
		agent.speed = 2f;

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
