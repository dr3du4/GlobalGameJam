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
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient) return;
        if (serverConnection == null) return;

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
        isConnected.Value = connected;

        if (serverConnection == null) return;

        if (connected)
        {
            // Wyślij info o włączeniu świateł/hazardów do Runnera
            SetLightsAndHazardsClientRpc(
                (int)serverConnection.TileLightCircuit, 
                (int)serverConnection.DangerType
            );
        }
        else
        {
            ClearLightsAndHazardsClientRpc();
        }
    }

    [ClientRpc]
    private void SetLightsAndHazardsClientRpc(int circuit, int danger)
    {
        // Tylko Runner przetwarza to
        if (NetworkGameManager.Instance != null && !NetworkGameManager.Instance.IsLocalPlayerRunner())
        {
            return;
        }

        // Znajdź TileManager i zastosuj zmiany
        TileManager tileManager = FindFirstObjectByType<TileManager>();
        if (tileManager != null)
        {
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
