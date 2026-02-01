using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Komponent sieciowy do dodania na obiekty z ServerConnection.
/// Gdy kabel zostanie podłączony, wysyła info do serwera,
/// który informuje Runnera o zmianie świateł/hazardów.
/// </summary>
public class NetworkCableInteraction : NetworkBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private GameObject connectionIndicator;

    // Pobieramy dane z ServerConnection (jeden źródło prawdy)
    private ServerConnection serverConnection;

    // Stan synchronizowany przez sieć
    private NetworkVariable<bool> isConnected = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        serverConnection = GetComponent<ServerConnection>();
    }

    private void Start()
    {
        isConnected.OnValueChanged += OnConnectionStateChanged;
        UpdateVisuals(isConnected.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isConnected.OnValueChanged -= OnConnectionStateChanged;
    }

    /// <summary>
    /// Wywoływane przez ServerConnection.OnCablePluggedIn()
    /// </summary>
    public void OnCableConnected()
    {
        Debug.Log($"[NetworkCableInteraction] OnCableConnected! NetworkManager={NetworkManager.Singleton != null}, IsClient={NetworkManager.Singleton?.IsClient}");
        
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning("[NetworkCableInteraction] ❌ Brak NetworkManager lub nie jesteśmy klientem!");
            return;
        }
        if (serverConnection == null)
        {
            Debug.LogWarning("[NetworkCableInteraction] ❌ Brak ServerConnection!");
            return;
        }

        Debug.Log($"[NetworkCableInteraction] ✅ Wysyłam ServerRpc! Circuit={serverConnection.TileLightCircuit}, Danger={serverConnection.DangerType}");
        RequestConnectionServerRpc(true);
    }

    /// <summary>
    /// Wywoływane przez ServerConnection.OnCableUnplugged()
    /// </summary>
    public void OnCableDisconnected()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient) return;

        RequestConnectionServerRpc(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestConnectionServerRpc(bool connected, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[NetworkCableInteraction] 📡 ServerRpc otrzymany! connected={connected}");
        
        isConnected.Value = connected;

        if (serverConnection == null)
        {
            Debug.LogWarning("[NetworkCableInteraction] ❌ serverConnection == null na serwerze!");
            return;
        }

        if (connected)
        {
            Debug.Log($"[NetworkCableInteraction] ✅ Wysyłam ClientRpc: circuit={(int)serverConnection.TileLightCircuit}, danger={(int)serverConnection.DangerType}");
            // Wyślij info o włączeniu świateł/hazardów do Runnera
            SetLightsAndHazardsClientRpc(
                (int)serverConnection.TileLightCircuit, 
                (int)serverConnection.DangerType
            );
        }
        else
        {
            Debug.Log("[NetworkCableInteraction] Wysyłam ClearLightsAndHazardsClientRpc");
            ClearLightsAndHazardsClientRpc();
        }
    }

    [ClientRpc]
    private void SetLightsAndHazardsClientRpc(int circuit, int danger)
    {
        Debug.Log($"[NetworkCableInteraction] 📥 ClientRpc otrzymany! circuit={circuit}, danger={danger}");
        
        // Tylko Runner przetwarza to
        bool isRunner = NetworkGameManager.Instance == null || NetworkGameManager.Instance.IsLocalPlayerRunner();
        Debug.Log($"[NetworkCableInteraction] IsLocalPlayerRunner = {isRunner}");
        
        if (NetworkGameManager.Instance != null && !NetworkGameManager.Instance.IsLocalPlayerRunner())
        {
            Debug.Log("[NetworkCableInteraction] Nie jestem Runnerem - ignoruję");
            return;
        }

        // Znajdź TileManager i zastosuj zmiany
        TileManager tileManager = FindFirstObjectByType<TileManager>();
        Debug.Log($"[NetworkCableInteraction] TileManager = {tileManager != null}");
        
        if (tileManager != null)
        {
            Debug.Log($"[NetworkCableInteraction] ✅ Ustawiam światła={circuit}, zagrożenia={danger}");
            tileManager.SetupLights((Tile.LightCircuit)circuit);
            tileManager.SetupDangers((Danger.DangerType)danger);
        }

        // Alternatywnie przez NetworkTileManager
        NetworkTileManager netTileManager = FindFirstObjectByType<NetworkTileManager>();
        if (netTileManager != null)
        {
            netTileManager.SetLightCircuit((Tile.LightCircuit)circuit);
            netTileManager.SetDangerType((Danger.DangerType)danger);
        }
    }

    [ClientRpc]
    private void ClearLightsAndHazardsClientRpc()
    {
        if (NetworkGameManager.Instance != null && !NetworkGameManager.Instance.IsLocalPlayerRunner())
        {
            return;
        }

        TileManager tileManager = FindFirstObjectByType<TileManager>();
        tileManager?.ClearAll();

        NetworkTileManager netTileManager = FindFirstObjectByType<NetworkTileManager>();
        netTileManager?.ClearAll();
    }

    private void OnConnectionStateChanged(bool previous, bool current)
    {
        UpdateVisuals(current);
    }

    private void UpdateVisuals(bool connected)
    {
        if (connectionIndicator != null)
        {
            connectionIndicator.SetActive(connected);
        }
    }

    public bool IsConnected => isConnected.Value;
}
