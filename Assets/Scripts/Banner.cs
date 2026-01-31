using UnityEngine;

public class Banner : MonoBehaviour
{
    [SerializeField] private bool isGridWalkerBanner = false;
    void Update()
    {
        Vector3 pos = isGridWalkerBanner ? GameManager.instance.secondCamera.transform.position : GameManager.instance.mainCamera.transform.position;
        Vector3 directionToCamera = pos - transform.position;
        // directionToCamera.y = 0;
        transform.rotation = Quaternion.LookRotation(directionToCamera);
    }
}