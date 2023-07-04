using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCell
{
    public Vector2 gridPosition;
	public int entropyLevel;
	public List<GameObject> possibleSprites = new List<GameObject>();
	public bool isFilled = false;

    public MapCell(Vector2 gridPosition, List<GameObject> possibleSprites) {
        this.gridPosition = gridPosition;
		this.possibleSprites = possibleSprites;
		entropyLevel = possibleSprites.Count;
	}
}
