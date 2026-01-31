using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Bazowy kontroler dla graczy w multiplayer.
/// Włącza odpowiedni skrypt ruchu i konfiguruje kamerę.
/// </summary>
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Movement Scripts")]
    [SerializeField] private MovementSpine movementSpine;
    [SerializeField] private GridMovement gridMovement;

    [Header("Camera Settings")]
    [SerializeField] private string operatorCameraTag = "OperatorCamera";
    [SerializeField] private string runnerCameraTag = "GridCamera";
    
    [Header("Camera Follow Settings (identyczne dla obu kamer)")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 10, -10);
    [SerializeField] private float cameraSmoothSpeed = 5f;
    [SerializeField] private bool cameraLookAtTarget = true;

    private Camera assignedCamera;
    private InputSystem_Actions inputActions;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Tylko właściciel może kontrolować tego gracza
        if (!IsOwner)
        {
            DisableNonOwnerComponents();
            return;
        }

        // Setup input
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();

        // Sprawdź rolę i włącz odpowiedni skrypt
        if (NetworkGameManager.Instance != null)
        {
            var role = NetworkGameManager.Instance.GetLocalPlayerRole();
            
            if (role == NetworkGameManager.PlayerRole.Operator)
            {
                SetupAsOperator();
            }
            else if (role == NetworkGameManager.PlayerRole.Runner)
            {
                SetupAsRunner();
            }
            else
            {
                // Fallback - wykryj po tym jaki skrypt jest na prefabie
                if (movementSpine != null && gridMovement == null)
                {
                    SetupAsOperator();
                }
                else if (gridMovement != null && movementSpine == null)
                {
                    SetupAsRunner();
                }
                else if (movementSpine != null)
                {
                    SetupAsOperator();
                }
            }
        }
        else
        {
            // Fallback bez NetworkGameManager
            if (movementSpine != null) movementSpine.enabled = true;
            if (gridMovement != null) gridMovement.enabled = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (inputActions != null)
        {
            inputActions.Player.Disable();
            inputActions.Dispose();
        }
    }

    private void SetupAsOperator()
    {
        if (movementSpine != null) movementSpine.enabled = true;
        if (gridMovement != null) gridMovement.enabled = false;
        SetupCamera(operatorCameraTag);
    }

    private void SetupAsRunner()
    {
        if (gridMovement != null) gridMovement.enabled = true;
        if (movementSpine != null) movementSpine.enabled = false;
        SetupCamera(runnerCameraTag);
    }
    
    /// <summary>
    /// Wspólna metoda do ustawiania kamery - identyczne zachowanie dla obu graczy
    /// </summary>
    private void SetupCamera(string cameraTag)
    {
        GameObject cameraObj = GameObject.FindGameObjectWithTag(cameraTag);
        
        if (cameraObj == null)
        {
            Debug.LogError($"[NetworkPlayerController] Brak kamery z tagiem: {cameraTag}");
            return;
        }

        cameraObj.SetActive(true);
        assignedCamera = cameraObj.GetComponent<Camera>();
        
        if (assignedCamera == null)
        {
            Debug.LogError($"[NetworkPlayerController] Obiekt {cameraObj.name} nie ma komponentu Camera!");
            return;
        }

        assignedCamera.enabled = true;
        
        // Dodaj lub pobierz CameraFollow
        CameraFollow follow = assignedCamera.GetComponent<CameraFollow>();
        if (follow == null)
        {
            follow = assignedCamera.gameObject.AddComponent<CameraFollow>();
        }
        
        // Ustaw IDENTYCZNE parametry dla obu kamer
        follow.target = transform;
        follow.Configure(cameraOffset, cameraSmoothSpeed, cameraLookAtTarget);
    }

    private void DisableNonOwnerComponents()
    {
        if (movementSpine != null) movementSpine.enabled = false;
        if (gridMovement != null) gridMovement.enabled = false;
    }

    /// <summary>
    /// Getter dla Input Actions (dla skryptów ruchu)
    /// </summary>
    public InputSystem_Actions GetInputActions()
    {
        return inputActions;
    }
}
