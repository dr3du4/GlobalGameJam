using UnityEngine;
using UnityEngine.InputSystem;

public class ServerConnection : MonoBehaviour
{
    [Header("Connection Settings")]
    public float connectionRange = 2f;
    public bool isCableConnected = false;
    
    [Header("Visual Feedback")]
    public GameObject connectedIndicator; // Optional: object to show when connected
    
    private Transform player;
    private CableHolder nearbyCable = null;
    private bool isPlayerNearby = false;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
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
    
    void FindNearbyCable()
    {
        // Find all CableHolders in the scene
        CableHolder[] cables = FindObjectsOfType<CableHolder>();
        
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
        // Add your game logic here
        // Examples:
        // - Enable power to something
        // - Unlock a door
        // - Start a puzzle sequence
        // - etc.
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
        
        Debug.Log("Cable disconnected from server");
    }
    
    // Visual feedback in editor
    void OnDrawGizmos()
    {
        Gizmos.color = isCableConnected ? Color.green : (isPlayerNearby ? Color.cyan : Color.blue);
        Gizmos.DrawWireSphere(transform.position, connectionRange);
    }
}

