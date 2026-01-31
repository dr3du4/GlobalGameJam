using UnityEngine;

public class MovementSpine : AbstractPlayer
{
    [SerializeField] private float speed = 5f;
    
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

        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();

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
