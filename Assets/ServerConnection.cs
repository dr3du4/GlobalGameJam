using UnityEngine;
using UnityEngine.InputSystem;

public class ServerConnection : MonoBehaviour
{
    [Header("Connection Settings")]
    public float connectionRange = 1f;
    public CableColor serverColor = CableColor.Yellow;
    public bool isCableConnected = false;
    
    [Header("Visual Feedback")]
    public GameObject connectedIndicator; // Optional: object to show when connected
    public string emissiveMaterialName = "emmisive"; // Name of the emissive material
    
    private Transform player;
    private CableHolder nearbyCable = null;
    private bool isPlayerNearby = false;
    private Material emissiveMaterial;
    
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
        
        // Find and update emissive material color
        FindAndUpdateEmissiveMaterial();
    }
    
    void FindAndUpdateEmissiveMaterial()
    {
        // Get all renderers in this object and children
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                // Check if material name contains "emmisive" or "emissive"
                if (mat.name.ToLower().Contains("emmisive") || mat.name.ToLower().Contains("emissive"))
                {
                    emissiveMaterial = mat;
                    UpdateEmissiveColor();
                    return;
                }
            }
        }
        
        Debug.LogWarning($"Emissive material not found on {gameObject.name}");
    }
    
    void UpdateEmissiveColor()
    {
        if (emissiveMaterial == null) return;
        
        Color color = GetColorFromEnum(serverColor);
        
        // Set base color
        emissiveMaterial.color = color;
        emissiveMaterial.SetColor("_Color", color);
        
        // Set emission color for glow effect
        emissiveMaterial.EnableKeyword("_EMISSION");
        emissiveMaterial.SetColor("_EmissionColor", color * 2f); // Brighter emission
    }
    
    Color GetColorFromEnum(CableColor color)
    {
        switch (color)
        {
            case CableColor.Yellow:
                return new Color(1f, 0.92f, 0.016f); // Bright yellow
            case CableColor.Red:
                return new Color(1f, 0f, 0f); // Pure red
            case CableColor.Green:
                return new Color(0f, 1f, 0f); // Pure green
            case CableColor.Blue:
                return new Color(0f, 0.5f, 1f); // Bright blue
            default:
                return Color.white;
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Check distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isPlayerNearby = distanceToPlayer <= connectionRange;
        
        // Only interact with the CLOSEST server
        bool isClosest = IsClosestServer();
        
        // Check if player is holding a cable (when not connected)
        if (isPlayerNearby && isClosest && !isCableConnected)
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
        if (isPlayerNearby && isClosest && isCableConnected)
        {
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                DisconnectCableFromServer();
            }
        }
    }
    
    bool IsClosestServer()
    {
        if (!isPlayerNearby) return false;
        
        ServerConnection[] allServers = FindObjectsByType<ServerConnection>(FindObjectsSortMode.None);
        float myDistance = Vector3.Distance(transform.position, player.position);
        
        foreach (ServerConnection server in allServers)
        {
            if (server == this) continue;
            
            float otherDistance = Vector3.Distance(server.transform.position, player.position);
            if (otherDistance <= server.connectionRange && otherDistance < myDistance)
            {
                return false; // Found a closer one
            }
        }
        
        return true; // This is the closest
    }

    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        if (isPlayerNearby && IsClosestServer())
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
        
        // Najpierw odłącz wszystkie inne kable - tylko jeden może być podłączony
        DisconnectAllOtherCables();
        
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
    
    void DisconnectAllOtherCables()
    {
        ServerConnection[] allServers = FindObjectsByType<ServerConnection>(FindObjectsSortMode.None);
        
        foreach (ServerConnection server in allServers)
        {
            if (server == this) continue;
            
            if (server.isCableConnected)
            {
                Debug.Log($"Odłączam poprzedni kabel z {server.gameObject.name}");
                server.ForceDisconnect();
            }
        }
    }
    
    public void ForceDisconnect()
    {
        if (nearbyCable != null)
        {
            nearbyCable.ReturnCableToHolder();
        }
        
        isCableConnected = false;
        nearbyCable = null;
        
        if (connectedIndicator != null)
        {
            connectedIndicator.SetActive(false);
        }
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
        
        // Automatyczne mapowanie koloru kabla na światła i zagrożenia
        Tile.LightCircuit circuit = GetLightCircuitFromColor(serverColor);
        Danger.DangerType danger = GetDangerTypeFromColor(serverColor);
        
        tileManager?.SetupLights(circuit);
        tileManager?.SetupDangers(danger);
        
        Debug.Log($"Kabel {serverColor} podłączony - włączam światła {circuit} i zagrożenia {danger}");
    }
    
    Tile.LightCircuit GetLightCircuitFromColor(CableColor color)
    {
        return color switch
        {
            CableColor.Yellow => Tile.LightCircuit.Yellow,
            CableColor.Red => Tile.LightCircuit.Red,
            CableColor.Green => Tile.LightCircuit.Green,
            CableColor.Blue => Tile.LightCircuit.Blue,
            _ => Tile.LightCircuit.White
        };
    }
    
    Danger.DangerType GetDangerTypeFromColor(CableColor color)
    {
        return color switch
        {
            CableColor.Yellow => Danger.DangerType.Fire,
            CableColor.Red => Danger.DangerType.Mechanic,
            CableColor.Green => Danger.DangerType.Toxic,
            CableColor.Blue => Danger.DangerType.Electric,
            _ => Danger.DangerType.Fire
        };
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
        
        Debug.Log($"Kabel {serverColor} odłączony - wyłączam światła i zagrożenia");
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
    
    void OnValidate()
    {
        // Update colors when values change in inspector
        if (Application.isPlaying)
        {
            FindAndUpdateEmissiveMaterial();
        }
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
        
        // Brighter when connected or when this is the closest server to player
        bool isClosest = IsClosestServer();
        if (isCableConnected)
            Gizmos.color = gizmoColor;
        else if (isPlayerNearby && isClosest)
            Gizmos.color = gizmoColor * 0.8f;
        else
            Gizmos.color = gizmoColor * 0.5f;
            
        Gizmos.DrawWireSphere(transform.position, connectionRange);
    }
}

