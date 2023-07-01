using UnityEngine;

/*
 * This interface will be implemented from every object that can be teleported
 * in a different grid's cell during the gamplay
 */
public interface ITeleportable
{
	public bool isOnOtherTeleportableObjects(Vector3 newPos);
}
