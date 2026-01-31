using UnityEngine;

[CreateAssetMenu(fileName = "DangerList", menuName = "Scriptable Objects/DangerList")]
public class DangerList : ScriptableObject
{
    [SerializeField] private GameObject[] objects;
    [SerializeField] private GameObject anim2DObj;
    public (GameObject, GameObject) InstantiateRandomDanger(Transform parent)
    {
        int randomIndex = Random.Range(0, objects.Length);
        var hazardObj = Instantiate(objects[randomIndex], parent);
        GameObject anim2D = null;
        if (anim2DObj != null)
        {
            anim2D = Instantiate(anim2DObj, parent);
        }
        return (hazardObj, anim2D);
    }
}
