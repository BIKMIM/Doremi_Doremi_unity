using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 악보 정보를 분석해서 피아노 매퍼에게 현재 화면의 음정 정보를 전달하는 클래스
/// </summary>
public class ScoreAnalyzer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DynamicPianoMapper pianoMapper;
    
    [Header("Analysis Settings")]
    [SerializeField] private float analyzeInterval = 1.0f; // 분석 주기 (초)
    [SerializeField] private bool enableAutoAnalysis = true; // 자동 분석 활성화
    
    private Dictionary<string, int> lastAnalyzedNotes = new Dictionary<string, int>();
    
    private void Start()
    {
        if (pianoMapper == null)
        {
            pianoMapper = FindObjectOfType<DynamicPianoMapper>();
        }
        
        if (pianoMapper == null)
        {
            Debug.LogError("DynamicPianoMapper not found! Please assign it in the inspector.");
            return;
        }
        
        // 자동 분석이 활성화된 경우에만 주기적으로 분석
        if (enableAutoAnalysis)
        {
            InvokeRepeating(nameof(AnalyzeCurrentScore), 1.0f, analyzeInterval);
        }
        
        Debug.Log("ScoreAnalyzer initialized. Auto analysis: " + enableAutoAnalysis);
    }
    
    /// <summary>
    /// 현재 화면에 표시된 악보를 분석해서 음정과 옥타브 정보를 추출
    /// </summary>
    public void AnalyzeCurrentScore()
    {
        Dictionary<string, int> currentNotes = new Dictionary<string, int>();
        
        // 현재는 간단한 테스트용 분석
        // 실제 프로젝트의 음표 시스템과 연동하려면 NoteSpawner나 다른 컴포넌트 사용
        AnalyzeFromNoteSpawner(currentNotes);
        
        // 분석 결과가 이전과 다르면 피아노 매퍼 업데이트
        if (!AreDictionariesEqual(currentNotes, lastAnalyzedNotes))
        {
            Debug.Log("Score changed, updating piano mapping...");
            pianoMapper?.UpdateCurrentNotes(currentNotes);
            lastAnalyzedNotes = new Dictionary<string, int>(currentNotes);
            
            // 디버그 출력
            DebugAnalysisResult(currentNotes);
        }
    }
    
    /// <summary>
    /// NoteSpawner 컴포넌트를 통해 현재 활성화된 음표들 분석
    /// </summary>
    private void AnalyzeFromNoteSpawner(Dictionary<string, int> noteDict)
    {
        // NoteSpawner 찾기
        NoteSpawner noteSpawner = FindObjectOfType<NoteSpawner>();
        if (noteSpawner == null)
        {
            return;
        }
        
        // NoteSpawner에서 현재 활성화된 음표들 정보 가져오기
        // 실제 구조는 NoteSpawner 구현에 따라 다를 수 있음
        
        // 현재는 기본값으로 4옥타브 설정
        // 실제 프로젝트에서는 NoteSpawner의 실제 데이터를 사용해야 함
        
        // GameObject로 음표 찾기 시도
        AnalyzeActiveNoteGameObjects(noteDict);
    }
    
    /// <summary>
    /// 활성화된 GameObject들에서 음표 정보 분석
    /// </summary>
    private void AnalyzeActiveNoteGameObjects(Dictionary<string, int> noteDict)
    {
        // "Note"가 포함된 활성화된 GameObject들 찾기
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.activeInHierarchy && obj.name.ToLower().Contains("note"))
            {
                string noteName = ExtractNoteNameFromGameObject(obj);
                int octave = ExtractOctaveFromGameObject(obj);
                
                if (!string.IsNullOrEmpty(noteName) && octave > 0)
                {
                    noteDict[noteName] = octave;
                }
            }
        }
        
        // 만약 아무 음표도 찾지 못했다면 기본값 사용
        if (noteDict.Count == 0)
        {
            SetDefaultNotes(noteDict);
        }
    }
    
    /// <summary>
    /// 기본 음정들 설정 (테스트용)
    /// </summary>
    private void SetDefaultNotes(Dictionary<string, int> noteDict)
    {
        // 기본적으로 4옥타브의 주요 음정들 설정
        noteDict["C"] = 4;
        noteDict["D"] = 4;
        noteDict["E"] = 4;
        noteDict["F"] = 4;
        noteDict["G"] = 4;
        noteDict["A"] = 4;
        noteDict["B"] = 4;
    }
    
    private string ExtractNoteNameFromGameObject(GameObject noteObj)
    {
        // GameObject 이름에서 음정명 추출 (예: "Note_C4" -> "C")
        string objName = noteObj.name;
        
        if (objName.Contains("_"))
        {
            string[] parts = objName.Split('_');
            if (parts.Length > 1)
            {
                return ExtractNoteNameFromData(parts[1]);
            }
        }
        
        return ExtractNoteNameFromData(objName);
    }
    
    private int ExtractOctaveFromGameObject(GameObject noteObj)
    {
        // GameObject 이름에서 옥타브 추출 (예: "Note_C4" -> 4)
        string objName = noteObj.name;
        
        if (objName.Contains("_"))
        {
            string[] parts = objName.Split('_');
            if (parts.Length > 1)
            {
                return ExtractOctaveFromData(parts[1]);
            }
        }
        
        return ExtractOctaveFromData(objName);
    }
    
    private string ExtractNoteNameFromData(string data)
    {
        // "C4", "G#3", "Bb5" 등에서 음정명만 추출
        if (string.IsNullOrEmpty(data)) return "";
        
        if (data.Length >= 2 && (data[1] == '#' || data[1] == 'b'))
        {
            return data.Substring(0, 2); // "C#", "Bb" 등
        }
        else if (data.Length >= 1)
        {
            return data[0].ToString(); // "C", "G" 등
        }
        
        return "";
    }
    
    private int ExtractOctaveFromData(string data)
    {
        // "C4", "G#3" 등에서 옥타브 숫자 추출
        if (string.IsNullOrEmpty(data)) return 4;
        
        for (int i = data.Length - 1; i >= 0; i--)
        {
            if (char.IsDigit(data[i]))
            {
                if (int.TryParse(data[i].ToString(), out int octave))
                {
                    return octave;
                }
            }
        }
        
        return 4; // 기본 옥타브
    }
    
    private bool AreDictionariesEqual(Dictionary<string, int> dict1, Dictionary<string, int> dict2)
    {
        if (dict1.Count != dict2.Count) return false;
        
        foreach (var kvp in dict1)
        {
            if (!dict2.ContainsKey(kvp.Key) || dict2[kvp.Key] != kvp.Value)
            {
                return false;
            }
        }
        
        return true;
    }
    
    private void DebugAnalysisResult(Dictionary<string, int> notes)
    {
        if (notes.Count > 0)
        {
            Debug.Log("Current notes on screen:");
            foreach (var kvp in notes)
            {
                Debug.Log($"  {kvp.Key}{kvp.Value}");
            }
        }
        else
        {
            Debug.Log("No notes detected on screen, using defaults");
        }
    }
    
    /// <summary>
    /// 수동으로 특정 음정들을 설정 (테스트용)
    /// </summary>
    public void SetTestNotes(string notesString)
    {
        // 예: "C4,G3,E4" 형식의 문자열을 파싱
        Dictionary<string, int> testNotes = new Dictionary<string, int>();
        
        string[] notes = notesString.Split(',');
        foreach (string note in notes)
        {
            string trimmed = note.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                string noteName = ExtractNoteNameFromData(trimmed);
                int octave = ExtractOctaveFromData(trimmed);
                testNotes[noteName] = octave;
            }
        }
        
        pianoMapper?.UpdateCurrentNotes(testNotes);
        Debug.Log($"Test notes set: {notesString}");
    }
    
    /// <summary>
    /// 수동으로 특정 옥타브의 모든 음정 설정
    /// </summary>
    public void SetAllNotesToOctave(int octave)
    {
        Dictionary<string, int> allNotes = new Dictionary<string, int>
        {
            {"C", octave}, {"D", octave}, {"E", octave}, {"F", octave},
            {"G", octave}, {"A", octave}, {"B", octave},
            {"C#", octave}, {"D#", octave}, {"F#", octave}, {"G#", octave}, {"A#", octave}
        };
        
        pianoMapper?.UpdateCurrentNotes(allNotes);
        Debug.Log($"All notes set to octave {octave}");
    }
    
    /// <summary>
    /// 자동 분석 활성화/비활성화
    /// </summary>
    public void SetAutoAnalysis(bool enabled)
    {
        enableAutoAnalysis = enabled;
        
        if (enabled)
        {
            InvokeRepeating(nameof(AnalyzeCurrentScore), analyzeInterval, analyzeInterval);
        }
        else
        {
            CancelInvoke(nameof(AnalyzeCurrentScore));
        }
        
        Debug.Log($"Auto analysis {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// 즉시 한 번 분석 실행
    /// </summary>
    [ContextMenu("Analyze Now")]
    public void AnalyzeNow()
    {
        AnalyzeCurrentScore();
    }
}
