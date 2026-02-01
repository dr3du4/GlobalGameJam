using UnityEngine;

public class LastTile : MonoBehaviour
{
    Collider playerCollider;
    Collider tileCollider;
    void Start()
    {
        playerCollider = FindFirstObjectByType<GridMovement>()?.GetComponent<Collider>();
        tileCollider = GetComponent<Collider>();
        // Debug.Log($"{playerCollider}    {tileCollider}"); 
    }
    void FixedUpdate()
    {
        if (tileCollider.bounds.Intersects(playerCollider.bounds))
        {
            GameManager.instance.NextLevel();
        }
    }
}