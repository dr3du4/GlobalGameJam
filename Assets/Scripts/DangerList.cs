using UnityEngine;

/// <summary>
/// ScriptableObject przechowujący listę prefabów hazardów.
/// Używany przez Danger.cs do losowego spawnowania wyglądu hazardu.
/// </summary>
[CreateAssetMenu(fileName = "DangerList", menuName = "Scriptable Objects/DangerList")]
public class DangerList : ScriptableObject
{
    [Header("Danger Prefabs")]
    [SerializeField] private GameObject[] objects;
    [SerializeField] private GameObject anim2DObj;

    /// <summary>
    /// Tworzy losowy hazard jako dziecko podanego transformu.
    /// Zwraca tuple (hazardObject, anim2DObject)
    /// </summary>
    public (GameObject, GameObject) InstantiateRandomDanger(Transform parent)
    {
        if (objects == null || objects.Length == 0)
        {
            Debug.LogWarning("[DangerList] Brak prefabów hazardów w liście!");
            return (null, null);
        }

        int randomIndex = Random.Range(0, objects.Length);
        var hazardObj = Instantiate(objects[randomIndex], parent);
        
        GameObject anim2D = null;
        if (anim2DObj != null)
        {
            anim2D = Instantiate(anim2DObj, parent);
        }
        
        return (hazardObj, anim2D);
    }

    /// <summary>
    /// Tworzy konkretny hazard na podstawie typu.
    /// </summary>
    public (GameObject, GameObject) InstantiateDangerByType(Danger.DangerType type, Transform parent)
    {
        if (objects == null || objects.Length == 0)
        {
            return (null, null);
        }

        // Mapowanie: indeks = typ (Fire=0, Water=1, Mechanic=2, Electric=3, Toxic=4)
        int index = (int)type;
        
        GameObject hazardObj;
        if (index >= 0 && index < objects.Length)
        {
            hazardObj = Instantiate(objects[index], parent);
        }
        else
        {
            // Fallback do losowego
            hazardObj = Instantiate(objects[Random.Range(0, objects.Length)], parent);
        }

        GameObject anim2D = null;
        if (anim2DObj != null)
        {
            anim2D = Instantiate(anim2DObj, parent);
        }

        return (hazardObj, anim2D);
    }
}
