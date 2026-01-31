using UnityEngine;

public class GridMovement : AbstractPlayer
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private Vector2 gridOrigin = Vector2.zero; // Bottom-left corner of grid in world space
    
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Awake()
    {
        SetDefaultAnim();
        targetPosition = transform.position;
    }

    override protected void MyUpdate()
    {
        if (!isMoving)
        {
            SetWalking(false);
            Vector2 input = GameManager.instance.inputActions.Player.Move.ReadValue<Vector2>();
            
            // Only move in one direction at a time (X-Z plane)
            Vector3 moveDirection = Vector3.zero;
            if (Mathf.Abs(input.x) > 0.5f)
                moveDirection = new Vector3(Mathf.Sign(input.x), 0, 0);
            else if (Mathf.Abs(input.y) > 0.5f)
                moveDirection = new Vector3(0, 0, Mathf.Sign(input.y));
            
            if (moveDirection != Vector3.zero)
            {
                Vector3 newPosition = transform.position + moveDirection * gridSize;
                
                // Calculate actual world bounds from grid dimensions
                float minX = gridOrigin.x;
                float maxX = gridOrigin.x + (gridWidth - 1) * gridSize;
                float minZ = gridOrigin.y;
                float maxZ = gridOrigin.y + (gridHeight - 1) * gridSize;
                
                // Check if new position is within bounds
                if (newPosition.x >= minX && newPosition.x <= maxX &&
                    newPosition.z >= minZ && newPosition.z <= maxZ)
                {
                    targetPosition = newPosition;
                    isMoving = true;
                }
            }
        }
        else
        {
            SetWalking(true);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            // Debug.Log($"{transform.position}");
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition; // Snap to exact position
                isMoving = false;
            }
        }
        Vector3 moveDirection3D = (targetPosition - transform.position).normalized;
        UpdateAnimationDirection(new Vector2(moveDirection3D.x, moveDirection3D.z));
    }

    public void HandleDangerCollision(Danger danger)
    {
        SetDefaultAnim();
        Debug.Log($"Collided with danger of type: {danger.Type} on circuit: {danger.LightCircuit}");
    }
}