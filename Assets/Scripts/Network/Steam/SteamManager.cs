using UnityEngine;

#if FACEPUNCH_STEAMWORKS
using Steamworks;
#endif

/// <summary>
/// Inicjalizuje Steam SDK. Musi być na scenie przed użyciem Steam API.
/// UWAGA: Jeśli używasz FacepunchTransport, on sam inicjalizuje Steam.
/// W takim przypadku SteamManager tylko śledzi stan.
/// </summary>
public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }
    public static bool Initialized { get; private set; }

    [Header("Settings")]
    [SerializeField] private uint appId = 480; // Testowe App ID
    [SerializeField] private bool initializeSteam = false; // FALSE jeśli używasz FacepunchTransport!

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (initializeSteam)
        {
            InitializeSteam();
        }
        else
        {
            // FacepunchTransport zainicjalizuje Steam - tylko sprawdzamy stan
            CheckSteamStatus();
        }
    }

    private void InitializeSteam()
    {
#if FACEPUNCH_STEAMWORKS
        // Sprawdź czy Steam już nie jest zainicjalizowany (przez FacepunchTransport)
        try
        {
            if (SteamClient.IsValid)
            {
                Initialized = true;
                Debug.Log($"[SteamManager] ✅ Steam już zainicjalizowany! User: {SteamClient.Name}");
                return;
            }
        }
        catch { }

        try
        {
            SteamClient.Init(appId, true);
            Initialized = true;
            Debug.Log($"[SteamManager] ✅ Steam zainicjalizowany! User: {SteamClient.Name}");
        }
        catch (System.Exception e)
        {
            Initialized = false;
            Debug.LogError($"[SteamManager] ❌ Błąd inicjalizacji Steam: {e.Message}");
        }
#else
        Initialized = false;
        Debug.LogWarning("[SteamManager] ⚠️ Facepunch.Steamworks nie zainstalowany!");
#endif
    }

    private void CheckSteamStatus()
    {
#if FACEPUNCH_STEAMWORKS
        // Czekamy aż FacepunchTransport zainicjalizuje Steam
        Invoke(nameof(DelayedSteamCheck), 0.5f);
#else
        Initialized = false;
#endif
    }

    private void DelayedSteamCheck()
    {
#if FACEPUNCH_STEAMWORKS
        try
        {
            if (SteamClient.IsValid)
            {
                Initialized = true;
                Debug.Log($"[SteamManager] ✅ Steam gotowy! User: {SteamClient.Name}");
            }
            else
            {
                Initialized = false;
                Debug.LogWarning("[SteamManager] Steam nie zainicjalizowany przez transport");
            }
        }
        catch
        {
            Initialized = false;
        }
#endif
    }

    private void Update()
    {
#if FACEPUNCH_STEAMWORKS
        // Sprawdzaj status Steam jeśli jeszcze nie zainicjalizowany
        if (!Initialized)
        {
            try
            {
                if (SteamClient.IsValid)
                {
                    Initialized = true;
                    Debug.Log($"[SteamManager] ✅ Steam gotowy! User: {SteamClient.Name}");
                }
            }
            catch { }
        }

        // RunCallbacks tylko jeśli sami inicjalizowaliśmy
        if (Initialized && initializeSteam)
        {
            SteamClient.RunCallbacks();
        }
#endif
    }

    private void OnApplicationQuit()
    {
        Shutdown();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Shutdown();
        }
    }

    private void Shutdown()
    {
#if FACEPUNCH_STEAMWORKS
        // Zamykamy Steam tylko jeśli sami go inicjalizowaliśmy
        if (Initialized && initializeSteam)
        {
            SteamClient.Shutdown();
            Initialized = false;
            Debug.Log("[SteamManager] Steam zamknięty");
        }
#endif
    }

    public static ulong GetMySteamId()
    {
#if FACEPUNCH_STEAMWORKS
        try
        {
            if (SteamClient.IsValid)
                return SteamClient.SteamId;
        }
        catch { }
#endif
        return 0;
    }

    public static string GetMyName()
    {
#if FACEPUNCH_STEAMWORKS
        try
        {
            if (SteamClient.IsValid)
                return SteamClient.Name;
        }
        catch { }
#endif
        return "Player";
    }
}
