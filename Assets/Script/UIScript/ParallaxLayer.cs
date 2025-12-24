using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Range(0f, 1f)]
    [Tooltip("0 = tidak bergerak (background), 1 = bergerak penuh dengan camera (foreground)")]
    public float parallaxFactor = 0.5f;
    
    [Header("Optional: Auto Scroll")]
    [Tooltip("Enable untuk layer yang scroll otomatis (seperti clouds)")]
    public bool autoScroll = false;
    public float autoScrollSpeed = 0.5f;
    
    [Header("Infinite Scroll Settings")]
    public bool enableInfiniteScroll = false;
    public float repeatDistance = 20f;
    
    private Transform cameraTransform;
    private Vector3 previousCameraPosition;
    private float spriteWidth;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    
    void Start()
    {
        cameraTransform = Camera.main.transform;
        previousCameraPosition = cameraTransform.position;
        startPosition = transform.position;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            spriteWidth = spriteRenderer.sprite.bounds.size.x;
        }
    }
    
    void LateUpdate()
    {
        // Calculate parallax movement based on camera movement
        float deltaX = (cameraTransform.position.x - previousCameraPosition.x) * parallaxFactor;
        float deltaY = (cameraTransform.position.y - previousCameraPosition.y) * parallaxFactor;
        
        transform.position += new Vector3(deltaX, deltaY, 0f);
        
        previousCameraPosition = cameraTransform.position;
        
        // Auto scroll (untuk clouds dan atmospheric effects)
        if (autoScroll)
        {
            transform.position += new Vector3(autoScrollSpeed * Time.deltaTime, 0f, 0f);
        }
        
        // Infinite scrolling (repeat when out of view)
        if (enableInfiniteScroll && spriteWidth > 0)
        {
            float distanceFromStart = transform.position.x - startPosition.x;
            
            if (Mathf.Abs(distanceFromStart) > repeatDistance)
            {
                transform.position = new Vector3(
                    startPosition.x,
                    transform.position.y,
                    transform.position.z
                );
            }
        }
    }
    
    // Method untuk reset position (berguna saat scene restart)
    public void ResetPosition()
    {
        transform.position = startPosition;
        previousCameraPosition = cameraTransform.position;
    }
}