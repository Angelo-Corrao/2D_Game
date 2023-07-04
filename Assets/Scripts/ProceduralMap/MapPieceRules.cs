using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/MapPieceRules")]
public class MapPieceRules : ScriptableObject
{
    public List<GameObject> upPossibleSprites = new List<GameObject>();
	public List<GameObject> downPossibleSprites = new List<GameObject>();
	public List<GameObject> rightPossibleSprites = new List<GameObject>();
	public List<GameObject> leftPossibleSprites = new List<GameObject>();
}
