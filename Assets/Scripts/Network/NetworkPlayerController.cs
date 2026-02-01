using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Bazowy kontroler dla graczy w multiplayer.
/// Włącza odpowiedni skrypt ruchu i aktywuje kamerę.
/// Kamery są już skonfigurowane w scenie przez programistę.
/// </summary>
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Movement Scripts")]
    [SerializeField] private MovementSpine movementSpine;
    [SerializeField] private GridMovement gridMovement;

    [Header("Camera Settings")]
    [SerializeField] private string operatorCameraTag = "OperatorCamera";
    [SerializeField] private string runnerCameraTag = "GridCamera";

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
    /// Aktywuje odpowiednią kamerę - kamery są już skonfigurowane w scenie.
    /// </summary>
    private void SetupCamera(string cameraTag)
    {
        GameObject cameraObj = GameObject.FindGameObjectWithTag(cameraTag);
        
        if (cameraObj == null)
        {
            Debug.LogWarning($"[NetworkPlayerController] Brak kamery z tagiem: {cameraTag} - używam domyślnej");
            return;
        }

        cameraObj.SetActive(true);
        assignedCamera = cameraObj.GetComponent<Camera>();
        
        if (assignedCamera != null)
        {
            assignedCamera.enabled = true;
            Debug.Log($"[NetworkPlayerController] ✅ Aktywowano kamerę: {cameraObj.name}");
        }
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
