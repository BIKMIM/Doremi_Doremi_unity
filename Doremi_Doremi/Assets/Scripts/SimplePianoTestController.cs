using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DynamicPianoMapper를 테스트하는 컨트롤러 (정리된 버전)
/// </summary>
public class SimplePianoTestController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DynamicPianoMapper pianoMapper;
    [SerializeField] private ScoreAnalyzer scoreAnalyzer;
    
    [Header("Test Settings")]
    [SerializeField] private bool enableKeyboardControls = true;
    [SerializeField] private bool showDebugMessages = true;
    
    private float lastInputCheckTime = 0f;
    
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
            Debug.LogError("DynamicPianoMapper를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log("=== 피아노 계이름 맞추기 게임 시작! ===");
        Debug.Log("✅ DynamicPianoMapper 연결됨");
        Debug.Log("✅ 실제 옥타브별 AudioClip 사용");
        Debug.Log("");
        Debug.Log("🎹 키보드 단축키:");
        Debug.Log("1 = 3옥타브 테스트 (낮은 소리)");
        Debug.Log("2 = 4옥타브 테스트 (기본 소리)");
        Debug.Log("3 = 5옥타브 테스트 (높은 소리)");
        Debug.Log("4 = 혼합 옥타브 테스트 (실제 게임 상황)");
        Debug.Log("5 = 기본 옥타브로 리셋");
        Debug.Log("Q = C4 개별 테스트");
        Debug.Log("W = G3 개별 테스트");
        Debug.Log("E = E5 개별 테스트");
        Debug.Log("=====================");
    }
    
    private void Update()
    {
        if (!enableKeyboardControls) return;
        
        // 5초마다 작동 상태 메시지 출력
        if (showDebugMessages && Time.time - lastInputCheckTime > 5f)
        {
            Debug.Log($"[{Time.time:F1}s] 동적 옥타브 매핑 시스템 작동 중... 키를 눌러보세요!");
            lastInputCheckTime = Time.time;
        }
        
        // 키보드 입력 감지
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("1키 감지됨!");
            TestOctave3();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("2키 감지됨!");
            TestOctave4();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("3키 감지됨!");
            TestOctave5();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("4키 감지됨!");
            TestMixedNotes();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("5키 감지됨!");
            TestDefaultOctave();
        }
        
        // 개별 음정 테스트
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q키 감지됨!");
            TestIndividualNote("C", 4);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("W키 감지됨!");
            TestIndividualNote("G", 3);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E키 감지됨!");
            TestIndividualNote("E", 5);
        }
        
        // 숫자패드도 지원
        else if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            Debug.Log("숫자패드 1키 감지됨!");
            TestOctave3();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            Debug.Log("숫자패드 2키 감지됨!");
            TestOctave4();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            Debug.Log("숫자패드 3키 감지됨!");
            TestOctave5();
        }
        
        // 스페이스바로 빠른 테스트
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("스페이스바 감지됨! 기본 테스트 실행!");
            TestOctave4();
        }
    }
    
    /// <summary>
    /// 모든 건반을 3옥타브로 설정하여 테스트
    /// </summary>
    public void TestOctave3()
    {
        Debug.Log("=== 🎵 3옥타브 테스트 시작 ===");
        SetAllNotesToOctave(3);
        Debug.Log("모든 건반이 3옥타브 AudioClip으로 설정됨 (낮은 소리)");
        Debug.Log("🎹 피아노 건반을 클릭해서 낮은 옥타브 소리를 확인하세요!");
    }
    
    /// <summary>
    /// 모든 건반을 4옥타브로 설정하여 테스트 (기본)
    /// </summary>
    public void TestOctave4()
    {
        Debug.Log("=== 🎵 4옥타브 테스트 시작 ===");
        SetAllNotesToOctave(4);
        Debug.Log("모든 건반이 4옥타브 AudioClip으로 설정됨 (기본 소리)");
        Debug.Log("🎹 피아노 건반을 클릭해서 기본 옥타브 소리를 확인하세요!");
    }
    
    /// <summary>
    /// 모든 건반을 5옥타브로 설정하여 테스트
    /// </summary>
    public void TestOctave5()
    {
        Debug.Log("=== 🎵 5옥타브 테스트 시작 ===");
        SetAllNotesToOctave(5);
        Debug.Log("모든 건반이 5옥타브 AudioClip으로 설정됨 (높은 소리)");
        Debug.Log("🎹 피아노 건반을 클릭해서 높은 옥타브 소리를 확인하세요!");
    }
    
    /// <summary>
    /// 혼합된 옥타브로 테스트 (실제 게임 상황 시뮬레이션)
    /// </summary>
    public void TestMixedNotes()
    {
        Debug.Log("=== 🎮 혼합 옥타브 테스트 시작 (실제 게임 시뮬레이션) ===");
        Dictionary<string, int> testNotes = new Dictionary<string, int>
        {
            {"C", 4},   // 도 → 4옥타브 AudioClip
            {"G", 3},   // 솔 → 3옥타브 AudioClip (낮은 솔)
            {"E", 4},   // 미 → 4옥타브 AudioClip
            {"F#", 5},  // 파# → 5옥타브 AudioClip (높은 파#)
            {"A", 3},   // 라 → 3옥타브 AudioClip (낮은 라)
            {"D", 4}    // 레 → 4옥타브 AudioClip
        };
        
        pianoMapper.UpdateCurrentNotes(testNotes);
        Debug.Log("혼합 옥타브 설정 완료:");
        Debug.Log("🎹 C건반 = 4옥타브, G건반 = 3옥타브, E건반 = 4옥타브");
        Debug.Log("🎹 F#건반 = 5옥타브, A건반 = 3옥타브, D건반 = 4옥타브");
        Debug.Log("각 건반을 눌러서 서로 다른 옥타브 소리를 확인해보세요!");
    }
    
    /// <summary>
    /// 기본 옥타브로 리셋
    /// </summary>
    public void TestDefaultOctave()
    {
        Debug.Log("=== 🔄 기본 옥타브로 리셋 ===");
        pianoMapper.SetGlobalOctave(4);
        pianoMapper.UpdateCurrentNotes(new Dictionary<string, int>());
        Debug.Log("모든 건반이 기본 4옥타브로 리셋됨.");
    }
    
    /// <summary>
    /// 특정 음정의 옥타브만 변경하는 테스트
    /// </summary>
    public void TestIndividualNote(string noteName, int octave)
    {
        Debug.Log($"=== 🎯 {noteName}{octave} 개별 테스트 ===");
        pianoMapper.UpdateNoteOctave(noteName, octave);
        Debug.Log($"{noteName} 건반이 {octave}옥타브 AudioClip으로 설정됨.");
        Debug.Log($"🎹 {noteName} 건반을 클릭해서 확인하세요!");
    }
    
    /// <summary>
    /// 모든 음정을 특정 옥타브로 설정
    /// </summary>
    private void SetAllNotesToOctave(int octave)
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
        Debug.Log("=== 🔍 현재 피아노 매핑 상태 ===");
        pianoMapper?.DebugCurrentMapping();
    }
    
    /// <summary>
    /// 키보드 컨트롤 활성화/비활성화
    /// </summary>
    public void SetKeyboardControls(bool enabled)
    {
        enableKeyboardControls = enabled;
        Debug.Log($"키보드 컨트롤 {(enabled ? "활성화" : "비활성화")}");
    }
    
    /// <summary>
    /// 디버그 메시지 활성화/비활성화
    /// </summary>
    public void SetDebugMessages(bool enabled)
    {
        showDebugMessages = enabled;
        Debug.Log($"디버그 메시지 {(enabled ? "활성화" : "비활성화")}");
    }
    
    /// <summary>
    /// 수동 테스트용 Context Menu 버튼들
    /// </summary>
    [ContextMenu("Manual Test - 3옥타브")]
    public void ManualTestOctave3() { TestOctave3(); }
    
    [ContextMenu("Manual Test - 4옥타브")]
    public void ManualTestOctave4() { TestOctave4(); }
    
    [ContextMenu("Manual Test - 5옥타브")]
    public void ManualTestOctave5() { TestOctave5(); }
    
    [ContextMenu("Manual Test - 혼합 옥타브")]
    public void ManualTestMixed() { TestMixedNotes(); }
    
    [ContextMenu("Manual Test - 리셋")]
    public void ManualTestReset() { TestDefaultOctave(); }
}
