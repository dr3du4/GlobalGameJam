using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public RoomGenerator roomGenerator;
    public InputSystem_Actions inputActions;

    void Awake()
    {
        instance = this;
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
    }

    public void GenerateRoom(Vector3 positionDoor)
    {
        roomGenerator.GenerateRoom(positionDoor);
        Debug.Log("Room generated at: " + positionDoor);
    }
}
