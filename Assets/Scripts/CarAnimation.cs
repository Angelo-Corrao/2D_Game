using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CarAnimation : MonoBehaviour
{
	public float totSteps = 10;
	public float animationTime = 0.5f;
	public float oneStepDistance = 0.1f;
	public UnityEvent animationFinished;

	private float timeBetweenSteps;
	private Animator animator;

	private void Awake() {
		timeBetweenSteps = animationTime / totSteps;
		animator = GetComponent<Animator>();
	}

	// Starts the right animation based on the starting orientation of the car when the player move and the direction he wants to move
	public void Animate(string startingOrientation, string targetDirection, Vector2 direction) {
		string animationToStop = "";
		switch (targetDirection) {
			case "up":
				switch (startingOrientation) {
					case "left":
						animator.SetBool("LeftToUp", true);
						animationToStop = "LeftToUp";
						break;

					case "right":
						animator.SetBool("RightToUp", true);
						animationToStop = "RightToUp";
						break;

					case "down":
						animator.SetBool("DownToUp", true);
						animationToStop = "DownToUp";
						break;
				}
				break;

			case "down":
				switch (startingOrientation) {
					case "up":
						animator.SetBool("UpToDown", true);
						animationToStop = "UpToDown";
						break;

					case "left":
						animator.SetBool("LeftToDown", true);
						animationToStop = "LeftToDown";
						break;

					case "right":
						animator.SetBool("RightToDown", true);
						animationToStop = "RightToDown";
						break;
				}
				break;

			case "right":
				switch (startingOrientation) {
					case "up":
						animator.SetBool("UpToRight", true);
						animationToStop = "UpToRight";
						break;

					case "left":
						animator.SetBool("LeftToRight", true);
						animationToStop = "LeftToRight";
						break;

					case "down":
						animator.SetBool("DownToRight", true);
						animationToStop = "DownToRight";
						break;
				}
				break;

			case "left":
				switch (startingOrientation) {
					case "up":
						animator.SetBool("UpToLeft", true);
						animationToStop = "UpToLeft";
						break;

					case "right":
						animator.SetBool("RightToLeft", true);
						animationToStop = "RightToLeft";
						break;

					case "down":
						animator.SetBool("DownToLeft", true);
						animationToStop = "DownToLeft";
						break;
				}
				break;
		}

		StartCoroutine(MoveBySteps(direction, oneStepDistance, timeBetweenSteps, animationToStop,
								   startingOrientation, targetDirection));
	}

	// Moves the car by little steps instead of teleporting it
	private IEnumerator MoveBySteps(Vector2 direction, float oneStepDistance, float timeBetweenSteps,
									string animationToStop, string startingOrientation, string targetDirection) {
		for (int i = 0; i < totSteps; i++) {
			transform.position += (Vector3)direction * oneStepDistance;
			yield return new WaitForSeconds(timeBetweenSteps);
		}

		if (animationToStop != "")
			animator.SetBool(animationToStop, false);

		animationFinished?.Invoke();
	}
}
