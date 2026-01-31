using UnityEngine;

[CreateAssetMenu(fileName = "DangerList", menuName = "Scriptable Objects/DangerList")]
public class DangerList : ScriptableObject
{
    [SerializeField] private GameObject[] objects;
    public GameObject InstantiateRandomDanger(Transform parent)
    {
        int randomIndex = Random.Range(0, objects.Length);
        return Instantiate(objects[randomIndex], parent);
    }
}
