using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI do hostowania/dołączania przez Steam Lobby z kodami.
/// </summary>
public class SteamConnectionUI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Lobby Panel (po utworzeniu/dołączeniu)")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private TextMeshProUGUI lobbyInfoText;
    [SerializeField] private Button leaveButton;

    [Header("Settings")]
    [SerializeField] private float hideUIDelay = 3f;

    private void Start()
    {
        if (hostButton != null)
            hostButton.onClick.AddListener(OnHostClicked);
        
        if (joinButton != null)
            joinButton.onClick.AddListener(OnJoinClicked);
        
        if (leaveButton != null)
            leaveButton.onClick.AddListener(OnLeaveClicked);

        if (SteamLobbyManager.Instance != null)
        {
            SteamLobbyManager.Instance.OnLobbyCreated += OnLobbyCreated;
            SteamLobbyManager.Instance.OnLobbyJoined += OnLobbyJoined;
            SteamLobbyManager.Instance.OnLobbyFailed += OnLobbyFailed;
            SteamLobbyManager.Instance.OnLobbyLeft += OnLobbyLeft;
        }

        ShowConnectionPanel();
        
        // Sprawdź czy Steam działa
        if (!SteamManager.Initialized)
        {
            SetStatus("⚠️ Steam nie zainicjalizowany");
        }
        else
        {
            SetStatus($"Zalogowano: {SteamManager.GetMyName()}");
        }
    }

    private void OnDestroy()
    {
        if (hostButton != null)
            hostButton.onClick.RemoveListener(OnHostClicked);
        
        if (joinButton != null)
            joinButton.onClick.RemoveListener(OnJoinClicked);
        
        if (leaveButton != null)
            leaveButton.onClick.RemoveListener(OnLeaveClicked);

        if (SteamLobbyManager.Instance != null)
        {
            SteamLobbyManager.Instance.OnLobbyCreated -= OnLobbyCreated;
            SteamLobbyManager.Instance.OnLobbyJoined -= OnLobbyJoined;
            SteamLobbyManager.Instance.OnLobbyFailed -= OnLobbyFailed;
            SteamLobbyManager.Instance.OnLobbyLeft -= OnLobbyLeft;
        }
    }

    #region Button Handlers

    private void OnHostClicked()
    {
        if (!CheckSteam()) return;

        SetStatus("Tworzenie lobby...");
        SetButtonsInteractable(false);
        
        SteamLobbyManager.Instance.CreateLobby();
    }

    private void OnJoinClicked()
    {
        if (!CheckSteam()) return;

        string code = codeInput != null ? codeInput.text.Trim().ToUpperInvariant() : "";
        
        if (string.IsNullOrEmpty(code))
        {
            SetStatus("Wpisz kod lobby!");
            return;
        }

        if (!LobbyCodeHelper.IsValidCode(code))
        {
            SetStatus("Nieprawidłowy kod!");
            return;
        }

        SetStatus($"Dołączanie do {code}...");
        SetButtonsInteractable(false);
        
        SteamLobbyManager.Instance.JoinLobbyWithCode(code);
    }

    private void OnLeaveClicked()
    {
        SteamLobbyManager.Instance?.LeaveLobby();
    }

    #endregion

    #region Lobby Events

    private void OnLobbyCreated(string code)
    {
        ShowLobbyPanel();
        
        if (lobbyCodeText != null)
            lobbyCodeText.text = code;
        
        if (lobbyInfoText != null)
            lobbyInfoText.text = "Jesteś HOSTEM (Runner)\nCzekam na gracza...";

        SetStatus($"Lobby: {code}");
    }

    private void OnLobbyJoined(string hostName)
    {
        ShowLobbyPanel();
        
        if (lobbyCodeText != null)
            lobbyCodeText.text = SteamLobbyManager.Instance?.CurrentLobbyCode ?? "???";
        
        if (lobbyInfoText != null)
            lobbyInfoText.text = $"Dołączono do: {hostName}\nJesteś OPERATOREM";

        SetStatus("Połączono!");

        Invoke(nameof(HideAllPanels), hideUIDelay);
    }

    private void OnLobbyFailed(string error)
    {
        SetStatus($"❌ {error}");
        SetButtonsInteractable(true);
        ShowConnectionPanel();
    }

    private void OnLobbyLeft()
    {
        ShowConnectionPanel();
        SetStatus("Rozłączono");
        SetButtonsInteractable(true);
    }

    #endregion

    #region UI Helpers

    private bool CheckSteam()
    {
        if (!SteamManager.Initialized)
        {
            SetStatus("❌ Steam nie jest uruchomiony!");
            return false;
        }

        if (SteamLobbyManager.Instance == null)
        {
            SetStatus("❌ Brak SteamLobbyManager!");
            return false;
        }

        return true;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
        
        Debug.Log($"[SteamUI] {message}");
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (hostButton != null) hostButton.interactable = interactable;
        if (joinButton != null) joinButton.interactable = interactable;
    }

    private void ShowConnectionPanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        SetButtonsInteractable(true);
    }

    private void ShowLobbyPanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
    }

    private void HideAllPanels()
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
    }

    #endregion

    #region Keyboard Shortcuts

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (SteamLobbyManager.Instance != null && SteamLobbyManager.Instance.IsInLobby)
            {
                OnLeaveClicked();
            }
        }
    }

    #endregion
}
