using UnityEngine;

[System.Serializable]
public class Loot
{
	public string itemName = "New Item";
	[Range(0, 1)] public float spawnPercent = 0.5f;
	public GameObject itemPrefab;
}

[CreateAssetMenu(fileName = "New Loot Table", menuName = "Loot Table Asset")]
public class LootAsset : ScriptableObject
{
	//This is a modifiable and instancable unity asset, which holds a bunch of data that define a loot table.

	public Loot[] availableItems;

	public GameObject GetRandomLoot()
	{
		//Store the final object which will be given out as the item.
		GameObject selectedItem = null;

		//Get a random value and store the current potential loot item.
		float randomValue = Random.value;
		float currentLowestChance = 1;

		//Loop through every item to get the selected item.
		foreach(Loot loot in availableItems)
		{
			//Check if the random value is lower the the spawn percent.
			if(randomValue <= loot.spawnPercent)
			{
				//Check if the random value is lower than the current lowest percent.
				if(randomValue <= currentLowestChance)
				{
					//Assign the new lowest chance item.
					currentLowestChance = loot.spawnPercent;
					selectedItem = loot.itemPrefab;
				}
			}
		}

		//Give out the selected item after the loop.
		return selectedItem;
	}
}