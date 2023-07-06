using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// This is the object that can be serialized and deserialized
public class GameData
{
	public SerializableMatrix<bool> grid = new SerializableMatrix<bool>();
	public Vector3 playerPosition;
	public int projectilesCounter;
	public Vector3 enemyPosition;
	public List<Vector3> wellsPosition= new List<Vector3>();
	public List<Vector3> teleportsPosition = new List<Vector3>();
	public int gameMode;
}
