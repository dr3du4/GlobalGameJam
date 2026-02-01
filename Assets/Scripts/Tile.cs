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
        Gray,
        Red,
        Green,
        Blue,
        Yellow,
        White,
    }

    void OnDrawGizmos()
    {
        Color gizmoColor = lightCircuit switch
        {
            LightCircuit.Gray => Color.gray,
            LightCircuit.Red => Color.red,
            LightCircuit.Green => Color.green,
            LightCircuit.Blue => Color.blue,
            LightCircuit.Yellow => Color.yellow,
            LightCircuit.White => Color.white,
            _ => Color.white
        };
        gizmoColor.a = 0.5f;
        Gizmos.color = gizmoColor;
        
        Gizmos.DrawCube(
            new Vector3(transform.position.x + 0.2f, 0.1f, transform.position.z), 
            new Vector3(0.4f, 0.01f, 0.8f)
        );
    }
}
