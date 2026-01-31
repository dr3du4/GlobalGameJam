using UnityEngine;

public class MovementSpine : AbstractPlayer
{
    [SerializeField] private float speed = 5f;
    
    private int debugCounter = 0;
    
    override protected void MyUpdate()
    {
        debugCounter++;
        
        // Sprawdź czy to lokalny gracz
        if (!InputHelper.CanPlayerMove(gameObject))
        {
            if (debugCounter % 120 == 0) Debug.LogWarning($"[MovementSpine] CanPlayerMove = false dla {gameObject.name}");
            return;
        }

        // Pobierz input actions
        var inputActions = InputHelper.GetInputActionsForPlayer(gameObject);
        if (inputActions == null)
        {
            if (debugCounter % 120 == 0) Debug.LogWarning($"[MovementSpine] Brak Input Actions dla {gameObject.name}!");
            return;
        }

        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        
        // Debug każdy input większy od 0
        if (moveInput.magnitude > 0.1f)
        {
            Debug.Log($"[MovementSpine] Input: {moveInput}");
        }

        // Ruch
        if (moveInput.magnitude > 0.1f)
        {
            SetWalking(true);
            transform.Translate(new Vector3(moveInput.x, 0, moveInput.y) * speed * Time.deltaTime);
            UpdateAnimationDirection(moveInput);
        }
        else
        {
            SetWalking(false);
        }
    }
}
