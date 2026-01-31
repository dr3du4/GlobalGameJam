using Spine.Unity.Editor;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Danger : MonoBehaviour
{
    [SerializeField] private DangerType dangerType;
    [SerializeField] private Tile.LightCircuit lightCircuit;
    [SerializeField] private DangerList dangerList;
    [SerializeField] private GameObject chosenDangerObject;
    [Space]
    private Collider dangerCollider;
    private MeshRenderer dangerMeshRenderer;

    public DangerType Type => dangerType;
    public Tile.LightCircuit LightCircuit => lightCircuit;

    private Collider playerCollider;

    void Awake()
    {
        dangerCollider = GetComponent<Collider>();
        dangerMeshRenderer = GetComponent<MeshRenderer>();
        chosenDangerObject = dangerList.InstantiateRandomDanger(transform);
        dangerMeshRenderer = chosenDangerObject.GetComponent<MeshRenderer>();
    }

    void Start()
    {
        playerCollider = FindFirstObjectByType<GridMovement>()?.GetComponent<Collider>();
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
    }

    public void SetDangerVisible(bool isVisible)
    {
        dangerMeshRenderer.enabled = isVisible;
    }

    public enum DangerType
    {
        Fire,
        Water,
        Electric,
        Toxic   
    }

    void OnDrawGizmos()
    {
        Color gizmoColor = lightCircuit switch
        {
            Tile.LightCircuit.Red => Color.red,
            Tile.LightCircuit.Green => Color.green,
            Tile.LightCircuit.Blue => Color.blue,
            Tile.LightCircuit.Yellow => Color.yellow,
            _ => Color.white
        };
        gizmoColor.a = 0.5f;

        Gizmos.color = gizmoColor;
        Vector3 pos = transform.position + new Vector3(-0.21f, 0.5f, 0f);
        pos.y = 0.1f;
        Gizmos.DrawCube(pos, new Vector3(0.4f, 0.01f, 0.8f));

        gizmoColor = dangerType switch
        {
            DangerType.Fire => Color.red,
            DangerType.Water => Color.blue,
            DangerType.Electric => Color.yellow,
            _ => Color.white
        };
        gizmoColor.a = 0.9f;

        Gizmos.color = gizmoColor;
        pos.y = 0.1f;
        Gizmos.DrawCube(pos, new Vector3(0.2f, 0.01f, 0.6f));
    }
}