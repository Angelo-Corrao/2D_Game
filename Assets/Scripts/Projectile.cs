using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Projectile : MonoBehaviour {
	public Tilemap road;
	public float totSteps = 10;
	public float animationTime = 0.5f;
	public Vector2 direction;
	public float spawnOffset;

	private float oneStepDistance;
	private float timeBetweenSteps;
	private bool isfirstMovement = true;
	private float defaultTotSteps;

	private void Awake() {
		defaultTotSteps = totSteps;
		timeBetweenSteps = animationTime / totSteps;
		oneStepDistance = 1 / totSteps;
	}

	private void Start() {
		Move();
	}

	private void Update() {
		//if the match end => Destroy(gameObject);
	}

	public void Move() {
		// Controll the validity of the projectile's path
		if (CanMove())
			StartCoroutine(MoveBySteps());
		else
			Destroy(gameObject);
	}

	private IEnumerator MoveBySteps() {
		/*
		 * Because the projectile doesn't spawn at the center of the tile to avoid collision with car,
		 * the first time we'll need less steps to reach the next tile based on the spawn offset
		 */
		if (isfirstMovement) {
			totSteps = totSteps - spawnOffset * 10;
			isfirstMovement = false;
		}
		else
			totSteps = defaultTotSteps;

		// Move the projectile by little steps instead of teleporting it
		for (int i = 0; i < totSteps; i++) {
			transform.position += (Vector3)direction * oneStepDistance;
			yield return new WaitForSeconds(timeBetweenSteps);
		}

		if (IsInCurve())
			StartCoroutine(MoveBySteps());
		else {
			Vector3Int actualGridPosition = road.WorldToCell(transform.position);
			/*
			 * Check if at the end of a curve there is a tile or not
			 * If not this avoid the game to crush but with the full map this should not happen
			 */
			if (road.GetTile(actualGridPosition) != null)
				Move();
			else
				Destroy(gameObject);
		}
	}

	private bool CanMove() {
		Vector3Int actualGridPosition = road.WorldToCell(transform.position);
		Vector3Int nextGridPosition = road.WorldToCell(transform.position + (Vector3)direction);

		switch (road.GetTile(actualGridPosition).name) {
			case "roadNEWS":
				// Here the player can shoot in any direction
				if (isCellNotEmpty(nextGridPosition))
					return true;
				break;

			case "roadEW":
				// Check if the player shot horizontally
				if (direction.y == 0) {
					if (isCellNotEmpty(nextGridPosition))
						return true;
				}
				break;

			case "roadNS":
				// Check if the player shot vertically
				if (direction.x == 0) {
					if (isCellNotEmpty(nextGridPosition))
						return true;
				}
				break;

			case "roadNEW":
				if (direction.y == 0 || direction.y == 1) {
					// The player can shoot in any direction except south
					if (isCellNotEmpty(nextGridPosition))
						return true;
				}
				break;

			case "roadEWS":
				// The player can shoot in any direction except north
				if (direction.y == 0 || direction.y == -1) {
					if (isCellNotEmpty(nextGridPosition))
						return true;
				}
				break;

			case "roadNES":
				// The player can shoot in any direction except west
				if (direction.x == 0 || direction.x == 1) {
					if (isCellNotEmpty(nextGridPosition))
						return true;
				}
				break;

			case "roadNWS":
				// The player can shoot in any direction except est
				if (direction.x == 0 || direction.x == -1) {
					if (isCellNotEmpty(nextGridPosition))
						return true;
				}
				break;
		}

		return false;
	}

	// Check if exist a tile in the road tilemap in the direction the player want to shoot
	private bool isCellNotEmpty(Vector3Int nextGridPosition) {
		// The second check is nedeed so the player can't shoot diagonally
		if (road.HasTile(nextGridPosition) && direction.magnitude <= 1)
			return true;
		else
			return false;
	}

	// Change the projectile's direction if it's in a curve, based on the direction from which it comes
	public bool IsInCurve() {
		Vector3Int actualGridPosition;
		actualGridPosition = road.WorldToCell(transform.position);

		if (road.GetTile(actualGridPosition) == null)
			return false;

		switch (road.GetTile(actualGridPosition).name) {
			case "roadNE":
				if (direction.x == -1) {
					direction = new Vector2(0, 1);
					return true;
				}
				else {
					direction = new Vector2(1, 0);
					return true;
				}

			case "roadNW":
				if (direction.x == 1) {
					direction = new Vector2(0, 1);
					return true;
				}
				else {
					direction = new Vector2(-1, 0);
					return true;
				}

			case "roadSE":
				if (direction.x == -1) {
					direction = new Vector2(0, -1);
					return true;
				}
				else {
					direction = new Vector2(1, 0);
					return true;
				}

			case "roadSW":
				if (direction.x == 1) {
					direction = new Vector2(0, -1);
					return true;
				}
				else {
					direction = new Vector2(-1, 0);
					return true;
				}
		}

		return false;
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.CompareTag("Player")) {
			Debug.Log("Dead");
		}

		if (collision.gameObject.CompareTag("Enemy")) {
			Debug.Log("Win");
		}

		Destroy(gameObject);
	}
}
