using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/MapPieceRules")]
public class MapPieceRules : ScriptableObject
{
    public List<TileBase> upPossibleSprites = new List<TileBase>();
	public List<TileBase> downPossibleSprites = new List<TileBase>();
	public List<TileBase> rightPossibleSprites = new List<TileBase>();
	public List<TileBase> leftPossibleSprites = new List<TileBase>();
}
