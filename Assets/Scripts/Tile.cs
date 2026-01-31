using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private LightCircuit lightCircuit;
    [SerializeField] private float glowIntensity = 50f;
    [Space]
    [SerializeField] private MeshRenderer tileMeshRenderer;
    private Material tileMaterial;
    private Material glowingTileMaterial;

    public LightCircuit Circuit => lightCircuit;

    void Awake()
    {
        tileMaterial = tileMeshRenderer.materials[1];
        glowingTileMaterial = new Material(tileMeshRenderer.materials[1]);
        glowingTileMaterial.EnableKeyword("_EMISSION");
        Color color = lightCircuit switch
        {
            LightCircuit.Red => Color.red,
            LightCircuit.Green => Color.green,
            LightCircuit.Blue => Color.blue,
            LightCircuit.Yellow => Color.yellow,
            _ => Color.white
        };
        glowingTileMaterial.SetColor("_EmissionColor", color * glowIntensity);
    } 

    public void SetTileVisible(bool isVisible)
    {
        Material[] materials = tileMeshRenderer.materials;
        materials[1] = isVisible ? glowingTileMaterial : tileMaterial;
        tileMeshRenderer.materials = materials;
    }

    public enum LightCircuit
    {
        Red,
        Green,
        Blue,
        Yellow
    }

    void OnDrawGizmos()
    {
        Color gizmoColor = lightCircuit switch
        {
            LightCircuit.Red => Color.red,
            LightCircuit.Green => Color.green,
            LightCircuit.Blue => Color.blue,
            LightCircuit.Yellow => Color.yellow,
            _ => Color.white
        };
        gizmoColor.a = 0.5f;

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position + new Vector3(0.21f, 0.5f, 0f), new Vector3(0.4f, 0.01f, 0.8f));
    }
}
