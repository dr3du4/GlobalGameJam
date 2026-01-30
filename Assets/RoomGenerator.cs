using UnityEngine;
using System.Collections.Generic; 

public class RoomGenerator : MonoBehaviour
{
    public List<GameObject> roomsPrefabs;

    public void GenerateRoom(Vector3 positionDoor)
    {
        Debug.Log("Generating room at: " + positionDoor);
    
    }

}
