using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	public GameObject enemy;
	public float spawnDelay = 3f;
	public float spawnSpeed = 1f;
	public int spawnAmount = 7;

	public Transform[] spawnPoints;
	private int spawnedAmount;

	private void Start()
	{
		//Start spawning enemies.
		InvokeRepeating("SpawnEnemy", spawnDelay, spawnSpeed);
	}

	private void SpawnEnemy()
	{
		//Get a random point from the spawn points array and spawn the enemy.
		Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
		Instantiate(enemy, randomPoint.position, randomPoint.rotation);

		//Update the spawn counter.
		spawnedAmount ++;

		//Check if the counter reached the max value.
		if(spawnedAmount == spawnAmount)
		{
			//Stop spawning enemies.
			CancelInvoke();
		}
	}
}
