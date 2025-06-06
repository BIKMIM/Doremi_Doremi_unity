using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 피아노 시스템을 테스트하는 간단한 컨트롤러 (컴파일 에러 방지 버전)
/// </summary>
public class SimplePianoTestController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DynamicPianoMapper pianoMapper;
    [SerializeField] private ScoreAnalyzer scoreAnalyzer;
    
    [Header("Test Settings")]
    [SerializeField] private bool enableKeyboardControls = true;
    
    private void Start()
    {
        // 자동으로 컴포넌트 찾기
        if (pianoMapper == null)
        {
            pianoMapper = FindObjectOfType<DynamicPianoMapper>();
        }
        
        if (scoreAnalyzer == null)
        {
            scoreAnalyzer = FindObjectOfType<ScoreAnalyzer>();
        }
        
        if (pianoMapper == null)
        {
            Debug.LogError("DynamicPianoMapper not found!");
            return;
        }
        
        Debug.Log("SimplePianoTestController initialized!");
        Debug.Log("키보드 단축키:");
        Debug.Log("1 = 3옥타브 테스트");
        Debug.Log("2 = 4옥타브 테스트 (기본)");
        Debug.Log("3 = 5옥타브 테스트");
        Debug.Log("4 = 혼합 옥타브 테스트");
        Debug.Log("5 = 기본 옥타브로 리셋");
        Debug.Log("Q = C4 테스트");
        Debug.Log("W = G3 테스트");
        Debug.Log("E = E5 테스트");
    }
    
    private void Update()
    {
        if (!enableKeyboardControls) return;
        
        // 키보드로 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestOctave3();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestOctave4();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestOctave5();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestMixedNotes();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TestDefaultOctave();
        }
        
        // 개별 음정 테스트
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            TestIndividualNote("C", 4);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            TestIndividualNote("G", 3);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TestIndividualNote("E", 5);
        }
    }
    
    /// <summary>
    /// 모든 건반을 3옥타브로 설정하여 테스트
    /// </summary>
    public void TestOctave3()
    {
        if (scoreAnalyzer != null)
        {
            scoreAnalyzer.SetAllNotesToOctave(3);
        }
        else
        {
            SetAllKeysToOctaveDirectly(3);
        }
        Debug.Log("Piano mapped to Octave 3. Press piano keys to test.");
    }
    
    /// <summary>
    /// 모든 건반을 4옥타브로 설정하여 테스트 (기본)
    /// </summary>
    public void TestOctave4()
    {
        if (scoreAnalyzer != null)
        {
            scoreAnalyzer.SetAllNotesToOctave(4);
        }
        else
        {
            SetAllKeysToOctaveDirectly(4);
        }
        Debug.Log("Piano mapped to Octave 4. Press piano keys to test.");
    }
    
    /// <summary>
    /// 모든 건반을 5옥타브로 설정하여 테스트
    /// </summary>
    public void TestOctave5()
    {
        if (scoreAnalyzer != null)
        {
            scoreAnalyzer.SetAllNotesToOctave(5);
        }
        else
        {
            SetAllKeysToOctaveDirectly(5);
        }
        Debug.Log("Piano mapped to Octave 5. Press piano keys to test.");
    }
    
    /// <summary>
    /// 혼합된 옥타브로 테스트 (실제 게임 상황 시뮬레이션)
    /// </summary>
    public void TestMixedNotes()
    {
        Dictionary<string, int> testNotes = new Dictionary<string, int>
        {
            {"C", 4},   // 도
            {"G", 3},   // 낮은 솔
            {"E", 4},   // 미
            {"F#", 5},  // 높은 파#
            {"A", 3},   // 낮은 라
            {"D", 4}    // 레
        };
        
        pianoMapper.UpdateCurrentNotes(testNotes);
        Debug.Log("Piano mapped to mixed octaves:");
        Debug.Log("C=4옥타브, G=3옥타브, E=4옥타브, F#=5옥타브, A=3옥타브, D=4옥타브");
    }
    
    /// <summary>
    /// 기본 옥타브로 리셋
    /// </summary>
    public void TestDefaultOctave()
    {
        pianoMapper.SetGlobalOctave(4);
        pianoMapper.UpdateCurrentNotes(new Dictionary<string, int>());
        Debug.Log("Piano reset to default octave 4.");
    }
    
    /// <summary>
    /// 특정 음정의 옥타브만 변경하는 테스트
    /// </summary>
    public void TestIndividualNote(string noteName, int octave)
    {
        pianoMapper.UpdateNoteOctave(noteName, octave);
        Debug.Log($"Changed {noteName} to octave {octave}. Press {noteName} key to test.");
    }
    
    /// <summary>
    /// ScoreAnalyzer 없이 직접 설정
    /// </summary>
    private void SetAllKeysToOctaveDirectly(int octave)
    {
        Dictionary<string, int> allNotes = new Dictionary<string, int>
        {
            {"C", octave}, {"D", octave}, {"E", octave}, {"F", octave},
            {"G", octave}, {"A", octave}, {"B", octave},
            {"C#", octave}, {"D#", octave}, {"F#", octave}, {"G#", octave}, {"A#", octave}
        };
        
        pianoMapper.UpdateCurrentNotes(allNotes);
    }
    
    /// <summary>
    /// 현재 매핑 상태를 콘솔에 출력
    /// </summary>
    [ContextMenu("Debug Current Piano Mapping")]
    public void DebugCurrentMapping()
    {
        if (pianoMapper != null)
        {
            pianoMapper.DebugCurrentMapping();
        }
    }
    
    /// <summary>
    /// 키보드 컨트롤 활성화/비활성화
    /// </summary>
    public void SetKeyboardControls(bool enabled)
    {
        enableKeyboardControls = enabled;
        Debug.Log($"Keyboard controls {(enabled ? "enabled" : "disabled")}");
    }
}
