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

        Debug.Log($"[NetworkPlayerController] abc OnNetworkSpawn dla {gameObject.name}, IsOwner={IsOwner}, OwnerClientId={OwnerClientId}");

        // Tylko właściciel może kontrolować tego gracza
        if (!IsOwner)
        {
            // Wyłącz wszystko dla innych graczy
            DisableNonOwnerComponents();
            return;
        }

        // Setup input
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        Debug.Log($"[NetworkPlayerController] ✅ Input Actions utworzone i włączone dla gracza {OwnerClientId}");

        // Sprawdź rolę i włącz odpowiedni skrypt
        if (NetworkGameManager.Instance != null)
        {
            var role = NetworkGameManager.Instance.GetLocalPlayerRole();
            Debug.Log($"[NetworkPlayerController] Rola gracza {OwnerClientId}: {role}");
            
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
                Debug.LogWarning($"[NetworkPlayerController] ⚠️ Rola = None! Próbuję wykryć po prefabie...");
                // Fallback - wykryj po tym jaki skrypt jest na prefabie
                if (movementSpine != null && gridMovement == null)
                {
                    Debug.Log("[NetworkPlayerController] Wykryto MovementSpine - ustawiam jako Operator");
                    SetupAsOperator();
                }
                else if (gridMovement != null && movementSpine == null)
                {
                    Debug.Log("[NetworkPlayerController] Wykryto GridMovement - ustawiam jako Runner");
                    SetupAsRunner();
                }
                else if (movementSpine != null)
                {
                    // Oba przypisane - domyślnie Operator dla prefaba CableEnjoyer
                    Debug.Log("[NetworkPlayerController] Oba skrypty - domyślnie Operator");
                    SetupAsOperator();
                }
            }
        }
        else
        {
            Debug.LogError("[NetworkPlayerController] ❌ NetworkGameManager.Instance jest NULL!");
            // Fallback bez NetworkGameManager
            if (movementSpine != null)
            {
                movementSpine.enabled = true;
                Debug.Log("[NetworkPlayerController] Fallback: włączam MovementSpine");
            }
            if (gridMovement != null)
            {
                gridMovement.enabled = true;
                Debug.Log("[NetworkPlayerController] Fallback: włączam GridMovement");
            }
        }

        Debug.Log($"[NetworkPlayerController] Gracz {OwnerClientId} gotowy do gry!");
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
        Debug.Log("[NetworkPlayerController] ▶️ Setup jako OPERATOR");

        // Włącz ruch Operatora
        if (movementSpine != null)
        {
            movementSpine.enabled = true;
            Debug.Log($"[NetworkPlayerController] ✅ MovementSpine włączony (enabled={movementSpine.enabled})");
        }
        else
        {
            Debug.LogError("[NetworkPlayerController] ❌ MovementSpine nie przypisany w prefabie!");
        }
        
        if (gridMovement != null)
        {
            gridMovement.enabled = false;
        }

        // Znajdź i włącz kamerę Operatora
        SetupCamera(operatorCameraTag);
    }

    private void SetupAsRunner()
    {
        Debug.Log("[NetworkPlayerController] Setup jako RUNNER");

        // Włącz ruch Runnera
        if (gridMovement != null)
        {
            gridMovement.enabled = true;
        }
        else
        {
            Debug.LogWarning("[NetworkPlayerController] GridMovement nie przypisany!");
        }
        
        if (movementSpine != null)
        {
            movementSpine.enabled = false;
        }

        // Znajdź i włącz kamerę Runnera
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
        
        Debug.Log($"[NetworkPlayerController] Kamera {cameraObj.name} gotowa (offset={cameraOffset})");
    }

    private void DisableNonOwnerComponents()
    {
        // Wyłącz skrypty ruchu dla graczy którymi nie sterujemy
        if (movementSpine != null) movementSpine.enabled = false;
        if (gridMovement != null) gridMovement.enabled = false;
    }

    /// <summary>
    /// Getter dla Input Actions (dla skryptów ruchu)
    /// </summary>
    public InputSystem_Actions GetInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogWarning($"[NetworkPlayerController] GetInputActions() zwraca NULL dla {gameObject.name}!");
        }
        return inputActions;
    }
}

