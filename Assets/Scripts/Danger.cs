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
    private GameObject anim2DObject;
    private bool isDangerActive = false;
    private bool isDangerVisible = false;
    private float initialScaleY;

    public DangerType Type => dangerType;
    public Tile.LightCircuit LightCircuit => lightCircuit;

    private Collider playerCollider;

    void Awake()
    {
        dangerCollider = GetComponent<Collider>();
        dangerMeshRenderer = GetComponent<MeshRenderer>();
        (chosenDangerObject, anim2DObject) = dangerList.InstantiateRandomDanger(transform);
        anim2DObject?.gameObject.SetActive(false);
        dangerMeshRenderer = chosenDangerObject.GetComponent<MeshRenderer>();
        initialScaleY = chosenDangerObject.transform.localScale.y;
    }

    void Start()
    {
        playerCollider = FindFirstObjectByType<GridMovement>()?.GetComponent<Collider>();
    }

    void Update()
    {
        if (isDangerActive)
        {
            float scaleY = 
                initialScaleY * (1f + 0.1f * Mathf.Sin(Time.time * 5f));
            Vector3 localScale = chosenDangerObject.transform.localScale;
            localScale.z = scaleY;
            chosenDangerObject.transform.localScale = localScale;
        }
        else
        {
            Vector3 localScale = chosenDangerObject.transform.localScale;
            localScale.z = initialScaleY;
            chosenDangerObject.transform.localScale = localScale;
        }
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
        anim2DObject?.SetActive(isDangerVisible && isDangerActive);
    }

    public void SetDangerVisible(bool isVisible)
    {
        dangerMeshRenderer.enabled = isVisible;
        isDangerVisible = isVisible;
        anim2DObject?.SetActive(isDangerVisible && isDangerActive);
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
            DangerType.Electric => Color.blue,
            DangerType.Mechanic => Color.yellow,
            DangerType.Toxic => Color.green,
            _ => Color.white
        };
        gizmoColor.a = 0.9f;

        Gizmos.color = gizmoColor;
        pos.y = 0.1f;
        Gizmos.DrawCube(pos, new Vector3(0.2f, 0.01f, 0.6f));
    }
}