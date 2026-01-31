using UnityEngine;

public class Tile : MonoBehaviour
{
    public TileDanger tileDanger;
    public TileLight tileLightType;
    [Space]
    [SerializeField] private Light tileLight;
    [SerializeField] private MeshFilter safeMesh;
    [SerializeField] private MeshFilter hazardMesh;

    private bool isSafe = true;

    public void SetSafe(bool isSafe)
    {
        this.isSafe = isSafe;
    }

    public void SetLight(bool isOn)
    {
        tileLight.enabled = isOn;
    }

    public enum TileDanger
    {
        Safe,
        Hazard1,
        Hazard2
    }

    public enum TileLight
    {
        CircuitA,
        CircuitB
    }
}
