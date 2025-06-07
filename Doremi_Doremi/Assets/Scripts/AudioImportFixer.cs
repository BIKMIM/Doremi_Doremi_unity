using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class AudioImportFixer : MonoBehaviour
{
    [ContextMenu("Fix Audio Import Settings")]
    public void FixAudioImportSettings()
    {
        Debug.Log("=== Audio Import Settings Fix ===");
        
        // 2oc 폴더 오디오 파일들 처리
        string[] audioFolders = {"2oc", "3oc", "4oc", "5oc", "6oc"};
        
        foreach (string folder in audioFolders)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { $"Assets/Resources/audio/{folder}" });
            
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                AudioImporter audioImporter = AssetImporter.GetAtPath(assetPath) as AudioImporter;
                
                if (audioImporter != null)
                {
                    Debug.Log($"Checking {assetPath}:");
                    Debug.Log($"  Current settings: compressionFormat={audioImporter.defaultSampleSettings.compressionFormat}, quality={audioImporter.defaultSampleSettings.quality}, loadType={audioImporter.defaultSampleSettings.loadType}");
                    
                    // 기본 설정을 무압축으로 변경
                    AudioImporterSampleSettings sampleSettings = audioImporter.defaultSampleSettings;
                    sampleSettings.compressionFormat = AudioCompressionFormat.PCM;
                    sampleSettings.quality = 1.0f; // 최고 품질
                    sampleSettings.loadType = AudioClipLoadType.DecompressOnLoad;
                    sampleSettings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate; // 원본 샘플레이트 유지
                    sampleSettings.preloadAudioData = true; // 새로운 방식: SampleSettings에 포함됨
                    
                    audioImporter.defaultSampleSettings = sampleSettings;
                    audioImporter.loadInBackground = false;
                    audioImporter.ambisonic = false;
                    
                    // 설정 적용
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    
                    Debug.Log($"  Fixed settings: compressionFormat={sampleSettings.compressionFormat}, quality={sampleSettings.quality}, loadType={sampleSettings.loadType}, preloadAudioData={sampleSettings.preloadAudioData}");
                }
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log("=== Audio Import Settings Fix Complete ===");
    }
    
    [ContextMenu("Check Current Audio Settings")]
    public void CheckCurrentAudioSettings()
    {
        Debug.Log("=== Current Audio Settings Check ===");
        
        // c2 파일만 체크
        string assetPath = "Assets/Resources/audio/2oc/c2.wav";
        AudioImporter audioImporter = AssetImporter.GetAtPath(assetPath) as AudioImporter;
        
        if (audioImporter != null)
        {
            var settings = audioImporter.defaultSampleSettings;
            Debug.Log($"C2 Audio Settings:");
            Debug.Log($"  - Compression Format: {settings.compressionFormat}");
            Debug.Log($"  - Quality: {settings.quality}");
            Debug.Log($"  - Load Type: {settings.loadType}");
            Debug.Log($"  - Sample Rate Setting: {settings.sampleRateSetting}");
            Debug.Log($"  - Preload Audio Data: {settings.preloadAudioData}");
            Debug.Log($"  - Load In Background: {audioImporter.loadInBackground}");
            Debug.Log($"  - Ambisonic: {audioImporter.ambisonic}");
            
            // 실제 AudioClip 정보도 확인
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (clip != null)
            {
                Debug.Log($"AudioClip Runtime Info:");
                Debug.Log($"  - Frequency: {clip.frequency}Hz");
                Debug.Log($"  - Length: {clip.length:F2}s");
                Debug.Log($"  - Channels: {clip.channels}");
                Debug.Log($"  - Samples: {clip.samples}");
                Debug.Log($"  - Load Type: {clip.loadType}");
                Debug.Log($"  - Preload Audio Data: {clip.preloadAudioData}");
            }
        }
        
        Debug.Log("=== End Check ===");
    }
}
#endif