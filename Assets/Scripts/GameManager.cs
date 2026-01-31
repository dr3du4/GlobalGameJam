using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    public bool useFirstMap { get; private set; } = true;
    public static GameManager instance;
    // public RoomGenerator roomGenerator;
    public InputSystem_Actions inputActions;

    public Camera mainCamera;
    public Camera secondCamera;

    public bool isCableEnjoyerChosen { get; private set; } = true;
    void Awake()
    {
        instance = this;
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        
        // ZAKOMENTOWANE DLA MULTIPLAYER - przełączanie graczy na spację tylko w singleplayer
        // Odkomentuj jeśli wracasz do singleplayer
        /*
        inputActions.Player.Jump.performed += OnSpacePressed;
        SwitchToCableEnjoyer();
        */
    }

    /* ZAKOMENTOWANE DLA MULTIPLAYER - odkomentuj dla singleplayer
    void OnSpacePressed(InputAction.CallbackContext context)
    {
        SwitchPlayer();
    }

    void SwitchPlayer()
    {
        if (isCableEnjoyerChosen)
        {
            SwitchToGridWalker();
        }
        else
        {
            SwitchToCableEnjoyer();
        }
    }

    void SwitchToCableEnjoyer()
    {
        Debug.Log("Use first map: " + useFirstMap);
        useFirstMap = true;
        isCableEnjoyerChosen = true;
        mainCamera.enabled = true;
        secondCamera.enabled = false;
        if (FindFirstObjectByType<MovementSpine>() is MovementSpine movementSpine) movementSpine.enabled = true;
        if (FindFirstObjectByType<GridMovement>() is GridMovement gridMovement) gridMovement.enabled = false;
    }

    void SwitchToGridWalker()
    {
        Debug.Log("Use first map: " + useFirstMap);
        useFirstMap = false;
        isCableEnjoyerChosen = false;
        mainCamera.enabled = false;
        secondCamera.enabled = true;
        if (FindFirstObjectByType<GridMovement>() is GridMovement gridMovement) gridMovement.enabled = true;
        if (FindFirstObjectByType<MovementSpine>() is MovementSpine movementSpine) movementSpine.enabled = false;
    }
    */
}
