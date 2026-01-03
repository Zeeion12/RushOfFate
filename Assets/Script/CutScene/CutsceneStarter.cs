using System.Collections;
using UnityEngine;

public class CutsceneStarter : MonoBehaviour
{
    [Header("Settings")]
    public float delayBeforeStart = 0.5f;
    
    void Start()
    {
        StartCoroutine(StartCutscene());
    }
    
    IEnumerator StartCutscene()
    {
        Debug.Log("=== CUTSCENE MAIN LOADED ===");
        
        // Pastikan scene mulai dengan black screen
        // (SceneTransition akan handle fade in)
        
        // Small delay
        yield return new WaitForSeconds(delayBeforeStart);
        
        // Fade in sudah dihandle oleh SceneTransition
        // Footsteps sudah playing dari loading scene
        
        Debug.Log("=== CUTSCENE READY ===");
    }
}