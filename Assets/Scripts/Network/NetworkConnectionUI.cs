using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.InputSystem;

/// <summary>
/// Uniwersalne UI dla połączeń - działa z Steam (kod) i IP/Port.
/// Automatycznie wykrywa tryb na podstawie transportu.
/// </summary>
public class NetworkConnectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Steam Mode (kod)")]
    [SerializeField] private GameObject steamPanel;
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TextMeshProUGUI lobbyCodeDisplay;

    [Header("IP Mode (fallback)")]
    [SerializeField] private GameObject ipPanel;
    [SerializeField] private TMP_InputField ipAddressInput;
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private string defaultIP = "127.0.0.1";
    [SerializeField] private ushort defaultPort = 7777;
    
    [Header("Local Testing (wymusza IP mode)")]
    [SerializeField] private bool forceIPMode = false;
    [SerializeField] private Toggle ipModeToggle;

    [Header("Waiting/Lobby Panel")]
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private TextMeshProUGUI waitingText;
    [SerializeField] private Button leaveButton;

    private bool useSteam = false;
    private bool steamCheckDone = false;

    private void Start()
    {
        // Setup przycisków
        if (hostButton != null) hostButton.onClick.AddListener(OnHostClicked);
        if (joinButton != null) joinButton.onClick.AddListener(OnJoinClicked);
        if (leaveButton != null) leaveButton.onClick.AddListener(OnLeaveClicked);
        if (ipModeToggle != null) ipModeToggle.onValueChanged.AddListener(OnIPModeToggleChanged);

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        // Opóźnione sprawdzenie Steam (daj czas FacepunchTransport)
        ShowConnectionPanel();
        
        // Jeśli wymuszony IP mode, pomiń sprawdzanie Steam
        if (forceIPMode)
        {
            useSteam = false;
            SetupUI();
            SetStatus("🔧 Tryb IP (wymuszony dla testów lokalnych)");
        }
        else
        {
            SetStatus("Sprawdzam Steam...");
            Invoke(nameof(DelayedSteamCheck), 0.5f);
        }
    }
    
    private void OnIPModeToggleChanged(bool isOn)
    {
        forceIPMode = isOn;
        if (isOn)
        {
            useSteam = false;
            SetupUI();
            SetStatus("🔧 Tryb IP (dla testów lokalnych)");
        }
        else
        {
            steamCheckDone = false;
            DelayedSteamCheck();
        }
    }

    private void DelayedSteamCheck()
    {
        // Sprawdź czy używamy FacepunchTransport
        bool hasFacepunchTransport = false;
        string transportName = "brak";
        
        if (NetworkManager.Singleton != null)
        {
            var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (transport != null)
            {
                transportName = transport.GetType().Name;
                hasFacepunchTransport = transportName.Contains("Facepunch");
            }
        }

        // Debug info
        Debug.Log($"[NetworkConnectionUI] Transport: {transportName}");
        Debug.Log($"[NetworkConnectionUI] SteamManager.Initialized: {SteamManager.Initialized}");
        Debug.Log($"[NetworkConnectionUI] SteamLobbyManager.Instance: {(SteamLobbyManager.Instance != null ? "OK" : "NULL")}");
        Debug.Log($"[NetworkConnectionUI] Steam User: {SteamManager.GetMyName()}");

        // Sprawdź czy Steam jest zainicjalizowany
        useSteam = hasFacepunchTransport && SteamManager.Initialized && SteamLobbyManager.Instance != null;

        // Jeśli mamy FacepunchTransport ale Steam jeszcze nie gotowy - sprawdź ponownie (max 3 razy)
        if (hasFacepunchTransport && !SteamManager.Initialized && !steamCheckDone)
        {
            steamCheckDone = true;
            SetStatus("Czekam na Steam...");
            Debug.Log("[NetworkConnectionUI] Steam nie gotowy, czekam...");
            Invoke(nameof(DelayedSteamCheck), 1f);
            return;
        }

        if (!useSteam && hasFacepunchTransport)
        {
            Debug.LogWarning("[NetworkConnectionUI] ⚠️ FacepunchTransport jest, ale Steam NIE działa!");
            Debug.LogWarning("[NetworkConnectionUI] Sprawdź czy Steam jest uruchomiony i zrestartuj Unity.");
        }

        // Subskrybuj eventy Steam jeśli dostępny
        if (useSteam && SteamLobbyManager.Instance != null)
        {
            SteamLobbyManager.Instance.OnLobbyCreated += OnSteamLobbyCreated;
            SteamLobbyManager.Instance.OnLobbyJoined += OnSteamLobbyJoined;
            SteamLobbyManager.Instance.OnLobbyFailed += OnSteamLobbyFailed;
            SteamLobbyManager.Instance.OnLobbyLeft += OnSteamLobbyLeft;
        }

        SetupUI();
    }

    private void OnDestroy()
    {
        if (hostButton != null) hostButton.onClick.RemoveListener(OnHostClicked);
        if (joinButton != null) joinButton.onClick.RemoveListener(OnJoinClicked);
        if (leaveButton != null) leaveButton.onClick.RemoveListener(OnLeaveClicked);
        if (ipModeToggle != null) ipModeToggle.onValueChanged.RemoveListener(OnIPModeToggleChanged);

        if (SteamLobbyManager.Instance != null)
        {
            SteamLobbyManager.Instance.OnLobbyCreated -= OnSteamLobbyCreated;
            SteamLobbyManager.Instance.OnLobbyJoined -= OnSteamLobbyJoined;
            SteamLobbyManager.Instance.OnLobbyFailed -= OnSteamLobbyFailed;
            SteamLobbyManager.Instance.OnLobbyLeft -= OnSteamLobbyLeft;
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void SetupUI()
    {
        // Pokaż odpowiedni panel
        if (steamPanel != null) steamPanel.SetActive(useSteam);
        if (ipPanel != null) ipPanel.SetActive(!useSteam);

        // Ustaw domyślne wartości dla IP
        if (!useSteam)
        {
            if (ipAddressInput != null) ipAddressInput.text = defaultIP;
            if (portInput != null) portInput.text = defaultPort.ToString();
        }

        // Status
        if (useSteam)
        {
            SetStatus($"Steam: {SteamManager.GetMyName()}");
        }
        else
        {
            SetStatus("Tryb IP (Steam niedostępny)");
        }

        ShowConnectionPanel();
    }

    #region Button Handlers

    private void OnHostClicked()
    {
        SetStatus("Tworzenie gry...");
        SetButtonsInteractable(false);

        if (useSteam)
        {
            SteamLobbyManager.Instance.CreateLobby();
        }
        else
        {
            StartIPHost();
        }
    }

    private void OnJoinClicked()
    {
        if (useSteam)
        {
            string code = codeInput != null ? codeInput.text.Trim().ToUpperInvariant() : "";
            
            if (string.IsNullOrEmpty(code))
            {
                SetStatus("Wpisz kod!");
                return;
            }

            if (!LobbyCodeHelper.IsValidCode(code))
            {
                SetStatus("Nieprawidłowy kod!");
                return;
            }

            SetStatus($"Dołączanie: {code}...");
            SetButtonsInteractable(false);
            SteamLobbyManager.Instance.JoinLobbyWithCode(code);
        }
        else
        {
            StartIPClient();
        }
    }

    private void OnLeaveClicked()
    {
        if (useSteam && SteamLobbyManager.Instance != null)
        {
            SteamLobbyManager.Instance.LeaveLobby();
        }
        else if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        ShowConnectionPanel();
        SetStatus(useSteam ? $"Steam: {SteamManager.GetMyName()}" : "Rozłączono");
        SetButtonsInteractable(true);
    }

    #endregion

    #region IP Mode

    private void StartIPHost()
    {
        ConfigureIPTransport();

        if (NetworkManager.Singleton.StartHost())
        {
            SetStatus("Hosting! Czekam na gracza...");
            ShowWaitingPanel("Jesteś RUNNEREM\nCzekam na Operatora...");
        }
        else
        {
            SetStatus("Błąd hostowania!");
            SetButtonsInteractable(true);
        }
    }

    private void StartIPClient()
    {
        SetStatus("Łączenie...");
        SetButtonsInteractable(false);
        ConfigureIPTransport();

        if (NetworkManager.Singleton.StartClient())
        {
            ShowWaitingPanel("Łączenie z serwerem...");
        }
        else
        {
            SetStatus("Błąd połączenia!");
            SetButtonsInteractable(true);
        }
    }

    private void ConfigureIPTransport()
    {
        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        
        if (transport == null)
        {
            Debug.LogError("[NetworkConnectionUI] ❌ Brak UnityTransport! Dodaj go do NetworkManager dla trybu IP.");
            Debug.LogError("[NetworkConnectionUI] W Unity: NetworkManager → Add Component → Unity Transport");
            SetStatus("❌ Brak UnityTransport!");
            return;
        }
        
        // Ustaw UnityTransport jako aktywny transport
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = transport;
        
        string ip = ipAddressInput != null ? ipAddressInput.text : defaultIP;
        ushort port = defaultPort;
        
        if (portInput != null && ushort.TryParse(portInput.text, out ushort parsedPort))
        {
            port = parsedPort;
        }

        Debug.Log($"[NetworkConnectionUI] IP Mode: {ip}:{port}");
        transport.SetConnectionData(ip, port);
    }

    #endregion

    #region Steam Events

    private void OnSteamLobbyCreated(string code)
    {
        if (lobbyCodeDisplay != null)
        {
            lobbyCodeDisplay.text = code;
            lobbyCodeDisplay.gameObject.SetActive(true);
        }

        ShowWaitingPanel($"Jesteś RUNNEREM\n\nKod: {code}\n\nCzekam na Operatora...");
    }

    private void OnSteamLobbyJoined(string hostName)
    {
        ShowWaitingPanel($"Dołączono do: {hostName}\nJesteś OPERATOREM");
        Invoke(nameof(HideAllPanels), 2f);
    }

    private void OnSteamLobbyFailed(string error)
    {
        SetStatus($"❌ {error}");
        SetButtonsInteractable(true);
        ShowConnectionPanel();
    }

    private void OnSteamLobbyLeft()
    {
        ShowConnectionPanel();
        SetButtonsInteractable(true);
    }

    #endregion

    #region Network Events

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
            
            if (playerCount >= 2)
            {
                SetStatus("Gra rozpoczęta!");
                Invoke(nameof(HideAllPanels), 1f);
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            SetStatus("Rozłączono!");
            ShowConnectionPanel();
            SetButtonsInteractable(true);
        }
    }

    #endregion

    #region UI Helpers

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
        Debug.Log($"[UI] {message}");
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (hostButton != null) hostButton.interactable = interactable;
        if (joinButton != null) joinButton.interactable = interactable;
    }

    private void ShowConnectionPanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(true);
        if (waitingPanel != null) waitingPanel.SetActive(false);
        if (lobbyCodeDisplay != null) lobbyCodeDisplay.gameObject.SetActive(false);
        SetButtonsInteractable(true);
    }

    private void ShowWaitingPanel(string message)
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
        if (waitingPanel != null) waitingPanel.SetActive(true);
        if (waitingText != null) waitingText.text = message;
    }

    private void HideAllPanels()
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
        if (waitingPanel != null) waitingPanel.SetActive(false);
    }

    #endregion

    #region Keyboard Shortcuts

    private void Update()
    {
        if (Keyboard.current == null || NetworkManager.Singleton == null) return;

        // Tylko gdy nie połączony
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                OnHostClicked();
            }
            else if (Keyboard.current.jKey.wasPressedThisFrame)
            {
                OnJoinClicked();
            }
        }

        // ESC - rozłącz
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
            {
                OnLeaveClicked();
            }
        }
    }

    #endregion
}
