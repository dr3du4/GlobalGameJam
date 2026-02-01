using UnityEngine;

public class Banner : MonoBehaviour
{
    [SerializeField] private bool isGridWalkerBanner = false;
    void Update()
    {
        if (!isGridWalkerBanner)
        {
            Vector3 pos = GameManager.instance.mainCamera.transform.position;
            Vector3 directionToCamera = pos - transform.position;
            directionToCamera.y = 0;
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
        else
        {
            transform.rotation = Quaternion.Euler(-55, -170, 0);
        }
    }
}