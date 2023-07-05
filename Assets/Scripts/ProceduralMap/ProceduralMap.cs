using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralMap : MonoBehaviour {
	public List<GameObject> allSprites = new List<GameObject>();
	public List<MapPieceRules> spritesRules = new List<MapPieceRules>();
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
		int lowerEntropyLevel = allSprites.Count + 1;

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
		else if (cells[lowerEntropyIdx].possibleSprites.Count == 1) {
			Instantiate(cells[lowerEntropyIdx].possibleSprites[0], cells[lowerEntropyIdx].gridPosition, Quaternion.identity);
		}
		else {
			int randomSprite = PickRandomSprite();
			Instantiate(cells[lowerEntropyIdx].possibleSprites[randomSprite], cells[lowerEntropyIdx].gridPosition, Quaternion.identity);
			GameObject instantiatedSprite = cells[lowerEntropyIdx].possibleSprites[randomSprite];
			cells[lowerEntropyIdx].possibleSprites.Clear();
			cells[lowerEntropyIdx].possibleSprites.Add(instantiatedSprite);
			cells[lowerEntropyIdx].entropyLevel = cells[lowerEntropyIdx].possibleSprites.Count;
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
			/*
			 * cells[lowerEntropyIdx].possibleSprites[0] is the last instantiated sprite.
			 * For each possible sprite of the up, down, right and left cell of the last instantiated cell check if it exists
			 * in the rules of the propagating cell, if not remove it from the possible sprite list and update the entropy level.
			 */
			foreach (MapPiece mp in mapPieces) {
				if (mp.sprite.name == cells[lowerEntropyIdx].possibleSprites[0].name) {
					// up cell
					int upIdx = lowerEntropyIdx + 1;
					if (upIdx >= 0 && upIdx < cells.Count) {
						foreach (GameObject s in cells[upIdx].possibleSprites.ToList()) {
							bool isSpriteValid = false;
							for (int i = 0; i < mp.mapPieceRules.upPossibleSprites.Count; i++) {
								if (s == mp.mapPieceRules.upPossibleSprites[i]) {
									isSpriteValid = true;
									break;
								}
							}
							if (!isSpriteValid)
								cells[upIdx].possibleSprites.Remove(s);
						}
						cells[upIdx].entropyLevel = cells[upIdx].possibleSprites.Count;
					}
					// down cell
					int downIdx = lowerEntropyIdx - 1;
					if (downIdx >= 0 && downIdx < cells.Count) {
						foreach (GameObject s in cells[downIdx].possibleSprites.ToList()) {
							bool isSpriteValid = false;
							for (int i = 0; i < mp.mapPieceRules.downPossibleSprites.Count; i++) {
								if (s == mp.mapPieceRules.downPossibleSprites[i]) {
									isSpriteValid = true;
									break;
								}
							}
							if (!isSpriteValid)
								cells[downIdx].possibleSprites.Remove(s);
						}
						cells[downIdx].entropyLevel = cells[downIdx].possibleSprites.Count;
					}
					// right cell
					int rightIdx = lowerEntropyIdx + 20;
					if (rightIdx >= 0 && rightIdx < cells.Count) {
						foreach (GameObject s in cells[rightIdx].possibleSprites.ToList()) {
							bool isSpriteValid = false;
							for (int i = 0; i < mp.mapPieceRules.rightPossibleSprites.Count; i++) {
								if (s == mp.mapPieceRules.rightPossibleSprites[i]) {
									isSpriteValid = true;
									break;
								}
							}
							if (!isSpriteValid) {
								cells[rightIdx].possibleSprites.Remove(s);
							}
						}
						cells[rightIdx].entropyLevel = cells[rightIdx].possibleSprites.Count;
					}
					// left cell
					int leftIdx = lowerEntropyIdx - 20 - (-10 + (-1 * ((int)cells[lowerEntropyIdx].gridPosition.y)));
					if (leftIdx >= 0 && leftIdx < cells.Count) {
						foreach (GameObject s in cells[leftIdx].possibleSprites.ToList()) {
							bool isSpriteValid = false;
							for (int i = 0; i < mp.mapPieceRules.leftPossibleSprites.Count; i++) {
								if (s == mp.mapPieceRules.leftPossibleSprites[i]) {
									isSpriteValid = true;
									break;
								}
							}
							if (!isSpriteValid)
								cells[leftIdx].possibleSprites.Remove(s);
						}
						cells[leftIdx].entropyLevel = cells[leftIdx].possibleSprites.Count;
					}

					break;
				}
			}
		}
	}

	private void GenerateMapPieces() {
		for (int i = 0; i < allSprites.Count; i++) {
			mapPieces.Add(new MapPiece(allSprites[i], spritesRules[i]));
		}
	}

	private void GenerateMapCells() {
		for (int i = -10; i < 10; i++) {
			for (int j = -10; j < 10; j++) {
				if (i == -10 && j == -10) {
					// Only possible sprite bottom left corner
					List<GameObject> possibileSprites = new List<GameObject>();
					foreach (GameObject s in allSprites.ToList()) {
						if (s.name == "roadNE")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == 9 && j == -10) {
					// Only possible sprite bottom right corner
					List<GameObject> possibileSprites = new List<GameObject>();
					foreach (GameObject s in allSprites.ToList()) {
						if (s.name == "roadNW")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == -10 && j == 9) {
					// Only possible sprite top left corner
					List<GameObject> possibileSprites = new List<GameObject>();
					foreach (GameObject s in allSprites.ToList()) {
						if (s.name == "roadSE")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == 9 && j == 9) {
					// Only possible sprite top right corner
					List<GameObject> possibileSprites = new List<GameObject>();
					foreach (GameObject s in allSprites.ToList()) {
						if (s.name == "roadSW")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == -10 && j != -10 && j != 9) {
					// Only possible sprites are: NS, NES
					List<GameObject> possibileSprites = new List<GameObject>();
					foreach (GameObject s in allSprites.ToList()) {
						if (s.name == "roadNS" || s.name == "roadNES")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == 9 && j != -10 && j != 9) {
					// Only possible sprites are: NS, NWS
					List<GameObject> possibileSprites = new List<GameObject>();
					foreach (GameObject s in allSprites.ToList()) {
						if (s.name == "roadNS" || s.name == "roadNWS")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (j == 9 && i != -10 && i != 9) {
					// Only possible sprites are: EW, EWS
					List<GameObject> possibileSprites = new List<GameObject>();
					foreach (GameObject s in allSprites.ToList()) {
						if (s.name == "roadEW" || s.name == "roadEWS")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (j == -10 && i != -10 && i != 9) {
					// Only possible sprites are: EW, NEW
					List<GameObject> possibileSprites = new List<GameObject>();
					foreach (GameObject s in allSprites.ToList()) {
						if (s.name == "roadEW" || s.name == "roadNEW")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else {
					// All sprites are possible
					cells.Add(new MapCell(new Vector2(i, j), allSprites.ToList()));
				}
			}
		}
	}
}
