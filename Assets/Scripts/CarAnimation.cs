using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CarAnimation : MonoBehaviour
{
	private Animator animator;

	private void Awake() {
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

		StartCoroutine(StopAnimation(animationToStop));
	}

	private IEnumerator StopAnimation(string animationToStop) {
		yield return new WaitForSeconds(0.2f);
		if (animationToStop != "")
			animator.SetBool(animationToStop, false);
	}
}
