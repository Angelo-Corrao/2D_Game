using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralMap : MonoBehaviour, IDataPersistence {
	public List<TileBase> allTiles = new List<TileBase>();
	public List<MapPieceRules> tilesRules = new List<MapPieceRules>();
	public Tilemap road;
	public GameObject gameManager;
	public GameObject carController;
	public Canvas loading;
	public Canvas inGame;
	public static event Action mapCompleted; 

	[HideInInspector]
	public bool isMapCompleted = false;

	private List<MapCell> cells = new List<MapCell>();
	private List<MapPiece> mapPieces = new List<MapPiece>();
	private int lowerEntropyIdx = 0;
	private SerializableMatrix<string> proceduralTiles = new SerializableMatrix<string>();

	private void Start() {
		DataPersistenceManager.Instance.LoadGame();
		if (!isMapCompleted) {
			GenerateMapPieces();
			GenerateMapCells();
		}
	}

	private void Update() {
		if (!isMapCompleted)
			Loop();
	}

	private void Loop() {
		LowerEntropyCell();
		InstantiateSprite();
		Propagate();
	}

	private void LowerEntropyCell() {
		int lowerEntropyLevel = allTiles.Count + 1;

		for (int i = 0; i < cells.Count; i++) {
			if (!cells[i].isFilled && cells[i].entropyLevel < lowerEntropyLevel) {
				lowerEntropyLevel = cells[i].entropyLevel;
				lowerEntropyIdx = i;
			}
		}
	}

	private void InstantiateSprite() {
		Vector3Int gridPosition = road.WorldToCell((Vector3)cells[lowerEntropyIdx].gridPosition + new Vector3(-0.5f, -0.5f, 0));
		if (cells[lowerEntropyIdx].possibleTiles.Count == 1) {
			road.SetTile(gridPosition, cells[lowerEntropyIdx].possibleTiles[0]);
			// Save tile
			Vector3 cell = (Vector3)gridPosition;
			proceduralTiles.matrix[((int)cell.x) + 10, ((int)cell.y) + 10] = cells[lowerEntropyIdx].possibleTiles[0].name;
		}
		else {
			int randomSprite = PickRandomSprite();
			road.SetTile(gridPosition, cells[lowerEntropyIdx].possibleTiles[randomSprite]);
			TileBase instantiatedSprite = cells[lowerEntropyIdx].possibleTiles[randomSprite];
			cells[lowerEntropyIdx].possibleTiles.Clear();
			cells[lowerEntropyIdx].possibleTiles.Add(instantiatedSprite);
			cells[lowerEntropyIdx].entropyLevel = cells[lowerEntropyIdx].possibleTiles.Count;
			// Save tile
			Vector3 cell = (Vector3)gridPosition;
			proceduralTiles.matrix[((int)cell.x) + 10, ((int)cell.y) + 10] = cells[lowerEntropyIdx].possibleTiles[0].name;
		}

		cells[lowerEntropyIdx].isFilled = true;
	}

	private int PickRandomSprite() {
		int possibleSpriteCount = cells[lowerEntropyIdx].possibleTiles.Count;
		return UnityEngine.Random.Range(0, possibleSpriteCount);
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
			SetupStartGame();
			return;
		}
		else {
			/*
			 * cells[lowerEntropyIdx].possibleTiles[0] is the last instantiated sprite.
			 * For each possible sprite of the up, down, right and left cell of the last instantiated cell check if it exists
			 * in the rules of the propagating cell, if not remove it from the possible sprite list and update the entropy level.
			 */
			foreach (MapPiece mp in mapPieces) {
				if (cells[lowerEntropyIdx].possibleTiles.Count > 0) {
					if (mp.tile.name == cells[lowerEntropyIdx].possibleTiles[0].name) {
						// up cell
						int upIdx = lowerEntropyIdx + 1;
						if (upIdx >= 0 && upIdx < cells.Count && cells[lowerEntropyIdx].gridPosition.y != 9) {
							foreach (TileBase s in cells[upIdx].possibleTiles.ToList()) {
								bool isSpriteValid = false;
								for (int i = 0; i < mp.mapPieceRules.upPossibleTiles.Count; i++) {
									if (s == mp.mapPieceRules.upPossibleTiles[i]) {
										isSpriteValid = true;
										break;
									}
								}
								if (!isSpriteValid)
									cells[upIdx].possibleTiles.Remove(s);
							}
							cells[upIdx].entropyLevel = cells[upIdx].possibleTiles.Count;
						}
						// down cell
						int downIdx = lowerEntropyIdx - 1;
						if (downIdx >= 0 && downIdx < cells.Count && cells[lowerEntropyIdx].gridPosition.y != -10) {
							foreach (TileBase s in cells[downIdx].possibleTiles.ToList()) {
								bool isSpriteValid = false;
								for (int i = 0; i < mp.mapPieceRules.downPossibleTiles.Count; i++) {
									if (s == mp.mapPieceRules.downPossibleTiles[i]) {
										isSpriteValid = true;
										break;
									}
								}
								if (!isSpriteValid)
									cells[downIdx].possibleTiles.Remove(s);
							}
							cells[downIdx].entropyLevel = cells[downIdx].possibleTiles.Count;
						}
						// right cell
						int rightIdx = lowerEntropyIdx + 20;
						if (rightIdx >= 0 && rightIdx < cells.Count) {
							foreach (TileBase s in cells[rightIdx].possibleTiles.ToList()) {
								bool isSpriteValid = false;
								for (int i = 0; i < mp.mapPieceRules.rightPossibleTiles.Count; i++) {
									if (s == mp.mapPieceRules.rightPossibleTiles[i]) {
										isSpriteValid = true;
										break;
									}
								}
								if (!isSpriteValid) {
									cells[rightIdx].possibleTiles.Remove(s);
								}
							}
							cells[rightIdx].entropyLevel = cells[rightIdx].possibleTiles.Count;
						}
						// left cell
						int leftIdx = lowerEntropyIdx - 20;
						if (leftIdx >= 0 && leftIdx < cells.Count) {
							foreach (TileBase s in cells[leftIdx].possibleTiles.ToList()) {
								bool isSpriteValid = false;
								for (int i = 0; i < mp.mapPieceRules.leftPossibleTiles.Count; i++) {
									if (s == mp.mapPieceRules.leftPossibleTiles[i]) {
										isSpriteValid = true;
										break;
									}
								}
								if (!isSpriteValid)
									cells[leftIdx].possibleTiles.Remove(s);
							}
							cells[leftIdx].entropyLevel = cells[leftIdx].possibleTiles.Count;
						}

						break;
					}
				}
			}
		}
	}

	private void GenerateMapPieces() {
		for (int i = 0; i < allTiles.Count; i++) {
			mapPieces.Add(new MapPiece(allTiles[i], tilesRules[i]));
		}
	}

	private void GenerateMapCells() {
		for (int i = -9; i < 11; i++) {
			for (int j = -9; j < 11; j++) {
				if (i == -9 && j == -9) {
					// Only possible sprite bottom left corner
					List<TileBase> possibileSprites = new List<TileBase>();
					foreach (TileBase s in allTiles.ToList()) {
						if (s.name == "roadNE")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == 10 && j == -9) {
					// Only possible sprite bottom right corner
					List<TileBase> possibileSprites = new List<TileBase>();
					foreach (TileBase s in allTiles.ToList()) {
						if (s.name == "roadNW")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == -9 && j == 10) {
					// Only possible sprite top left corner
					List<TileBase> possibileSprites = new List<TileBase>();
					foreach (TileBase s in allTiles.ToList()) {
						if (s.name == "roadSE")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == 10 && j == 10) {
					// Only possible sprite top right corner
					List<TileBase> possibileSprites = new List<TileBase>();
					foreach (TileBase s in allTiles.ToList()) {
						if (s.name == "roadSW")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == -9 && j != -9 && j != 10) {
					// Only possible sprites are: NS, NES
					List<TileBase> possibileSprites = new List<TileBase>();
					foreach (TileBase s in allTiles.ToList()) {
						if (s.name == "roadNS" || s.name == "roadNES")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == 10 && j != -9 && j != 10) {
					// Only possible sprites are: NS, NWS
					List<TileBase> possibileSprites = new List<TileBase>();
					foreach (TileBase s in allTiles.ToList()) {
						if (s.name == "roadNS" || s.name == "roadNWS")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (j == 10 && i != -9 && i != 10) {
					// Only possible sprites are: EW, EWS
					List<TileBase> possibileSprites = new List<TileBase>();
					foreach (TileBase s in allTiles.ToList()) {
						if (s.name == "roadEW" || s.name == "roadEWS")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (j == -9 && i != -9 && i != 10) {
					// Only possible sprites are: EW, NEW
					List<TileBase> possibileSprites = new List<TileBase>();
					foreach (TileBase s in allTiles.ToList()) {
						if (s.name == "roadEW" || s.name == "roadNEW")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else if (i == 1 && j == 1) {
					// This is the central cell where the player spawns
					// In this way the player can't spawn in a curve
					List<TileBase> possibileSprites = new List<TileBase>();
					foreach (TileBase s in allTiles.ToList()) {
						if (s.name != "roadNE" && s.name != "roadNW" && s.name != "roadSE" && s.name != "roadSW")
							possibileSprites.Add(s);
					}
					cells.Add(new MapCell(new Vector2(i, j), possibileSprites));
				}
				else {
					// All sprites are possible
					cells.Add(new MapCell(new Vector2(i, j), allTiles.ToList()));
				}
			}
		}
	}

	public void SetupStartGame() {
		gameManager.SetActive(true);
		carController.SetActive(true);
		mapCompleted.Invoke();
		loading.gameObject.SetActive(false);
		inGame.gameObject.SetActive(true);
	}

	public void RecreateMap() {
		for (int i = 0; i < 20; i++) {
			for (int j = 0; j < 20; j++) {
				foreach (TileBase tile in allTiles) {
					if (tile.name == proceduralTiles.matrix[i, j]) {
						// - 10 is nedeed because the grid in world space goes from -10 to +10 and the matrix starts from the position [0, 0]
						Vector3Int gridPosition = road.WorldToCell(new Vector3(i - 10, j - 10, 0));
						road.SetTile(gridPosition, tile);

						break;
					}
				}
			}
		}
	}

	public void LoadData(GameData gameData, bool isNewGame) {
		if (!isNewGame) {
			proceduralTiles = gameData.proceduralTiles;
			RecreateMap();
			isMapCompleted = true;
			SetupStartGame();
		}
	}

	public void SaveData(ref GameData gameData) {
		gameData.proceduralTiles = proceduralTiles;
	}
}
