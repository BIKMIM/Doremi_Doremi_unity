using UnityEngine;
using System;

/// <summary>
/// 게임 상태를 관리하는 클래스
/// - 점수, 진행도, 현재 상태 등을 담당
/// </summary>
[System.Serializable]
public class GameState
{
    [Header("게임 진행 상태")]
    public bool isActive = false;
    public bool isWaitingForInput = false;
    public bool isCompleted = false;
    
    [Header("점수 시스템")]
    public int correctAnswers = 0;
    public int wrongAnswers = 0;
    public int totalNotes = 0;
    
    [Header("진행도")]
    public int currentNoteIndex = 0;
    
    // 이벤트
    public event Action OnGameStart;
    public event Action OnGameStop;
    public event Action OnGameComplete;
    public event Action<int> OnCorrectAnswer;
    public event Action<int> OnWrongAnswer;
    public event Action<float> OnProgressChanged;
    
    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame(int totalNoteCount)
    {
        isActive = true;
        isWaitingForInput = false;
        isCompleted = false;
        
        correctAnswers = 0;
        wrongAnswers = 0;
        totalNotes = totalNoteCount;
        currentNoteIndex = 0;
        
        OnGameStart?.Invoke();
        OnProgressChanged?.Invoke(GetProgress());
        
        Debug.Log($"🎮 게임 시작 - 총 {totalNotes}개 음표");
    }
    
    /// <summary>
    /// 게임 중지
    /// </summary>
    public void StopGame()
    {
        isActive = false;
        isWaitingForInput = false;
        
        OnGameStop?.Invoke();
        Debug.Log("🛑 게임 중지");
    }
    
    /// <summary>
    /// 정답 처리
    /// </summary>
    public void ProcessCorrectAnswer()
    {
        if (!isActive) return;
        
        correctAnswers++;
        isWaitingForInput = false;
        
        OnCorrectAnswer?.Invoke(correctAnswers);
        OnProgressChanged?.Invoke(GetProgress());
        
        Debug.Log($"✅ 정답! ({correctAnswers}/{totalNotes})");
    }
    
    /// <summary>
    /// 오답 처리
    /// </summary>
    public void ProcessWrongAnswer()
    {
        if (!isActive) return;
        
        wrongAnswers++;
        isWaitingForInput = false;
        
        OnWrongAnswer?.Invoke(wrongAnswers);
        OnProgressChanged?.Invoke(GetProgress());
        
        Debug.Log($"❌ 오답! (오답: {wrongAnswers}개)");
    }
    
    /// <summary>
    /// 다음 음표로 이동
    /// </summary>
    public void MoveToNextNote()
    {
        if (!isActive) return;
        
        currentNoteIndex++;
        
        if (currentNoteIndex >= totalNotes)
        {
            CompleteGame();
        }
        else
        {
            isWaitingForInput = true;
            OnProgressChanged?.Invoke(GetProgress());
        }
    }
    
    /// <summary>
    /// 게임 완료 처리
    /// </summary>
    private void CompleteGame()
    {
        isActive = false;
        isWaitingForInput = false;
        isCompleted = true;
        
        OnGameComplete?.Invoke();
        OnProgressChanged?.Invoke(1.0f);
        
        float accuracy = GetAccuracy();
        Debug.Log($"🎉 게임 완료! 정확도: {accuracy:F1}%");
    }
    
    /// <summary>
    /// 입력 대기 상태로 변경
    /// </summary>
    public void SetWaitingForInput()
    {
        if (isActive)
        {
            isWaitingForInput = true;
        }
    }
    
    /// <summary>
    /// 진행도 계산 (0.0 ~ 1.0)
    /// </summary>
    public float GetProgress()
    {
        if (totalNotes <= 0) return 0f;
        return (float)currentNoteIndex / totalNotes;
    }
    
    /// <summary>
    /// 정확도 계산 (0.0 ~ 100.0)
    /// </summary>
    public float GetAccuracy()
    {
        int totalAnswered = correctAnswers + wrongAnswers;
        if (totalAnswered <= 0) return 0f;
        return (float)correctAnswers / totalAnswered * 100f;
    }
    
    /// <summary>
    /// 게임 결과 요약 반환
    /// </summary>
    public string GetGameSummary()
    {
        float accuracy = GetAccuracy();
        bool isPerfect = wrongAnswers == 0 && isCompleted;
        
        return $"정답: {correctAnswers}/{totalNotes}, 오답: {wrongAnswers}개\n" +
               $"정확도: {accuracy:F1}% {(isPerfect ? "🎉 완벽!" : "")}";
    }
    
    /// <summary>
    /// 상태 초기화
    /// </summary>
    public void Reset()
    {
        isActive = false;
        isWaitingForInput = false;
        isCompleted = false;
        
        correctAnswers = 0;
        wrongAnswers = 0;
        totalNotes = 0;
        currentNoteIndex = 0;
    }
}
