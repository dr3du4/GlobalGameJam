using UnityEngine;

/// <summary>
/// Prosta kamera śledząca gracza.
/// Automatycznie dodawana przez NetworkPlayerController.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -10);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool lookAtTarget = true;

    private void LateUpdate()
    {
        if (target == null) return;

        // Oblicz docelową pozycję
        Vector3 desiredPosition = target.position + offset;

        // Smooth follow
        if (smoothSpeed > 0)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = desiredPosition;
        }

        // Patrz na gracza
        if (lookAtTarget)
        {
            transform.LookAt(target);
        }
    }

    /// <summary>
    /// Ustaw offset kamery (np. dla różnych perspektyw)
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    /// <summary>
    /// Ustaw smooth speed (0 = instant)
    /// </summary>
    public void SetSmoothSpeed(float speed)
    {
        smoothSpeed = speed;
    }
    
    /// <summary>
    /// Ustaw czy kamera ma patrzeć na target
    /// </summary>
    public void SetLookAtTarget(bool lookAt)
    {
        lookAtTarget = lookAt;
    }
    
    /// <summary>
    /// Konfiguruj wszystkie parametry naraz
    /// </summary>
    public void Configure(Vector3 newOffset, float newSmoothSpeed, bool newLookAtTarget)
    {
        offset = newOffset;
        smoothSpeed = newSmoothSpeed;
        lookAtTarget = newLookAtTarget;
    }
}


