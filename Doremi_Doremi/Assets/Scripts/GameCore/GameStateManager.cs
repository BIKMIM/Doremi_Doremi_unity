using UnityEngine;
using System;

/// <summary>
/// 게임 상태를 관리하는 단순한 클래스
/// - 현재 게임 상태 추적
/// - 점수 관리
/// - 진행률 계산
/// </summary>
public class GameStateManager : MonoBehaviour
{
    [Header("게임 상태")]
    [SerializeField] private bool gameIsActive = false;
    [SerializeField] private bool waitingForInput = false;
    [SerializeField] private int currentNoteIndex = 0;
    [SerializeField] private int totalNotes = 0;
    
    [Header("점수 시스템")]
    [SerializeField] private int correctAnswers = 0;
    [SerializeField] private int wrongAnswers = 0;
    
    // 이벤트들
    public event Action OnGameStart;
    public event Action OnGameStop;
    public event Action OnGameComplete;
    public event Action<int> OnCorrectAnswer;  // 점수 전달
    public event Action<int> OnWrongAnswer;    // 점수 전달
    public event Action<float> OnProgressUpdate; // 진행률 전달
    
    // 프로퍼티들
    public bool IsGameActive => gameIsActive;
    public bool IsWaitingForInput => waitingForInput;
    public int CurrentNoteIndex => currentNoteIndex;
    public int TotalNotes => totalNotes;
    public int CorrectAnswers => correctAnswers;
    public int WrongAnswers => wrongAnswers;
    public float Accuracy => (correctAnswers + wrongAnswers) > 0 ? 
        (float)correctAnswers / (correctAnswers + wrongAnswers) * 100f : 0f;
    public float Progress => totalNotes > 0 ? (float)currentNoteIndex / totalNotes : 0f;
    
    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame(int totalNoteCount)
    {
        gameIsActive = true;
        waitingForInput = false;
        currentNoteIndex = 0;
        totalNotes = totalNoteCount;
        correctAnswers = 0;
        wrongAnswers = 0;
        
        OnGameStart?.Invoke();
        Debug.Log($"🎮 게임 시작 - 총 {totalNotes}개 음표");
    }
    
    /// <summary>
    /// 게임 정지
    /// </summary>
    public void StopGame()
    {
        gameIsActive = false;
        waitingForInput = false;
        
        OnGameStop?.Invoke();
        Debug.Log("🛑 게임 정지");
    }
    
    /// <summary>
    /// 다음 음표로 진행
    /// </summary>
    public bool MoveToNextNote()
    {
        if (!gameIsActive) return false;
        
        waitingForInput = true;
        OnProgressUpdate?.Invoke(Progress);
        
        // 게임 완료 체크
        if (currentNoteIndex >= totalNotes)
        {
            CompleteGame();
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 정답 처리
    /// </summary>
    public void ProcessCorrectAnswer()
    {
        if (!gameIsActive || !waitingForInput) return;
        
        correctAnswers++;
        waitingForInput = false;
        currentNoteIndex++;
        
        OnCorrectAnswer?.Invoke(correctAnswers);
        Debug.Log($"✅ 정답! ({correctAnswers}/{currentNoteIndex})");
    }
    
    /// <summary>
    /// 오답 처리
    /// </summary>
    public void ProcessWrongAnswer()
    {
        if (!gameIsActive || !waitingForInput) return;
        
        wrongAnswers++;
        waitingForInput = false;
        currentNoteIndex++;
        
        OnWrongAnswer?.Invoke(wrongAnswers);
        Debug.Log($"❌ 오답! ({wrongAnswers}/{currentNoteIndex})");
    }
    
    /// <summary>
    /// 게임 완료
    /// </summary>
    private void CompleteGame()
    {
        gameIsActive = false;
        waitingForInput = false;
        
        OnGameComplete?.Invoke();
        Debug.Log($"🎉 게임 완료! 정확도: {Accuracy:F1}%");
    }
    
    /// <summary>
    /// 게임 상태 초기화
    /// </summary>
    public void ResetGame()
    {
        gameIsActive = false;
        waitingForInput = false;
        currentNoteIndex = 0;
        totalNotes = 0;
        correctAnswers = 0;
        wrongAnswers = 0;
        
        Debug.Log("🔄 게임 상태 초기화");
    }
}
