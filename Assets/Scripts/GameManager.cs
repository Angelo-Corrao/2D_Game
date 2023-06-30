using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;
	public Tilemap road;
	public Tilemap fogOfWar;
	public GameObject player;
    public GameObject enemyPrefab;
	public GameObject wellPrefab;
	public GameObject teleportPrefab;
	public int wellsToSpawn = 3;
	public int teleportsToSpawn = 2;

	private GameObject enemy;
	private List<GameObject> wells = new List<GameObject>();
	private List<GameObject> teleports = new List<GameObject>();

	private void Awake() {
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	void Start()
    {
		Vector3Int startGridPosition = road.WorldToCell(player.transform.position);
		fogOfWar.SetTile(startGridPosition, null);
		SpawnEnemy();
		SpawnWells();
		SpawnTeleports();
	}

    private void SpawnEnemy() {
		float randomSpawnX;
		float randomSpawnY;
		bool invalidSpawn = false;

		do {
			randomSpawnX = Random.Range(-9, 11) - 0.5f;
			randomSpawnY = Random.Range(-9, 11) - 0.5f;

			Vector3Int enemySpawnGridPosition = road.WorldToCell(new Vector3(randomSpawnX, randomSpawnY, 0));
			Vector3Int playerGridPosition = road.WorldToCell(player.transform.position);
			// Check tile validity
			invalidSpawn = IsInCurveOrWall(enemySpawnGridPosition);
			// Check if it was spawned in the same position of the player
			if (!invalidSpawn) {
				if (enemySpawnGridPosition == playerGridPosition)
					invalidSpawn = true;
				else
					invalidSpawn = false;
			}
		} while (invalidSpawn);

		enemy = Instantiate(enemyPrefab, new Vector3(randomSpawnX, randomSpawnY, 0), Quaternion.identity);
	}

	private void SpawnWells() {
		float randomSpawnX;
		float randomSpawnY;
		bool invalidSpawn = false;
		int wellsCounter = 0;

		do {
			do {
				randomSpawnX = Random.Range(-9, 11) - 0.5f;
				randomSpawnY = Random.Range(-9, 11) - 0.5f;

				Vector3Int wellSpawnGridPosition = road.WorldToCell(new Vector3(randomSpawnX, randomSpawnY, 0));
				Vector3Int enemyGridPosition = road.WorldToCell(enemy.transform.position);
				Vector3Int playerGridPosition = road.WorldToCell(player.transform.position);

				// Check tile validity
				invalidSpawn = IsInCurveOrWall(wellSpawnGridPosition);

				// Check if it was spawned in the same position of the enemy or the player
				if (!invalidSpawn) {
					if (wellSpawnGridPosition == enemyGridPosition || wellSpawnGridPosition == playerGridPosition)
						invalidSpawn = true;

					// Check if it was spawned in the same position of another well
					else
						invalidSpawn = IsOnWell(wellSpawnGridPosition);
				}
			} while (invalidSpawn);

			wells.Add(Instantiate(wellPrefab, new Vector3(randomSpawnX, randomSpawnY, 0), Quaternion.identity));
			wellsCounter++;
		} while (wellsCounter < wellsToSpawn);
	}

	private void SpawnTeleports() {
		float randomSpawnX;
		float randomSpawnY;
		bool invalidSpawn = false;
		int teleportsCounter = 0;

		do {
			do {
				randomSpawnX = Random.Range(-9, 11) - 0.5f;
				randomSpawnY = Random.Range(-9, 11) - 0.5f;

				Vector3Int teleportSpawnGridPosition = road.WorldToCell(new Vector3(randomSpawnX, randomSpawnY, 0));
				Vector3Int enemyGridPosition = road.WorldToCell(enemy.transform.position);
				Vector3Int playerGridPosition = road.WorldToCell(player.transform.position);

				// Check tile validity
				invalidSpawn = IsInCurveOrWall(teleportSpawnGridPosition);

				// Check if it was spawned in the same position of the enemy or the player
				if (!invalidSpawn) {
					if (teleportSpawnGridPosition == enemyGridPosition || teleportSpawnGridPosition == playerGridPosition)
						invalidSpawn = true;

					// Check if it was spawned in the same position of a well
					else
						invalidSpawn = IsOnWell(teleportSpawnGridPosition);
				}

				// Check if it was spawned in the same position of another teleport
				if (!invalidSpawn)
					invalidSpawn = IsOnTeleport(teleportSpawnGridPosition);
			} while (invalidSpawn);

			teleports.Add(Instantiate(teleportPrefab, new Vector3(randomSpawnX, randomSpawnY, 0), Quaternion.identity));
			teleportsCounter++;
		} while (teleportsCounter < teleportsToSpawn);
	}

	public void TeleportPlayer() {
		float randomSpawnX;
		float randomSpawnY;
		bool invalidSpawn = false;
		Vector3Int newPlayerGridPosition;

		do {
			randomSpawnX = Random.Range(-9, 11) - 0.5f;
			randomSpawnY = Random.Range(-9, 11) - 0.5f;

			newPlayerGridPosition = road.WorldToCell(new Vector3(randomSpawnX, randomSpawnY, 0));
			Vector3Int enemyGridPosition = road.WorldToCell(enemy.transform.position);

			// Check tile validity
			invalidSpawn = IsInCurveOrWall(newPlayerGridPosition);

			// Check if it was teleported in the same position of the enemy
			if (!invalidSpawn) {
				if (newPlayerGridPosition == enemyGridPosition)
					invalidSpawn = true;

				// Check if it was teleported in the same position of a well
				else {
					invalidSpawn = IsOnWell(newPlayerGridPosition);
				}
			}

			// Check if it was teleported in the same position of a teleport
			if (!invalidSpawn)
				invalidSpawn = IsOnTeleport(newPlayerGridPosition);
		} while (invalidSpawn);

		player.transform.position = new Vector3(randomSpawnX, randomSpawnY, 0);
		fogOfWar.SetTile(newPlayerGridPosition, null);
	}

	private bool IsInCurveOrWall(Vector3Int spawnGridPosition) {
		if (road.GetTile(spawnGridPosition).name == "roadNE" ||
			road.GetTile(spawnGridPosition).name == "roadNW" ||
			road.GetTile(spawnGridPosition).name == "roadSE" ||
			road.GetTile(spawnGridPosition).name == "roadSW" ||
			road.GetTile(spawnGridPosition).name == "roadPLAZA") {
			return true;
		}
		else
			return false;
	}

	private bool IsOnWell(Vector3 spawnGridPosition) {
		foreach (GameObject well in wells) {
			Vector3Int wellGridPosition = road.WorldToCell(well.transform.position);
			if (spawnGridPosition == wellGridPosition) {
				return true;
			}
		}

		return false;
	}

	private bool IsOnTeleport(Vector3 spawnGridPosition) {
		foreach (GameObject teleport in teleports) {
			Vector3Int teleportGridPosition = road.WorldToCell(teleport.transform.position);
			if (spawnGridPosition == teleportGridPosition) {
				return true;
			}
		}

		return false;
	}
}
