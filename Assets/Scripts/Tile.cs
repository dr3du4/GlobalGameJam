using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private LightCircuit lightCircuit;
    [Space]
    [SerializeField] private MeshRenderer tileMeshRenderer;

    public LightCircuit Circuit => lightCircuit;

    public void SetTileVisible(bool isVisible)
    {
        tileMeshRenderer.enabled = isVisible;
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
