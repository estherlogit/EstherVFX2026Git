using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Private variables/references
    GameObject playerGO;
    Transform playerTransform;

    // These two floats are the min and max X positions the camera can move to.
    public float clampPosMax = 13.5f;
    public float clampPosMin = -6.8f;
    
    internal float cameraXSmoothing;

    void Start()
    {
        playerGO = GameObject.Find("Player");
        playerTransform = playerGO.GetComponent<Transform>();
    }

    void FixedUpdate()
    {
        float worldSpaceMousePosX = GetMousePosition();
        float playerPosX = playerTransform.position.x;
        
        float currentCameraX = Camera.main.transform.position.x;
        float newCameraX = Mathf.Clamp(Mathf.Lerp(playerPosX,-worldSpaceMousePosX, 0.5f), clampPosMin, clampPosMax);
        float targetCameraX = Mathf.SmoothDamp(currentCameraX, newCameraX, ref cameraXSmoothing, 0.2f);
        
        Camera.main.transform.position = new Vector3(targetCameraX, 1.78f, -20f);
    }

    float GetMousePosition()
    {
        Vector2 mousePosition = Utility._inputProvider.MousePositionAction();
        
        return Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -20f)).x; 
    }
}
