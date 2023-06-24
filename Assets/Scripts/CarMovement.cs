using UnityEngine;
using UnityEngine.Tilemaps;

public class CarMovement : MonoBehaviour
{
    public Tilemap ground;
    public Tilemap fogOfWar;

    private PlayerInput playerInput;
	private string previousDirection = "";

	private void Awake() {
		playerInput = new PlayerInput();
		playerInput.Car.Movement.performed += ctx => CanMove(ctx.ReadValue<Vector2>());
	}

	private void OnEnable() {
		playerInput.Enable();
	}

	private void OnDisable() {
		playerInput.Disable();
	}

	private void Move(Vector2 direction, Vector3Int nextGridPosition) {
		transform.position += (Vector3)direction;
		fogOfWar.SetTile(nextGridPosition, null);

		if (direction.x == 0) {
			if (direction.y == 1)
				previousDirection = "up";
			else
				previousDirection = "down";
		}
		else if (direction.x == 1)
			previousDirection = "right";
		else
			previousDirection = "left";
	}

	private void CanMove(Vector2 direction) {
		Vector3Int actualGridPosition = ground.WorldToCell(transform.position);
		Vector3Int nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);

		// Check if the direction the player want to move is valid based on the tile he's on
		switch (ground.GetTile(actualGridPosition).name) {
			case "roadNEWS":
				// Here the player can move in any direction
				if (isCellNotEmpty(nextGridPosition, direction))
					Move(direction, nextGridPosition);
				break;

			case "roadEW":
				// Check if the player moved horizontally
				if (direction.y == 0) {
					if (isCellNotEmpty(nextGridPosition, direction))
						Move(direction, nextGridPosition);
				}
				break;

			case "roadNS":
				// Check if the player moved vertically
				if (direction.x == 0) {
					if (isCellNotEmpty(nextGridPosition, direction))
						Move(direction, nextGridPosition);
				}
				break;
		}

		/*
		 * Mangage the player position if he is in a curve (tunnel) after he moved
		 * The player is moved in another cell based on the direction from which it comes
		 * When it will no longer be in a curve the loop will end
		 */
		do {
			actualGridPosition = ground.WorldToCell(transform.position);
			switch (ground.GetTile(actualGridPosition).name) {
				case "roadNE":
					if (previousDirection == "down") {
						direction = new Vector2(1, 0);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(direction, nextGridPosition);
						previousDirection = "right";
					}
					else {
						direction = new Vector2(0, 1);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(direction, nextGridPosition);
						previousDirection = "up";
					}
					break;

				case "roadNW":
					if (previousDirection == "down") {
						direction = new Vector2(-1, 0);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(direction, nextGridPosition);
						previousDirection = "left";
					}
					else {
						direction = new Vector2(0, 1);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(direction, nextGridPosition);
						previousDirection = "up";
					}
					break;

				case "roadSE":
					if (previousDirection == "up") {
						direction = new Vector2(1, 0);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(direction, nextGridPosition);
						previousDirection = "right";
					}
					else {
						direction = new Vector2(0, -1);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(direction, nextGridPosition);
						previousDirection = "down";
					}
					break;

				case "roadSW":
					if (previousDirection == "up") {
						direction = new Vector2(-1, 0);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(direction, nextGridPosition);
						previousDirection = "left";
					}
					else {
						direction = new Vector2(0, -1);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(direction, nextGridPosition);
						previousDirection = "down";
					}
					break;
			}

			actualGridPosition = ground.WorldToCell(transform.position);
		} while (ground.GetTile(actualGridPosition).name == "roadNE" ||
				 ground.GetTile(actualGridPosition).name == "roadNW" ||
				 ground.GetTile(actualGridPosition).name == "roadSE" ||
				 ground.GetTile(actualGridPosition).name == "roadSW");
	}

	private bool isCellNotEmpty(Vector3Int nextGridPosition, Vector2 direction) {
		// Check if exist a tile in the ground tilemap in the direction the player want to move
		if (ground.HasTile(nextGridPosition) && direction.magnitude <= 1) // The second check is nedeed so the player can't move diagonally
			return true;
		else
			return false;
	}
}
