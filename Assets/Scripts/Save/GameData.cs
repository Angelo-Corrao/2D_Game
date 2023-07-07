using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

// This is the object that can be serialized and deserialized
public class GameData
{
	public SerializableMatrix<bool> visitedCells = new SerializableMatrix<bool>();
	public Vector3 playerPosition;
	public int projectilesCounter;
	public Vector3 enemyPosition;
	public List<Vector3> wellsPosition= new List<Vector3>();
	public List<Vector3> teleportsPosition = new List<Vector3>();
	public int gameMode;
	public SerializableMatrix<string> proceduralTiles = new SerializableMatrix<string>();
}
