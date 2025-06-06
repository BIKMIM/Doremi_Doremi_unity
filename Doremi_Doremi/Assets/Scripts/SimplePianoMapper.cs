using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 간단한 피아노 매퍼 - 현재 화면의 음정에 따라 건반 소리를 동적으로 변경
/// </summary>
public class SimplePianoMapper : MonoBehaviour
{
    [Header("Piano Key GameObjects")]
    public GameObject[] pianoKeys; // Inspector에서 할당
    
    [Header("Current Octave")]
    public int currentOctave = 4;
    
    private Dictionary<string, AudioSource> keyAudioSources = new Dictionary<string, AudioSource>();
    private Dictionary<string, int> currentNoteOctaves = new Dictionary<string, int>();
    
    void Start()
    {
        InitializePianoKeys();
        Debug.Log("SimplePianoMapper initialized with " + keyAudioSources.Count + " keys");
        Debug.Log("키보드 단축키: 1=3옥타브, 2=4옥타브, 3=5옥타브");
    }
    
    void Update()
    {
        // 키보드 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetAllKeysToOctave(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetAllKeysToOctave(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetAllKeysToOctave(5);
        }
    }
    
    void InitializePianoKeys()
    {
        keyAudioSources.Clear();
        
        // 자동으로 패널에서 건반들 찾기
        Transform pianoPanel = transform.parent.Find("Panel_Piano");
        if (pianoPanel == null)
        {
            Debug.LogError("Panel_Piano not found!");
            return;
        }
        
        // 각 건반의 AudioSource 저장
        string[] keyNames = {"C4", "C4S", "D4", "D4S", "E4", "F4", "F4S", "G4", "G4S", "A4", "A4S", "B4", "C5"};
        
        foreach (string keyName in keyNames)
        {
            Transform keyTransform = pianoPanel.Find(keyName);
            if (keyTransform != null)
            {
                AudioSource audioSource = keyTransform.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    string noteName = GetNoteNameFromKey(keyName);
                    keyAudioSources[noteName] = audioSource;
                    Debug.Log($"Found key: {keyName} -> {noteName}");
                }
            }
        }
    }
    
    string GetNoteNameFromKey(string keyName)
    {
        if (keyName.Contains("S"))
        {
            return keyName[0] + "#"; // "C4S" -> "C#"
        }
        return keyName[0].ToString(); // "C4" -> "C"
    }
    
    public void SetAllKeysToOctave(int octave)
    {
        currentOctave = octave;
        Debug.Log($"모든 건반을 {octave}옥타브로 설정");
        
        // 실제로는 여기서 AudioClip을 바꿔야 하지만
        // 지금은 간단히 볼륨으로 차이를 표현
        foreach (var kvp in keyAudioSources)
        {
            AudioSource audioSource = kvp.Value;
            if (audioSource != null)
            {
                // 옥타브에 따라 피치 조정 (임시)
                float pitchMultiplier = Mathf.Pow(2f, octave - 4); // 4옥타브 기준
                audioSource.pitch = pitchMultiplier;
            }
        }
    }
    
    public void UpdateNoteOctave(string noteName, int octave)
    {
        currentNoteOctaves[noteName] = octave;
        
        if (keyAudioSources.ContainsKey(noteName))
        {
            AudioSource audioSource = keyAudioSources[noteName];
            if (audioSource != null)
            {
                float pitchMultiplier = Mathf.Pow(2f, octave - 4);
                audioSource.pitch = pitchMultiplier;
                Debug.Log($"{noteName} 키를 {octave}옥타브로 설정 (피치: {pitchMultiplier})");
            }
        }
    }
}
