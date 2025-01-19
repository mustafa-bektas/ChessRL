using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _mainCamera;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera != null) _mainCamera.transform.position = new Vector3(3.5f, 3.5f, -10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
