using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OnlineShoot : NetworkBehaviour {
	public OnlineCarController carController;
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
			if (canShoot && !carController.isMoving && carController.projectilesCounter > 0 && !OnlineGameManager.Instance.isGamePaused)
				Fire(ctx.ReadValue<Vector2>());
		};
	}

	private void OnEnable() {
		playerInput.Enable();
		OnlineProjectile.projDestroyed += () => EndTurn();
	}

	private void OnDisable() {
		playerInput.Disable();
		OnlineProjectile.projDestroyed -= () => EndTurn();
	}

	private void Fire(Vector2 direction) {
		AudioManager.Instance.PlaySFX("Shoot");
		// Every time the player shoot the number of available projectile is decreased and the UI is updated
		carController.projectilesCounter--;
		carController.isMoving = true;
		OnlineGameManager.Instance.UpdateAmmoUI(carController.projectilesCounter);
		// When the the last projectile is shot check until there is no projectile on the map, than is Game Over
		if (carController.projectilesCounter == 0)
			OnlineGameManager.Instance.checkActiveProjectiles = true;
		RotateCar(direction);

		// Spawn the projectile with an offset from the car so it doesn't collide with it
		Vector3 spawnPoint = carController.transform.position + ((Vector3)direction * spawnOffset);
		OnlineProjectile proj = Instantiate(projectilePrefab, spawnPoint, Quaternion.identity).GetComponent<OnlineProjectile>();
		OnlineGameManager.Instance.activeProjectiles.Add(proj);
		proj.direction = direction;
		proj.road = road;
		proj.spawnOffset = spawnOffset;

		canShoot = false;
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

	private void EndTurn() {
		canShoot = true;
		carController.isMoving = false;
		OnlineGameManager.Instance.ChangeActivePlayerServerRpc();
	}
}
