using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.IO;

public class GameManager : MonoBehaviour, IDataPersistence
{
	public static GameManager Instance;
	public Tilemap road;
	public Tilemap fogOfWar;
	public GameObject player;
    public GameObject enemyPrefab;
	public GameObject wellPrefab;
	public GameObject teleportPrefab;
	public int wellsToSpawn = 6;
	public int teleportsToSpawn = 3;
	public Sprite enemySprite;
	public Sprite wellSprite;
	public Sprite teleportSprite;
	public Image[] uiSlots;
	public Text projectilesCounterText;
	public Text totProjectilesText;
	public Canvas pauseMenu;
	public Canvas victoryUI;
	public Canvas gameOverUI;
	public AudioMixerSnapshot pausedSnapshot;
	public AudioMixerSnapshot unpausedSnapshot;

	[HideInInspector]
	public List<Projectile> activeProjectiles;
	[HideInInspector]
	public bool checkActiveProjectiles;
	[HideInInspector]
	public List<ITeleportable> teleportables = new List<ITeleportable>();
	[HideInInspector]
	public GameObject enemy;
	[HideInInspector]
	public bool canPause = true;
	[HideInInspector]
	public bool isGamePaused = false; // Used to block player's input when the game is in paused
	[HideInInspector]
	public bool isEnemyAlive = true;
	[HideInInspector]
	public bool isPlayerAlive = true;
	[HideInInspector]
	public bool hasMatchEnded = false;
	[HideInInspector]
	public SerializableMatrix<bool> visitedCells = new SerializableMatrix<bool>();

	private PlayerInput playerInput;
	private List<GameObject> wells = new List<GameObject>();
	private List<GameObject> teleports = new List<GameObject>();

	private void Awake() {
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);

		playerInput = new PlayerInput();
		playerInput.Car.Pause.performed += _ => {
			if (canPause) {
				if (!isGamePaused)
					Pause();
				else
					Unpause();
			}
		};

		for (int i = 0; i < 20; i++) {
			for (int j = 0; j < 20; j++) {
				visitedCells.matrix[i, j] = false;
			}
		}
	}

	private void OnEnable() {
		playerInput.Enable();
	}

	private void OnDisable() {
		playerInput.Disable();
	}

	private void Start() {
		// Load Saves. The enemy's, wells's and teleports's spawn are managed in the Load method of this class
		DataPersistenceManager.Instance.LoadGame();
		DataPersistenceManager.Instance.SaveGame();

		// Update the fog of war for the player's start position 
		AudioManager.Instance.PlayMusic("In Game");
		Vector3Int startGridPosition = road.WorldToCell(player.transform.position);
		fogOfWar.SetTile(startGridPosition, null);
		Vector3 cell = (Vector3)startGridPosition;
		visitedCells.matrix[((int)cell.x) + 10, ((int)cell.y) + 10] = true;

		// Update the list with all teleportable objects and check nearby objects
		teleportables.Add(player.GetComponent<CarController>());
		teleportables.Add(enemy.GetComponent<Enemy>());
		CheckNearbyObjects(startGridPosition);

		// Update ammo UI
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
				// With this is possible to win killing the enemy with the last projectile
				if (isEnemyAlive)
					GameOver();
			}
		}
	}

	public void Continue() {
		AudioManager.Instance.PlaySFX("Button");
		Unpause();
	}

	public void MainMenu() {
		/*
		 * If the player comes back to main menu after the game end (Victory or Game Over) delete the save file
		 * so he can't click the continue button
		 */
		if (!hasMatchEnded)
			DataPersistenceManager.Instance.SaveGame();
		else
			FileDataHandler.Instance.Delete(Path.Combine(Application.persistentDataPath, DataPersistenceManager.Instance.fileName));

		AudioManager.Instance.PlaySFX("Button");
		Unpause();
		Cursor.lockState = CursorLockMode.None;
		SceneManager.LoadScene(0);
	}

	public void Pause() {
		isGamePaused = true;
		pauseMenu.gameObject.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
		pausedSnapshot.TransitionTo(0);
		Time.timeScale = 0;
	}

	public void Unpause() {
		isGamePaused = false;
		pauseMenu.gameObject.SetActive(false);
		Cursor.lockState = CursorLockMode.Locked;
		unpausedSnapshot.TransitionTo(0f);
		Time.timeScale = 1;
	}

	public void PlayAgain() {
		DataPersistenceManager.Instance.isNewGame = true;
		isGamePaused = false;
		Time.timeScale = 1;
		switch (MainMenuManager.Instance.gameMode) {
			case GameMode.STANDARD:
				SceneManager.LoadScene(1);
				break;

			case GameMode.PROCEDURAL:
				SceneManager.LoadScene(2);
				break;
		}
	}

	public void SetCanPause(bool value) {
		canPause = value;
	}

	private void SpawnEnemy(bool isNewGame) {
		float randomSpawnX;
		float randomSpawnY;
		bool invalidSpawn = false;

		if (isNewGame) {
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
		else {
			enemy = Instantiate(enemyPrefab, DataPersistenceManager.Instance.gameData.enemyPosition, Quaternion.identity);
		}
	}

	private void SpawnWells(bool isNewGame) {
		float randomSpawnX;
		float randomSpawnY;
		bool invalidSpawn = false;
		int wellsCounter = 0;

		if (isNewGame) {
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
		else {
			foreach (Vector3 pos in DataPersistenceManager.Instance.gameData.wellsPosition) {
				wells.Add(Instantiate(wellPrefab, pos, Quaternion.identity));
			}
		}
	}

	private void SpawnTeleports(bool isNewGame) {
		float randomSpawnX;
		float randomSpawnY;
		bool invalidSpawn = false;
		int teleportsCounter = 0;

		if (isNewGame) {
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
		else {
			foreach (Vector3 pos in DataPersistenceManager.Instance.gameData.teleportsPosition) {
				teleports.Add(Instantiate(teleportPrefab, pos, Quaternion.identity));
			}
		}
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

	/*
	 * Check nearby objects in all 8 directions, if the check is done only in the 4 cardinal direction the player would die if
	 * there was an enemy or well after a curve without being notified by UI before entering the curve.
	 */
	public void CheckNearbyObjects(Vector3Int playerGridPosition) {
		bool isEnemyNearby = false;
		bool isWellNearby = false;
		bool isTeleportNearby = false;

		Vector3 tileCenter = (Vector3)playerGridPosition + new Vector3(0.5f, 0.5f, 0);
		// North
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

		// North / Est
		hit = Physics2D.Raycast(tileCenter, new Vector2(1, 1), 1.4f);
		SaveNearbyObject(hit, ref isEnemyNearby, ref isWellNearby, ref isTeleportNearby);

		// North / West
		hit = Physics2D.Raycast(tileCenter, new Vector2(-1, 1), 1.4f);
		SaveNearbyObject(hit, ref isEnemyNearby, ref isWellNearby, ref isTeleportNearby);

		// South / Est
		hit = Physics2D.Raycast(tileCenter, new Vector2(1, -1), 1.4f);
		SaveNearbyObject(hit, ref isEnemyNearby, ref isWellNearby, ref isTeleportNearby);

		// South / West
		hit = Physics2D.Raycast(tileCenter, new Vector2(-1, -1), 1.4f);
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

	public void ShowEnemy() {
		Vector3Int enemyGridPosition = fogOfWar.WorldToCell(enemy.transform.position);
		if (fogOfWar.GetTile(enemyGridPosition) != null)
			fogOfWar.SetTile(enemyGridPosition, null);
	}

	public void Victory() {
		canPause = false;
		hasMatchEnded = true;
		victoryUI.gameObject.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
		isGamePaused = true;
		Time.timeScale = 0;
	}

	public void GameOver() {
		canPause = false;
		hasMatchEnded = true;
		gameOverUI.gameObject.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
		isGamePaused = true;
		Time.timeScale = 0;
	}

	// Remove the fog of war from the cells that the player has already visited based on the save file
	private void RecreateMap() {
		for (int i = 0; i < 20; i++) {
			for (int j = 0; j < 20; j++) {
				if (visitedCells.matrix[i, j]) {
					// - 10 is nedeed because the grid in world space goes from -10 to +10 and the matrix starts from the position [0, 0]
					Vector3Int gridPosition = fogOfWar.WorldToCell(new Vector3(i - 10, j - 10, 0));
					fogOfWar.SetTile(gridPosition, null);
				}
			}
		}
	}

	public void LoadData(GameData gameData, bool isNewGame) {
		if (isNewGame) {
			gameData.visitedCells = visitedCells;
			SpawnEnemy(true);
			SpawnWells(true);
			SpawnTeleports(true);
		}
		else {
			visitedCells = gameData.visitedCells;
			SpawnEnemy(false);
			SpawnWells(false);
			SpawnTeleports(false);
		}

		RecreateMap();
	}

	public void SaveData(ref GameData gameData) {
		gameData.visitedCells = visitedCells;
		gameData.enemyPosition = enemy.transform.position;
		gameData.wellsPosition.Clear();
		foreach (GameObject well in wells) {
			gameData.wellsPosition.Add(well.transform.position);
		}
		gameData.teleportsPosition.Clear();
		foreach (GameObject teleport in teleports) {
			gameData.teleportsPosition.Add(teleport.transform.position);
		}
	}
}
