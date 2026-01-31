using Spine.Unity.Editor;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Danger : MonoBehaviour
{
    [SerializeField] private DangerType dangerType;
    [SerializeField] private Tile.LightCircuit lightCircuit;
    [Space]
    [SerializeField] private Collider dangerCollider;
    [SerializeField] private MeshRenderer dangerMeshRenderer;

    public DangerType Type => dangerType;
    public Tile.LightCircuit LightCircuit => lightCircuit;

    private Collider playerCollider;

    void Awake()
    {
        dangerCollider = GetComponent<Collider>();
        dangerMeshRenderer = GetComponent<MeshRenderer>();
        playerCollider = FindFirstObjectByType<MovementSpine>()?.GetComponent<Collider>();
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
        Electric
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
        Gizmos.DrawCube(transform.position + new Vector3(-0.21f, 0.5f, 0f), new Vector3(0.4f, 0.01f, 0.8f));

        gizmoColor = dangerType switch
        {
            DangerType.Fire => Color.red,
            DangerType.Water => Color.blue,
            DangerType.Electric => Color.yellow,
            _ => Color.white
        };
        gizmoColor.a = 0.9f;

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position + new Vector3(-0.21f, 0.6f, 0f), new Vector3(0.2f, 0.01f, 0.6f));
    }
}