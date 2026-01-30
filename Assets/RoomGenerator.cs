using UnityEngine;
using System.Collections.Generic; 

public class RoomGenerator : MonoBehaviour
{
    public List<GameObject> roomsPrefabs;

    public void GenerateRoom(Vector3 positionDoor)
    {
        Debug.Log("Generating room at: " + positionDoor);
        bool isGenerated = false;
        while (!isGenerated)
        {
            int randomIndex = Random.Range(0, roomsPrefabs.Count);
            
            Renderer roomRenderer = roomsPrefabs[randomIndex].GetComponentInChildren<Renderer>();
            float roomWidth = roomRenderer.bounds.size.x;
            float roomDepth = roomRenderer.bounds.size.z;
            float roomHeight = roomRenderer.bounds.size.y;
            
            // Calculate spawn position based on door's forward direction
            Vector3 offset = doorForward * (roomDepth / 2);
            Vector3 spawnPosition = positionDoor + offset;
            spawnPosition.y = roomHeight / 2;
            
            // Check if area is free using OverlapBox
            Vector3 checkSize = new Vector3(roomWidth * 0.9f, roomHeight * 0.9f, roomDepth * 0.9f);
            Collider[] colliders = Physics.OverlapBox(spawnPosition, checkSize / 2);
            
            if (colliders.Length == 0)
            {
                // Area is free, spawn the room
                GameObject room = Instantiate(roomsPrefabs[randomIndex], spawnPosition, Quaternion.LookRotation(doorForward));
                Debug.Log("Room generated at: " + spawnPosition + " with dimensions: " + roomWidth + "x" + roomDepth);
                isGenerated = true;
            }
            else
            {
                Debug.Log("Area occupied, trying another room...");
            }
        }
    }

}
