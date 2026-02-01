using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Główny manager sieciowy.
/// - Przydziela role (Runner = Host, Operator = Client)
/// - Zarządza timerem gry
/// - Spawnuje graczy w odpowiednich strefach
/// </summary>
public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    [Header("Spawn Points")]
    [SerializeField] private Transform operatorSpawnPoint;
    [SerializeField] private Transform runnerSpawnPoint;
    
    [Header("Player Prefabs")]
    [SerializeField] private GameObject operatorPrefab;
    [SerializeField] private GameObject runnerPrefab;

    [Header("Timer")]
    [SerializeField] private float gameDuration = 300f; // 5 minut

    [Header("Events")]
    public UnityEvent OnGameStarted;
    public UnityEvent OnGameEnded;
    public UnityEvent<float> OnTimerUpdated;

    // Zmienne synchronizowane przez sieć
    private NetworkVariable<float> serverStartTime = new NetworkVariable<float>();
    private NetworkVariable<float> totalGameDuration = new NetworkVariable<float>();
    private NetworkVariable<bool> isGameActive = new NetworkVariable<bool>();
    private NetworkVariable<ulong> runnerClientId = new NetworkVariable<ulong>();
    private NetworkVariable<ulong> operatorClientId = new NetworkVariable<ulong>();

    private Dictionary<ulong, PlayerRole> playerRoles = new Dictionary<ulong, PlayerRole>();
    private bool rolesAssigned = false;

    public enum PlayerRole
    {
        None,
        Runner,     // Host - chodzi po mapie
        Operator    // Client - łączy kable
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            totalGameDuration.Value = gameDuration;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Host dostaje rolę Runner
            AssignRole(NetworkManager.Singleton.LocalClientId);
        }

        isGameActive.OnValueChanged += OnGameActiveChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        isGameActive.OnValueChanged -= OnGameActiveChanged;
    }

    #region Role Management

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[NetworkGameManager] 🔌 Klient połączony! ClientId: {clientId}, IsServer: {IsServer}");
        
        if (!IsServer) return;

        Debug.Log($"[NetworkGameManager] Przydzielam rolę dla clientId: {clientId}");
        AssignRole(clientId);

        Debug.Log($"[NetworkGameManager] Liczba graczy: {playerRoles.Count}, GameActive: {isGameActive.Value}");
        Debug.Log($"[NetworkGameManager] RunnerClientId: {runnerClientId.Value}, OperatorClientId: {operatorClientId.Value}");
        
        // Start gdy obaj gracze połączeni
        if (playerRoles.Count >= 2 && !isGameActive.Value)
        {
            Debug.Log("[NetworkGameManager] 🎮 Startujemy grę! Obaj gracze połączeni.");
            StartGame();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        if (playerRoles.ContainsKey(clientId))
        {
            playerRoles.Remove(clientId);
        }

        if (isGameActive.Value && playerRoles.Count < 2)
        {
            EndGame();
        }
    }

    private void AssignRole(ulong clientId)
    {
        if (playerRoles.ContainsKey(clientId)) return;

        PlayerRole role;
        // Pierwszy gracz (Host) = Runner
        // Drugi gracz (Client) = Operator
        if (!rolesAssigned || playerRoles.Count == 0)
        {
            role = PlayerRole.Runner;
            runnerClientId.Value = clientId;
            rolesAssigned = true;
        }
        else
        {
            role = PlayerRole.Operator;
            operatorClientId.Value = clientId;
        }

        playerRoles[clientId] = role;

        NotifyRoleClientRpc(clientId, role);
        SpawnPlayerForRole(clientId, role);
    }

    private void SpawnPlayerForRole(ulong clientId, PlayerRole role)
    {
        if (!IsServer) return;

        Debug.Log($"[NetworkGameManager] 🎭 Spawnuję gracza: ClientId={clientId}, Role={role}");

        Transform spawnPoint = role == PlayerRole.Runner ? runnerSpawnPoint : operatorSpawnPoint;
        GameObject prefab = role == PlayerRole.Runner ? runnerPrefab : operatorPrefab;

        if (prefab == null)
        {
            Debug.LogError($"[NetworkGameManager] ❌ Brak prefaba dla {role}! Ustaw go w inspektorze.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError($"[NetworkGameManager] ❌ Brak spawn pointu dla {role}! Ustaw go w inspektorze.");
            return;
        }

        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        Debug.Log($"[NetworkGameManager] Spawn position: {spawnPosition}, Prefab: {prefab.name}");

        GameObject playerObj = Instantiate(prefab, spawnPosition, spawnRotation);
        playerObj.name = $"{role}Player_{clientId}";

        NetworkObject netObj = playerObj.GetComponent<NetworkObject>();
        
        if (netObj != null)
        {
            netObj.SpawnAsPlayerObject(clientId);
            Debug.Log($"[NetworkGameManager] ✅ Gracz {role} zespawnowany dla clientId {clientId}!");
        }
        else
        {
            Debug.LogError($"[NetworkGameManager] ❌ Prefab {prefab.name} nie ma komponentu NetworkObject!");
            Destroy(playerObj);
        }
    }

    [ClientRpc]
    private void NotifyRoleClientRpc(ulong targetClientId, PlayerRole role)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            OnRoleAssigned(role);
        }
    }

    private void OnRoleAssigned(PlayerRole role)
    {
        // Można rozszerzyć w przyszłości
    }

    #endregion

    #region Game Timer

    private void StartGame()
    {
        if (!IsServer) return;

        serverStartTime.Value = Time.time;
        isGameActive.Value = true;
        
        StartGameClientRpc();
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        OnGameStarted?.Invoke();
    }

    private void Update()
    {
        if (!isGameActive.Value) return;

        float remainingTime = GetRemainingTime();
        OnTimerUpdated?.Invoke(remainingTime);

        if (IsServer && remainingTime <= 0)
        {
            EndGame();
        }
    }

    public float GetRemainingTime()
    {
        if (!isGameActive.Value) return totalGameDuration.Value;
        
        float elapsed = Time.time - serverStartTime.Value;
        return Mathf.Max(0, totalGameDuration.Value - elapsed);
    }

    private void EndGame()
    {
        if (!IsServer) return;

        isGameActive.Value = false;
        EndGameClientRpc();
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        OnGameEnded?.Invoke();
    }

    private void OnGameActiveChanged(bool previous, bool current)
    {
        // Można dodać logikę UI tutaj
    }

    #endregion

    #region Public API

    public PlayerRole GetLocalPlayerRole()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        
        if (localId == runnerClientId.Value) return PlayerRole.Runner;
        if (localId == operatorClientId.Value) return PlayerRole.Operator;
        
        return PlayerRole.None;
    }

    public bool IsLocalPlayerOperator()
    {
        return NetworkManager.Singleton.LocalClientId == operatorClientId.Value;
    }

    public bool IsLocalPlayerRunner()
    {
        return NetworkManager.Singleton.LocalClientId == runnerClientId.Value;
    }

    public bool IsGameActive => isGameActive.Value;
    public ulong RunnerClientId => runnerClientId.Value;
    public ulong OperatorClientId => operatorClientId.Value;

    #endregion
}
