using UnityEngine;
using UnityEngine.UI;

public class TmpTileController : MonoBehaviour
{
    [SerializeField] private Button redButton;
    [SerializeField] private Button greenButton;
    [SerializeField] private Button blueButton;
    [SerializeField] private Button yellowButton;
    [SerializeField] private Button fireButton;
    [SerializeField] private Button toxinButton;
    [SerializeField] private Button electricButton;
    [SerializeField] private Button mechanicButton;

    void Start()
    {
        TileManager tileManager = FindFirstObjectByType<TileManager>();
        redButton.onClick.AddListener(() => tileManager.SetupLights(Tile.LightCircuit.Red));
        greenButton.onClick.AddListener(() => tileManager.SetupLights(Tile.LightCircuit.Green));
        blueButton.onClick.AddListener(() => tileManager.SetupLights(Tile.LightCircuit.Blue));
        yellowButton.onClick.AddListener(() => tileManager.SetupLights(Tile.LightCircuit.Yellow));
        fireButton.onClick.AddListener(() => tileManager.SetupDangers(Danger.DangerType.Fire));
        electricButton.onClick.AddListener(() => tileManager.SetupDangers(Danger.DangerType.Electric));
        toxinButton.onClick.AddListener(() => tileManager.SetupDangers(Danger.DangerType.Toxic));
        mechanicButton.onClick.AddListener(() => tileManager.SetupDangers(Danger.DangerType.Mechanic));
    }
}
