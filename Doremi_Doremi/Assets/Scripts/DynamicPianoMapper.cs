using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 동적 피아노 매핑 시스템
/// - 현재 악보에 표시된 음표들의 옥타브에 따라 피아노 키 음향을 자동 매핑
/// - 4개 옥타브 범위 지원 (C2-C3, C3-C4, C4-C5, C5-C6)
/// - 게임 상황에 맞는 최적의 피아노 소리 제공
/// </summary>
public class DynamicPianoMapper : MonoBehaviour
{
    [Header("Piano Keys")]
    [SerializeField] private List<PianoKey> pianoKeys = new List<PianoKey>();
    
    [Header("Current Octave Settings")]
    [SerializeField] private int currentOctave = 4; // 기본 옥타브
    
    [Header("Audio Clips by Octave")]
    [SerializeField] private AudioClipSet[] audioClipSets;
    
    // 현재 악보에 표시된 음들의 옥타브 정보를 저장
    private Dictionary<string, int> currentNoteOctaves = new Dictionary<string, int>();
    
    private void Start()
    {
        Debug.Log("DynamicPianoMapper Start() called");
        CheckAudioSettings();
        LoadAudioClips();
        InitializePianoKeys();
        SetupDefaultOctave();
        Debug.Log("DynamicPianoMapper initialization completed");
    }
    
    /// <summary>
    /// 오디오 설정 상태 확인
    /// </summary>
    private void CheckAudioSettings()
    {
        Debug.Log("=== Audio Settings Check ===");
        
        // AudioListener 확인
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (listener != null)
        {
            Debug.Log($"AudioListener found on: {listener.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("No AudioListener found in scene!");
        }
        
        // 전체 볼륨 확인
        Debug.Log($"AudioListener.volume: {AudioListener.volume}");
        Debug.Log($"AudioListener.pause: {AudioListener.pause}");
        
        Debug.Log("=== End Audio Settings Check ===");
    }
    
    /// <summary>
    /// Resources 폴더에서 옥타브별 오디오 클립 로드
    /// </summary>
    private void LoadAudioClips()
    {
        Debug.Log("Loading AudioClips from Resources...");
        
        // AudioClipSet 4개를 초기화 (C2~C3, C3~C4, C4~C5, C5~C6)
        audioClipSets = new AudioClipSet[4];
        
        // C2~C3 옥타브 (0번 인덱스) - 2oc 폴더
        audioClipSets[0] = new AudioClipSet();
        audioClipSets[0].octave = 2;
        audioClipSets[0].description = "C2~C3";
        audioClipSets[0].C = Resources.Load<AudioClip>("audio/2oc/c2");
        audioClipSets[0].CS = Resources.Load<AudioClip>("audio/2oc/c2s");
        audioClipSets[0].D = Resources.Load<AudioClip>("audio/2oc/d2");
        audioClipSets[0].DS = Resources.Load<AudioClip>("audio/2oc/d2s");
        audioClipSets[0].E = Resources.Load<AudioClip>("audio/2oc/e2");
        audioClipSets[0].F = Resources.Load<AudioClip>("audio/2oc/f2");
        audioClipSets[0].FS = Resources.Load<AudioClip>("audio/2oc/f2s");
        audioClipSets[0].G = Resources.Load<AudioClip>("audio/2oc/g2");
        audioClipSets[0].GS = Resources.Load<AudioClip>("audio/2oc/g2s");
        audioClipSets[0].A = Resources.Load<AudioClip>("audio/2oc/a2");
        audioClipSets[0].AS = Resources.Load<AudioClip>("audio/2oc/a2s");
        audioClipSets[0].B = Resources.Load<AudioClip>("audio/2oc/b2");
        audioClipSets[0].C_Next = Resources.Load<AudioClip>("audio/2oc/c3");
        
        // C3~C4 옥타브 (1번 인덱스) - 3oc 폴더
        audioClipSets[1] = new AudioClipSet();
        audioClipSets[1].octave = 3;
        audioClipSets[1].description = "C3~C4";
        audioClipSets[1].C = Resources.Load<AudioClip>("audio/3oc/c3");
        audioClipSets[1].CS = Resources.Load<AudioClip>("audio/3oc/c3s");
        audioClipSets[1].D = Resources.Load<AudioClip>("audio/3oc/d3");
        audioClipSets[1].DS = Resources.Load<AudioClip>("audio/3oc/d3s");
        audioClipSets[1].E = Resources.Load<AudioClip>("audio/3oc/e3");
        audioClipSets[1].F = Resources.Load<AudioClip>("audio/3oc/f3");
        audioClipSets[1].FS = Resources.Load<AudioClip>("audio/3oc/f3s");
        audioClipSets[1].G = Resources.Load<AudioClip>("audio/3oc/g3");
        audioClipSets[1].GS = Resources.Load<AudioClip>("audio/3oc/g3s");
        audioClipSets[1].A = Resources.Load<AudioClip>("audio/3oc/a3");
        audioClipSets[1].AS = Resources.Load<AudioClip>("audio/3oc/a3s");
        audioClipSets[1].B = Resources.Load<AudioClip>("audio/3oc/b3");
        audioClipSets[1].C_Next = Resources.Load<AudioClip>("audio/3oc/c4");
        
        // C4~C5 옥타브 (2번 인덱스) - 4oc 폴더, 기본 옥타브
        audioClipSets[2] = new AudioClipSet();
        audioClipSets[2].octave = 4;
        audioClipSets[2].description = "C4~C5";
        audioClipSets[2].C = Resources.Load<AudioClip>("audio/4oc/c4");
        audioClipSets[2].CS = Resources.Load<AudioClip>("audio/4oc/c4s");
        audioClipSets[2].D = Resources.Load<AudioClip>("audio/4oc/d4");
        audioClipSets[2].DS = Resources.Load<AudioClip>("audio/4oc/d4s");
        audioClipSets[2].E = Resources.Load<AudioClip>("audio/4oc/e4");
        audioClipSets[2].F = Resources.Load<AudioClip>("audio/4oc/f4");
        audioClipSets[2].FS = Resources.Load<AudioClip>("audio/4oc/f4s");
        audioClipSets[2].G = Resources.Load<AudioClip>("audio/4oc/g4");
        audioClipSets[2].GS = Resources.Load<AudioClip>("audio/4oc/g4s");
        audioClipSets[2].A = Resources.Load<AudioClip>("audio/4oc/a4");
        audioClipSets[2].AS = Resources.Load<AudioClip>("audio/4oc/a4s");
        audioClipSets[2].B = Resources.Load<AudioClip>("audio/4oc/b4");
        audioClipSets[2].C_Next = Resources.Load<AudioClip>("audio/4oc/c5");
        
        // C5~C6 옥타브 (3번 인덱스) - 5oc 폴더
        audioClipSets[3] = new AudioClipSet();
        audioClipSets[3].octave = 5;
        audioClipSets[3].description = "C5~C6";
        audioClipSets[3].C = Resources.Load<AudioClip>("audio/5oc/c5");
        audioClipSets[3].CS = Resources.Load<AudioClip>("audio/5oc/c5s");
        audioClipSets[3].D = Resources.Load<AudioClip>("audio/5oc/d5");
        audioClipSets[3].DS = Resources.Load<AudioClip>("audio/5oc/d5s");
        audioClipSets[3].E = Resources.Load<AudioClip>("audio/5oc/e5");
        audioClipSets[3].F = Resources.Load<AudioClip>("audio/5oc/f5");
        audioClipSets[3].FS = Resources.Load<AudioClip>("audio/5oc/f5s");
        audioClipSets[3].G = Resources.Load<AudioClip>("audio/5oc/g5");
        audioClipSets[3].GS = Resources.Load<AudioClip>("audio/5oc/g5s");
        audioClipSets[3].A = Resources.Load<AudioClip>("audio/5oc/a5");
        audioClipSets[3].AS = Resources.Load<AudioClip>("audio/5oc/a5s");
        audioClipSets[3].B = Resources.Load<AudioClip>("audio/5oc/b5");
        audioClipSets[3].C_Next = Resources.Load<AudioClip>("audio/5oc/c6");
        
        // 로딩 상태 확인 및 오디오 클립 정보 출력
        for (int i = 0; i < audioClipSets.Length; i++)
        {
            int loadedCount = 0;
            if (audioClipSets[i].C != null) loadedCount++;
            if (audioClipSets[i].CS != null) loadedCount++;
            if (audioClipSets[i].D != null) loadedCount++;
            if (audioClipSets[i].DS != null) loadedCount++;
            if (audioClipSets[i].E != null) loadedCount++;
            if (audioClipSets[i].F != null) loadedCount++;
            if (audioClipSets[i].FS != null) loadedCount++;
            if (audioClipSets[i].G != null) loadedCount++;
            if (audioClipSets[i].GS != null) loadedCount++;
            if (audioClipSets[i].A != null) loadedCount++;
            if (audioClipSets[i].AS != null) loadedCount++;
            if (audioClipSets[i].B != null) loadedCount++;
            if (audioClipSets[i].C_Next != null) loadedCount++;
            
            Debug.Log($"Octave {audioClipSets[i].octave} ({audioClipSets[i].description}) - {loadedCount}/13 clips loaded");
            
            // C 클립 정보 상세 출력
            if (audioClipSets[i].C != null)
            {
                AudioClip clip = audioClipSets[i].C;
                Debug.Log($"  C{audioClipSets[i].octave} clip info: frequency={clip.frequency}Hz, channels={clip.channels}, length={clip.length:F2}s");
            }
        }
        
        Debug.Log("AudioClips loading completed");
    }
    
    /// <summary>
    /// 피아노 키들을 찾아서 PianoKey 컴포넌트 설정
    /// </summary>
    private void InitializePianoKeys()
    {
        Debug.Log("Initializing Piano Keys...");
        
        pianoKeys.Clear();
        
        // 각 건반을 찾아서 PianoKey로 설정 
        string[] keyNames = {"C4", "C4S", "D4", "D4S", "E4", "F4", "F4S", "G4", "G4S", "A4", "A4S", "B4", "C5"};
        
        foreach (string keyName in keyNames)
        {
            Transform keyTransform = transform.Find(keyName);
            if (keyTransform != null)
            {
                PianoKey pianoKey = keyTransform.GetComponent<PianoKey>();
                if (pianoKey == null)
                {
                    pianoKey = keyTransform.gameObject.AddComponent<PianoKey>();
                }
                
                // 건반 이름에서 음정 정보 추출
                string noteName = ExtractNoteName(keyName);
                AudioSource audioSource = keyTransform.GetComponent<AudioSource>();
                
                // AudioSource가 없으면 추가
                if (audioSource == null)
                {
                    audioSource = keyTransform.gameObject.AddComponent<AudioSource>();
                }
                
                // AudioSource 설정 최적화 (pitch 확인 중요!)
                audioSource.playOnAwake = false;
                audioSource.volume = 0.8f;
                audioSource.spatialBlend = 0f; // 2D 사운드로 설정
                audioSource.pitch = 1.0f; // ⭐ 중요: Pitch를 정확히 1.0으로 설정
                audioSource.loop = false;
                audioSource.priority = 128;
                
                Debug.Log($"AudioSource settings for {keyName}: pitch={audioSource.pitch}, volume={audioSource.volume}, spatialBlend={audioSource.spatialBlend}");
                
                pianoKey.Initialize(noteName, audioSource, this);
                pianoKeys.Add(pianoKey);
                
                Debug.Log($"Initialized piano key: {keyName} -> {noteName}");
            }
            else
            {
                Debug.LogWarning($"Piano key not found: {keyName}");
            }
        }
        
        Debug.Log($"Total piano keys initialized: {pianoKeys.Count}");
    }
    
    /// <summary>
    /// 건반 이름에서 음정 추출
    /// </summary>
    private string ExtractNoteName(string keyName)
    {
        // "C4S" -> "C#", "D4" -> "D" 등으로 변환
        // "C5" -> "C_High" (마지막 높은 도는 특별 처리)
        if (keyName == "C5")
        {
            return "C_High"; // 마지막 높은 도는 특별한 이름으로 처리
        }
        else if (keyName.Contains("S"))
        {
            return keyName[0] + "#";
        }
        return keyName[0].ToString();
    }
    
    /// <summary>
    /// 기본 옥타브로 모든 키 설정
    /// </summary>
    private void SetupDefaultOctave()
    {
        Debug.Log($"Setting up default octave: {currentOctave}");
        
        foreach (var key in pianoKeys)
        {
            UpdateKeyAudioClip(key.NoteName, currentOctave);
        }
        
        Debug.Log("Default octave setup completed");
    }
    
    /// <summary>
    /// 현재 화면에 표시된 음정들을 기반으로 건반 매핑을 업데이트
    /// </summary>
    /// <param name="noteOctaves">음정명과 옥타브 딕셔너리 (예: {"C": 4, "G": 3})</param>
    public void UpdateCurrentNotes(Dictionary<string, int> noteOctaves)
    {
        currentNoteOctaves = new Dictionary<string, int>(noteOctaves);
        
        // 각 건반의 AudioClip을 현재 화면의 음정에 맞게 업데이트
        foreach (var key in pianoKeys)
        {
            if (currentNoteOctaves.ContainsKey(key.NoteName))
            {
                UpdateKeyAudioClip(key.NoteName, currentNoteOctaves[key.NoteName]);
            }
            else
            {
                // 화면에 없는 음정은 기본 옥타브 사용
                UpdateKeyAudioClip(key.NoteName, currentOctave);
            }
        }
    }
    
    /// <summary>
    /// 특정 음정의 옥타브를 업데이트
    /// </summary>
    public void UpdateNoteOctave(string noteName, int octave)
    {
        currentNoteOctaves[noteName] = octave;
        UpdateKeyAudioClip(noteName, octave);
    }
    
    /// <summary>
    /// 특정 키의 오디오 클립 업데이트
    /// </summary>
    private void UpdateKeyAudioClip(string noteName, int octave)
    {
        // 해당 음정의 건반 찾기
        PianoKey targetKey = pianoKeys.Find(key => key.NoteName == noteName);
        if (targetKey == null) 
        {
            Debug.LogWarning($"Piano key not found for note: {noteName}");
            return;
        }
        
        // 옥타브에 맞는 AudioClip 찾기
        AudioClip newClip = GetAudioClip(noteName, octave);
        if (newClip != null)
        {
            targetKey.UpdateAudioClip(newClip);
            Debug.Log($"Updated {noteName} to octave {octave} clip: {newClip.name} (freq={newClip.frequency}Hz, len={newClip.length:F2}s)");
        }
        else
        {
            Debug.LogWarning($"No audio clip found for {noteName} in octave {octave}");
        }
    }
    
    /// <summary>
    /// 옥타브와 음정에 맞는 오디오 클립 반환
    /// </summary>
    private AudioClip GetAudioClip(string noteName, int octave)
    {
        // 옥타브 값을 정확히 매핑
        AudioClipSet targetSet = null;
        
        // 옥타브 값으로 올바른 AudioClipSet 찾기
        foreach (var clipSet in audioClipSets)
        {
            if (clipSet.octave == octave)
            {
                targetSet = clipSet;
                break;
            }
        }
        
        if (targetSet != null)
        {
            AudioClip clip = targetSet.GetClip(noteName);
            if (clip != null)
            {
                Debug.Log($"Found audio clip for {noteName} octave {octave}: {clip.name}");
                return clip;
            }
            else
            {
                Debug.LogWarning($"Audio clip for {noteName} not found in octave {octave} set");
            }
        }
        else
        {
            Debug.LogWarning($"AudioClipSet for octave {octave} not found");
        }
        
        // 해당 옥타브가 없으면 기본 옥타브 사용
        foreach (var clipSet in audioClipSets)
        {
            if (clipSet.octave == currentOctave)
            {
                AudioClip clip = clipSet.GetClip(noteName);
                if (clip != null)
                {
                    Debug.LogWarning($"Using default octave {currentOctave} for {noteName}");
                    return clip;
                }
                break;
            }
        }
        
        Debug.LogError($"No audio clip found for {noteName} in any octave");
        return null;
    }
    
    /// <summary>
    /// 현재 화면의 가장 주요한 옥타브를 기반으로 전체 건반 옥타브 설정
    /// OctaveController에서 호출됨
    /// </summary>
    public void SetGlobalOctave(int octave)
    {
        Debug.Log($"Setting global octave to: {octave}");
        
        currentOctave = octave;
        
        // 모든 건반에 새로운 옥타브 적용
        foreach (var key in pianoKeys)
        {
            UpdateKeyAudioClip(key.NoteName, octave);
        }
        
        Debug.Log($"Global octave set to: {octave}. All keys updated.");
    }
    
    /// <summary>
    /// 현재 옥타브 값 반환
    /// </summary>
    public int GetCurrentOctave()
    {
        return currentOctave;
    }
    
    // === 디버그 메서드들 ===
    
    /// <summary>
    /// 특정 AudioClip을 직접 재생해서 테스트 (디버그용)
    /// </summary>
    [ContextMenu("Test Play Original C2")]
    public void TestPlayOriginalC2()
    {
        AudioClip clip = Resources.Load<AudioClip>("audio/2oc/c2");
        if (clip != null)
        {
            // 임시 AudioSource 생성해서 원본 테스트
            GameObject tempGO = new GameObject("TempAudioPlayer");
            AudioSource tempAS = tempGO.AddComponent<AudioSource>();
            tempAS.clip = clip;
            tempAS.volume = 1.0f;
            tempAS.pitch = 1.0f;
            tempAS.spatialBlend = 0f;
            tempAS.Play();
            
            Debug.Log($"Playing original C2: freq={clip.frequency}Hz, len={clip.length:F2}s");
            
            // 3초 후 제거
            Destroy(tempGO, 4f);
        }
        else
        {
            Debug.LogError("Could not load original C2 clip!");
        }
    }
    
    /// <summary>
    /// 각 옥타브의 C 음을 연속으로 테스트 재생 (디버그용)
    /// </summary>
    [ContextMenu("Test Play All C Notes")]
    public void TestPlayAllCNotes()
    {
        StartCoroutine(PlayAllCNotesSequentially());
    }
    
    private System.Collections.IEnumerator PlayAllCNotesSequentially()
    {
        Debug.Log("=== Testing All C Notes ===");
        
        for (int i = 0; i < audioClipSets.Length; i++)
        {
            AudioClip clip = audioClipSets[i].C;
            if (clip != null)
            {
                // 임시 AudioSource 생성해서 테스트
                GameObject tempGO = new GameObject($"TempAudioPlayer_C{audioClipSets[i].octave}");
                AudioSource tempAS = tempGO.AddComponent<AudioSource>();
                tempAS.clip = clip;
                tempAS.volume = 1.0f;
                tempAS.pitch = 1.0f;
                tempAS.spatialBlend = 0f;
                tempAS.Play();
                
                Debug.Log($"Playing C{audioClipSets[i].octave}: {clip.name} (freq={clip.frequency}Hz)");
                
                // 재생 시간만큼 대기
                yield return new WaitForSeconds(Mathf.Min(clip.length, 2f));
                
                Destroy(tempGO);
            }
            else
            {
                Debug.LogError($"C{audioClipSets[i].octave} clip is null!");
            }
            
            // 각 노트 사이에 잠깐 대기
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("=== End Testing All C Notes ===");
    }
    
    /// <summary>
    /// 디버그용: 현재 매핑 상태 출력
    /// </summary>
    [ContextMenu("Debug Current Mapping")]
    public void DebugCurrentMapping()
    {
        Debug.Log($"=== DynamicPianoMapper Debug Info ===");
        Debug.Log($"Current Global Octave: {currentOctave}");
        Debug.Log($"Piano Keys Count: {pianoKeys.Count}");
        Debug.Log($"AudioClipSets Count: {(audioClipSets != null ? audioClipSets.Length : 0)}");
        Debug.Log($"AudioListener.volume: {AudioListener.volume}");
        Debug.Log($"AudioListener.pause: {AudioListener.pause}");
        
        Debug.Log("Current Note Octaves:");
        foreach (var kvp in currentNoteOctaves)
        {
            Debug.Log($"  {kvp.Key}: Octave {kvp.Value}");
        }
        
        Debug.Log("Piano Keys:");
        foreach (var key in pianoKeys)
        {
            AudioSource audioSource = key.GetComponent<AudioSource>();
            string clipInfo = audioSource?.clip != null ? 
                $"{audioSource.clip.name} (freq={audioSource.clip.frequency}Hz, pitch={audioSource.pitch})" : 
                "NULL";
            Debug.Log($"  {key.NoteName}: AudioSource={audioSource != null}, Clip={clipInfo}");
        }
        
        Debug.Log("AudioClipSets Status:");
        if (audioClipSets != null)
        {
            foreach (var clipSet in audioClipSets)
            {
                Debug.Log($"  Octave {clipSet.octave} ({clipSet.description}): C={clipSet.C != null}, C_Next={clipSet.C_Next != null}");
            }
        }
        Debug.Log($"=== End Debug Info ===");
    }
}

/// <summary>
/// 옥타브별 오디오 클립 세트
/// </summary>
[System.Serializable]
public class AudioClipSet
{
    public int octave;
    public string description; // "C4~C5" 같은 설명
    public AudioClip C, CS, D, DS, E, F, FS, G, GS, A, AS, B;
    public AudioClip C_Next; // 다음 옥타브의 C (C5, C6 등)
    
    public AudioClip GetClip(string noteName)
    {
        switch (noteName.ToUpper())
        {
            case "C": return C;
            case "C#": return CS;
            case "D": return D;
            case "D#": return DS;
            case "E": return E;
            case "F": return F;
            case "F#": return FS;
            case "G": return G;
            case "G#": return GS;
            case "A": return A;
            case "A#": return AS;
            case "B": return B;
            case "C_HIGH": return C_Next; // 마지막 높은 도
            default: return null;
        }
    }
}
