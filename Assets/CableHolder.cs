using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using Unity.Mathematics;

public class CableHolder : MonoBehaviour
{
    [Header("Cable Settings")]
    public float interactionRange = 2f;
    public SplineContainer cableSplineContainer;
    public float cableSag = 0.5f; // How much the cable hangs down
    public int splineResolution = 20; // Number of knots in the spline
    
    [Header("References")]
    public Transform cableStartPoint; // The point where cable starts from this holder
    
    private Transform player;
    private bool isPlayerNearby = false;
    private bool isCableHeld = false;
    private GameObject connectedServer = null;
    private Spline cableSpline;
    
    void Start()
    {
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Setup Spline Container
        if (cableSplineContainer == null)
        {
            GameObject splineObject = new GameObject("CableSpline");
            splineObject.transform.SetParent(transform);
            splineObject.transform.localPosition = Vector3.zero;
            cableSplineContainer = splineObject.AddComponent<SplineContainer>();
            
            // Add cable visualizer for mesh rendering
            CableVisualizer visualizer = splineObject.AddComponent<CableVisualizer>();
            visualizer.cableRadius = 0.05f;
        }
        
        // Create initial spline
        cableSpline = cableSplineContainer.Spline;
        if (cableSpline == null)
        {
            cableSpline = new Spline();
            cableSplineContainer.Spline = cableSpline;
        }
        
        // Set cable start point to this object if not assigned
        if (cableStartPoint == null)
        {
            cableStartPoint = transform;
        }
        
        // Hide spline initially
        cableSplineContainer.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Check distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isPlayerNearby = distanceToPlayer <= interactionRange;
        
        // Handle E key press
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isPlayerNearby && !isCableHeld)
            {
                PickUpCable();
            }
        }
        
        // Update cable visual if held
        if (isCableHeld)
        {
            UpdateCableVisual();
        }
    }
    
    void PickUpCable()
    {
        isCableHeld = true;
        cableSplineContainer.gameObject.SetActive(true);
        Debug.Log("Cable picked up! Take it to a Server.");
    }
    
    void UpdateCableVisual()
    {
        if (connectedServer != null)
        {
            // Cable is connected to server
            DrawCable(cableStartPoint.position, connectedServer.transform.position);
        }
        else
        {
            // Cable follows player
            DrawCable(cableStartPoint.position, player.position);
        }
    }
    
    void DrawCable(Vector3 start, Vector3 end)
    {
        // Clear existing knots
        cableSpline.Clear();
        
        // Calculate cable path with sag
        for (int i = 0; i < splineResolution; i++)
        {
            float t = i / (float)(splineResolution - 1);
            
            // Linear interpolation between start and end
            Vector3 position = Vector3.Lerp(start, end, t);
            
            // Add sag effect (parabolic curve)
            float sag = cableSag * Mathf.Sin(t * Mathf.PI);
            position.y -= sag;
            
            // Convert to local space relative to spline container
            Vector3 localPos = cableSplineContainer.transform.InverseTransformPoint(position);
            
            // Add knot to spline
            BezierKnot knot = new BezierKnot(new float3(localPos.x, localPos.y, localPos.z));
            cableSpline.Add(knot, TangentMode.AutoSmooth);
        }
    }
    
    public void ConnectToServer(GameObject server)
    {
        if (!isCableHeld) return;
        
        connectedServer = server;
        Debug.Log($"Cable connected to server: {server.name}");
        
        // Optional: Disable further interaction
        isCableHeld = false; // Can't pick up again
        
        // Notify the server that cable is connected
        ServerConnection serverComponent = server.GetComponent<ServerConnection>();
        if (serverComponent != null)
        {
            serverComponent.OnCableConnected(this);
        }
    }
    
    public void DisconnectCable()
    {
        connectedServer = null;
        if (cableSplineContainer != null)
        {
            cableSplineContainer.gameObject.SetActive(false);
        }
        isCableHeld = false;
        Debug.Log("Cable disconnected");
    }
    
    public bool IsCableHeld()
    {
        return isCableHeld;
    }
    
    public bool IsCableConnected()
    {
        return connectedServer != null;
    }
    
    // Visual feedback in editor
    void OnDrawGizmos()
    {
        Gizmos.color = isPlayerNearby ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        if (isCableHeld)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player != null ? player.position : transform.position);
        }
    }
}

