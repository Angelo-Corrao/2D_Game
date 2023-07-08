using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemy : MonoBehaviour, ITeleportable {
	public TileBase fogTile;

	public bool isOnOtherTeleportableObjects(Vector3 newPos) {
		Tilemap fogOfWar = OnlineGameManager.Instance.fogOfWar;
		bool isThereAValidPosition = IsThereAValdiPosition(fogOfWar);

		Vector3Int newPosGridPosition = fogOfWar.WorldToCell(newPos);
		if (!isThereAValidPosition) {
			bool result = CompareTeleportableObjects(newPos);
			return result;
		}
		else {
			if (fogOfWar.GetTile(newPosGridPosition) != null) {
				bool result = CompareTeleportableObjects(newPos);
				return result;
			}
			else
				return true;
		}
	}

	/*
	 * In this method the entire game grid is scrolled, and if there isn't a valid position to where teleport the enemy, it
	 * will be teleported in a random position already out of the Fog Of War.
	 * A valid position exist if there's a cell with the fog of war active and if hidden within it there is no well or teleport.
	 * If the only cells with the fog of war active are the ones with a well or teleport hidden within it, this means all the map
	 * has been explored by the player. So if we add an achievement like "Explore the entire map in a single game" it would be
	 * possible for the player to earn it.
	 */
	private bool IsThereAValdiPosition(Tilemap fogOfWar) {
		for (int i = -10; i < 10; i++) {
			for (int j = -10; j < 10; j++) {
				Vector3Int gridPosition = fogOfWar.WorldToCell(new Vector3(i, j, 0));
				if (fogOfWar.GetTile(gridPosition) != null) {
					if (!OnlineGameManager.Instance.IsOnWell(gridPosition) && !OnlineGameManager.Instance.IsOnTeleport(gridPosition)) {
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool CompareTeleportableObjects(Vector3 newPos) {
		List<ITeleportable> teleportables = OnlineGameManager.Instance.teleportables;
		for (int i = 0; i < teleportables.Count; i++) {
			// This check if i'm not comparing the same object
			if (this != teleportables[i]) {
				MonoBehaviour teleportableObject = (MonoBehaviour)teleportables[i];
				if (newPos.x == teleportableObject.transform.position.x && newPos.y == teleportableObject.transform.position.y) {
					return true;
				}
			}
		}

		return false;
	}
}
