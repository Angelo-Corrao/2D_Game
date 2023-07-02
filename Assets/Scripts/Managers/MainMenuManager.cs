using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
	public static MainMenuManager Instance;

	private void Awake() {
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	private void Start() {
		AudioManager.Instance.PlayMusic("Main Menu");
	}

	public void Play() {
		Cursor.lockState = CursorLockMode.Locked;
		AudioManager.Instance.PlaySFX("Button");
		SceneManager.LoadScene(1);
	}

	public void QuitGame() {
		AudioManager.Instance.PlaySFX("Button");
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}
}
