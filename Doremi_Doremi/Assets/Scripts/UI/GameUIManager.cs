using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 게임 UI를 관리하는 간단한 클래스
/// - 점수 표시
/// - 상태 메시지
/// - 버튼 관리
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [Header("UI 요소들")]
    [SerializeField] private Text gameStatusText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text currentSongText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button restartGameButton;
    
    [Header("메시지 설정")]
    [SerializeField] private string defaultMessage = "🎮 게임 시작 버튼을 누르세요!";
    [SerializeField] private string gameActiveMessage = "🎹 {0} 건반을 누르세요!\n({1}/{2})";
    [SerializeField] private string gameCompleteMessage = "🎉 완료! 정확도: {0:F1}%";
    
    // 이벤트들
    public event Action OnStartButtonClicked;
    public event Action OnRestartButtonClicked;
    
    private void Awake()
    {
        SetupButtons();
        SetDefaultUI();
    }
    
    /// <summary>
    /// 버튼 이벤트 설정
    /// </summary>
    private void SetupButtons()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(() => OnStartButtonClicked?.Invoke());
        }
        
        if (restartGameButton != null)
        {
            restartGameButton.onClick.AddListener(() => OnRestartButtonClicked?.Invoke());
        }
    }
    
    /// <summary>
    /// 기본 UI 상태 설정
    /// </summary>
    private void SetDefaultUI()
    {
        UpdateStatusText(defaultMessage);
        UpdateScoreText(0, 0, 0f);
    }
    
    /// <summary>
    /// 상태 텍스트 업데이트
    /// </summary>
    public void UpdateStatusText(string message)
    {
        if (gameStatusText != null)
        {
            gameStatusText.text = message;
        }
    }
    
    /// <summary>
    /// 점수 텍스트 업데이트
    /// </summary>
    public void UpdateScoreText(int correct, int wrong, float accuracy)
    {
        if (scoreText != null)
        {
            scoreText.text = $"정답: {correct} | 오답: {wrong}\n정확도: {accuracy:F1}%";
        }
    }
    
    /// <summary>
    /// 현재 곡 정보 업데이트
    /// </summary>
    public void UpdateCurrentSongText(string songTitle)
    {
        if (currentSongText != null)
        {
            currentSongText.text = songTitle;
        }
    }
    
    /// <summary>
    /// 게임 진행 중 상태 표시
    /// </summary>
    public void ShowGameProgressStatus(string expectedNote, int currentIndex, int totalNotes)
    {
        string noteDisplay = NoteDataParser.GetKoreanNoteName(expectedNote);
        string message = string.Format(gameActiveMessage, noteDisplay, currentIndex + 1, totalNotes);
        UpdateStatusText(message);
    }
    
    /// <summary>
    /// 게임 완료 상태 표시
    /// </summary>
    public void ShowGameCompleteStatus(float accuracy)
    {
        string message = string.Format(gameCompleteMessage, accuracy);
        UpdateStatusText(message);
    }
    
    /// <summary>
    /// 게임 시작 상태로 리셋
    /// </summary>
    public void ResetToStartState()
    {
        UpdateStatusText(defaultMessage);
        UpdateScoreText(0, 0, 0f);
    }
    
    /// <summary>
    /// 버튼 활성화/비활성화
    /// </summary>
    public void SetButtonsInteractable(bool interactable)
    {
        if (startGameButton != null)
            startGameButton.interactable = interactable;
        if (restartGameButton != null)
            restartGameButton.interactable = interactable;
    }
    
    /// <summary>
    /// 게임 상태에 따른 UI 자동 업데이트
    /// </summary>
    public void UpdateUIForGameState(GameStateManager gameState, string expectedNote = "")
    {
        if (gameState == null) return;
        
        // 점수 업데이트
        UpdateScoreText(gameState.CorrectAnswers, gameState.WrongAnswers, gameState.Accuracy);
        
        // 상태 메시지 업데이트
        if (gameState.IsGameActive)
        {
            if (gameState.IsWaitingForInput && !string.IsNullOrEmpty(expectedNote))
            {
                ShowGameProgressStatus(expectedNote, gameState.CurrentNoteIndex, gameState.TotalNotes);
            }
            else
            {
                UpdateStatusText("게임 진행중...");
            }
        }
        else
        {
            if (gameState.CurrentNoteIndex >= gameState.TotalNotes && gameState.TotalNotes > 0)
            {
                ShowGameCompleteStatus(gameState.Accuracy);
            }
            else
            {
                UpdateStatusText(defaultMessage);
            }
        }
    }
    
    // === 디버그 메서드들 ===
    
    [ContextMenu("테스트: 게임 진행 UI")]
    public void TestGameProgressUI()
    {
        ShowGameProgressStatus("C", 2, 10);
    }
    
    [ContextMenu("테스트: 게임 완료 UI")]
    public void TestGameCompleteUI()
    {
        ShowGameCompleteStatus(85.5f);
    }
    
    [ContextMenu("테스트: UI 리셋")]
    public void TestUIReset()
    {
        ResetToStartState();
    }
}
