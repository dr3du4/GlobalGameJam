using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    public bool useFirstMap = true;
    private bool wasSpacePressed = false;
    public static GameManager instance;
    public RoomGenerator roomGenerator;
    public InputSystem_Actions inputActions;

    void Awake()
    {
        instance = this;
        useFirstMap = true;
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            bool isSpacePressed = Keyboard.current.spaceKey.isPressed;
           
            if (isSpacePressed && !wasSpacePressed)
            {
                Debug.Log("Use first map: " + useFirstMap);
                useFirstMap = !useFirstMap;
            }
            
            wasSpacePressed = isSpacePressed;
        }
    }
}
