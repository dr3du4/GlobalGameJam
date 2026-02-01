using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private LightCircuit lightCircuit;
    [SerializeField] private float whiteGlowIntensity = 8f;
    [SerializeField] private float redGlowIntensity = 40f;
    [SerializeField] private float greenGlowIntensity = 30f;
    [SerializeField] private float blueGlowIntensity = 40f;
    [SerializeField] private float yellowGlowIntensity = 20f;
    [Space]
    [SerializeField] private MeshRenderer tileMeshRenderer;
    private Material tileMaterial;
    private Material glowingTileMaterial;

    public LightCircuit Circuit => lightCircuit;
    private float glowIntensity => lightCircuit switch
    {
        LightCircuit.Red => redGlowIntensity,
        LightCircuit.Green => greenGlowIntensity,
        LightCircuit.Blue => blueGlowIntensity,
        LightCircuit.Yellow => yellowGlowIntensity,
        LightCircuit.White => whiteGlowIntensity,
        _ => 0f
    };
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

    void Update()
    {
        if (lightCircuit == LightCircuit.White)
        {
            float emission = 1 + Mathf.PingPong(Time.time * glowIntensity, glowIntensity);
            Color baseColor = Color.white;
            glowingTileMaterial.SetColor("_EmissionColor", baseColor * emission * glowIntensity);
        }
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
