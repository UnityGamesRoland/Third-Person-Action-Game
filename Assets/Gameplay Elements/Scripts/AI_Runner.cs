using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AI_Runner : MonoBehaviour
{
	public LootAsset lootTable;
	public Animator animator;
	public GameObject dieEffect;
	public Renderer body;
	public int health = 2;
	public float attackDistance = 1.8f;
	public float attackSpeed = 1f;
	public bool prewarmDissolving;
	public bool isDead;

	private float attackTimer;
	private float dissolveAmount = 0f;

	private PlayerInformation target;
	private CharacterController controller;
	private NavMeshAgent agent;

	private void Start()
	{
		//Initialization.
		agent = GetComponent<NavMeshAgent>();
		target = PlayerInformation.Instance;
		controller = target.GetComponent<CharacterController>();

		//Set the enemy's destination.
		StartCoroutine(UpdateDestination());

		//Set the default dissolve amount.
		body.material.SetFloat("_DissolveAmount", prewarmDissolving ? 0 : 1);
		dissolveAmount = prewarmDissolving ? 0 : 1;
	}

	private void Update()
	{
		//Check if the enemy can attack.
		if(Time.time > attackTimer)
		{
			//Calculate the distance and check if the enemy is in range to attack.
			float distanceToTarget = (target.transform.position - transform.position).sqrMagnitude;
			if(distanceToTarget < attackDistance)
			{
				//Attack the player.
				StartCoroutine(PunchTarget());
			}
		}

		//Handle the enemy's movement animations.
		float moveVelocity = agent.velocity.normalized.magnitude;
		animator.SetFloat("Velocity", moveVelocity);

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

			//Get a random item from the loot table and spawn it.
			GameObject loot = lootTable.GetRandomLoot();
			if(loot != null) Instantiate(loot, transform.position, loot.transform.rotation);

			//Spawn the destroy effect at the enemy's position.
			GameObject destroyEffect = Instantiate(dieEffect, transform.position, transform.rotation);
			Destroy(destroyEffect, 5);

			//Destroy the enemy.
			StopAllCoroutines();
			Destroy(gameObject);
		}
	}

	private IEnumerator PunchTarget()
	{
		//Play the attacking animation and update the attack timer.
		animator.Play("punch", 1);
		attackTimer = Time.time + attackSpeed;

		//Sync the damage to the animation.
		yield return new WaitForSeconds(0.37f);

		//Calculate the distance and check if the attack is still in hit range.
		float distanceToTarget = (target.transform.position - transform.position).sqrMagnitude;
		if(distanceToTarget < attackDistance)
		{
			//Apply the damage on the player
			target.TakeHit(1);
		}
	}

	private IEnumerator UpdateDestination()
	{
		//Check if the enemy has a target.
		while(target != null && !isDead)
		{
			//Calculate and visualize the target position.
			Vector3 targetPosition = target.transform.position + controller.velocity * 0.2f + ((target.transform.position - transform.position).normalized * 0.25f) * controller.velocity.normalized.magnitude;
			Debug.DrawLine(transform.position, targetPosition, Color.red, 0.15f);

			//Calculate a path to the player's next frame position.
			NavMeshPath path = new NavMeshPath();
			NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);

			//Update the destination and delay the next update.
			if(path.status == NavMeshPathStatus.PathComplete) agent.SetPath(path);
			else agent.SetDestination(target.transform.position);
			yield return new WaitForSeconds(0.15f);
		}
	}
}
