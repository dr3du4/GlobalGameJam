using UnityEngine;

public class GameManger : MonoBehaviour
{
    public static GameManger instance;
    public RoomGenerator roomGenerator;

    void Awake()
    {
        instance = this;
    }

    public void GenerateRoom(Vector3 positionDoor, Vector3 doorForward)
    {
        roomGenerator.GenerateRoom(positionDoor, doorForward);
        Debug.Log("Room generated at: " + positionDoor);
    }
}
