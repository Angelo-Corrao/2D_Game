using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class OnlineCarController : NetworkBehaviour, ITeleportable{
	public Tilemap road;
	public Tilemap fogOfWar;
	[Tooltip("Is applied only when the game starts")]
	public float animationTime = 0.5f;
	public int totProjectiles = 5;
	public Canvas playerTurn;
	public Text turnText;

	[HideInInspector]
	public bool isMoving = false;
	[HideInInspector]
	public string startingOrientation = "up";
	[HideInInspector]
	public int projectilesCounter;

	private PlayerInput playerInput;
	private Vector2 direction;
	private string previousDirection = "";
	private bool checkCurve = false;
	private CarAnimation carAnimation;
	private float totSteps = 10;
	private float oneStepDistance;
	private float timeBetweenSteps;
	private bool hasToTeleport = false;

	private void Awake() {
		timeBetweenSteps = animationTime / totSteps;
		oneStepDistance = 1 / totSteps;
		projectilesCounter = totProjectiles;
		carAnimation = GetComponent<CarAnimation>();

		playerInput = new PlayerInput();
		playerInput.Car.Movement.performed += ctx => {
			if (!isMoving && !OnlineGameManager.Instance.isGamePaused) {
				direction = ctx.ReadValue<Vector2>();
				CanMove();
			}
		};
	}

	private void OnEnable() {
		playerInput.Enable();
	}

	private void OnDisable() {
		playerInput.Disable();
	}

	private void Update() {
		if (IsServer) {
			if (OnlineGameManager.Instance.activePlayer.Value == 2) {
				playerTurn.gameObject.SetActive(true);
				turnText.text = "PLAYER " + OnlineGameManager.Instance.activePlayer.Value.ToString() + " TURN";
				OnlineGameManager.Instance.isGamePaused = true;
			}
			else {
				playerTurn.gameObject.SetActive(false);
				OnlineGameManager.Instance.isGamePaused = false;
			}
		}
		else {
			if (OnlineGameManager.Instance.activePlayer.Value == 1) {
				playerTurn.gameObject.SetActive(true);
				turnText.text = "PLAYER " + OnlineGameManager.Instance.activePlayer.Value.ToString() + " TURN";
				OnlineGameManager.Instance.isGamePaused = true;
			}
			else {
				playerTurn.gameObject.SetActive(false);
				OnlineGameManager.Instance.isGamePaused = false;
			}
		}

		InCurveBehaviour();
		if (hasToTeleport) {
			// Wait for the car to stop moving before teleporting it so it will be at center of the tile without misalignments
			if (!isMoving) {
				OnlineGameManager.Instance.Teleport(this);
				hasToTeleport = false;
			}
		}
	}

	private void Move(Vector3Int nextGridPosition) {
		isMoving = true;
		fogOfWar.SetTile(nextGridPosition, null);
		// If there is a wall near the cell the player moved into it will also be revealed from the fog of war
		Vector3Int upGridPosition = road.WorldToCell(nextGridPosition + Vector3.up);
		Vector3Int estGridPosition = road.WorldToCell(nextGridPosition + Vector3.right);
		Vector3Int westGridPosition = road.WorldToCell(nextGridPosition + Vector3.left);
		Vector3Int downGridPosition = road.WorldToCell(nextGridPosition + Vector3.down);
		if (road.GetTile(upGridPosition)?.name == "roadPLAZA")
			fogOfWar.SetTile(upGridPosition, null);
		if (road.GetTile(estGridPosition)?.name == "roadPLAZA")
			fogOfWar.SetTile(estGridPosition, null);
		if (road.GetTile(westGridPosition)?.name == "roadPLAZA")
			fogOfWar.SetTile(westGridPosition, null);
		if (road.GetTile(downGridPosition)?.name == "roadPLAZA")
			fogOfWar.SetTile(downGridPosition, null);

		if (direction.x == 0) {
			if (direction.y == 1) {
				previousDirection = "up";
				if (carAnimation != null)
					carAnimation.Animate(startingOrientation, previousDirection, direction);
			}
			else {
				previousDirection = "down";
				if (carAnimation != null)
					carAnimation.Animate(startingOrientation, previousDirection, direction);
			}
		}
		else if (direction.x == 1) {
			previousDirection = "right";
			if (carAnimation != null)
				carAnimation.Animate(startingOrientation, previousDirection, direction);
		}
		else {
			previousDirection = "left";
			if (carAnimation != null)
				carAnimation.Animate(startingOrientation, previousDirection, direction);
		}

		StartCoroutine(ReachNextCell());

		// If The animation script is not attached to the gameObject the car will be teleported instead of being animated
		if (carAnimation == null) {
			transform.position += (Vector3)direction;
			isMoving = false;
		}
	}

	// Move the car by little steps instead of teleporting it
	private IEnumerator ReachNextCell() {
		for (int i = 0; i < totSteps; i++) {
			transform.position += (Vector3)direction * oneStepDistance;
			yield return new WaitForSeconds(timeBetweenSteps);
		}

		startingOrientation = previousDirection;
		isMoving = false;

		Vector3Int actualGridPosition = road.WorldToCell(transform.position);
		OnlineGameManager.Instance.CheckNearbyObjects(actualGridPosition);

		// Update the grid of the positions already visited from the player
		// + 10 is nedeed because the grid in world space goes from -10 to +10 and the matrix starts from the position [0, 0]
		Vector3 cell = (Vector3)actualGridPosition;
		OnlineGameManager.Instance.visitedCells.matrix[((int)cell.x) + 10, ((int)cell.y) + 10] = true;

		OnlineGameManager.Instance.ChangeActivePlayerServerRpc();
	}

	private void CanMove() {
		Vector3Int actualGridPosition = road.WorldToCell(transform.position);
		Vector3Int nextGridPosition = road.WorldToCell(transform.position + (Vector3)direction);

		// Check if the direction the player want to move is valid based on the tile he's on
		switch (road.GetTile(actualGridPosition).name) {
			case "roadNEWS":
				// Here the player can move in any direction
				if (isCellNotEmpty(nextGridPosition))
					Move(nextGridPosition);
				break;

			case "roadEW":
				// Check if the player moved horizontally
				if (direction.y == 0) {
					if (isCellNotEmpty(nextGridPosition))
						Move(nextGridPosition);
				}
				break;

			case "roadNS":
				// Check if the player moved vertically
				if (direction.x == 0) {
					if (isCellNotEmpty(nextGridPosition))
						Move(nextGridPosition);
				}
				break;

			case "roadNEW":
				// The player can move in any direction except south
				if (direction.y == 0 || direction.y == 1) {
					if (isCellNotEmpty(nextGridPosition))
						Move(nextGridPosition);
				}
				break;

			case "roadEWS":
				// The player can move in any direction except north
				if (direction.y == 0 || direction.y == -1) {
					if (isCellNotEmpty(nextGridPosition))
						Move(nextGridPosition);
				}
				break;

			case "roadNES":
				// The player can move in any direction except west
				if (direction.x == 0 || direction.x == 1) {
					if (isCellNotEmpty(nextGridPosition))
						Move(nextGridPosition);
				}
				break;

			case "roadNWS":
				// The player can move in any direction except est
				if (direction.x == 0 || direction.x == -1) {
					if (isCellNotEmpty(nextGridPosition))
						Move(nextGridPosition);
				}
				break;
		}

		// This is nedeed so we can check if the player has landed in a curve
		checkCurve = true;
	}

	// Check if exist a tile in the road tilemap in the direction the player want to move
	private bool isCellNotEmpty(Vector3Int nextGridPosition) {
		// The second check is nedeed so the player can't move diagonally
		if (road.HasTile(nextGridPosition) && direction.magnitude <= 1)
			return true;
		else
			return false;
	}

	/*
	 * Mangage the player position if he is in a curve after he moved
	 * The player is moved in another cell based on the direction from which it comes
	 * When it will no longer be in a curve the loop will end
	 */
	public void InCurveBehaviour() {
		// The second check is nedeed to change the car's direction only when it reaches the center of the tile
		if (checkCurve && !isMoving) {
			Vector3Int actualGridPosition;
			Vector3Int nextGridPosition;
			actualGridPosition = road.WorldToCell(transform.position);
			switch (road.GetTile(actualGridPosition).name) {
				case "roadNE":
					if (previousDirection == "down") {
						direction = new Vector2(1, 0);
						nextGridPosition = road.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "right";
					}
					else {
						direction = new Vector2(0, 1);
						nextGridPosition = road.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "up";
					}
					break;

				case "roadNW":
					if (previousDirection == "down") {
						direction = new Vector2(-1, 0);
						nextGridPosition = road.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "left";
					}
					else {
						direction = new Vector2(0, 1);
						nextGridPosition = road.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "up";
					}
					break;

				case "roadSE":
					if (previousDirection == "up") {
						direction = new Vector2(1, 0);
						nextGridPosition = road.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "right";
					}
					else {
						direction = new Vector2(0, -1);
						nextGridPosition = road.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "down";
					}
					break;

				case "roadSW":
					if (previousDirection == "up") {
						direction = new Vector2(-1, 0);
						nextGridPosition = road.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "left";
					}
					else {
						direction = new Vector2(0, -1);
						nextGridPosition = road.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "down";
					}
					break;
			}

			if (road.GetTile(actualGridPosition).name != "roadNE" &&
				road.GetTile(actualGridPosition).name != "roadNW" &&
				road.GetTile(actualGridPosition).name != "roadSE" &&
				road.GetTile(actualGridPosition).name != "roadSW")
				checkCurve = false;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Well")) {
			AudioManager.Instance.PlaySFX("Dead");
			OnlineGameManager.Instance.isPlayerAlive = false;
			OnlineGameManager.Instance.GameOver();
			Destroy(gameObject);
		}

		if (collision.gameObject.CompareTag("Teleport")) {
			AudioManager.Instance.PlaySFX("Teleport");
			hasToTeleport = true;
		}
	}

	// Check if the teleport position is on another object that implements ITeleportable
	public bool isOnOtherTeleportableObjects(Vector3 newPos) {
		List<ITeleportable> teleportables = OnlineGameManager.Instance.teleportables;
		for (int i = 0; i < teleportables.Count; i++) {
			// This check if i'm not comparing the same object
			if (this != teleportables[i]) {
				MonoBehaviour teleportableObject = (MonoBehaviour)teleportables[i];
				if (newPos.x == teleportableObject.transform.position.x && newPos.y == teleportableObject.transform.position.y) {
					return true;
				}
			}
		}

		return false;
	}
}
