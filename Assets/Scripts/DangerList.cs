using UnityEngine;

/// <summary>
/// ScriptableObject przechowujący listę prefabów hazardów.
/// Używany przez Danger.cs do losowego spawnowania wyglądu hazardu.
/// </summary>
[CreateAssetMenu(fileName = "DangerList", menuName = "Game/Danger List")]
public class DangerList : ScriptableObject
{
    [Header("Danger Prefabs")]
    [Tooltip("Lista prefabów wizualnych hazardów (ogień, woda, elektryczność, toksyna)")]
    [SerializeField] private GameObject[] dangerPrefabs;

    /// <summary>
    /// Tworzy losowy hazard jako dziecko podanego transformu.
    /// </summary>
    public GameObject InstantiateRandomDanger(Transform parent)
    {
        if (dangerPrefabs == null || dangerPrefabs.Length == 0)
        {
            Debug.LogWarning("[DangerList] Brak prefabów hazardów w liście!");
            return null;
        }

        // Wybierz losowy prefab
        GameObject randomPrefab = dangerPrefabs[Random.Range(0, dangerPrefabs.Length)];
        
        // Stwórz jako dziecko
        GameObject instantiated = Instantiate(randomPrefab, parent);
        instantiated.transform.localPosition = Vector3.zero;
        instantiated.transform.localRotation = Quaternion.identity;

        return instantiated;
    }

    /// <summary>
    /// Tworzy konkretny hazard na podstawie typu.
    /// </summary>
    public GameObject InstantiateDangerByType(Danger.DangerType type, Transform parent)
    {
        if (dangerPrefabs == null || dangerPrefabs.Length == 0)
        {
            return null;
        }

        // Proste mapowanie: indeks = typ
        // Fire=0, Water=1, Electric=2, Toxic=3
        int index = (int)type;
        
        if (index >= 0 && index < dangerPrefabs.Length)
        {
            GameObject instantiated = Instantiate(dangerPrefabs[index], parent);
            instantiated.transform.localPosition = Vector3.zero;
            instantiated.transform.localRotation = Quaternion.identity;
            return instantiated;
        }

        // Fallback do losowego
        return InstantiateRandomDanger(parent);
    }
}


