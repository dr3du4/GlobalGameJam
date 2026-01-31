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
    public float interactionRange = 1f;
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
        
        // Only interact with the CLOSEST cable holder
        bool isClosest = IsClosestCableHolder();
        
        // Handle E key press - pick up cable
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isPlayerNearby && isClosest && !isCableHeld && connectedServer == null)
            {
                PickUpCable();
            }
        }
        
        // Handle F key press - return cable to holder
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (isPlayerNearby && isClosest && isCableHeld && connectedServer == null)
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
    
    bool IsClosestCableHolder()
    {
        if (!isPlayerNearby) return false;
        
        CableHolder[] allHolders = FindObjectsByType<CableHolder>(FindObjectsSortMode.None);
        float myDistance = Vector3.Distance(transform.position, player.position);
        
        foreach (CableHolder holder in allHolders)
        {
            if (holder == this) continue;
            
            float otherDistance = Vector3.Distance(holder.transform.position, player.position);
            if (otherDistance <= holder.interactionRange && otherDistance < myDistance)
            {
                return false; // Found a closer one
            }
        }
        
        return true; // This is the closest
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
        if (visualizer == null) return;
        
        Color color = GetColorFromEnum(cableColor);
        
        // Create new material instance to avoid changing the original asset
        if (visualizer.cableMaterial == null)
        {
            visualizer.cableMaterial = new Material(Shader.Find("Standard"));
        }
        
        // Set both base color and emission
        visualizer.cableMaterial.color = color;
        visualizer.cableMaterial.SetColor("_Color", color);
        
        // Make it slightly emissive for visibility
        visualizer.cableMaterial.EnableKeyword("_EMISSION");
        visualizer.cableMaterial.SetColor("_EmissionColor", color * 0.3f);
    }
    
    Color GetColorFromEnum(CableColor color)
    {
        switch (color)
        {
            case CableColor.Yellow:
                return new Color(1f, 0.92f, 0.016f); // Bright yellow
            case CableColor.Red:
                return new Color(1f, 0f, 0f); // Pure red
            case CableColor.Green:
                return new Color(0f, 1f, 0f); // Pure green
            case CableColor.Blue:
                return new Color(0f, 0.5f, 1f); // Bright blue
            default:
                return Color.white;
        }
    }
    
    void OnValidate()
    {
        // Update cable color when values change in inspector
        if (Application.isPlaying && cableSplineContainer != null)
        {
            CableVisualizer visualizer = cableSplineContainer.GetComponent<CableVisualizer>();
            if (visualizer != null)
            {
                UpdateCableColor(visualizer);
            }
        }
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
        
        // Brighter if this is the closest holder to player
        bool isClosest = IsClosestCableHolder();
        Gizmos.color = (isPlayerNearby && isClosest) ? gizmoColor : gizmoColor * 0.5f;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        if (isCableHeld)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(transform.position, player != null ? player.position : transform.position);
        }
    }
}

