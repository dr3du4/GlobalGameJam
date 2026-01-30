using UnityEngine;

public class RoomStats : MonoBehaviour
{
    public float width;
    public float depth;

    void Awake()
    {
        width = GetComponent<MeshRenderer>().bounds.size.x;
        depth = GetComponent<MeshRenderer>().bounds.size.z;
    }
}
