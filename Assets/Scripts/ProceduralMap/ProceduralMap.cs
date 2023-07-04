using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMap : MonoBehaviour
{
    public List<GameObject> allSprites = new List<GameObject>();
	public List<MapPieceRules> spritesRules= new List<MapPieceRules>();
	public GameObject wallSprite;

	private List<MapCell> cells = new List<MapCell>();
	private List<MapPiece> mapPieces = new List<MapPiece>();
	private int lowerEntropyIdx = 0;
	private bool isMapCompleted = false;

	private void Start() {
		GenerateMapPieces();
		GenerateMapCells();
	}

	private void Update() {
		if (!isMapCompleted)
			Loop();
		else
			gameObject.SetActive(false);
	}

	private void Loop() {
		LowerEntropyCell();
		InstantiateSprite();
		Propagate();
	}

	private void LowerEntropyCell() {
		int lowerEntropyLevel = allSprites.Count;

		for (int i = 0; i < cells.Count; i++) {
			if (!cells[i].isFilled && cells[i].entropyLevel < lowerEntropyLevel) {
				lowerEntropyLevel = cells[i].entropyLevel;
				lowerEntropyIdx = i;
			}
		}
	}

	private void InstantiateSprite() {
		if (cells[lowerEntropyIdx].possibleSprites.Count == 0)
			Instantiate(wallSprite, cells[lowerEntropyIdx].gridPosition, Quaternion.identity);
		else if (cells[lowerEntropyIdx].possibleSprites.Count == 1)
			Instantiate(cells[lowerEntropyIdx].possibleSprites[0], cells[lowerEntropyIdx].gridPosition, Quaternion.identity);
		else {
			int randomSprite = PickRandomSprite();
			Instantiate(cells[lowerEntropyIdx].possibleSprites[randomSprite], cells[lowerEntropyIdx].gridPosition, Quaternion.identity);
		}

		cells[lowerEntropyIdx].isFilled = true;
	}

	private int PickRandomSprite() {
		int possibleSpriteCount = cells[lowerEntropyIdx].possibleSprites.Count;
		return Random.Range(0, possibleSpriteCount);
	}

	private void Propagate() {
		bool areAllCellsFilled = true;
		foreach (MapCell cell in cells) {
			if (!cell.isFilled) {
				areAllCellsFilled = false;
			}
		}
		if (areAllCellsFilled) {
			isMapCompleted = true;
			return;
		}
		else {
			// To-Do
		}
	}

	private void GenerateMapPieces() {
		for (int i = 0; i < allSprites.Count; i++) {
			mapPieces.Add(new MapPiece(allSprites[i], spritesRules[i]));
		}
	}

	private void GenerateMapCells() {
		List<GameObject> possibileSprites = new List<GameObject>();
		for (int i = -10; i < 10; i++) {
			for (int j = -10; j < 10; j++) {
				if (i == -10 && j == -10) {
					// Only possible sprite bottom left corner
					possibileSprites.Clear();
					foreach (GameObject s in allSprites) {
						if (s.name == "roadNE")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == 9 && j == -10) {
					// Only possible sprite bottom right corner
					possibileSprites.Clear();
					foreach (GameObject s in allSprites) {
						if (s.name == "roadNW")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == -10 && j == 9) {
					// Only possible sprite top left corner
					possibileSprites.Clear();
					foreach (GameObject s in allSprites) {
						if (s.name == "roadSE")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == 9 && j == 9) {
					// Only possible sprite top right corner
					possibileSprites.Clear();
					foreach (GameObject s in allSprites) {
						if (s.name == "roadSW")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == -10 && j != -10 && j != 9) {
					// Only possible sprites are: NS, NES
					possibileSprites.Clear();
					foreach (GameObject s in allSprites) {
						if (s.name == "roadNS" || s.name == "roadNES")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == 9 && j != -10 && j != 9) {
					// Only possible sprites are: NS, NWS
					possibileSprites.Clear();
					foreach (GameObject s in allSprites) {
						if (s.name == "roadNS" || s.name == "roadNWS")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (j == 9 && i != -10 && i != 9) {
					// Only possible sprites are: EW, EWS
					possibileSprites.Clear();
					foreach (GameObject s in allSprites) {
						if (s.name == "roadEW" || s.name == "roadEWS")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (j == -10 && i != -10 && i != 9) {
					// Only possible sprites are: EW, NEW
					possibileSprites.Clear();
					foreach (GameObject s in allSprites) {
						if (s.name == "roadEW" || s.name == "roadNEW")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else {
					// All sprites are possible
					cells.Add(new MapCell(new Vector2(i, j), allSprites));
				}
			}
		}
	}
}
