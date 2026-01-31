using UnityEngine;
using UnityEngine.InputSystem;

public class ServerConnection : MonoBehaviour
{
    [Header("Connection Settings")]
    public float connectionRange = 2f;
    public CableColor serverColor = CableColor.Yellow;
    public bool isCableConnected = false;
    
    [Header("Visual Feedback")]
    public GameObject connectedIndicator; // Optional: object to show when connected
    [SerializeField] private Tile.LightCircuit tileLightCircuit;
    [SerializeField] private Danger.DangerType dangerType;
    
    private Transform player;
    private CableHolder nearbyCable = null;
    private bool isPlayerNearby = false;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        GameManager.instance.inputActions.Player.Interact.performed += OnInteractPressed;
        
        // Make sure this object has the "Server" tag
        if (!gameObject.CompareTag("Server"))
        {
            Debug.LogWarning($"{gameObject.name} should have 'Server' tag!");
        }
        
        if (connectedIndicator != null)
        {
            connectedIndicator.SetActive(false);
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Check distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isPlayerNearby = distanceToPlayer <= connectionRange;
        
        // Check if player is holding a cable (when not connected)
        if (isPlayerNearby && !isCableConnected)
        {
            FindNearbyCable();
            
            // Try to connect cable with E key
            if (nearbyCable != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (nearbyCable.IsCableHeld() && !nearbyCable.IsCableConnected())
                {
                    // Check color compatibility before connecting
                    if (nearbyCable.GetCableColor() == serverColor)
                    {
                        ConnectCable();
                    }
                    else
                    {
                        Debug.LogWarning($"Color mismatch! Cable is {nearbyCable.GetCableColor()}, server requires {serverColor}");
                    }
                }
            }
        }
        
        // Disconnect cable with F key when connected
        if (isPlayerNearby && isCableConnected)
        {
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                DisconnectCableFromServer();
            }
        }
    }

    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        if (isPlayerNearby)
        {
            FindNearbyCable();
            
            // Try to connect cable
            if (nearbyCable != null)
            {
                if (nearbyCable.IsCableHeld() && !nearbyCable.IsCableConnected())
                {
                    // Check color compatibility before connecting
                    if (nearbyCable.GetCableColor() == serverColor)
                    {
                        ConnectCable();
                    }
                    else
                    {
                        Debug.LogWarning($"Color mismatch! Cable is {nearbyCable.GetCableColor()}, server requires {serverColor}");
                    }
                }
            }
        }
    }
    
    void FindNearbyCable()
    {
        // Find all CableHolders in the scene
        CableHolder[] cables = FindObjectsByType<CableHolder>(FindObjectsSortMode.None);
        
        nearbyCable = null;
        foreach (CableHolder cable in cables)
        {
            if (cable.IsCableHeld() && !cable.IsCableConnected())
            {
                nearbyCable = cable;
                break;
            }
        }
    }
    
    void ConnectCable()
    {
        if (nearbyCable == null) return;
        
        nearbyCable.ConnectToServer(gameObject);
        isCableConnected = true;
        
        if (connectedIndicator != null)
        {
            connectedIndicator.SetActive(true);
        }
        
        Debug.Log($"Cable connected to {gameObject.name}!");
        
        // Call any game logic here
        OnCablePluggedIn();
    }
    
    public void OnCableConnected(CableHolder cable)
    {
        isCableConnected = true;
        nearbyCable = cable;
        
        if (connectedIndicator != null)
        {
            connectedIndicator.SetActive(true);
        }
    }
    
    void OnCablePluggedIn()
    {
        TileManager tileManager = FindFirstObjectByType<TileManager>();
        tileManager?.SetupDangers(dangerType);
        tileManager?.SetupLights(tileLightCircuit);
        // Add your game logic here
        // Examples:
        // - Enable power to something
        // - Unlock a door
        // - Start a puzzle sequence
        // - etc.
    }
    
    void DisconnectCableFromServer()
    {
        // Player presses F at server - takes cable from server
        if (nearbyCable != null)
        {
            nearbyCable.DisconnectFromServer();
        }
        
        isCableConnected = false;
        
        if (connectedIndicator != null)
        {
            connectedIndicator.SetActive(false);
        }
        
        Debug.Log("Cable disconnected from server. Take it back to holder.");
        FindFirstObjectByType<TileManager>()?.ClearAll();
    }
    
    public void DisconnectCable()
    {
        // Complete disconnect (used when resetting)
        if (nearbyCable != null)
        {
            nearbyCable.DisconnectCable();
        }
        
        isCableConnected = false;
        nearbyCable = null;
        
        if (connectedIndicator != null)
        {
            connectedIndicator.SetActive(false);
        }
        
        Debug.Log("Cable disconnected from server");
        FindFirstObjectByType<TileManager>()?.ClearAll();
    }
    
    // Visual feedback in editor
    void OnDrawGizmos()
    {
        // Draw gizmo with server color
        Color gizmoColor = Color.white;
        switch (serverColor)
        {
            case CableColor.Yellow:
                gizmoColor = Color.yellow;
                break;
            case CableColor.Red:
                gizmoColor = Color.red;
                break;
            case CableColor.Green:
                gizmoColor = Color.green;
                break;
            case CableColor.Blue:
                gizmoColor = Color.blue;
                break;
        }
        
        // Brighter when connected, medium when player nearby, darker when far
        if (isCableConnected)
            Gizmos.color = gizmoColor;
        else if (isPlayerNearby)
            Gizmos.color = gizmoColor * 0.8f;
        else
            Gizmos.color = gizmoColor * 0.5f;
            
        Gizmos.DrawWireSphere(transform.position, connectionRange);
    }
}

