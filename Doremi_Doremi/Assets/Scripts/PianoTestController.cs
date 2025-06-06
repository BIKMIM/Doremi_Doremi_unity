using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 피아노 시스템을 테스트하는 간단한 컨트롤러
/// </summary>
public class PianoTestController : MonoBehaviour
{
    [Header("Test UI")]
    [SerializeField] private Button testOctave3Button;
    [SerializeField] private Button testOctave4Button;
    [SerializeField] private Button testOctave5Button;
    [SerializeField] private Button testMixedNotesButton;
    
    private DynamicPianoMapper pianoMapper;
    
    private void Start()
    {
        pianoMapper = FindObjectOfType<DynamicPianoMapper>();
        
        if (pianoMapper == null)
        {
            Debug.LogError("DynamicPianoMapper not found in the scene!");
            return;
        }
        
        // 테스트용 버튼들 설정
        SetupTestButtons();
        
        Debug.Log("PianoTestController initialized. You can test different octaves.");
        Debug.Log("키보드 단축키:");
        Debug.Log("1 = 3옥타브 테스트");
        Debug.Log("2 = 4옥타브 테스트 (기본)");
        Debug.Log("3 = 5옥타브 테스트");
        Debug.Log("4 = 혼합 옥타브 테스트");
        Debug.Log("5 = 기본 옥타브로 리셋");
    }
    
    private void SetupTestButtons()
    {
        // 필요하다면 UI에서 버튼을 찾거나 생성할 수 있습니다
        // 지금은 키보드 입력으로 테스트하겠습니다
    }
    
    private void Update()
    {
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
    }
    
    /// <summary>
    /// 모든 건반을 3옥타브로 설정하여 테스트
    /// </summary>
    public void TestOctave3()
    {
        Dictionary<string, int> testNotes = new Dictionary<string, int>
        {
            {"C", 3}, {"D", 3}, {"E", 3}, {"F", 3}, 
            {"G", 3}, {"A", 3}, {"B", 3},
            {"C#", 3}, {"D#", 3}, {"F#", 3}, {"G#", 3}, {"A#", 3}
        };
        
        pianoMapper.UpdateCurrentNotes(testNotes);
        Debug.Log("Piano mapped to Octave 3. Press piano keys to test.");
    }
    
    /// <summary>
    /// 모든 건반을 4옥타브로 설정하여 테스트 (기본)
    /// </summary>
    public void TestOctave4()
    {
        Dictionary<string, int> testNotes = new Dictionary<string, int>
        {
            {"C", 4}, {"D", 4}, {"E", 4}, {"F", 4}, 
            {"G", 4}, {"A", 4}, {"B", 4},
            {"C#", 4}, {"D#", 4}, {"F#", 4}, {"G#", 4}, {"A#", 4}
        };
        
        pianoMapper.UpdateCurrentNotes(testNotes);
        Debug.Log("Piano mapped to Octave 4. Press piano keys to test.");
    }
    
    /// <summary>
    /// 모든 건반을 5옥타브로 설정하여 테스트
    /// </summary>
    public void TestOctave5()
    {
        Dictionary<string, int> testNotes = new Dictionary<string, int>
        {
            {"C", 5}, {"D", 5}, {"E", 5}, {"F", 5}, 
            {"G", 5}, {"A", 5}, {"B", 5},
            {"C#", 5}, {"D#", 5}, {"F#", 5}, {"G#", 5}, {"A#", 5}
        };
        
        pianoMapper.UpdateCurrentNotes(testNotes);
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
    public void TestIndividualNoteChange(string noteName, int octave)
    {
        pianoMapper.UpdateNoteOctave(noteName, octave);
        Debug.Log($"Changed {noteName} to octave {octave}");
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
}
