using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/MapPieceRules")]
public class MapPieceRules : ScriptableObject
{
    public List<TileBase> upPossibleTiles = new List<TileBase>();
	public List<TileBase> downPossibleTiles = new List<TileBase>();
	public List<TileBase> rightPossibleTiles = new List<TileBase>();
	public List<TileBase> leftPossibleTiles = new List<TileBase>();
}
