using System.Linq;
using Spine.Unity.Editor;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Danger : MonoBehaviour
{
    [SerializeField] private DangerType dangerType;
    private Tile.LightCircuit lightCircuit;
    private Tile closestTile;
    // [SerializeField] private DangerList dangerList;
    // [SerializeField] private GameObject inactiveDangerModel;
    // [SerializeField] private GameObject activeDangerModel;
    [Space]
    private Collider dangerCollider;
    [SerializeField] private MeshRenderer inactiveDangerMeshRenderer;
    [SerializeField] private MeshRenderer activeDangerMeshRenderer;
    [SerializeField] private GameObject anim2DObject;
    private bool isDangerActive = false;
    private bool isDangerVisible = false;
    private float initialScaleY;

    public DangerType Type => dangerType;
    public Tile.LightCircuit LightCircuit => lightCircuit;

    private Collider playerCollider;

    void Awake()
    {
        dangerCollider = GetComponent<Collider>();
        // (chosenDangerObject, anim2DObject) = dangerList.InstantiateRandomDanger(transform);
        if (anim2DObject != null)
        {
            anim2DObject?.SetActive(false);
        }
        
        // inactiveDangerMeshRenderer = inactiveDangerModel.GetComponent<MeshRenderer>();

    }

    void Start()
    {
        playerCollider = FindFirstObjectByType<GridMovement>()?.GetComponent<Collider>();
        closestTile = FindObjectsByType<Tile>(FindObjectsSortMode.None)
            .OrderBy(t => Vector3.Distance(t.transform.position, transform.position))
            .First();
        lightCircuit = closestTile.Circuit;
        initialScaleY = activeDangerMeshRenderer.transform.localScale.y;
        UpdateVisibility();
    }

    void Update()
    {
        // if (isDangerActive)
        // {
            float scaleY = 
                initialScaleY * (1f + 0.1f * Mathf.Sin(Time.time * 5f));
            Vector3 localScale = activeDangerMeshRenderer.transform.localScale;
            localScale.z = scaleY;
            activeDangerMeshRenderer.transform.localScale = localScale;
        // }
        // else
        // {
        //     Vector3 localScale = activeDangerMeshRenderer.transform.localScale;
        //     localScale.z = initialScaleY;
        //     activeDangerMeshRenderer.transform.localScale = localScale;
        // }
    }

    void FixedUpdate()
    {
        if (dangerCollider.enabled && dangerCollider.bounds.Intersects(playerCollider.bounds))
        {
            playerCollider.GetComponent<GridMovement>()?.HandleDangerCollision(this);
        }
    }

    public void SetDangerActive(bool isActive)
    {
        dangerCollider.enabled = isActive;
        isDangerActive = isActive;
        UpdateVisibility();
    }

    public void SetDangerVisible(bool isVisible)
    {
        // inactiveDangerMeshRenderer.enabled = isVisible;
        isDangerVisible = isVisible;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (anim2DObject != null)
        {
            anim2DObject?.SetActive(isDangerVisible && isDangerActive);
        }
        if (closestTile && closestTile.Circuit != Tile.LightCircuit.Gray)
        {
            closestTile?.gameObject?.SetActive(!isDangerVisible);
        }
        inactiveDangerMeshRenderer.enabled = !isDangerActive && isDangerVisible;
        activeDangerMeshRenderer.enabled = isDangerActive && isDangerVisible;
    }

    public enum DangerType
    {
        Fire,
        Mechanic,
        Electric,
        Toxic   
    }

    void OnDrawGizmos()
    {
        // Color gizmoColor = lightCircuit switch
        // {
        //     Tile.LightCircuit.Red => Color.red,
        //     Tile.LightCircuit.Green => Color.green,
        //     Tile.LightCircuit.Blue => Color.blue,
        //     Tile.LightCircuit.Yellow => Color.yellow,
        //     _ => Color.white
        // };
        // gizmoColor.a = 0.5f;

        // Gizmos.color = gizmoColor;
        // Vector3 pos = transform.position + new Vector3(-0.21f, 0.5f, 0f);
        // pos.y = 0.1f;
        // Gizmos.DrawCube(pos, new Vector3(0.4f, 0.01f, 0.8f));

        Color gizmoColor = dangerType switch
        {
            DangerType.Fire => Color.red,
            DangerType.Electric => Color.blue,
            DangerType.Mechanic => Color.yellow,
            DangerType.Toxic => Color.green,
            _ => Color.white
        };
        gizmoColor.a = 0.9f;
        Gizmos.color = gizmoColor;

        Gizmos.DrawCube(
            new Vector3(transform.position.x - 0.2f, 0.1f, transform.position.z), 
            new Vector3(0.4f, 0.01f, 0.2f)
        );
    }
}