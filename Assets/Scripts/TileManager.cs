using UnityEngine;

public class TileManager : MonoBehaviour
{
    Tile[] tiles;
    Danger[] dangers;
    void Awake()
    {
        tiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
        dangers = FindObjectsByType<Danger>(FindObjectsSortMode.None);
        foreach (var tile in tiles)
        {
            tile.SetTileVisible(false);
        }
        foreach (var danger in dangers)
        {
            danger.SetDangerActive(false);
            danger.SetDangerVisible(false);
        }
    }

    public void SetupDangers(Danger.DangerType dangerType)
    {
        foreach(var danger in dangers)
        {
            danger.SetDangerActive(false);
            if (danger.Type == dangerType)
            {
                danger.SetDangerActive(true);
            }
        }
    }

    public void SetupLights(Tile.LightCircuit circuit)
    {
        foreach (var danger in dangers)
        {
            danger.SetDangerVisible(false);
            if (danger.LightCircuit == circuit)
            {
                danger.SetDangerVisible(true);
            }
        }

        foreach (var tile in tiles)
        {
            tile.SetTileVisible(false);
            if (tile.Circuit == circuit)
            {
                tile.SetTileVisible(true);
            }
        }
    }

    public void ClearAll()
    {
        foreach (var danger in dangers)
        {
            danger.SetDangerActive(false);
            danger.SetDangerVisible(false);
        }

        foreach (var tile in tiles)
        {
            tile.SetTileVisible(false);
        }
    }
}
