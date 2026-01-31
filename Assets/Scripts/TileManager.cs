using UnityEngine;

public class TileManager : MonoBehaviour
{
    Tile[] tiles;
    void Awake()
    {
        tiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
    }

    public void UpdateDanger(Tile.TileDanger danger)
    {
        foreach (var tile in tiles)
        {
            tile.SetSafe(tile.tileDanger != danger);
        }
    }

    public void UpdateLight(Tile.TileLight lightType)
    {
        foreach (var tile in tiles)
        {
            tile.SetLight(tile.tileLightType == lightType);
        }
    }
}
