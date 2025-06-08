using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 피아노 키 클릭을 처리하고 게임 컨트롤러에 알리는 컴포넌트
/// - 각 피아노 키에 이 컴포넌트를 붙여서 사용
/// - 게임 컨트롤러들과 자동 연결
/// </summary>
public class PianoKeyHandler : MonoBehaviour
{
    [Header("이 키의 정보")]
    public string noteName = "C4";  // 이 키가 나타내는 음표 (예: C4, D4, E4...)
    
    [Header("참조 (자동 연결)")]
    public SongGameController songGameController;
    public ModularGameController modularGameController;
    public NoteColorManager colorManager;
    
    private Button button;
    
    private void Start()
    {
        // 버튼 컴포넌트 가져오기
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnKeyPressed);
        }
        
        // 자동으로 컨트롤러들 찾기
        FindGameControllers();
    }
    
    /// <summary>
    /// 게임 컨트롤러들 자동 검색
    /// </summary>
    private void FindGameControllers()
    {
        if (songGameController == null)
        {
            songGameController = FindObjectOfType<SongGameController>();
        }
        
        if (modularGameController == null)
        {
            modularGameController = FindObjectOfType<ModularGameController>();
        }
        
        if (colorManager == null)
        {
            colorManager = FindObjectOfType<NoteColorManager>();
        }
        
        Debug.Log($"🎹 PianoKeyHandler ({noteName}) 초기화:");
        Debug.Log($"   SongGameController: {(songGameController != null ? "✅" : "❌")}");
        Debug.Log($"   ModularGameController: {(modularGameController != null ? "✅" : "❌")}");
        Debug.Log($"   NoteColorManager: {(colorManager != null ? "✅" : "❌")}");
    }
    
    /// <summary>
    /// 피아노 키가 눌렸을 때 호출되는 메서드
    /// </summary>
    public void OnKeyPressed()
    {
        Debug.Log($"🎹 {noteName} 키가 눌렸습니다.");
        
        // SongGameController에 알림
        if (songGameController != null)
        {
            try
            {
                songGameController.OnKeyPressed(noteName);
                Debug.Log($"✅ SongGameController에 {noteName} 전달");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ SongGameController.OnKeyPressed 호출 실패: {e.Message}");
            }
        }
        
        // ModularGameController에 알림
        if (modularGameController != null)
        {
            try
            {
                modularGameController.OnKeyPressed(noteName);
                Debug.Log($"✅ ModularGameController에 {noteName} 전달");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ ModularGameController.OnKeyPressed 호출 실패: {e.Message}");
            }
        }
        
        // 둘 다 없으면 경고
        if (songGameController == null && modularGameController == null)
        {
            Debug.LogWarning("⚠️ 게임 컨트롤러를 찾을 수 없습니다!");
        }
    }
    
    /// <summary>
    /// 수동으로 게임 컨트롤러 재검색
    /// </summary>
    [ContextMenu("게임 컨트롤러 재검색")]
    public void RefreshGameControllers()
    {
        FindGameControllers();
    }
    
    /// <summary>
    /// 키 테스트 (수동 호출용)
    /// </summary>
    [ContextMenu("키 테스트")]
    public void TestKey()
    {
        OnKeyPressed();
    }
    
    /// <summary>
    /// 음표 이름 설정 (Inspector에서 변경 시 자동 호출)
    /// </summary>
    private void OnValidate()
    {
        // Inspector에서 noteName이 변경되면 GameObject 이름도 변경
        if (!string.IsNullOrEmpty(noteName))
        {
            gameObject.name = $"Key_{noteName}";
        }
    }
}
