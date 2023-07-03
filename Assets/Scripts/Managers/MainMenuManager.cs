using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
	public static MainMenuManager Instance;

	private bool saveAlreadyExists = false;
	private bool hasToSave = true;

	private void Awake() {
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	private void Start() {
		AudioManager.Instance.PlayMusic("Main Menu");
		if (File.Exists(Path.Combine(Application.persistentDataPath, DataPersistenceManager.Instance.fileName))) {
			saveAlreadyExists = true;
		}
	}

	/*
	 * When New Game is clicked, setting "isNewGame" to true make every object that implements IDataPersistence
	 * load default value for it's properties
	 */
	public void NewGame() {
		DataPersistenceManager.Instance.NewGame();
		DataPersistenceManager.Instance.SaveGame();
		DataPersistenceManager.Instance.isNewGame = true;
		Cursor.lockState = CursorLockMode.Locked;
		AudioManager.Instance.PlaySFX("Button");
		SceneManager.LoadScene(1);
	}

	// If the save file doesn't exist the player can't click continue
	public void Continue() {
		if (saveAlreadyExists) {
			DataPersistenceManager.Instance.isNewGame = false;
			Cursor.lockState = CursorLockMode.Locked;
			AudioManager.Instance.PlaySFX("Button");
			SceneManager.LoadScene(1);
		}
	}

	// I want to save only if i come back to main menu from a game, so if i quit from main menu the game will not save
	public void QuitGame() {
		hasToSave = false;
		AudioManager.Instance.PlaySFX("Button");
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}

	private void OnApplicationQuit() {
		if (hasToSave)
			DataPersistenceManager.Instance.SaveGame();
	}
}
