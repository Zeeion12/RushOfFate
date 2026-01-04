using UnityEngine;

public class FootstepStopTrigger : MonoBehaviour
{
    [Header("Settings")]
    public bool stopFootstepsOnEnter = true;
    public bool stopBGMOnEnter = false; // Optional: stop BGM juga
    
    [Header("Debug")]
    public bool showDebugLog = true;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if player entered
        if (collision.CompareTag("Player"))
        {
            if (showDebugLog)
            {
                Debug.Log("Player entered LevelNavigation - Stopping footsteps");
            }
            
            // Stop footsteps
            if (stopFootstepsOnEnter && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopFootsteps();
            }
            
            // Optional: Stop BGM
            if (stopBGMOnEnter && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopBGM();
            }
        }
    }
}