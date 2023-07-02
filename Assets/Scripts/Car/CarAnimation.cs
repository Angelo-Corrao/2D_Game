using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CarAnimation : MonoBehaviour
{
	public Animator animator;
	[HideInInspector]
	public string currentAnimation = "";

	// Starts the right animation based on the starting orientation of the car when the player move and the direction he wants to move
	public void Animate(string startingOrientation, string targetDirection, Vector2 direction) {
		switch (targetDirection) {
			case "up":
				switch (startingOrientation) {
					case "left":
						animator.SetBool("LeftToUp", true);
						currentAnimation = "LeftToUp";
						break;

					case "right":
						animator.SetBool("RightToUp", true);
						currentAnimation = "RightToUp";
						break;

					case "down":
						animator.SetBool("DownToUp", true);
						currentAnimation = "DownToUp";
						break;
				}
				break;

			case "down":
				switch (startingOrientation) {
					case "up":
						animator.SetBool("UpToDown", true);
						currentAnimation = "UpToDown";
						break;

					case "left":
						animator.SetBool("LeftToDown", true);
						currentAnimation = "LeftToDown";
						break;

					case "right":
						animator.SetBool("RightToDown", true);
						currentAnimation = "RightToDown";
						break;
				}
				break;

			case "right":
				switch (startingOrientation) {
					case "up":
						animator.SetBool("UpToRight", true);
						currentAnimation = "UpToRight";
						break;

					case "left":
						animator.SetBool("LeftToRight", true);
						currentAnimation = "LeftToRight";
						break;

					case "down":
						animator.SetBool("DownToRight", true);
						currentAnimation = "DownToRight";
						break;
				}
				break;

			case "left":
				switch (startingOrientation) {
					case "up":
						animator.SetBool("UpToLeft", true);
						currentAnimation = "UpToLeft";
						break;

					case "right":
						animator.SetBool("RightToLeft", true);
						currentAnimation = "RightToLeft";
						break;

					case "down":
						animator.SetBool("DownToLeft", true);
						currentAnimation = "DownToLeft";
						break;
				}
				break;
		}

		StartCoroutine(StopAnimation());
	}

	public IEnumerator StopAnimation() {
		yield return new WaitForSeconds(0.2f);
		if (currentAnimation != "")
			animator.SetBool(currentAnimation, false);
	}
}
