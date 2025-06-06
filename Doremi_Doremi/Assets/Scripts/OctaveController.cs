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
        InitializeComponents();
        SetupButtonEvents();
        UpdateDisplay();
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
    }
    
    private void SetupButtonEvents()
    {
        if (upArrowButton != null)
            upArrowButton.onClick.AddListener(OnUpArrowClicked);
        
        if (downArrowButton != null)
            downArrowButton.onClick.AddListener(OnDownArrowClicked);
    }
    
    private void OnUpArrowClicked()
    {
        // 옥타브 올리기 (최대 C5~C6)
        if (currentOctaveIndex < octaveValues.Length - 1)
        {
            currentOctaveIndex++;
            UpdateOctave();
        }
    }
    
    private void OnDownArrowClicked()
    {
        // 옥타브 내리기 (최소 C2~C3)
        if (currentOctaveIndex > 0)
        {
            currentOctaveIndex--;
            UpdateOctave();
        }
    }
    
    private void UpdateOctave()
    {
        // 피아노 매퍼에 새로운 옥타브 설정
        int newOctave = octaveValues[currentOctaveIndex];
        if (pianoMapper != null)
        {
            pianoMapper.SetGlobalOctave(newOctave);
        }
        
        // 화면 업데이트
        UpdateDisplay();
        
        Debug.Log($"Octave changed to: {octaveDescriptions[currentOctaveIndex]} (Octave {newOctave})");
    }
    
    private void UpdateDisplay()
    {
        if (octaveDisplayText != null)
        {
            octaveDisplayText.text = octaveDescriptions[currentOctaveIndex];
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
                    currentOctaveIndex = 0; // C2~C3
                    UpdateOctave();
                    break;
                case '2':
                    currentOctaveIndex = 1; // C3~C4
                    UpdateOctave();
                    break;
                case '3':
                    currentOctaveIndex = 2; // C4~C5
                    UpdateOctave();
                    break;
                case '4':
                    currentOctaveIndex = 3; // C5~C6
                    UpdateOctave();
                    break;
            }
        }
    }
}