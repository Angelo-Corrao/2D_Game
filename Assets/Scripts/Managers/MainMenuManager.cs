using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public enum GameMode{
	STANDARD = 0,
	PROCEDURAL = 1
}

public class MainMenuManager : MonoBehaviour, IDataPersistence {
	public static MainMenuManager Instance;

	[HideInInspector]
	public GameMode gameMode;

	[HideInInspector]
	public bool isHost;

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
	public void NewGame(int gameMode) {
		Cursor.lockState = CursorLockMode.Locked;
		AudioManager.Instance.PlaySFX("Button");

		switch (gameMode) {
			case 0:
				this.gameMode = (GameMode)gameMode;
				SceneManager.LoadScene(1);
				break;

			case 1:
				this.gameMode = (GameMode)gameMode;
				SceneManager.LoadScene(2);
				break;
		}

		DataPersistenceManager.Instance.NewGame();
		DataPersistenceManager.Instance.SaveGame();
		DataPersistenceManager.Instance.isNewGame = true;
	}

	// If the save file doesn't exist the player can't click continue
	public void Continue() {
		if (saveAlreadyExists) {
			DataPersistenceManager.Instance.LoadGame();
			DataPersistenceManager.Instance.isNewGame = false;
			Cursor.lockState = CursorLockMode.Locked;
			AudioManager.Instance.PlaySFX("Button");

			switch (gameMode) {
				case GameMode.STANDARD:
					SceneManager.LoadScene(1);
					break;

				case GameMode.PROCEDURAL:
					SceneManager.LoadScene(2);
					break;
			}
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

	public void SetHost(bool isHost) {
		this.isHost = isHost;
		SceneManager.LoadScene(3);
	}

	private void OnApplicationQuit() {
		if (hasToSave)
			DataPersistenceManager.Instance.SaveGame();
	}

	public void LoadData(GameData gameData, bool isNewGame) {
		gameMode = (GameMode)gameData.gameMode;
	}

	public void SaveData(ref GameData gameData) {
		gameData.gameMode = (int)gameMode;
	}
}
