using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class ServerConnection : MonoBehaviour
{
    [Header("Connection Settings")]
    public float connectionRange = 2f;
    public bool isCableConnected = false;
    
    [Header("Visual Feedback")]
    public GameObject connectedIndicator; // Optional: object to show when connected
    [SerializeField] private Tile.LightCircuit tileLightCircuit;
    [SerializeField] private Danger.DangerType dangerType;
    
    [Header("Network Integration")]
    [SerializeField] private NetworkCableInteraction networkCableInteraction;
    
    private Transform player;
    private CableHolder nearbyCable = null;
    private bool isPlayerNearby = false;
    private bool useNetworking = false;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Check if we're in networked mode
        useNetworking = NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient;
        
        // Only subscribe to input if GameManager exists (non-networked mode)
        if (GameManager.instance != null)
        {
            GameManager.instance.inputActions.Player.Interact.performed += OnInteractPressed;
        }
        
        // Auto-find NetworkCableInteraction if not assigned
        if (networkCableInteraction == null)
        {
            networkCableInteraction = GetComponent<NetworkCableInteraction>();
        }
        
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
        if (player == null || isCableConnected) return;
        
        // Check distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isPlayerNearby = distanceToPlayer <= connectionRange;
        
        // Check if player is holding a cable
        if (isPlayerNearby)
        {
            FindNearbyCable();
            
            // Try to connect cable with E key
            if (nearbyCable != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (nearbyCable.IsCableHeld() && !nearbyCable.IsCableConnected())
                {
                    ConnectCable();
                }
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
                    ConnectCable();
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
        // Network integration - wysyłamy info do serwera
        if (useNetworking)
        {
            if (networkCableInteraction != null)
            {
                networkCableInteraction.OnCableConnected();
                Debug.Log($"[ServerConnection] Kabel podłączony - wysłano do sieci");
            }
            else
            {
                Debug.LogWarning("[ServerConnection] Brak NetworkCableInteraction! Dodaj ten komponent do tego serwera.");
            }
        }
        else
        {
            // Tryb lokalny (bez sieci) - bezpośrednio zmień tile'e
            TileManager tileManager = FindFirstObjectByType<TileManager>();
            tileManager?.SetupDangers(dangerType);
            tileManager?.SetupLights(tileLightCircuit);
            Debug.Log($"[ServerConnection] Kabel podłączony (tryb lokalny)");
        }
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
        
        // Network integration
        if (useNetworking && networkCableInteraction != null)
        {
            networkCableInteraction.OnCableDisconnected();
        }
        
        Debug.Log("Cable disconnected from server");
        FindFirstObjectByType<TileManager>()?.ClearAll();
    }
    
    // Visual feedback in editor
    void OnDrawGizmos()
    {
        Gizmos.color = isCableConnected ? Color.green : (isPlayerNearby ? Color.cyan : Color.blue);
        Gizmos.DrawWireSphere(transform.position, connectionRange);
    }
}

