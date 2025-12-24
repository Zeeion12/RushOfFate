using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float startSize = 5f;
    public float endSize = 10f;
    public float zoomDuration = 3f;
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Follow Settings")]
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(3f, 2f, -10f);
    public bool followOnStart = true;
    
    [Header("Boundaries")]
    public bool useBoundaries = false;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -5f;
    public float maxY = 5f;
    
    private Camera cam;
    private float zoomTimer = 0f;
    private bool isZooming = false;
    private bool isFollowing = false;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographicSize = startSize;
        }
        
        isFollowing = followOnStart;
    }
    
    void Update()
    {
        if (isZooming)
        {
            zoomTimer += Time.deltaTime;
            float t = Mathf.Clamp01(zoomTimer / zoomDuration);
            float curveValue = zoomCurve.Evaluate(t);
            cam.orthographicSize = Mathf.Lerp(startSize, endSize, curveValue);
            
            if (t >= 1f)
            {
                isZooming = false;
            }
        }
    }
    
    void LateUpdate()
    {
        if (isFollowing && target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            
            // Apply boundaries if enabled
            if (useBoundaries)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
            }
            
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
    
    // Public methods untuk control dari script lain
    public void StartZoomOut()
    {
        isZooming = true;
        zoomTimer = 0f;
    }
    
    public void SetZoomInstant(float size)
    {
        if (cam != null)
        {
            cam.orthographicSize = size;
        }
    }
    
    public void StartFollowing()
    {
        isFollowing = true;
    }
    
    public void StopFollowing()
    {
        isFollowing = false;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}