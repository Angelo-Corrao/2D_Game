using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapCell
{
    public Vector2 gridPosition;
	public int entropyLevel;
	public List<TileBase> possibleTiles = new List<TileBase>();
	public bool isFilled = false;

    public MapCell(Vector2 gridPosition, List<TileBase> possibleTiles) {
        this.gridPosition = gridPosition;
		this.possibleTiles = possibleTiles;
		entropyLevel = possibleTiles.Count;
	}
}
