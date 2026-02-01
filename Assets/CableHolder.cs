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
    public float cableSag = 0.5f;
    public int splineResolution = 20;
    
    [Header("References")]
    public Transform cableStartPoint;
    public string emissiveMaterialName = "emmisive";
    public CableVisualizer templateVisualizer;
    
    private Transform player;
    private bool isPlayerNearby = false;
    private bool isCableHeld = false;
    private GameObject connectedServer = null;
    private Spline cableSpline;
    private Material emissiveMaterial;
    
    void Start()
    {
        // Znajdź gracza - Operatora (ten z MovementSpine)
        FindOperatorPlayer();
        
        // Setup Spline Container
        if (cableSplineContainer == null)
        {
            GameObject splineObject = new GameObject("CableSpline");
            splineObject.transform.SetParent(transform);
            splineObject.transform.localPosition = Vector3.zero;
            cableSplineContainer = splineObject.AddComponent<SplineContainer>();
            
            CableVisualizer visualizer = splineObject.AddComponent<CableVisualizer>();
            visualizer.cableRadius = 0.05f;
            
            if (templateVisualizer != null)
            {
                visualizer.yellowCableMaterial = templateVisualizer.yellowCableMaterial;
                visualizer.redCableMaterial = templateVisualizer.redCableMaterial;
                visualizer.greenCableMaterial = templateVisualizer.greenCableMaterial;
                visualizer.blueCableMaterial = templateVisualizer.blueCableMaterial;
            }
        }
        
        cableSpline = cableSplineContainer.Spline;
        if (cableSpline == null)
        {
            cableSpline = new Spline();
            cableSplineContainer.Spline = cableSpline;
        }
        
        if (cableStartPoint == null)
        {
            cableStartPoint = transform;
        }
        
        cableSplineContainer.gameObject.SetActive(false);
        FindAndUpdateEmissiveMaterial();
    }
    
    void FindOperatorPlayer()
    {
        // Szukaj LOKALNEGO gracza - sprawdź NetworkObject.IsOwner
        var networkObjects = FindObjectsByType<Unity.Netcode.NetworkObject>(FindObjectsSortMode.None);
        Debug.Log($"[CableHolder] Szukam lokalnego gracza, znalazłem {networkObjects.Length} NetworkObject");
        
        foreach (var netObj in networkObjects)
        {
            // Sprawdź czy to nasz gracz (IsOwner) i czy ma MovementSpine (Operator)
            if (netObj.IsOwner)
            {
                var movement = netObj.GetComponent<MovementSpine>();
                if (movement != null && movement.enabled)
                {
                    Debug.Log($"[CableHolder] ✅ Znalazłem lokalnego Operatora: {netObj.gameObject.name}");
                    player = netObj.transform;
                    return;
                }
            }
        }
        
        // Fallback dla single player - szukaj aktywnego MovementSpine
        MovementSpine[] operators = FindObjectsByType<MovementSpine>(FindObjectsSortMode.None);
        foreach (var op in operators)
        {
            if (op.enabled)
            {
                Debug.Log($"[CableHolder] Fallback SP - znalazłem: {op.gameObject.name}");
                player = op.transform;
                return;
            }
        }
        
        Debug.LogWarning("[CableHolder] ❌ Nie znaleziono Operatora! (Kable są tylko dla Operatora)");
    }
    
    void FindAndUpdateEmissiveMaterial()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.name.ToLower().Contains("emmisive") || mat.name.ToLower().Contains("emissive"))
                {
                    emissiveMaterial = mat;
                    UpdateHolderEmissiveColor();
                    return;
                }
            }
        }
    }
    
    void UpdateHolderEmissiveColor()
    {
        if (emissiveMaterial == null) return;
        
        Color color = GetColorFromEnum(cableColor);
        emissiveMaterial.color = color;
        emissiveMaterial.SetColor("_Color", color);
        emissiveMaterial.EnableKeyword("_EMISSION");
        emissiveMaterial.SetColor("_EmissionColor", color * 2f);
    }
    
    void Update()
    {
        // Jeśli nie ma gracza, spróbuj znaleźć ponownie
        if (player == null)
        {
            FindOperatorPlayer();
            if (player == null) return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isPlayerNearby = distanceToPlayer <= interactionRange;
        
        bool isClosest = IsClosestCableHolder();
        
        // E - podniesienie kabla
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log($"[CableHolder] E pressed! isPlayerNearby={isPlayerNearby}, isClosest={isClosest}, isCableHeld={isCableHeld}, connectedServer={connectedServer != null}");
            if (isPlayerNearby && isClosest && !isCableHeld && connectedServer == null)
            {
                Debug.Log("[CableHolder] ✅ Podbieram kabel!");
                PickUpCable();
            }
        }
        
        // F - odłożenie kabla
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (isPlayerNearby && isClosest && isCableHeld && connectedServer == null)
            {
                ReturnCableToHolder();
            }
        }
        
        if (isCableHeld || connectedServer != null)
        {
            UpdateCableVisual();
        }
    }
    
    bool IsClosestCableHolder()
    {
        if (!isPlayerNearby || player == null) return false;
        
        CableHolder[] allHolders = FindObjectsByType<CableHolder>(FindObjectsSortMode.None);
        float myDistance = Vector3.Distance(transform.position, player.position);
        
        foreach (CableHolder holder in allHolders)
        {
            if (holder == this) continue;
            
            float otherDistance = Vector3.Distance(holder.transform.position, player.position);
            if (otherDistance <= holder.interactionRange && otherDistance < myDistance)
            {
                return false;
            }
        }
        
        return true;
    }
    
    void PickUpCable()
    {
        isCableHeld = true;
        cableSplineContainer.gameObject.SetActive(true);
    }
    
    void UpdateCableVisual()
    {
        if (player == null) return;
        
        if (connectedServer != null)
        {
            DrawCable(cableStartPoint.position, connectedServer.transform.position);
        }
        else
        {
            DrawCable(cableStartPoint.position, player.position);
        }
    }
    
    void DrawCable(Vector3 start, Vector3 end)
    {
        cableSpline.Clear();
        
        for (int i = 0; i < splineResolution; i++)
        {
            float t = i / (float)(splineResolution - 1);
            Vector3 position = Vector3.Lerp(start, end, t);
            float sag = cableSag * Mathf.Sin(t * Mathf.PI);
            position.y -= sag;
            
            Vector3 localPos = cableSplineContainer.transform.InverseTransformPoint(position);
            BezierKnot knot = new BezierKnot(new float3(localPos.x, localPos.y, localPos.z));
            cableSpline.Add(knot, TangentMode.AutoSmooth);
        }
    }
    
    public void ConnectToServer(GameObject server)
    {
        if (!isCableHeld) return;
        
        ServerConnection serverComponent = server.GetComponent<ServerConnection>();
        if (serverComponent == null) return;
        
        if (serverComponent.serverColor != cableColor)
        {
            return;
        }
        
        connectedServer = server;
        isCableHeld = false;
        
        serverComponent.OnCableConnected(this);
    }
    
    public void DisconnectFromServer()
    {
        if (connectedServer != null)
        {
            connectedServer = null;
            isCableHeld = true;
        }
    }
    
    public void ReturnCableToHolder()
    {
        if (cableSplineContainer != null)
        {
            cableSplineContainer.gameObject.SetActive(false);
        }
        isCableHeld = false;
        connectedServer = null;
    }
    
    public void DisconnectCable()
    {
        connectedServer = null;
        if (cableSplineContainer != null)
        {
            cableSplineContainer.gameObject.SetActive(false);
        }
        isCableHeld = false;
    }
    
    public bool IsCableHeld() => isCableHeld;
    public bool IsCableConnected() => connectedServer != null;
    public CableColor GetCableColor() => cableColor;
    
    Color GetColorFromEnum(CableColor color)
    {
        return color switch
        {
            CableColor.Yellow => new Color(1f, 0.92f, 0.016f),
            CableColor.Red => new Color(1f, 0f, 0f),
            CableColor.Green => new Color(0f, 1f, 0f),
            CableColor.Blue => new Color(0f, 0.5f, 1f),
            _ => Color.white
        };
    }
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            FindAndUpdateEmissiveMaterial();
        }
    }
    
    void OnDrawGizmos()
    {
        Color gizmoColor = GetColorFromEnum(cableColor);
        
        bool isClosest = IsClosestCableHolder();
        Gizmos.color = (isPlayerNearby && isClosest) ? gizmoColor : gizmoColor * 0.5f;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        if (isCableHeld && player != null)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
