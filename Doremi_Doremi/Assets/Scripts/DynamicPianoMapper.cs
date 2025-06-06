using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        InitializePianoKeys();
        SetupDefaultOctave();
    }
    
    private void InitializePianoKeys()
    {
        // 기존 건반들을 자동으로 찾아서 PianoKey 리스트에 추가
        Transform pianoPanel = this.transform;
        if (pianoPanel == null) 
        {
            Debug.LogError("Piano panel not found!");
            return;
        }
        
        pianoKeys.Clear();
        
        // 각 건반을 찾아서 PianoKey로 설정
        string[] keyNames = {"C4", "C4S", "D4", "D4S", "E4", "F4", "F4S", "G4", "G4S", "A4", "A4S", "B4", "C5"};
        
        foreach (string keyName in keyNames)
        {
            Transform keyTransform = pianoPanel.Find(keyName);
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
    
    private string ExtractNoteName(string keyName)
    {
        // "C4S" -> "C#", "D4" -> "D" 등으로 변환
        if (keyName.Contains("S"))
        {
            return keyName[0] + "#";
        }
        return keyName[0].ToString();
    }
    
    private void SetupDefaultOctave()
    {
        // 기본 옥타브로 모든 건반 설정
        Debug.Log($"Setting up default octave: {currentOctave}");
        
        foreach (var key in pianoKeys)
        {
            // AudioClipSet이 없으면 기존 AudioClip을 그대로 사용
            if (audioClipSets == null || audioClipSets.Length == 0)
            {
                Debug.Log($"No AudioClipSets found, using existing clips for {key.NoteName}");
                continue;
            }
            
            UpdateKeyAudioClip(key.NoteName, currentOctave);
        }
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
    
    private void UpdateKeyAudioClip(string noteName, int octave)
    {
        // 해당 음정의 건반 찾기
        PianoKey targetKey = pianoKeys.Find(key => key.NoteName == noteName);
        if (targetKey == null) 
        {
            Debug.LogWarning($"Piano key not found for note: {noteName}");
            return;
        }
        
        // AudioClipSet이 없으면 업데이트하지 않음 (기존 클립 유지)
        if (audioClipSets == null || audioClipSets.Length == 0)
        {
            Debug.Log($"No AudioClipSets available, keeping existing clip for {noteName}");
            return;
        }
        
        // 옥타브에 맞는 AudioClip 찾기
        AudioClip newClip = GetAudioClip(noteName, octave);
        if (newClip != null)
        {
            targetKey.UpdateAudioClip(newClip);
            Debug.Log($"Updated {noteName} to octave {octave}");
        }
        else
        {
            Debug.LogWarning($"No audio clip found for {noteName} in octave {octave}");
        }
    }
    
    private AudioClip GetAudioClip(string noteName, int octave)
    {
        // audioClipSets에서 해당 옥타브의 클립 찾기
        foreach (var clipSet in audioClipSets)
        {
            if (clipSet.octave == octave)
            {
                return clipSet.GetClip(noteName);
            }
        }
        
        // 해당 옥타브가 없으면 기본 옥타브 사용
        foreach (var clipSet in audioClipSets)
        {
            if (clipSet.octave == currentOctave)
            {
                return clipSet.GetClip(noteName);
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 현재 화면의 가장 주요한 옥타브를 기반으로 전체 건반 옥타브 설정
    /// OctaveController에서 호출됨
    /// </summary>
    public void SetGlobalOctave(int octave)
    {
        currentOctave = octave;
        
        Debug.Log($"Setting global octave to: {octave}");
        
        // AudioClipSet이 없으면 건반 이름만 업데이트
        if (audioClipSets == null || audioClipSets.Length == 0)
        {
            Debug.Log("No AudioClipSets available, octave change will not affect audio clips");
            
            // 옥타브 표시는 업데이트하지만 실제 오디오는 변경하지 않음
            // 대신 기존 AudioClip을 그대로 사용
            foreach (var key in pianoKeys)
            {
                Debug.Log($"Keeping existing audio clip for {key.NoteName}");
            }
            return;
        }
        
        // 특별히 설정된 음정이 없는 건반들은 새로운 전역 옥타브 적용
        foreach (var key in pianoKeys)
        {
            if (!currentNoteOctaves.ContainsKey(key.NoteName))
            {
                UpdateKeyAudioClip(key.NoteName, octave);
            }
        }
        
        Debug.Log($"Global octave set to: {octave}");
    }
    
    /// <summary>
    /// 현재 옥타브 값 반환
    /// </summary>
    public int GetCurrentOctave()
    {
        return currentOctave;
    }
    
    /// <summary>
    /// 디버그용: 현재 매핑 상태 출력
    /// </summary>
    [ContextMenu("Debug Current Mapping")]
    public void DebugCurrentMapping()
    {
        Debug.Log($"Current Global Octave: {currentOctave}");
        Debug.Log($"Piano Keys Count: {pianoKeys.Count}");
        Debug.Log($"AudioClipSets Count: {(audioClipSets != null ? audioClipSets.Length : 0)}");
        
        Debug.Log("Current Note Octaves:");
        foreach (var kvp in currentNoteOctaves)
        {
            Debug.Log($"  {kvp.Key}: Octave {kvp.Value}");
        }
        
        Debug.Log("Piano Keys:");
        foreach (var key in pianoKeys)
        {
            Debug.Log($"  {key.NoteName}: {(key != null ? "OK" : "NULL")}");
        }
    }
}

[System.Serializable]
public class AudioClipSet
{
    public int octave;
    public AudioClip C, CS, D, DS, E, F, FS, G, GS, A, AS, B;
    
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
            default: return null;
        }
    }
}