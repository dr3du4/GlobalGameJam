using UnityEngine;

public class MovementSpine : AbstractPlayer
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private Rigidbody rb;
    
    private Vector2 moveInput;

    override protected void MyUpdate()
    {
        // Sprawdź czy to lokalny gracz (multiplayer) lub zawsze (single-player)
        if (!InputHelper.CanPlayerMove(gameObject))
        {
            return;
        }

        // Pobierz input actions
        var inputActions = InputHelper.GetInputActionsForPlayer(gameObject);
        if (inputActions == null)
        {
            return;
        }

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        // Animacja
        SetWalking(moveInput.magnitude > 0.1f);
        UpdateAnimationDirection(moveInput);
    }

    void FixedUpdate()
    {
        // Sprawdź czy możemy się ruszać
        if (!InputHelper.CanPlayerMove(gameObject))
        {
            return;
        }
        
        // Ruch przez Rigidbody (lepsze kolizje)
        if (rb != null && moveInput.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + new Vector3(moveInput.x, 0, moveInput.y) * speed * Time.fixedDeltaTime);
            rb.linearVelocity = Vector3.zero;
        }
    }
}


