using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapPiece
{
    public TileBase tile;
    public MapPieceRules mapPieceRules;

    public MapPiece(TileBase tile, MapPieceRules mapPieceRules) {
        this.tile = tile;
        this.mapPieceRules = mapPieceRules;
    }
}
