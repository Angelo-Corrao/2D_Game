using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPiece
{
    public GameObject sprite;
    public MapPieceRules mapPieceRules;

    public MapPiece(GameObject sprite, MapPieceRules mapPieceRules) {
        this.sprite = sprite;
        this.mapPieceRules = mapPieceRules;
    }
}
