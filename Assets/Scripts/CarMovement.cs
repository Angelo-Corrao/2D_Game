using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CarMovement : MonoBehaviour {
	public Tilemap road;
	public Tilemap fogOfWar;
	[Tooltip ("Is applied only when the game starts")]
	public float animationTime = 0.5f;
	public string startingOrientation = "up";
	public bool isMoving = false;

	private PlayerInput playerInput;
	private Vector2 direction;
	private string previousDirection = "";
	private bool checkCurve = false;
	private CarAnimation carAnimation;
	private float totSteps = 10;
	private float oneStepDistance;
	private float timeBetweenSteps;

	private void Awake() {
		timeBetweenSteps = animationTime / totSteps;
		oneStepDistance = 1 / totSteps;
		carAnimation = GetComponent<CarAnimation>();

		playerInput = new PlayerInput();
		playerInput.Car.Movement.performed += ctx => {
			if (!isMoving) {
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
		InCurveBehaviour();
	}

	private void Move(Vector3Int nextGridPosition) {
		isMoving = true;
		fogOfWar.SetTile(nextGridPosition, null);

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
}
