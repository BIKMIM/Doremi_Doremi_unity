using UnityEngine;
using UnityEngine.UI;

public class OctaveController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button upArrowButton;
    [SerializeField] private Button downArrowButton;
    [SerializeField] private Text octaveDisplayText;
    
    [Header("Piano Reference")]
    [SerializeField] private DynamicPianoMapper pianoMapper;
    
    // 옥타브 설정
    private int currentOctaveIndex = 2; // C4~C5 (높은음자리 기본)
    private readonly string[] octaveDescriptions = {
        "C2~C3",
        "C3~C4\n(낮은음자리기본)",
        "C4~C5\n(높은음자리기본)",
        "C5~C6"
    };
    private readonly int[] octaveValues = { 2, 3, 4, 5 };
    
    private void Start()
    {
        Debug.Log("OctaveController Start() called");
        InitializeComponents();
        SetupButtonEvents();
        UpdateDisplay();
        Debug.Log("OctaveController initialization completed");
    }
    
    private void InitializeComponents()
    {
        // 자동으로 컴포넌트들을 찾기
        if (upArrowButton == null)
            upArrowButton = transform.Find("UpArrowButton")?.GetComponent<Button>();
        
        if (downArrowButton == null)
            downArrowButton = transform.Find("DownArrowButton")?.GetComponent<Button>();
        
        if (octaveDisplayText == null)
            octaveDisplayText = transform.Find("OctaveDisplay")?.GetComponent<Text>();
        
        if (pianoMapper == null)
            pianoMapper = GetComponentInParent<DynamicPianoMapper>();
        
        // 컴포넌트 확인
        if (upArrowButton == null) Debug.LogError("UpArrowButton not found!");
        if (downArrowButton == null) Debug.LogError("DownArrowButton not found!");
        if (octaveDisplayText == null) Debug.LogError("OctaveDisplay not found!");
        if (pianoMapper == null) Debug.LogError("DynamicPianoMapper not found!");
        
        Debug.Log($"Components found - UpButton: {upArrowButton != null}, DownButton: {downArrowButton != null}, Display: {octaveDisplayText != null}, Mapper: {pianoMapper != null}");
    }
    
    private void SetupButtonEvents()
    {
        if (upArrowButton != null)
        {
            upArrowButton.onClick.RemoveAllListeners();
            upArrowButton.onClick.AddListener(OnUpArrowClicked);
            Debug.Log("Up arrow button event connected");
        }
        
        if (downArrowButton != null)
        {
            downArrowButton.onClick.RemoveAllListeners();
            downArrowButton.onClick.AddListener(OnDownArrowClicked);
            Debug.Log("Down arrow button event connected");
        }
    }
    
    private void OnUpArrowClicked()
    {
        Debug.Log("Up arrow clicked!");
        
        // 옥타브 올리기 (최대 C5~C6)
        if (currentOctaveIndex < octaveValues.Length - 1)
        {
            currentOctaveIndex++;
            UpdateOctave();
        }
        else
        {
            Debug.Log("Already at maximum octave");
        }
    }
    
    private void OnDownArrowClicked()
    {
        Debug.Log("Down arrow clicked!");
        
        // 옥타브 내리기 (최소 C2~C3)
        if (currentOctaveIndex > 0)
        {
            currentOctaveIndex--;
            UpdateOctave();
        }
        else
        {
            Debug.Log("Already at minimum octave");
        }
    }
    
    private void UpdateOctave()
    {
        // 피아노 매퍼에 새로운 옥타브 설정
        int newOctave = octaveValues[currentOctaveIndex];
        
        Debug.Log($"Updating octave to: {octaveDescriptions[currentOctaveIndex]} (Octave {newOctave})");
        
        if (pianoMapper != null)
        {
            pianoMapper.SetGlobalOctave(newOctave);
            Debug.Log("Successfully set global octave on piano mapper");
        }
        else
        {
            Debug.LogError("PianoMapper is null! Cannot update octave.");
        }
        
        // 화면 업데이트
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (octaveDisplayText != null)
        {
            octaveDisplayText.text = octaveDescriptions[currentOctaveIndex];
            Debug.Log($"Display updated to: {octaveDescriptions[currentOctaveIndex]}");
        }
        else
        {
            Debug.LogError("OctaveDisplayText is null! Cannot update display.");
        }
        
        // 버튼 활성화/비활성화
        if (upArrowButton != null)
            upArrowButton.interactable = currentOctaveIndex < octaveValues.Length - 1;
        
        if (downArrowButton != null)
            downArrowButton.interactable = currentOctaveIndex > 0;
    }
    
    // 외부에서 옥타브를 설정할 때 사용
    public void SetOctave(int octave)
    {
        for (int i = 0; i < octaveValues.Length; i++)
        {
            if (octaveValues[i] == octave)
            {
                currentOctaveIndex = i;
                UpdateDisplay();
                break;
            }
        }
    }
    
    // 현재 옥타브 값 반환
    public int GetCurrentOctave()
    {
        return octaveValues[currentOctaveIndex];
    }
    
    // 키보드 입력도 지원 (기존 숫자키 기능 유지)
    private void Update()
    {
        // 숫자키 1-4로 옥타브 변경
        if (Input.inputString.Length > 0)
        {
            char keyPressed = Input.inputString[0];
            
            switch (keyPressed)
            {
                case '1':
                    if (currentOctaveIndex != 0)
                    {
                        currentOctaveIndex = 0; // C2~C3
                        UpdateOctave();
                        Debug.Log("Keyboard shortcut: Set to octave 1 (C2~C3)");
                    }
                    break;
                case '2':
                    if (currentOctaveIndex != 1)
                    {
                        currentOctaveIndex = 1; // C3~C4
                        UpdateOctave();
                        Debug.Log("Keyboard shortcut: Set to octave 2 (C3~C4)");
                    }
                    break;
                case '3':
                    if (currentOctaveIndex != 2)
                    {
                        currentOctaveIndex = 2; // C4~C5
                        UpdateOctave();
                        Debug.Log("Keyboard shortcut: Set to octave 3 (C4~C5)");
                    }
                    break;
                case '4':
                    if (currentOctaveIndex != 3)
                    {
                        currentOctaveIndex = 3; // C5~C6
                        UpdateOctave();
                        Debug.Log("Keyboard shortcut: Set to octave 4 (C5~C6)");
                    }
                    break;
            }
        }
    }
    
    // 디버그용 메서드
    [ContextMenu("Debug Octave Controller")]
    public void DebugOctaveController()
    {
        Debug.Log($"=== OctaveController Debug Info ===");
        Debug.Log($"Current Octave Index: {currentOctaveIndex}");
        Debug.Log($"Current Octave Value: {octaveValues[currentOctaveIndex]}");
        Debug.Log($"Current Description: {octaveDescriptions[currentOctaveIndex]}");
        Debug.Log($"Up Button: {(upArrowButton != null ? "Present" : "Missing")}");
        Debug.Log($"Down Button: {(downArrowButton != null ? "Present" : "Missing")}");
        Debug.Log($"Display Text: {(octaveDisplayText != null ? "Present" : "Missing")}");
        Debug.Log($"Piano Mapper: {(pianoMapper != null ? "Present" : "Missing")}");
        Debug.Log($"=== End Debug Info ===");
    }
}