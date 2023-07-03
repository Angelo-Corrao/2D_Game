using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using System.IO;

public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager Instance { get; private set; }
	public string fileName = "SaveData.json";
	public GameData gameData;
	public List<IDataPersistence> dataPersistenceObjects;
	public bool isNewGame;
	private string fullPath;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else {
			Destroy(gameObject);
		}

		fullPath = Path.Combine(Application.persistentDataPath, fileName);
	}

	private void Start() {
		SceneManager.activeSceneChanged += ChangedActiveScene;
		dataPersistenceObjects = FindAllDataPersistenceObjects();
	}

	public void NewGame() {
		gameData = new GameData();
	}

	public void SaveGame() {
		foreach (IDataPersistence dataPersistenceObjcet in dataPersistenceObjects) {
			dataPersistenceObjcet.SaveData(ref gameData);
		}

		FileDataHandler.Instance.Save(gameData, fullPath);
	}

	public void LoadGame() {
		gameData = FileDataHandler.Instance.Load(fullPath);

		if (gameData == null) {
			NewGame();
		}

		foreach (IDataPersistence dataPersistenceObjcet in dataPersistenceObjects) {
			dataPersistenceObjcet.LoadData(gameData, isNewGame);
		}
	}

	private void ChangedActiveScene(Scene arg0, Scene arg1) {
		dataPersistenceObjects = FindAllDataPersistenceObjects();
	}

	public List<IDataPersistence> FindAllDataPersistenceObjects() {
		IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
		return dataPersistenceObjects.ToList();
	}
}
