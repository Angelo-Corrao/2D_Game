using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Shoot : MonoBehaviour {
	public float fireRate = 1f;
	public CarMovement carMovement;
	public GameObject projectilePrefab;
	public Tilemap ground;

	private PlayerInput playerInput;
	private CarAnimation carAnimation;
	private bool canShoot = true;
	// This must have a single decimal value
	private float spawnOffset = 0.4f;

	private void Awake() {
		carAnimation = GetComponent<CarAnimation>();
		playerInput = new PlayerInput();

		playerInput.Car.Shoot.performed += ctx => {
			if (canShoot)
				StartCoroutine(Fire(ctx.ReadValue<Vector2>()));
		};
	}

	private void OnEnable() {
		playerInput.Enable();
	}

	private void OnDisable() {
		playerInput.Disable();
	}

	private IEnumerator Fire(Vector2 direction) {
		RotateCar(direction);

		Vector3 spawnPoint = carMovement.transform.position + ((Vector3)direction * spawnOffset);
		Projectile proj = Instantiate(projectilePrefab, spawnPoint, Quaternion.identity).GetComponent<Projectile>();
		proj.direction = direction;
		proj.ground = ground;
		proj.spawnOffset = spawnOffset;

		canShoot = false;
		yield return new WaitForSeconds(fireRate);
		canShoot = true;
	}

	private void RotateCar(Vector2 direction) {
		string targetDirection = "";

		if (direction.x == 0) {
			if (direction.y == 1) {
				targetDirection = "up";
			}
			else {
				targetDirection = "down";
			}
		}
		else if (direction.x == 1) {
			targetDirection = "right";
		}
		else {
			targetDirection = "left";
		}

		carAnimation.Animate(carMovement.startingOrientation, targetDirection, direction);
		carMovement.startingOrientation = targetDirection;
		if (carMovement.isMoving)
			carMovement.hasShootAfterMoved = true;
	}
}
