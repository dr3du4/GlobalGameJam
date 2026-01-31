using UnityEngine;

public class Banner : MonoBehaviour
{
    void Update()
    {
        Vector3 directionToCamera = GameManager.instance.mainCamera.transform.position - transform.position;
        directionToCamera.y = 0;
        transform.rotation = Quaternion.LookRotation(directionToCamera);
    }
}