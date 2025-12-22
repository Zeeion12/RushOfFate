using System;
using UnityEngine;

[System.Serializable]
public class SceneZoomSettings
{
    [Header("Custom Zoom")]
    [Tooltip("Aktifkan custom zoom untuk scene ini")]
    public bool useCustomZoom = false;
    
    [Tooltip("Scale awal (1.0 = normal, 1.5 = 150%)")]
    [Range(1.0f, 2.0f)]
    public float startScale = 1.3f;
    
    [Tooltip("Scale akhir")]
    [Range(1.0f, 2.0f)]
    public float endScale = 1.0f;
    
    [Tooltip("Durasi zoom (detik)")]
    [Range(1f, 30f)]
    public float duration = 15f;
    
    [Tooltip("Tipe easing")]
    public StoryManager.ZoomEasingType easingType = StoryManager.ZoomEasingType.EaseInOut;
}

[System.Serializable]
public class SubtitleData
{
    [Header("Timing")]
    [Tooltip("Waktu mulai subtitle muncul (detik dari awal scene)")]
    public float startTime;
    
    [Tooltip("Waktu subtitle hilang (detik dari awal scene)")]
    public float endTime;
    
    [Header("Content")]
    [TextArea(2, 4)]
    [Tooltip("Text yang akan ditampilkan")]
    public string text;
    
    [Header("Style (Optional)")]
    [Tooltip("Ukuran font khusus untuk subtitle ini (0 = gunakan default)")]
    public int customFontSize = 0;
    
    [Tooltip("Warna text khusus (opsional)")]
    public bool useCustomColor = false;
    public Color customColor = Color.white;
}

[System.Serializable]
public class SceneSubtitles
{
    [Tooltip("Nama scene (untuk referensi)")]
    public string sceneName;
    
    [Tooltip("Daftar subtitle untuk scene ini")]
    public SubtitleData[] subtitles;
}