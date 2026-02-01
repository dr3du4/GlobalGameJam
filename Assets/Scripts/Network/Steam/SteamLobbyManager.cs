using System;
using UnityEngine;
using Unity.Netcode;

#if FACEPUNCH_STEAMWORKS
using Steamworks;
using Steamworks.Data;
using Netcode.Transports.Facepunch;
#endif

/// <summary>
/// Zarządza Steam Lobby - tworzenie, dołączanie, i połączenie z Netcode.
/// WYMAGA: Facepunch.Steamworks i Netcode.Transports.Facepunch
/// </summary>
public class SteamLobbyManager : MonoBehaviour
{
    public static SteamLobbyManager Instance { get; private set; }

    [Header("Lobby Settings")]
    [SerializeField] private int maxPlayers = 2;

    // Events
    public event Action<string> OnLobbyCreated;
    public event Action<string> OnLobbyJoined;
    public event Action<string> OnLobbyFailed;
    public event Action OnLobbyLeft;

    // Current state
    public string CurrentLobbyCode { get; private set; }
    public bool IsHost { get; private set; }
    public bool IsInLobby { get; private set; }

#if FACEPUNCH_STEAMWORKS
    private Lobby? currentLobby;
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

#if FACEPUNCH_STEAMWORKS
    private void Start()
    {
        SteamMatchmaking.OnLobbyMemberJoined += OnMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnMemberLeft;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
    }

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyMemberJoined -= OnMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnMemberLeft;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
    }
#endif

    #region Host

    public async void CreateLobby()
    {
#if FACEPUNCH_STEAMWORKS
        if (!SteamManager.Initialized)
        {
            OnLobbyFailed?.Invoke("Steam nie jest zainicjalizowany!");
            return;
        }

        try
        {
            var lobbyResult = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);

            if (!lobbyResult.HasValue)
            {
                OnLobbyFailed?.Invoke("Nie udało się utworzyć lobby!");
                return;
            }

            currentLobby = lobbyResult.Value;
            IsHost = true;
            IsInLobby = true;

            currentLobby.Value.SetPublic();
            currentLobby.Value.SetJoinable(true);
            currentLobby.Value.SetData("game", "GlobalGameJam2025");

            CurrentLobbyCode = LobbyCodeHelper.LobbyIdToCode(currentLobby.Value.Id);

            Debug.Log($"[SteamLobby] ✅ Lobby utworzone! ID: {currentLobby.Value.Id}, Kod: {CurrentLobbyCode}");

            // Sprawdź FacepunchTransport
            var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            if (transport != null)
            {
                Debug.Log($"[SteamLobby] FacepunchTransport znaleziony, mój SteamId: {SteamClient.SteamId}");
            }
            else
            {
                Debug.LogError("[SteamLobby] BRAK FacepunchTransport na NetworkManager!");
            }

            Debug.Log("[SteamLobby] Uruchamiam Host...");
            NetworkManager.Singleton.StartHost();
            Debug.Log($"[SteamLobby] Host uruchomiony! IsServer={NetworkManager.Singleton.IsServer}, IsHost={NetworkManager.Singleton.IsHost}");
            OnLobbyCreated?.Invoke(CurrentLobbyCode);
        }
        catch (Exception e)
        {
            OnLobbyFailed?.Invoke($"Błąd tworzenia lobby: {e.Message}");
        }
#else
        OnLobbyFailed?.Invoke("Steam SDK nie zainstalowany!");
#endif
    }

    #endregion

    #region Join

    public async void JoinLobbyWithCode(string code)
    {
#if FACEPUNCH_STEAMWORKS
        if (!SteamManager.Initialized)
        {
            OnLobbyFailed?.Invoke("Steam nie jest zainicjalizowany!");
            return;
        }

        if (!LobbyCodeHelper.IsValidCode(code))
        {
            OnLobbyFailed?.Invoke("Nieprawidłowy kod lobby!");
            return;
        }

        try
        {
            ulong lobbyId = LobbyCodeHelper.CodeToLobbyId(code);
            Debug.Log($"[SteamLobby] Próba dołączenia do lobby: {lobbyId}");

            var lobby = new Lobby(lobbyId);
            var result = await lobby.Join();

            if (result != RoomEnter.Success)
            {
                OnLobbyFailed?.Invoke($"Nie udało się dołączyć: {result}");
                return;
            }

            currentLobby = lobby;
            CurrentLobbyCode = code.ToUpperInvariant();
            IsHost = false;
            IsInLobby = true;

            Debug.Log($"[SteamLobby] ✅ Dołączono do lobby!");
            
            // OnLobbyEntered zostanie wywołane automatycznie
        }
        catch (Exception e)
        {
            OnLobbyFailed?.Invoke($"Błąd dołączania: {e.Message}");
        }
#else
        OnLobbyFailed?.Invoke("Steam SDK nie zainstalowany!");
#endif
    }

#if FACEPUNCH_STEAMWORKS
    private void OnLobbyEntered(Lobby lobby)
    {
        Debug.Log($"[SteamLobby] OnLobbyEntered wywołane! IsHost={IsHost}, LobbyId={lobby.Id}");
        
        if (IsHost)
        {
            Debug.Log("[SteamLobby] Jestem hostem - ignoruję OnLobbyEntered");
            return;
        }

        var hostId = lobby.Owner.Id;
        Debug.Log($"[SteamLobby] 🎯 Host: {lobby.Owner.Name} ({hostId})");
        Debug.Log($"[SteamLobby] Mój SteamId: {SteamClient.SteamId}");

        // Pobierz FacepunchTransport bezpośrednio
        var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
        
        if (transport == null)
        {
            Debug.LogError("[SteamLobby] ❌ Nie znaleziono FacepunchTransport na NetworkManager!");
            OnLobbyFailed?.Invoke("Brak FacepunchTransport!");
            return;
        }

        Debug.Log($"[SteamLobby] ✅ FacepunchTransport znaleziony");
        Debug.Log($"[SteamLobby] Ustawiam targetSteamId na: {hostId}");
        transport.targetSteamId = hostId;

        Debug.Log($"[SteamLobby] 🚀 Uruchamiam klienta Netcode...");
        
        // Dodaj callback na połączenie
        NetworkManager.Singleton.OnClientConnectedCallback += OnNetcodeClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnNetcodeClientDisconnected;
        NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
        
        bool started = NetworkManager.Singleton.StartClient();
        Debug.Log($"[SteamLobby] StartClient() zwróciło: {started}");
        Debug.Log($"[SteamLobby] IsClient={NetworkManager.Singleton.IsClient}, IsConnectedClient={NetworkManager.Singleton.IsConnectedClient}");
        
        if (started)
        {
            OnLobbyJoined?.Invoke(lobby.Owner.Name);
        }
        else
        {
            Debug.LogError("[SteamLobby] ❌ StartClient() nie powiodło się!");
            OnLobbyFailed?.Invoke("Nie udało się uruchomić klienta!");
        }
    }
    
    private void OnTransportFailure()
    {
        Debug.LogError("[SteamLobby] 💥 TRANSPORT FAILURE! Połączenie nie powiodło się.");
    }
    
    private void OnNetcodeClientConnected(ulong clientId)
    {
        Debug.Log($"[SteamLobby] 🔗 Netcode: Klient połączony! ClientId: {clientId}");
    }
    
    private void OnNetcodeClientDisconnected(ulong clientId)
    {
        Debug.Log($"[SteamLobby] ⚠️ Netcode: Klient rozłączony! ClientId: {clientId}");
    }
#endif

    #endregion

    #region Leave

    public void LeaveLobby()
    {
#if FACEPUNCH_STEAMWORKS
        if (currentLobby.HasValue)
        {
            currentLobby.Value.Leave();
            currentLobby = null;
        }
#endif
        CurrentLobbyCode = null;
        IsHost = false;
        IsInLobby = false;

        Debug.Log("[SteamLobby] Opuszczono lobby");

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        OnLobbyLeft?.Invoke();
    }

    #endregion

#if FACEPUNCH_STEAMWORKS
    #region Events

    private void OnMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log($"[SteamLobby] 👤 {friend.Name} ({friend.Id}) dołączył do lobby Steam!");
        Debug.Log($"[SteamLobby] Aktualna liczba członków w lobby: {lobby.MemberCount}");
    }

    private void OnMemberLeft(Lobby lobby, Friend friend)
    {
        Debug.Log($"[SteamLobby] 👋 {friend.Name} opuścił lobby");
    }

    #endregion
#endif
}
