using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
struct Package<T> {
	public int index0;
	public int index1;
	public T value;
	public Package(int index0, int index1, T value) {
		this.index0 = index0;
		this.index1 = index1;
		this.value = value;
	}
}

/*
 * In this project i use Json Utility for serializing and deserializing objects so i need this custom class
 * to be able to save the matrix that represents the game's map
 */
[System.Serializable]
public class SerializableMatrix<T> : ISerializationCallbackReceiver {
	public T[,] matrix = new T[20, 20];

	[SerializeField] 
	private List<Package<T>> serializableMatrix;

	public void OnAfterDeserialize() {
		matrix = new T[20, 20];
		foreach (var package in serializableMatrix) {
			matrix[package.index0, package.index1] = package.value;
		}
	}

	public void OnBeforeSerialize() {
		serializableMatrix = new List<Package<T>>();
		for (int i = 0; i < matrix.GetLength(0); i++) {
			for (int j = 0; j < matrix.GetLength(1); j++) {
				serializableMatrix.Add(new Package<T>(i, j, matrix[i, j]));
			}
		}
	}
}
