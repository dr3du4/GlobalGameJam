using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using Unity.Mathematics;

public enum CableColor
{
    Yellow,
    Red,
    Green,
    Blue
}

public class CableHolder : MonoBehaviour
{
    [Header("Cable Settings")]
    public float interactionRange = 2f;
    public CableColor cableColor = CableColor.Yellow;
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
            
            // Set cable color based on holder color
            UpdateCableColor(visualizer);
        }
        
        // Create initial spline
        cableSpline = cableSplineContainer.Spline;
        if (cableSpline == null)
        {
            cableSpline = new Spline();
            cableSplineContainer.Spline = cableSpline;
        }
        
        // Update color if visualizer already exists
        CableVisualizer existingVisualizer = cableSplineContainer.GetComponent<CableVisualizer>();
        if (existingVisualizer != null)
        {
            UpdateCableColor(existingVisualizer);
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
        
        // Handle E key press - pick up cable
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isPlayerNearby && !isCableHeld && connectedServer == null)
            {
                PickUpCable();
            }
        }
        
        // Handle F key press - return cable to holder
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (isPlayerNearby && isCableHeld && connectedServer == null)
            {
                // Player is holding cable and near holder - hang it back
                ReturnCableToHolder();
            }
        }
        
        // Update cable visual if held or connected
        if (isCableHeld || connectedServer != null)
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
        
        // Check if server has matching color
        ServerConnection serverComponent = server.GetComponent<ServerConnection>();
        if (serverComponent == null) return;
        
        if (serverComponent.serverColor != cableColor)
        {
            Debug.LogWarning($"Cable color mismatch! Cable is {cableColor}, but server is {serverComponent.serverColor}");
            return;
        }
        
        connectedServer = server;
        isCableHeld = false; // No longer held by player, connected to server
        Debug.Log($"Cable ({cableColor}) connected to server: {server.name}");
        
        // Notify the server that cable is connected
        serverComponent.OnCableConnected(this);
    }
    
    public void DisconnectFromServer()
    {
        // Called when player presses F at server - picks up cable from server
        if (connectedServer != null)
        {
            connectedServer = null;
            isCableHeld = true; // Player now holds the cable
            Debug.Log("Cable disconnected from server. Bring it back to holder.");
        }
    }
    
    public void ReturnCableToHolder()
    {
        // Player returns cable to holder
        if (cableSplineContainer != null)
        {
            cableSplineContainer.gameObject.SetActive(false);
        }
        isCableHeld = false;
        connectedServer = null;
        Debug.Log("Cable returned to holder.");
    }
    
    public void DisconnectCable()
    {
        // Complete disconnect (used when resetting)
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
    
    public CableColor GetCableColor()
    {
        return cableColor;
    }
    
    void UpdateCableColor(CableVisualizer visualizer)
    {
        if (visualizer == null || visualizer.cableMaterial == null) return;
        
        Color color = Color.white;
        switch (cableColor)
        {
            case CableColor.Yellow:
                color = Color.yellow;
                break;
            case CableColor.Red:
                color = Color.red;
                break;
            case CableColor.Green:
                color = Color.green;
                break;
            case CableColor.Blue:
                color = Color.blue;
                break;
        }
        
        visualizer.cableMaterial.color = color;
    }
    
    // Visual feedback in editor
    void OnDrawGizmos()
    {
        // Draw gizmo with cable color
        Color gizmoColor = Color.white;
        switch (cableColor)
        {
            case CableColor.Yellow:
                gizmoColor = Color.yellow;
                break;
            case CableColor.Red:
                gizmoColor = Color.red;
                break;
            case CableColor.Green:
                gizmoColor = Color.green;
                break;
            case CableColor.Blue:
                gizmoColor = Color.blue;
                break;
        }
        
        Gizmos.color = isPlayerNearby ? gizmoColor : gizmoColor * 0.5f;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        if (isCableHeld)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(transform.position, player != null ? player.position : transform.position);
        }
    }
}

