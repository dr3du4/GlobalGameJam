using UnityEngine;
using System.Collections.Generic; 

public class RoomGenerator : MonoBehaviour
{
    public List<GameObject> roomsPrefabs;

    public void GenerateRoom(Vector3 positionDoor)
    {
        bool isGenerated = false;
        while (!isGenerated)
        {
            int randomIndex = Random.Range(0, roomsPrefabs.Count);
            
            Renderer roomRenderer = roomsPrefabs[randomIndex].GetComponentInChildren<Renderer>();
            float roomWidth = roomRenderer.bounds.size.x;
            float roomDepth = roomRenderer.bounds.size.z;
            float roomHeight = roomRenderer.bounds.size.y;
            positionDoor.y = 0;
            Vector3 spawnPosition = positionDoor + new Vector3(0, roomHeight/2, roomDepth / 2);
            GameObject room = Instantiate(roomsPrefabs[randomIndex], spawnPosition, Quaternion.identity);
            
            Debug.Log("Room generated at: " + spawnPosition + " with width: " + roomWidth + " and depth: " + roomDepth);
            isGenerated = true;
        }
    }

}
