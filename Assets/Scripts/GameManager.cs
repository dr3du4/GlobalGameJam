using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string nextLevelName = "Level2";
    public bool useFirstMap { get; private set; } = true;
    public static GameManager instance;
    // public RoomGenerator roomGenerator;
    public InputSystem_Actions inputActions;

    public Camera mainCamera;
    public Camera secondCamera;

    public bool isCableEnjoyerChosen { get; private set; } = true;

    private bool isNextLevelCalled = false;
    void Awake()
    {
        instance = this;
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += OnSpacePressed;
        SwitchToCableEnjoyer();
    }

    void Start()
    {
        MusicManager.I?.PlayGameplayMusic();
    }

    void OnSpacePressed(InputAction.CallbackContext context)
    {
        SwitchPlayer();
    }

    void SwitchPlayer()
    {
        if (isCableEnjoyerChosen)
        {
            SwitchToGridWalker();
        }
        else
        {
            SwitchToCableEnjoyer();
        }
    }

    void SwitchToCableEnjoyer()
    {
        Debug.Log("Use first map: " + useFirstMap);
        useFirstMap = true;
        isCableEnjoyerChosen = true;
        mainCamera.enabled = true;
        secondCamera.enabled = false;
        if (FindFirstObjectByType<MovementSpine>() is MovementSpine movementSpine) movementSpine.enabled = true;
        if (FindFirstObjectByType<GridMovement>() is GridMovement gridMovement) gridMovement.enabled = false;
    }

    void SwitchToGridWalker()
    {
        Debug.Log("Use first map: " + useFirstMap);
        useFirstMap = false;
        isCableEnjoyerChosen = false;
        mainCamera.enabled = false;
        secondCamera.enabled = true;
        if (FindFirstObjectByType<GridMovement>() is GridMovement gridMovement) gridMovement.enabled = true;
        if (FindFirstObjectByType<MovementSpine>() is MovementSpine movementSpine) movementSpine.enabled = false;
    }

    public void DeathHandler()
    {
        StartCoroutine(DeathCoroutine());
        MusicManager.I?.PlaySFX(MusicManager.I.looseMusicSource);
    }

    private IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Restarting level...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        if (!isNextLevelCalled)
        {
            isNextLevelCalled = true;
            StartCoroutine(NextLevelCoroutine());
            MusicManager.I?.PlaySFX(MusicManager.I.winMusicSource);
        }
    }

    private IEnumerator NextLevelCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(nextLevelName);
    }
}
