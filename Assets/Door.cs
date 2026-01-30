using UnityEngine;

public class Door : MonoBehaviour
{
   
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManger.instance.GenerateRoom(transform.position);
            Debug.Log("Position of the door: " + transform.position);
        }
    }
}
