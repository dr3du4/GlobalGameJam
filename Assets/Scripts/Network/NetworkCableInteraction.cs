using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Komponent do dodania na ServerConnection.
/// Gdy kabel zostanie podłączony, wysyła info do serwera,
/// który informuje Runnera o zmianie świateł/hazardów.
/// </summary>
public class NetworkCableInteraction : NetworkBehaviour
{
    [Header("Kolor Obwodu")]
    [Tooltip("Który kolor świateł/hazardów włączyć u Runnera")]
    [SerializeField] private Tile.LightCircuit lightCircuit;
    
    [Header("Typ Zagrożenia")]
    [Tooltip("Jaki typ hazardu aktywować")]
    [SerializeField] private Danger.DangerType dangerType;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject connectionIndicator;

    // Stan synchronizowany przez sieć
    private NetworkVariable<bool> isConnected = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

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

        Debug.Log($"[NetworkCableInteraction] Kabel podłączony - wysyłam {lightCircuit}");
        RequestConnectionServerRpc(true);
    }

    /// <summary>
    /// Wywoływane przez ServerConnection.DisconnectCable()
    /// </summary>
    public void OnCableDisconnected()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient) return;

        Debug.Log($"[NetworkCableInteraction] Kabel odłączony");
        RequestConnectionServerRpc(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestConnectionServerRpc(bool connected, ServerRpcParams rpcParams = default)
    {
        // Aktualizuj stan sieciowy
        isConnected.Value = connected;

        // Wyślij zmianę do Runnera
        if (connected)
        {
            // Włącz światła i hazardy danego koloru
            SetLightsAndHazardsClientRpc((int)lightCircuit, (int)dangerType);
        }
        else
        {
            // Wyczyść wszystko
            ClearLightsAndHazardsClientRpc();
        }

        Debug.Log($"[Server] Kabel {(connected ? "podłączony" : "odłączony")} - {lightCircuit}");
    }

    [ClientRpc]
    private void SetLightsAndHazardsClientRpc(int circuit, int danger)
    {
        // Tylko Runner przetwarza to
        if (NetworkGameManager.Instance != null && !NetworkGameManager.Instance.IsLocalPlayerRunner())
        {
            return;
        }

        Debug.Log($"[Runner] Włączam światła: {(Tile.LightCircuit)circuit}, hazardy: {(Danger.DangerType)danger}");

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

        Debug.Log("[Runner] Czyszczę światła i hazardy");

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
    public Tile.LightCircuit Circuit => lightCircuit;
}
