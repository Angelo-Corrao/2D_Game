using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Projectile : MonoBehaviour
{
	public Tilemap ground;
	public float totSteps = 10;
	public float animationTime = 0.5f;
	public Vector2 direction;

	private float oneStepDistance;
	private float timeBetweenSteps;

	private void Awake() {
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
		for (int i = 0; i < totSteps; i++) {
			transform.position += (Vector3)direction * oneStepDistance;
			yield return new WaitForSeconds(timeBetweenSteps);
		}

		if (IsStillInCurve())
			StartCoroutine(MoveBySteps());
		else {
			Vector3Int actualGridPosition = ground.WorldToCell(transform.position);
			/*
			 * Check if at the end of a curve there is a tile or not
			 * If not this avoid the game to crush but with the full map this should not happen
			 */
			if (ground.GetTile(actualGridPosition) != null)
				Move();
			else
				Destroy(gameObject);
		}
	}

	private bool CanMove() {
		Vector3Int actualGridPosition = ground.WorldToCell(transform.position);
		Vector3Int nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);

		// This make impossible to shoot in a curve because curves are not a case in this switch
		switch (ground.GetTile(actualGridPosition).name) {
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
		}

		return false;
	}

	// Check if exist a tile in the ground tilemap in the direction the player want to shoot
	private bool isCellNotEmpty(Vector3Int nextGridPosition) {
		// The second check is nedeed so the player can't shoot diagonally
		if (ground.HasTile(nextGridPosition) && direction.magnitude <= 1)
			return true;
		else
			return false;
	}

	// Change the projectile's direction after it comes out of a curve, if it's still in a curve, based on the direction from which it comes
	public bool IsStillInCurve() {
		Vector3Int actualGridPosition;
		actualGridPosition = ground.WorldToCell(transform.position);

		if (ground.GetTile(actualGridPosition) == null)
			return false;

		switch (ground.GetTile(actualGridPosition).name) {
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
}
