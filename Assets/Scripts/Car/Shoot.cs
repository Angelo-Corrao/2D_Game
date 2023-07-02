using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Shoot : MonoBehaviour {
	public float fireRate = 1f;
	public CarController carController;
	public GameObject projectilePrefab;
	public Tilemap road;

	private PlayerInput playerInput;
	private CarAnimation carAnimation;
	private bool canShoot = true;
	// Must have a single decimal value
	private float spawnOffset = 0.4f;

	private void Awake() {
		carAnimation = GetComponent<CarAnimation>();
		playerInput = new PlayerInput();

		playerInput.Car.Shoot.performed += ctx => {
			// To avoid undesidered behaviors it's not possible to shoot while the car is moving
			if (canShoot && !carController.isMoving && carController.projectilesCounter > 0 && !GameManager.Instance.isGamePaused)
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
		AudioManager.Instance.PlaySFX("Shoot");
		// Every time the player shoot the number of available projectile is decreased and the UI is updated
		carController.projectilesCounter--;
		GameManager.Instance.UpdateAmmoUI(carController.projectilesCounter);
		// When the the last projectile is shot check until there is no projectile on the map, than is Game Over
		if (carController.projectilesCounter == 0)
			GameManager.Instance.checkActiveProjectiles = true;
		RotateCar(direction);

		// Spawn the projectile with an offset from the car so it doesn't collide with it
		Vector3 spawnPoint = carController.transform.position + ((Vector3)direction * spawnOffset);
		Projectile proj = Instantiate(projectilePrefab, spawnPoint, Quaternion.identity).GetComponent<Projectile>();
		GameManager.Instance.activeProjectiles.Add(proj);
		proj.direction = direction;
		proj.road = road;
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

		carAnimation.Animate(carController.startingOrientation, targetDirection, direction);
		carController.startingOrientation = targetDirection;
	}
}
