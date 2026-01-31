using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    public bool useFirstMap = true;
    private bool wasSpacePressed = false;
    public static GameManager instance;
    public RoomGenerator roomGenerator;
    public InputSystem_Actions inputActions;

    public Camera mainCamera;
    public Camera secondCamera;
    void Awake()
    {
        instance = this;
        useFirstMap = true;
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        mainCamera.enabled = true;
        secondCamera.enabled = false;
        inputActions.Player.Jump.performed += OnSpacePressed;
    }

    void OnSpacePressed(InputAction.CallbackContext context)
    {
        Debug.Log("Use first map: " + useFirstMap);
        useFirstMap = !useFirstMap;
        mainCamera.enabled = !mainCamera.enabled;
        secondCamera.enabled = !secondCamera.enabled;
    }
}
