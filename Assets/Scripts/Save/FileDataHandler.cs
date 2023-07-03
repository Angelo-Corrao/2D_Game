using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Unity.VisualScripting;

public class FileDataHandler : MonoBehaviour
{
    public static FileDataHandler Instance;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
			Destroy(gameObject);
	}

	public GameData Load(string path) {
        GameData loadedData = null;
		string dataToLoad = "";

		if (File.Exists(path)) {
            using (StreamReader sr = File.OpenText(path)) {
                dataToLoad = sr.ReadToEnd();
            }
        }

        loadedData = JsonUtility.FromJson<GameData>(dataToLoad);

        return loadedData;
	}

    public void Save(GameData gameData, string path) {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string dataToStore = JsonUtility.ToJson(gameData);
        using (StreamWriter sw = File.CreateText(path)) {
            sw.WriteLine(dataToStore);
        }
	}

    public void Delete(string path) {
        File.Delete(path);
    }
}
