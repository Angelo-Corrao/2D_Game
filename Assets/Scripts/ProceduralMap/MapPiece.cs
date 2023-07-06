using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapPiece
{
    public TileBase sprite;
    public MapPieceRules mapPieceRules;

    public MapPiece(TileBase sprite, MapPieceRules mapPieceRules) {
        this.sprite = sprite;
        this.mapPieceRules = mapPieceRules;
    }
}
