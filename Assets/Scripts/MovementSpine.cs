using UnityEngine;

public class MovementSpine : AbstractPlayer
{
    [SerializeField] private float speed = 5f;
    override protected void MyUpdate()
    {
        Vector2 moveInput = GameManager.instance.inputActions.Player.Move.ReadValue<Vector2>();

        if (GameManager.instance.isCableEnjoyerChosen)
        {
            SetWalking(moveInput.magnitude > 0.1f);
            transform.Translate(new Vector3(moveInput.x, 0, moveInput.y) * speed * Time.deltaTime);
            UpdateAnimationDirection(moveInput);
        }
    }
}
