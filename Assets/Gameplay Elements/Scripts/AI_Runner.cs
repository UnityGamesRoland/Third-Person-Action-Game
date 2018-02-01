﻿using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AI_Runner : MonoBehaviour
{
	public Animator animator;
	public int health = 3;
	public float attackDistance = 4.5f;
	public float attackSpeed = 1f;
	public bool isDead;

	private float attackTimer;

	private PlayerInformation target;
	private CharacterController controller;
	private NavMeshAgent agent;

	private void Start()
	{
		//Initialization.
		agent = GetComponent<NavMeshAgent>();
		target = FindObjectOfType<PlayerInformation>();
		controller = target.GetComponent<CharacterController>();

		//Set the enemy's destination.
		StartCoroutine(UpdateDestination());
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

		//Apply the damage on the player.
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
