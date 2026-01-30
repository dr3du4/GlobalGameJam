using UnityEngine;

public class Door : MonoBehaviour
{
   
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Use world position and forward direction
            Vector3 worldPosition = transform.position;
            Vector3 forwardDirection = transform.forward;
            
            GameManger.instance.GenerateRoom(worldPosition, forwardDirection);
            Debug.Log("Door world position: " + worldPosition + ", forward: " + forwardDirection);
        }
    }
}
