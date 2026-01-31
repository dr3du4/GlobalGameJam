using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Network-aware TileManager that syncs tile/danger visibility across clients.
/// Used for the Runner's world to respond to Operator actions.
/// </summary>
public class NetworkTileManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private TileManager localTileManager;

    // Network-synced state
    private NetworkVariable<int> currentLightCircuit = new NetworkVariable<int>(
        -1, // -1 = none
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<int> currentDangerType = new NetworkVariable<int>(
        -1, // -1 = none
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        if (localTileManager == null)
        {
            localTileManager = FindFirstObjectByType<TileManager>();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to state changes
        currentLightCircuit.OnValueChanged += OnLightCircuitChanged;
        currentDangerType.OnValueChanged += OnDangerTypeChanged;

        // Apply initial state
        ApplyCurrentState();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        currentLightCircuit.OnValueChanged -= OnLightCircuitChanged;
        currentDangerType.OnValueChanged -= OnDangerTypeChanged;
    }

    /// <summary>
    /// Server sets the light circuit (called from RunnerWorldController)
    /// </summary>
    public void SetLightCircuit(Tile.LightCircuit circuit)
    {
        if (!IsServer) return;
        currentLightCircuit.Value = (int)circuit;
    }

    /// <summary>
    /// Server sets the danger type
    /// </summary>
    public void SetDangerType(Danger.DangerType dangerType)
    {
        if (!IsServer) return;
        currentDangerType.Value = (int)dangerType;
    }

    /// <summary>
    /// Server clears all
    /// </summary>
    public void ClearAll()
    {
        if (!IsServer) return;
        currentLightCircuit.Value = -1;
        currentDangerType.Value = -1;
    }

    private void OnLightCircuitChanged(int previous, int current)
    {
        if (localTileManager == null) return;

        if (current >= 0)
        {
            localTileManager.SetupLights((Tile.LightCircuit)current);
        }
        else
        {
            // Clear tiles
            localTileManager.ClearAll();
        }
    }

    private void OnDangerTypeChanged(int previous, int current)
    {
        if (localTileManager == null) return;

        if (current >= 0)
        {
            localTileManager.SetupDangers((Danger.DangerType)current);
        }
    }

    private void ApplyCurrentState()
    {
        if (localTileManager == null) return;

        if (currentLightCircuit.Value >= 0)
        {
            localTileManager.SetupLights((Tile.LightCircuit)currentLightCircuit.Value);
        }

        if (currentDangerType.Value >= 0)
        {
            localTileManager.SetupDangers((Danger.DangerType)currentDangerType.Value);
        }
    }

    #region ServerRpc Wrappers (for client requests)

    [ServerRpc(RequireOwnership = false)]
    public void RequestSetLightCircuitServerRpc(int circuit)
    {
        currentLightCircuit.Value = circuit;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSetDangerTypeServerRpc(int dangerType)
    {
        currentDangerType.Value = dangerType;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestClearAllServerRpc()
    {
        ClearAll();
    }

    #endregion
}

