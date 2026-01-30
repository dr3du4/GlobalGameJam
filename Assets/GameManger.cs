using UnityEngine;

public class GameManger : MonoBehaviour
{
    public static GameManger instance;
    public RoomGenerator roomGenerator;

    void Awake()
    {
        instance = this;
    }

    public void GenerateRoom(Vector3 positionDoor)
    {
        roomGenerator.GenerateRoom(positionDoor);
        Debug.Log("Room generated at: " + positionDoor);
    }
}
