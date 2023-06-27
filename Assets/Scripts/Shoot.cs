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

		GameObject proj = Instantiate(projectilePrefab, carMovement.transform.position, Quaternion.identity);
		proj.GetComponent<Projectile>().direction = direction;
		proj.GetComponent<Projectile>().ground = ground;

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

		carAnimation.Animate(carMovement.startingOrientation, targetDirection, direction, false);
		carMovement.startingOrientation = targetDirection;
		if (carMovement.isMoving)
			carAnimation.hasShootAfterMoved = true;
	}
}
