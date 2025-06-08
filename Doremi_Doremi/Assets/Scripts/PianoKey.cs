using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PianoKey : MonoBehaviour
{
    [Header("Key Info")]
    [SerializeField] private string noteName; // "C", "C#", "D" 등
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Button keyButton;
    
    private DynamicPianoMapper pianoMapper;
    private SongGameController gameController;
    
    public string NoteName => noteName;
    
    public void Initialize(string note, AudioSource audio, DynamicPianoMapper mapper)
    {
        noteName = note;
        audioSource = audio;
        pianoMapper = mapper;
        
        // 게임 컨트롤러 찾기
        if (gameController == null)
            gameController = FindObjectOfType<SongGameController>();
        
        // 버튼 컴포넌트 가져오기
        keyButton = GetComponent<Button>();
        if (keyButton == null)
        {
            keyButton = gameObject.AddComponent<Button>();
        }
        
        // 클릭 이벤트 연결
        keyButton.onClick.RemoveAllListeners();
        keyButton.onClick.AddListener(PlaySound);
        
        Debug.Log($"PianoKey {noteName} initialized - AudioSource: {(audioSource != null ? "OK" : "NULL")}, Button: {(keyButton != null ? "OK" : "NULL")}");
    }
    
    public void UpdateAudioClip(AudioClip newClip)
    {
        if (audioSource != null)
        {
            audioSource.clip = newClip;
            
            // 오디오 설정 재확인 (pitch가 변경되었을 수 있음)
            if (audioSource.pitch != 1.0f)
            {
                Debug.LogWarning($"AudioSource pitch was {audioSource.pitch}, resetting to 1.0f for {noteName}");
                audioSource.pitch = 1.0f;
            }
            
            string clipInfo = newClip != null ? 
                $"{newClip.name} (freq={newClip.frequency}Hz, len={newClip.length:F2}s)" : 
                "NULL";
            Debug.Log($"Updated AudioClip for {noteName}: {clipInfo}");
        }
        else
        {
            Debug.LogWarning($"AudioSource is null for {noteName}");
        }
    }
    
    /// <summary>
    /// 게임 로직과 연동해서 소리 재생 + 게임 컨트롤러에 알림
    /// </summary>
    public void PlaySound()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            // 재생 전 오디오 설정 재확인
            if (audioSource.pitch != 1.0f)
            {
                Debug.LogWarning($"Correcting pitch from {audioSource.pitch} to 1.0f for {noteName}");
                audioSource.pitch = 1.0f;
            }
            
            audioSource.Stop(); // 이전 소리 정지
            audioSource.Play(); // 새 소리 재생
            
            Debug.Log($"Playing sound for {noteName}: {audioSource.clip.name} (freq={audioSource.clip.frequency}Hz, pitch={audioSource.pitch}, volume={audioSource.volume})");
            
            // 시각적 피드백
            StartCoroutine(KeyPressEffect());
        }
        else
        {
            string debugInfo = $"Cannot play sound for {noteName} - ";
            debugInfo += $"AudioSource: {(audioSource != null ? "OK" : "NULL")}, ";
            debugInfo += $"AudioClip: {(audioSource?.clip != null ? audioSource.clip.name : "NULL")}";
            
            Debug.LogWarning(debugInfo);
            
            // 오디오가 없어도 시각적 피드백은 제공
            StartCoroutine(KeyPressEffect());
        }
        
        // 게임 컨트롤러에 키 입력 알림
        if (gameController != null)
        {
            gameController.OnKeyPressed(noteName);
        }
    }
    
    /// <summary>
    /// 게임 로직 알림 없이 소리만 재생 (미리듣기용)
    /// </summary>
    public void PlaySoundOnly()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            // 재생 전 오디오 설정 재확인
            if (audioSource.pitch != 1.0f)
            {
                Debug.LogWarning($"Correcting pitch from {audioSource.pitch} to 1.0f for {noteName}");
                audioSource.pitch = 1.0f;
            }
            
            audioSource.Stop(); // 이전 소리 정지
            audioSource.Play(); // 새 소리 재생
            
            Debug.Log($"Playing sound ONLY for {noteName}: {audioSource.clip.name} (preview mode)");
            
            // 시각적 피드백
            StartCoroutine(KeyPressEffect());
        }
        else
        {
            string debugInfo = $"Cannot play sound for {noteName} - ";
            debugInfo += $"AudioSource: {(audioSource != null ? "OK" : "NULL")}, ";
            debugInfo += $"AudioClip: {(audioSource?.clip != null ? audioSource.clip.name : "NULL")}";
            
            Debug.LogWarning(debugInfo);
            
            // 오디오가 없어도 시각적 피드백은 제공
            StartCoroutine(KeyPressEffect());
        }
        
        // 게임 컨트롤러에는 알리지 않음 (미리듣기용이므로)
    }
    
    private IEnumerator KeyPressEffect()
    {
        // 건반을 누른 효과 (색상 변경 등)
        Image keyImage = GetComponent<Image>();
        if (keyImage != null)
        {
            Color originalColor = keyImage.color;
            keyImage.color = Color.gray;
            
            yield return new WaitForSeconds(0.1f);
            
            keyImage.color = originalColor;
        }
        else
        {
            // Image가 없으면 스케일 효과로 대체
            Vector3 originalScale = transform.localScale;
            transform.localScale = originalScale * 0.95f;
            
            yield return new WaitForSeconds(0.1f);
            
            transform.localScale = originalScale;
        }
    }
    
    // 외부에서 직접 호출할 수 있는 메서드
    public void TriggerKey()
    {
        PlaySound();
    }
    
    // 키보드 입력 처리 (선택사항)
    private void Update()
    {
        // 키보드 매핑 (선택사항)
        if (Input.inputString.Length > 0)
        {
            char keyPressed = Input.inputString[0];
            
            // 간단한 키보드 매핑
            bool shouldPlay = false;
            switch (noteName.ToUpper())
            {
                case "C":
                    shouldPlay = (keyPressed == 'z' || keyPressed == 'Z');
                    break;
                case "C#":
                    shouldPlay = (keyPressed == 's' || keyPressed == 'S');
                    break;
                case "D":
                    shouldPlay = (keyPressed == 'x' || keyPressed == 'X');
                    break;
                case "D#":
                    shouldPlay = (keyPressed == 'd' || keyPressed == 'D');
                    break;
                case "E":
                    shouldPlay = (keyPressed == 'c' || keyPressed == 'C');
                    break;
                case "F":
                    shouldPlay = (keyPressed == 'v' || keyPressed == 'V');
                    break;
                case "F#":
                    shouldPlay = (keyPressed == 'g' || keyPressed == 'G');
                    break;
                case "G":
                    shouldPlay = (keyPressed == 'b' || keyPressed == 'B');
                    break;
                case "G#":
                    shouldPlay = (keyPressed == 'h' || keyPressed == 'H');
                    break;
                case "A":
                    shouldPlay = (keyPressed == 'n' || keyPressed == 'N');
                    break;
                case "A#":
                    shouldPlay = (keyPressed == 'j' || keyPressed == 'J');
                    break;
                case "B":
                    shouldPlay = (keyPressed == 'm' || keyPressed == 'M');
                    break;
                case "C_HIGH":
                    shouldPlay = (keyPressed == ',' || keyPressed == '<');
                    break;
            }
            
            if (shouldPlay)
            {
                PlaySound();
            }
        }
    }
    
    /// <summary>
    /// 오디오 설정 강제 재설정 (디버깅용)
    /// </summary>
    [ContextMenu("Reset Audio Settings")]
    public void ResetAudioSettings()
    {
        if (audioSource != null)
        {
            Debug.Log($"Resetting audio settings for {noteName}...");
            Debug.Log($"Before: pitch={audioSource.pitch}, volume={audioSource.volume}, spatialBlend={audioSource.spatialBlend}");
            
            audioSource.pitch = 1.0f;
            audioSource.volume = 0.8f;
            audioSource.spatialBlend = 0f;
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            
            Debug.Log($"After: pitch={audioSource.pitch}, volume={audioSource.volume}, spatialBlend={audioSource.spatialBlend}");
        }
    }
    
    /// <summary>
    /// 원본 파일과 비교 테스트 (디버깅용)
    /// </summary>
    [ContextMenu("Compare With Original")]
    public void CompareWithOriginal()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            Debug.Log($"=== Audio Comparison for {noteName} ===");
            Debug.Log($"Current AudioSource settings:");
            Debug.Log($"  - Clip: {audioSource.clip.name}");
            Debug.Log($"  - Frequency: {audioSource.clip.frequency}Hz");
            Debug.Log($"  - Length: {audioSource.clip.length:F2}s");
            Debug.Log($"  - Channels: {audioSource.clip.channels}");
            Debug.Log($"  - AudioSource pitch: {audioSource.pitch}");
            Debug.Log($"  - AudioSource volume: {audioSource.volume}");
            Debug.Log($"  - AudioSource spatialBlend: {audioSource.spatialBlend}");
            Debug.Log($"  - LoadType: {audioSource.clip.loadType}");
            Debug.Log($"  - PreloadAudioData: {audioSource.clip.preloadAudioData}");
            Debug.Log($"=== End Comparison ===");
        }
    }
    
    // 디버그용 메서드
    [ContextMenu("Debug Key Info")]
    public void DebugKeyInfo()
    {
        Debug.Log($"=== PianoKey Debug Info ===");
        Debug.Log($"Note Name: {noteName}");
        Debug.Log($"AudioSource: {(audioSource != null ? "Present" : "Missing")}");
        Debug.Log($"AudioClip: {(audioSource?.clip != null ? audioSource.clip.name : "Missing")}");
        Debug.Log($"Button: {(keyButton != null ? "Present" : "Missing")}");
        Debug.Log($"GameObject: {gameObject.name}");
        
        if (audioSource != null)
        {
            Debug.Log($"AudioSource Details:");
            Debug.Log($"  - Pitch: {audioSource.pitch}");
            Debug.Log($"  - Volume: {audioSource.volume}");
            Debug.Log($"  - SpatialBlend: {audioSource.spatialBlend}");
            Debug.Log($"  - Is Playing: {audioSource.isPlaying}");
            Debug.Log($"  - Is Virtual: {audioSource.isVirtual}");
        }
        
        Debug.Log($"=== End Debug Info ===");
    }
    
    // 테스트용 메서드
    [ContextMenu("Test Play Sound")]
    public void TestPlaySound()
    {
        PlaySound();
    }
    
    [ContextMenu("Test Play Sound Only")]
    public void TestPlaySoundOnly()
    {
        PlaySoundOnly();
    }
}