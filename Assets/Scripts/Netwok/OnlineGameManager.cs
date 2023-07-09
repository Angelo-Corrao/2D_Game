using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.IO;
using Unity.Netcode;

public class OnlineGameManager : NetworkBehaviour {
	public static OnlineGameManager Instance;
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
	public Canvas waitMatchStart;

	[HideInInspector]
	public List<OnlineProjectile> activeProjectiles;
	[HideInInspector]
	public bool checkActiveProjectiles;
	[HideInInspector]
	public List<ITeleportable> teleportables = new List<ITeleportable>();
	[HideInInspector]
	public NetworkVariable<NetworkObjectReference> enemy = new NetworkVariable<NetworkObjectReference>();
	[HideInInspector]
	public bool isGamePaused = false; // Used to block player's input when the game is in paused
	[HideInInspector]
	public bool isEnemyAlive = true;
	[HideInInspector]
	public bool isPlayerAlive = true;
	[HideInInspector]
	public NetworkVariable<bool> hasMatchEnded = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
		NetworkVariableWritePermission.Owner);
	[HideInInspector]
	public SerializableMatrix<bool> visitedCells = new SerializableMatrix<bool>();
	[HideInInspector]
	public NetworkVariable<int> activePlayer = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone,
		NetworkVariableWritePermission.Owner);
	[HideInInspector]
	public NetworkVariable<bool> isMatchStarted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
		NetworkVariableWritePermission.Owner);

	private OnlineCarController carController;
	private NetworkList<NetworkObjectReference> wells = new NetworkList<NetworkObjectReference>();
	private NetworkList<NetworkObjectReference> teleports = new NetworkList<NetworkObjectReference>();
	private NetworkVariable<int> winningPlayer = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
		NetworkVariableWritePermission.Owner);
	private bool isInitialObjectCheckingCompleted = false;

	private void Awake() {
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);

		for (int i = 0; i < 20; i++) {
			for (int j = 0; j < 20; j++) {
				visitedCells.matrix[i, j] = false;
			}
		}

		carController = player.GetComponent<OnlineCarController>();
		enemy.OnValueChanged += SetEnemy;
	}

	private void Start() {
		// Starts host or client
		if (MainMenuManager.Instance.isHost)
			NetworkManager.Singleton.StartHost();
		else
			NetworkManager.Singleton.StartClient();

		if (IsServer) {
			isGamePaused = true;
			waitMatchStart.gameObject.SetActive(true);

			// Spawn all the game elements
			SpawnEnemy();
			SpawnWells();
			SpawnTeleports();
		}

		// Update the fog of war for the player's start position 
		AudioManager.Instance.PlayMusic("In Game");
		Vector3Int startGridPosition = road.WorldToCell(player.transform.position);
		fogOfWar.SetTile(startGridPosition, null);
		Vector3 cell = (Vector3)startGridPosition;
		visitedCells.matrix[((int)cell.x) + 10, ((int)cell.y) + 10] = true;

		// Update the list with all teleportable objects and check nearby objects
		teleportables.Add(player.GetComponent<OnlineCarController>());
		CheckNearbyObjects(startGridPosition);

		// Update ammo UI
		totProjectilesText.text = carController.totProjectiles.ToString();
		UpdateAmmoUI(carController.projectilesCounter);
		checkActiveProjectiles = false;
	}

	private void Update() {
		// Check nedeed for latency problems
		if (carController.totProjectiles != 0) {
			totProjectilesText.text = carController.totProjectiles.ToString();
			UpdateAmmoUI(carController.projectilesCounter);
		}

		// Check nedeed for latency problems
		if (teleports.Count != 0) {
			if (!isInitialObjectCheckingCompleted) {
				Vector3Int startGridPosition = road.WorldToCell(player.transform.position);
				CheckNearbyObjects(startGridPosition);
				isInitialObjectCheckingCompleted = true;
			}
		}

		// Wait for the client to connect to start the match
		if (!isMatchStarted.Value) {
			if (IsServer) {
				if (NetworkManager.Singleton.ConnectedClients.Count == 2) {
					waitMatchStart.gameObject.SetActive(false);
					isGamePaused = false;
					isMatchStarted.Value = true;
				}
			}
		}

		if (hasMatchEnded.Value) {
			if (IsServer) {
				if (winningPlayer.Value == 1)
					victoryUI.gameObject.SetActive(true);
				else
					gameOverUI.gameObject.SetActive(true);
			}
			else if (winningPlayer.Value == 2)
				victoryUI.gameObject.SetActive(true);
			else
				gameOverUI.gameObject.SetActive(true);
		}

		// When the the last projectile is shot check until there is no projectile on the map, than is Game Over
		if (checkActiveProjectiles) {
			if (activeProjectiles.Count == 0) {
				checkActiveProjectiles = false;
				// With this is possible to win killing the enemy with the last projectile
				if (isEnemyAlive) {
					ChangeActivePlayerServerRpc();
					GameOverServerRpc();
				}
			}
		}
	}

	private void SetEnemy(NetworkObjectReference previous, NetworkObjectReference current) {
		GameObject enemy = this.enemy.Value;
		teleportables.Add(enemy.GetComponent<OnlineEnemy>());
	}

	[ServerRpc(RequireOwnership = false)]
	public void ChangeActivePlayerServerRpc() {
		if (activePlayer.Value == 1)
			activePlayer.Value = 2;
		else
			activePlayer.Value = 1;
	}

	public void MainMenu() {
		NetworkManager.Singleton.Shutdown();
		AudioManager.Instance.PlaySFX("Button");
		Cursor.lockState = CursorLockMode.None;
		SceneManager.LoadScene(0);
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

		GameObject enemyGameObject = Instantiate(enemyPrefab, new Vector3(randomSpawnX, randomSpawnY, 0), Quaternion.identity);
		enemyGameObject.GetComponent<NetworkObject>().Spawn();
		enemy.Value = enemyGameObject;
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
				GameObject enemy = this.enemy.Value;
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

			GameObject wellGameObject = Instantiate(wellPrefab, new Vector3(randomSpawnX, randomSpawnY, 0), Quaternion.identity);
			wellGameObject.GetComponent<NetworkObject>().Spawn();
			wells.Add(wellGameObject);
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
				GameObject enemy = this.enemy.Value;
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

			GameObject teleportGameObject = Instantiate(teleportPrefab, new Vector3(randomSpawnX, randomSpawnY, 0), Quaternion.identity);
			teleportGameObject.GetComponent<NetworkObject>().Spawn();
			teleports.Add(teleportGameObject);
			teleportsCounter++;
		} while (teleportsCounter < teleportsToSpawn);
	}

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
		if (teleportable.GetType() == typeof(OnlineCarController))
			fogOfWar.SetTile(newGridPosition, null);
		Vector3Int playerGridPosition = road.WorldToCell(player.transform.position);
		StartCoroutine(Wait(playerGridPosition));
	}

	[ServerRpc(RequireOwnership = false)]
	public void TeleportEnemyServerRpc() {
		float randomSpawnX;
		float randomSpawnY;
		bool invalidSpawn = false;
		Vector3Int newGridPosition;
		Vector3Int playerGridPosition;

		do {
			randomSpawnX = Random.Range(-9, 11) - 0.5f;
			randomSpawnY = Random.Range(-9, 11) - 0.5f;

			newGridPosition = road.WorldToCell(new Vector3(randomSpawnX, randomSpawnY, 0f));
			playerGridPosition = road.WorldToCell(player.transform.position);

			// Check tile validity
			invalidSpawn = IsInCurveOrWall(newGridPosition);

			// Check if it was teleported in the same position of another teleportable object
			if (!invalidSpawn) {
				if (newGridPosition == playerGridPosition)
					invalidSpawn = true;

				// Check if it was teleported in the same position of a well
				if (!invalidSpawn)
					invalidSpawn = IsOnWell(newGridPosition);

				// Check if it was teleported in the same position of a teleport
				if (!invalidSpawn)
					invalidSpawn = IsOnTeleport(newGridPosition);
			}
		} while (invalidSpawn);

		GameObject enemy = this.enemy.Value;
		enemy.transform.position = new Vector3(randomSpawnX, randomSpawnY, 0f);
		this.enemy.Value = enemy;
	}

	[ServerRpc(RequireOwnership = false)]
	public void DestroyEnemyServerRpc() {
		GameObject enemy = this.enemy.Value;
		Destroy(enemy);
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
		GameObject enemy = this.enemy.Value;
		Vector3Int enemyGridPosition = fogOfWar.WorldToCell(enemy.transform.position);
		if (fogOfWar.GetTile(enemyGridPosition) != null)
			fogOfWar.SetTile(enemyGridPosition, null);
	}

	[ServerRpc(RequireOwnership = false)]
	public void VictoryServerRpc() {
		hasMatchEnded.Value = true;

		if (activePlayer.Value == 1)
			winningPlayer.Value = 1;
		else
			winningPlayer.Value = 2;

		Cursor.lockState = CursorLockMode.None;
		isGamePaused = true;
	}

	[ServerRpc(RequireOwnership = false)]
	public void GameOverServerRpc() {
		hasMatchEnded.Value = true;

		if (activePlayer.Value == 1)
			winningPlayer.Value = 2;
		else
			winningPlayer.Value = 1;

		Cursor.lockState = CursorLockMode.None;
		isGamePaused = true;
	}
}
