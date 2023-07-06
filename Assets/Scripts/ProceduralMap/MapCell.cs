using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapCell
{
    public Vector2 gridPosition;
	public int entropyLevel;
	public List<TileBase> possibleSprites = new List<TileBase>();
	public bool isFilled = false;

    public MapCell(Vector2 gridPosition, List<TileBase> possibleSprites) {
        this.gridPosition = gridPosition;
		this.possibleSprites = possibleSprites;
		entropyLevel = possibleSprites.Count;
	}
}
