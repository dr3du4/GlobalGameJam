using UnityEngine;
using UnityEngine.InputSystem;

public class ServerConnection : MonoBehaviour
{
    [Header("Connection Settings")]
    public float connectionRange = 1f;
    public CableColor serverColor = CableColor.Yellow;
    public bool isCableConnected = false;
    
    [Header("Visual Feedback")]
    public GameObject connectedIndicator;
    public string emissiveMaterialName = "emmisive";
    
    [Header("Network Settings")]
    [SerializeField] private Tile.LightCircuit tileLightCircuit;
    [SerializeField] private Danger.DangerType dangerType;
    
    private Transform player;
    private CableHolder nearbyCable = null;
    private bool isPlayerNearby = false;
    private Material emissiveMaterial;
    
    void Start()
    {
        FindOperatorPlayer();
        
        if (!gameObject.CompareTag("Server"))
        {
            Debug.LogWarning($"{gameObject.name} should have 'Server' tag!");
        }
        
        if (connectedIndicator != null)
        {
            connectedIndicator.SetActive(false);
        }
        
        FindAndUpdateEmissiveMaterial();
    }
    
    void FindOperatorPlayer()
    {
        // Szukaj Operatora - gracza z MovementSpine
        MovementSpine[] operators = FindObjectsByType<MovementSpine>(FindObjectsSortMode.None);
        foreach (var op in operators)
        {
            var netObj = op.GetComponent<Unity.Netcode.NetworkObject>();
            if (netObj != null)
            {
                if (netObj.IsOwner)
                {
                    player = op.transform;
                    return;
                }
            }
            else
            {
                player = op.transform;
                return;
            }
        }
        
        // Fallback
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    void FindAndUpdateEmissiveMaterial()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.name.ToLower().Contains("emmisive") || mat.name.ToLower().Contains("emissive"))
                {
                    emissiveMaterial = mat;
                    UpdateEmissiveColor();
                    return;
                }
            }
        }
    }
    
    void UpdateEmissiveColor()
    {
        if (emissiveMaterial == null) return;
        
        Color color = GetColorFromEnum(serverColor);
        emissiveMaterial.color = color;
        emissiveMaterial.SetColor("_Color", color);
        emissiveMaterial.EnableKeyword("_EMISSION");
        emissiveMaterial.SetColor("_EmissionColor", color * 2f);
    }
    
    Color GetColorFromEnum(CableColor color)
    {
        return color switch
        {
            CableColor.Yellow => new Color(1f, 0.92f, 0.016f),
            CableColor.Red => new Color(1f, 0f, 0f),
            CableColor.Green => new Color(0f, 1f, 0f),
            CableColor.Blue => new Color(0f, 0.5f, 1f),
            _ => Color.white
        };
    }
    
    void Update()
    {
        if (player == null)
        {
            FindOperatorPlayer();
            if (player == null) return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isPlayerNearby = distanceToPlayer <= connectionRange;
        
        bool isClosest = IsClosestServer();
        
        // Podłączenie kabla - E
        if (isPlayerNearby && isClosest && !isCableConnected)
        {
            FindNearbyCable();
            
            if (nearbyCable != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (nearbyCable.IsCableHeld() && !nearbyCable.IsCableConnected())
                {
                    if (nearbyCable.GetCableColor() == serverColor)
                    {
                        ConnectCable();
                    }
                }
            }
        }
        
        // Odłączenie kabla - F
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
        if (!isPlayerNearby || player == null) return false;
        
        ServerConnection[] allServers = FindObjectsByType<ServerConnection>(FindObjectsSortMode.None);
        float myDistance = Vector3.Distance(transform.position, player.position);
        
        foreach (ServerConnection server in allServers)
        {
            if (server == this) continue;
            
            float otherDistance = Vector3.Distance(server.transform.position, player.position);
            if (otherDistance <= server.connectionRange && otherDistance < myDistance)
            {
                return false;
            }
        }
        
        return true;
    }
    
    void FindNearbyCable()
    {
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
        // Sprawdź czy jesteśmy w trybie multiplayer
        if (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsClient)
        {
            // Multiplayer - wyślij przez NetworkCableInteraction
            var networkInteraction = GetComponent<NetworkCableInteraction>();
            if (networkInteraction != null)
            {
                networkInteraction.OnCableConnected();
                return;
            }
        }
        
        // Single player lub brak NetworkCableInteraction - zmień lokalnie
        TileManager tileManager = FindFirstObjectByType<TileManager>();
        tileManager?.SetupDangers(dangerType);
        tileManager?.SetupLights(tileLightCircuit);
    }
    
    void DisconnectCableFromServer()
    {
        if (nearbyCable != null)
        {
            nearbyCable.DisconnectFromServer();
        }
        
        isCableConnected = false;
        
        if (connectedIndicator != null)
        {
            connectedIndicator.SetActive(false);
        }
        
        OnCableUnplugged();
    }
    
    void OnCableUnplugged()
    {
        // Sprawdź czy jesteśmy w trybie multiplayer
        if (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsClient)
        {
            var networkInteraction = GetComponent<NetworkCableInteraction>();
            if (networkInteraction != null)
            {
                networkInteraction.OnCableDisconnected();
                return;
            }
        }
        
        // Single player - wyczyść lokalnie
        FindFirstObjectByType<TileManager>()?.ClearAll();
    }
    
    public void DisconnectCable()
    {
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
        
        OnCableUnplugged();
    }
    
    // Publiczne właściwości dla NetworkCableInteraction
    public Tile.LightCircuit TileLightCircuit => tileLightCircuit;
    public Danger.DangerType DangerType => dangerType;
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            FindAndUpdateEmissiveMaterial();
        }
    }
    
    void OnDrawGizmos()
    {
        Color gizmoColor = GetColorFromEnum(serverColor);
        
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
