using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.InputSystem;

/// <summary>
/// Simple UI for hosting/joining a multiplayer session.
/// Designed for quick setup in game jam conditions.
/// </summary>
public class NetworkConnectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField ipAddressInput;
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Waiting UI")]
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private TextMeshProUGUI waitingText;

    [Header("Settings")]
    [SerializeField] private string defaultIP = "127.0.0.1";
    [SerializeField] private ushort defaultPort = 7777;

    private void Start()
    {
        // Setup button listeners
        if (hostButton != null)
        {
            hostButton.onClick.AddListener(OnHostClicked);
        }

        if (clientButton != null)
        {
            clientButton.onClick.AddListener(OnClientClicked);
        }

        // Set defaults
        if (ipAddressInput != null)
        {
            ipAddressInput.text = defaultIP;
        }

        if (portInput != null)
        {
            portInput.text = defaultPort.ToString();
        }

        // Subscribe to network events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        ShowConnectionPanel();
    }

    private void OnDestroy()
    {
        if (hostButton != null)
        {
            hostButton.onClick.RemoveListener(OnHostClicked);
        }

        if (clientButton != null)
        {
            clientButton.onClick.RemoveListener(OnClientClicked);
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnHostClicked()
    {
        SetStatus("Starting host...");

        // Configure transport
        ConfigureTransport();

        // Start as host (server + client)
        if (NetworkManager.Singleton.StartHost())
        {
            SetStatus("Hosting! Waiting for player 2...");
            ShowWaitingPanel("Jesteś Runnerem!\nCzekam na Operatora...");
        }
        else
        {
            SetStatus("Failed to start host!");
        }
    }

    private void OnClientClicked()
    {
        SetStatus("Connecting...");

        // Configure transport
        ConfigureTransport();

        // Start as client
        if (NetworkManager.Singleton.StartClient())
        {
            SetStatus("Connecting to server...");
            ShowWaitingPanel("Łączenie z serwerem...");
        }
        else
        {
            SetStatus("Failed to connect!");
        }
    }

    private void ConfigureTransport()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        
        if (transport != null)
        {
            string ip = ipAddressInput != null ? ipAddressInput.text : defaultIP;
            ushort port = defaultPort;
            
            if (portInput != null && ushort.TryParse(portInput.text, out ushort parsedPort))
            {
                port = parsedPort;
            }

            transport.SetConnectionData(ip, port);
            Debug.Log($"[NetworkConnectionUI] Configured transport: {ip}:{port}");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[NetworkConnectionUI] Client {clientId} connected");

        if (NetworkManager.Singleton.IsHost)
        {
            // Host sees new player
            int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
            
            if (playerCount >= 2)
            {
                SetStatus("Obaj gracze połączeni! Gra się rozpoczyna...");
                HideAllPanels();
            }
            else
            {
                ShowWaitingPanel($"Jesteś Runnerem!\nCzekam na Operatora... ({playerCount}/2)");
            }
        }
        else if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // This client just connected
            SetStatus("Połączono! Jesteś Operatorem!");
            ShowWaitingPanel("Jesteś Operatorem!\nCzekam na start gry...");
            
            // Hide UI after brief moment
            Invoke(nameof(HideAllPanels), 2f);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[NetworkConnectionUI] Client {clientId} disconnected");

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            // We got disconnected
            SetStatus("Rozłączono!");
            ShowConnectionPanel();
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[NetworkConnectionUI] {message}");
    }

    private void ShowConnectionPanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(true);
        if (waitingPanel != null) waitingPanel.SetActive(false);
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

    #region Quick Actions (for testing)

    /// <summary>
    /// Quick host for testing (keyboard shortcut H)
    /// </summary>
    private void Update()
    {
        if (Keyboard.current == null) return;
        if (NetworkManager.Singleton == null) return;

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                OnHostClicked();
            }
            else if (Keyboard.current.jKey.wasPressedThisFrame)
            {
                OnClientClicked();
            }
        }

        // Debug disconnect
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.Shutdown();
                ShowConnectionPanel();
            }
        }
    }

    #endregion
}

