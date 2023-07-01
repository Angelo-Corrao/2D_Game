using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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
	public Sprite enemySprite;
	public Sprite wellSprite;
	public Sprite teleportSprite;
	public Image[] uiSlots;
	public Text projectilesCounterText;
	public Text totProjectilesText;
	public List<Projectile> activeProjectiles;
	public bool checkActiveProjectiles;
	public List<ITeleportable> teleportables = new List<ITeleportable>();
	public GameObject enemy;

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
		teleportables.Add(player.GetComponent<CarController>());
		teleportables.Add(enemy.GetComponent<Enemy>());
		CheckNearbyObjects(startGridPosition);
		CarController carController = player.GetComponent<CarController>();
		totProjectilesText.text = carController.totProjectiles.ToString();
		UpdateAmmoUI(carController.projectilesCounter);
		checkActiveProjectiles = false;
	}

	private void Update() {
		// When the the last projectile is shot check until there is no projectile on the map, than is Game Over
		if (checkActiveProjectiles) {
			if (activeProjectiles.Count == 0) {
				checkActiveProjectiles = false;
				Debug.Log("End Match");
				// End match
			}
		}
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

	/*
	 * In this method i pass an abstraction, the interface "ITeleportable". This allows me to use only this method for
	 * every object that can be teleported, implementing the defferents behaviors in each one of them separately.
	 * In this way if i want to add a new teleportable objcet all i would have to do would be implementing the ITeleportable interface
	 * in this objcet. In this specific game if i want the projectile to be able to teleport when it hits a teleport on the map
	 * all that i need to do is implementing ITeleportable in the Projectile.cs and call this method,
	 * respecting so the Open/Close principle.
	 */
	public void Teleport(ITeleportable teleportable) {
		float randomSpawnX;
		float randomSpawnY;
		bool invalidSpawn = false;
		Vector3Int newGridPosition;

		do {
			randomSpawnX = Random.Range(-9, 11) - 0.5f;
			randomSpawnY = Random.Range(-9, 11) - 0.5f;

			newGridPosition = road.WorldToCell(new Vector3(randomSpawnX, randomSpawnY, 0f));

			// Check tile validity
			invalidSpawn = IsInCurveOrWall(newGridPosition);

			// Check if it was teleported in the same position of another teleportable object
			if (!invalidSpawn) {
				invalidSpawn = teleportable.isOnOtherTeleportableObjects(new Vector3(randomSpawnX, randomSpawnY, 0f));

				// Check if it was teleported in the same position of a well
				if (!invalidSpawn)
					invalidSpawn = IsOnWell(newGridPosition);

				// Check if it was teleported in the same position of a teleport
				if (!invalidSpawn)
					invalidSpawn = IsOnTeleport(newGridPosition);
			}
		} while (invalidSpawn);

		MonoBehaviour teleportableObject = (MonoBehaviour)teleportable;
		teleportableObject.transform.position = new Vector3(randomSpawnX, randomSpawnY, 0f);
		if (teleportable.GetType() == typeof(CarController))
			fogOfWar.SetTile(newGridPosition, null);
		Vector3Int playerGridPosition = road.WorldToCell(player.transform.position);
		StartCoroutine(Wait(playerGridPosition));
	}

	// Wait before checking nearby objects after teleport
	private IEnumerator Wait(Vector3Int playerGridPosition) {
		yield return new WaitForSeconds(0.2f);
		CheckNearbyObjects(playerGridPosition);
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

	public bool IsOnWell(Vector3 spawnGridPosition) {
		foreach (GameObject well in wells) {
			Vector3Int wellGridPosition = road.WorldToCell(well.transform.position);
			if (spawnGridPosition == wellGridPosition) {
				return true;
			}
		}

		return false;
	}

	public bool IsOnTeleport(Vector3 spawnGridPosition) {
		foreach (GameObject teleport in teleports) {
			Vector3Int teleportGridPosition = road.WorldToCell(teleport.transform.position);
			if (spawnGridPosition == teleportGridPosition) {
				return true;
			}
		}

		return false;
	}

	public void CheckNearbyObjects(Vector3Int playerGridPosition) {
		bool isEnemyNearby = false;
		bool isWellNearby = false;
		bool isTeleportNearby = false;

		Vector3 tileCenter = (Vector3)playerGridPosition + new Vector3(0.5f, 0.5f, 0);
		// Up
		RaycastHit2D hit;
		hit = Physics2D.Raycast(tileCenter, Vector2.up, 1);
		SaveNearbyObject(hit, ref isEnemyNearby, ref isWellNearby, ref isTeleportNearby);

		// Est
		hit = Physics2D.Raycast(tileCenter, Vector2.right, 1);
		SaveNearbyObject(hit, ref isEnemyNearby, ref isWellNearby, ref isTeleportNearby);

		// West
		hit = Physics2D.Raycast(tileCenter, Vector2.left, 1);
		SaveNearbyObject(hit, ref isEnemyNearby, ref isWellNearby, ref isTeleportNearby);

		// South
		hit = Physics2D.Raycast(tileCenter, Vector2.down, 1);
		SaveNearbyObject(hit, ref isEnemyNearby, ref isWellNearby, ref isTeleportNearby);

		UpdateUI(isEnemyNearby, isWellNearby, isTeleportNearby);
	}

	private void SaveNearbyObject(RaycastHit2D hit, ref bool isEnemyNearby, ref bool isWellNearby, ref bool isTeleportNearby) {
		if (hit.collider != null) {
			if (hit.collider.gameObject.CompareTag("Enemy"))
				isEnemyNearby = true;

			if (hit.collider.gameObject.CompareTag("Well"))
				isWellNearby = true;

			if (hit.collider.gameObject.CompareTag("Teleport"))
				isTeleportNearby = true;
		}
	}

	// Update the UI with nearby objects in this order: enemy, well, teleport
	private void UpdateUI(bool isEnemyNearby, bool isWellNearby, bool isTeleportNearby) {
		if (isEnemyNearby) {
			uiSlots[0].gameObject.SetActive(true);
			uiSlots[0].sprite = enemySprite;

			if (isWellNearby) {
				uiSlots[1].gameObject.SetActive(true);
				uiSlots[1].sprite = wellSprite;

				if (isTeleportNearby) {
					uiSlots[2].gameObject.SetActive(true);
					uiSlots[2].sprite = teleportSprite;
				}
				else
					uiSlots[2].gameObject.SetActive(false);
			}
			else if (isTeleportNearby) {
				uiSlots[1].gameObject.SetActive(true);
				uiSlots[1].sprite = teleportSprite;
			}
			else
				uiSlots[1].gameObject.SetActive(false);
		}
		else {
			uiSlots[2].gameObject.SetActive(false);

			if (isWellNearby) {
				uiSlots[0].gameObject.SetActive(true);
				uiSlots[0].sprite = wellSprite;

				if (isTeleportNearby) {
					uiSlots[1].gameObject.SetActive(true);
					uiSlots[1].sprite = teleportSprite;
				}
				else
					uiSlots[1].gameObject.SetActive(false);
			}
			else {
				uiSlots[1].gameObject.SetActive(false);

				if (isTeleportNearby) {
					uiSlots[0].gameObject.SetActive(true);
					uiSlots[0].sprite = teleportSprite;
				}
				else
					uiSlots[0].gameObject.SetActive(false);
			}
		}
	}

	public void UpdateAmmoUI(int projectilesCounter) {
		projectilesCounterText.text = projectilesCounter.ToString();
	}
}
