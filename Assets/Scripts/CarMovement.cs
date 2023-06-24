using UnityEngine;
using UnityEngine.Tilemaps;

public class CarMovement : MonoBehaviour
{
    public Tilemap ground;
    public Tilemap fogOfWar;

    private PlayerInput playerInput;

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

	private void Move(Vector2 direction) {
		transform.position += (Vector3)direction;
	}

	private void CanMove(Vector2 direction) {
		Vector3Int gridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
		if (ground.HasTile(gridPosition) && direction.magnitude <= 1) {
			Move(direction);
			fogOfWar.SetTile(gridPosition, null);
		}
	}
}
