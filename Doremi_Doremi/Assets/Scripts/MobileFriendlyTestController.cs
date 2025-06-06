using UnityEngine;

/// <summary>
/// 모바일 친화적 화면 분할 테스트 컨트롤러
/// 마디선 개수에 따른 화면 분할과 음표 분산 배치를 실시간으로 조정
/// </summary>
public class MobileFriendlyTestController : MonoBehaviour
{
    [Header("🎼 NoteSpawner 참조")]
    public NoteSpawner noteSpawner;

    [Header("📱 모바일 친화적 설정")]
    [Space(10)]
    [Range(0.8f, 0.95f)]
    [Tooltip("화면 사용 비율 (모바일 최적화)")]
    public float screenUsage = 0.9f;
    
    [Range(0.05f, 0.15f)]
    [Tooltip("마디 내부 여백 비율")]
    public float measurePadding = 0.08f;

    [Range(0.02f, 0.1f)]
    [Tooltip("화면 가장자리 여백")]
    public float screenMargin = 0.05f;

    [Header("🔧 디버그 설정")]
    [Space(10)]
    [Tooltip("화면 분할 디버그 정보 출력")]
    public bool showDebugInfo = true;

    [Tooltip("인스펙터 값 변경 시 자동으로 악보 새로고침")]
    public bool autoRefresh = true;

    [Tooltip("업데이트 간격 (초)")]
    [Range(0.1f, 1.0f)]
    public float updateInterval = 0.3f;

    // 이전 값들을 저장하여 변경 감지
    private float lastScreenUsage;
    private float lastMeasurePadding;
    private float lastScreenMargin;
    private bool lastShowDebugInfo;
    private float lastUpdateTime;

    void Start()
    {
        // NoteSpawner 유효성 검사
        if (noteSpawner == null)
        {
            noteSpawner = FindObjectOfType<NoteSpawner>();
            if (noteSpawner == null)
            {
                Debug.LogError("❌ NoteSpawner를 찾을 수 없습니다! NoteSpawner 컴포넌트를 참조해주세요.");
                return;
            }
        }

        // 초기값 동기화
        SyncFromNoteSpawner();
        SaveCurrentValues();
        
        Debug.Log("📱 모바일 친화적 테스트 컨트롤러 초기화 완료");
    }

    void Update()
    {
        if (!autoRefresh || noteSpawner == null) return;

        // 업데이트 간격 체크
        if (Time.time - lastUpdateTime < updateInterval) return;

        // 값 변경 감지
        if (HasValuesChanged())
        {
            Debug.Log("🔄 설정 변경 감지 - 악보 업데이트 중...");
            ApplyToNoteSpawner();
            SaveCurrentValues();
            lastUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// NoteSpawner에서 현재 값들을 가져와서 동기화
    /// </summary>
    public void SyncFromNoteSpawner()
    {
        if (noteSpawner == null) return;

        screenUsage = noteSpawner.screenUsageRatio;
        measurePadding = noteSpawner.measurePaddingRatio;
        screenMargin = noteSpawner.screenMarginRatio;
        showDebugInfo = noteSpawner.showScreenDivisionDebug;
    }

    /// <summary>
    /// 현재 설정을 NoteSpawner에 적용
    /// </summary>
    public void ApplyToNoteSpawner()
    {
        if (noteSpawner == null) return;

        // 값 적용
        noteSpawner.screenUsageRatio = screenUsage;
        noteSpawner.measurePaddingRatio = measurePadding;
        noteSpawner.screenMarginRatio = screenMargin;
        noteSpawner.showScreenDivisionDebug = showDebugInfo;

        // 악보 새로고침
        noteSpawner.RefreshCurrentSong();

        Debug.Log($"📱 설정 적용: 화면사용={screenUsage:F2}, 마디여백={measurePadding:F2}, 화면여백={screenMargin:F2}");
    }

    /// <summary>
    /// 설정값 변경 여부 확인
    /// </summary>
    private bool HasValuesChanged()
    {
        return !Mathf.Approximately(screenUsage, lastScreenUsage) ||
               !Mathf.Approximately(measurePadding, lastMeasurePadding) ||
               !Mathf.Approximately(screenMargin, lastScreenMargin) ||
               showDebugInfo != lastShowDebugInfo;
    }

    /// <summary>
    /// 현재 값들을 저장
    /// </summary>
    private void SaveCurrentValues()
    {
        lastScreenUsage = screenUsage;
        lastMeasurePadding = measurePadding;
        lastScreenMargin = screenMargin;
        lastShowDebugInfo = showDebugInfo;
    }

    // 🔧 버튼으로 호출할 수 있는 공개 메서드들

    [ContextMenu("설정 즉시 적용")]
    public void ApplySettingsNow()
    {
        ApplyToNoteSpawner();
        SaveCurrentValues();
        Debug.Log("✅ 설정이 즉시 적용되었습니다.");
    }

    [ContextMenu("NoteSpawner에서 동기화")]
    public void SyncFromNoteSpawnerNow()
    {
        SyncFromNoteSpawner();
        SaveCurrentValues();
        Debug.Log("✅ NoteSpawner 설정과 동기화되었습니다.");
    }

    [ContextMenu("기본값으로 리셋")]
    public void ResetToDefaults()
    {
        screenUsage = 0.9f;
        measurePadding = 0.08f;
        screenMargin = 0.05f;
        showDebugInfo = true;
        autoRefresh = true;

        ApplyToNoteSpawner();
        SaveCurrentValues();
        
        Debug.Log("🔄 모든 설정이 기본값으로 리셋되었습니다.");
    }

    [ContextMenu("현재 설정 출력")]
    public void PrintCurrentSettings()
    {
        Debug.Log($"📱 현재 모바일 친화적 설정:");
        Debug.Log($"   화면 사용 비율: {screenUsage:F2}");
        Debug.Log($"   마디 내부 여백: {measurePadding:F2}");
        Debug.Log($"   화면 가장자리 여백: {screenMargin:F2}");
        Debug.Log($"   디버그 정보: {(showDebugInfo ? "활성화" : "비활성화")}");
        Debug.Log($"   자동 새로고침: {(autoRefresh ? "활성화" : "비활성화")}");
    }

    // 🎯 화면 사용 조정 메서드들

    [ContextMenu("화면 사용률 +5%")]
    public void IncreaseScreenUsage()
    {
        screenUsage = Mathf.Min(screenUsage + 0.05f, 0.95f);
        Debug.Log($"📱 화면 사용률 증가: {screenUsage:F2}");
        if (autoRefresh) ApplyToNoteSpawner();
    }

    [ContextMenu("화면 사용률 -5%")]
    public void DecreaseScreenUsage()
    {
        screenUsage = Mathf.Max(screenUsage - 0.05f, 0.8f);
        Debug.Log($"📱 화면 사용률 감소: {screenUsage:F2}");
        if (autoRefresh) ApplyToNoteSpawner();
    }

    [ContextMenu("마디 여백 증가")]
    public void IncreaseMeasurePadding()
    {
        measurePadding = Mathf.Min(measurePadding + 0.02f, 0.15f);
        Debug.Log($"📱 마디 여백 증가: {measurePadding:F2}");
        if (autoRefresh) ApplyToNoteSpawner();
    }

    [ContextMenu("마디 여백 감소")]
    public void DecreaseMeasurePadding()
    {
        measurePadding = Mathf.Max(measurePadding - 0.02f, 0.05f);
        Debug.Log($"📱 마디 여백 감소: {measurePadding:F2}");
        if (autoRefresh) ApplyToNoteSpawner();
    }

    [ContextMenu("화면 여백 증가")]
    public void IncreaseScreenMargin()
    {
        screenMargin = Mathf.Min(screenMargin + 0.01f, 0.1f);
        Debug.Log($"📱 화면 여백 증가: {screenMargin:F2}");
        if (autoRefresh) ApplyToNoteSpawner();
    }

    [ContextMenu("화면 여백 감소")]
    public void DecreaseScreenMargin()
    {
        screenMargin = Mathf.Max(screenMargin - 0.01f, 0.02f);
        Debug.Log($"📱 화면 여백 감소: {screenMargin:F2}");
        if (autoRefresh) ApplyToNoteSpawner();
    }

    // 🎵 모바일 최적화 프리셋들

    [ContextMenu("스마트폰 세로 모드")]
    public void SetPortraitPhoneMode()
    {
        screenUsage = 0.95f;
        measurePadding = 0.06f;
        screenMargin = 0.025f;
        
        ApplyToNoteSpawner();
        Debug.Log("📱 스마트폰 세로 모드로 설정되었습니다.");
    }

    [ContextMenu("스마트폰 가로 모드")]
    public void SetLandscapePhoneMode()
    {
        screenUsage = 0.88f;
        measurePadding = 0.08f;
        screenMargin = 0.06f;
        
        ApplyToNoteSpawner();
        Debug.Log("📱 스마트폰 가로 모드로 설정되었습니다.");
    }

    [ContextMenu("태블릿 모드")]
    public void SetTabletMode()
    {
        screenUsage = 0.85f;
        measurePadding = 0.1f;
        screenMargin = 0.075f;
        
        ApplyToNoteSpawner();
        Debug.Log("📱 태블릿 모드로 설정되었습니다.");
    }

    [ContextMenu("표준 모드")]
    public void SetStandardMode()
    {
        screenUsage = 0.9f;
        measurePadding = 0.08f;
        screenMargin = 0.05f;
        
        ApplyToNoteSpawner();
        Debug.Log("📱 표준 모드로 설정되었습니다.");
    }

    // 🎚️ UI 슬라이더 연동용 메서드들

    public void OnScreenUsageChanged(float value)
    {
        screenUsage = value;
        if (autoRefresh) ApplyToNoteSpawner();
    }

    public void OnMeasurePaddingChanged(float value)
    {
        measurePadding = value;
        if (autoRefresh) ApplyToNoteSpawner();
    }

    public void OnScreenMarginChanged(float value)
    {
        screenMargin = value;
        if (autoRefresh) ApplyToNoteSpawner();
    }

    public void OnAutoRefreshToggled(bool enabled)
    {
        autoRefresh = enabled;
        Debug.Log($"자동 새로고침: {(enabled ? "활성화" : "비활성화")}");
    }

    public void OnDebugToggled(bool enabled)
    {
        showDebugInfo = enabled;
        if (autoRefresh) ApplyToNoteSpawner();
    }

    // 🎮 키보드 단축키 지원
    void OnGUI()
    {
        if (!Application.isPlaying) return;

        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                case KeyCode.Alpha1:
                    SetStandardMode();
                    break;
                case KeyCode.Alpha2:
                    SetPortraitPhoneMode();
                    break;
                case KeyCode.Alpha3:
                    SetLandscapePhoneMode();
                    break;
                case KeyCode.Alpha4:
                    SetTabletMode();
                    break;
                case KeyCode.R:
                    ResetToDefaults();
                    break;
                case KeyCode.Space:
                    ApplySettingsNow();
                    break;
                case KeyCode.Plus:
                case KeyCode.KeypadPlus:
                    IncreaseScreenUsage();
                    break;
                case KeyCode.Minus:
                case KeyCode.KeypadMinus:
                    DecreaseScreenUsage();
                    break;
                case KeyCode.UpArrow:
                    IncreaseMeasurePadding();
                    break;
                case KeyCode.DownArrow:
                    DecreaseMeasurePadding();
                    break;
            }
        }
    }

    // 🔧 NoteSpawner 연동 메서드들 (수정됨)
    [ContextMenu("곡 정보 출력")]
    public void PrintSongInfo()
    {
        if (noteSpawner != null)
        {
            noteSpawner.PrintCurrentSongInfo();
        }
    }

    [ContextMenu("다음 곡")]
    public void NextSong()
    {
        if (noteSpawner != null)
        {
            noteSpawner.NextSong();
        }
    }

    [ContextMenu("이전 곡")]
    public void PreviousSong()
    {
        if (noteSpawner != null)
        {
            noteSpawner.PreviousSong();
        }
    }

    [ContextMenu("설정 정보 출력")]
    public void PrintNoteSpawnerSettings()
    {
        if (noteSpawner != null)
        {
            noteSpawner.PrintCurrentSettings();
        }
    }

    // 에디터에서 값 변경 시에도 적용되도록 OnValidate 사용
    void OnValidate()
    {
        if (Application.isPlaying && autoRefresh && noteSpawner != null)
        {
            // 값 범위 검증
            screenUsage = Mathf.Clamp(screenUsage, 0.8f, 0.95f);
            measurePadding = Mathf.Clamp(measurePadding, 0.05f, 0.15f);
            screenMargin = Mathf.Clamp(screenMargin, 0.02f, 0.1f);

            // 즉시 적용 (단, 너무 자주 호출되지 않도록 제한)
            if (Time.time - lastUpdateTime > updateInterval)
            {
                ApplyToNoteSpawner();
                SaveCurrentValues();
                lastUpdateTime = Time.time;
            }
        }
    }
}
