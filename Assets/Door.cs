using UnityEngine;

public class Door : MonoBehaviour
{
   
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            Debug.Log("Door triggered");
        }
    }
}
