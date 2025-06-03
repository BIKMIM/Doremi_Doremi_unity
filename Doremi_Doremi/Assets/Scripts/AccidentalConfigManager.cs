using UnityEngine;

// AccidentalConfigManager.cs - 임시표 크기 및 위치를 실시간으로 조정할 수 있는 매니저
public class AccidentalConfigManager : MonoBehaviour
{
    [Header("더블샵 설정")]
    [Range(0.1f, 3.0f)]
    public float doubleSharpWidthRatio = 1.0f;
    [Range(0.1f, 3.0f)]
    public float doubleSharpHeightRatio = 1.0f;

    [Header("더블플랫 설정")]
    [Range(0.1f, 3.0f)]
    public float doubleFlatWidthRatio = 1.2f;
    [Range(0.1f, 3.0f)]
    public float doubleFlatHeightRatio = 1.5f;
    [Range(-0.5f, 0.5f)]
    public float doubleFlatYOffsetRatio = 0.1f;

    [Header("내츄럴 설정")]
    [Range(0.1f, 3.0f)]
    public float naturalWidthRatio = 0.6f;
    [Range(0.1f, 3.0f)]
    public float naturalHeightRatio = 2.2f;

    [Header("샵 설정")]
    [Range(0.1f, 3.0f)]
    public float sharpWidthRatio = 0.8f;
    [Range(0.1f, 3.0f)]
    public float sharpHeightRatio = 1.8f;

    [Header("플랫 설정")]
    [Range(0.1f, 3.0f)]
    public float flatWidthRatio = 0.8f;
    [Range(0.1f, 3.0f)]
    public float flatHeightRatio = 1.5f;
    [Range(-0.5f, 0.5f)]
    public float flatYOffsetRatio = 0.1f;

    [Header("공통 위치 설정")]
    [Range(0.1f, 3.0f)]
    public float accidentalXOffsetRatio = 1.2f;

    [Header("실시간 업데이트")]
    public bool autoUpdate = true;

    private AccidentalHelper.AccidentalSizeConfig lastConfig;

    void Start()
    {
        ApplySettings();
    }

    void Update()
    {
        if (autoUpdate && HasConfigChanged())
        {
            ApplySettings();
        }
    }

    public void ApplySettings()
    {
        var config = new AccidentalHelper.AccidentalSizeConfig
        {
            doubleSharpWidthRatio = doubleSharpWidthRatio,
            doubleSharpHeightRatio = doubleSharpHeightRatio,
            doubleFlatWidthRatio = doubleFlatWidthRatio,
            doubleFlatHeightRatio = doubleFlatHeightRatio,
            doubleFlatYOffsetRatio = doubleFlatYOffsetRatio,
            naturalWidthRatio = naturalWidthRatio,
            naturalHeightRatio = naturalHeightRatio,
            sharpWidthRatio = sharpWidthRatio,
            sharpHeightRatio = sharpHeightRatio,
            flatWidthRatio = flatWidthRatio,
            flatHeightRatio = flatHeightRatio,
            flatYOffsetRatio = flatYOffsetRatio,
            accidentalXOffsetRatio = accidentalXOffsetRatio
        };

        AccidentalHelper.UpdateDefaultConfig(config);
        lastConfig = config;

        Debug.Log("임시표 설정이 적용되었습니다.");
    }

    private bool HasConfigChanged()
    {
        if (lastConfig == null) return true;

        return lastConfig.doubleSharpWidthRatio != doubleSharpWidthRatio ||
               lastConfig.doubleSharpHeightRatio != doubleSharpHeightRatio ||
               lastConfig.doubleFlatWidthRatio != doubleFlatWidthRatio ||
               lastConfig.doubleFlatHeightRatio != doubleFlatHeightRatio ||
               lastConfig.doubleFlatYOffsetRatio != doubleFlatYOffsetRatio ||
               lastConfig.naturalWidthRatio != naturalWidthRatio ||
               lastConfig.naturalHeightRatio != naturalHeightRatio ||
               lastConfig.sharpWidthRatio != sharpWidthRatio ||
               lastConfig.sharpHeightRatio != sharpHeightRatio ||
               lastConfig.flatWidthRatio != flatWidthRatio ||
               lastConfig.flatHeightRatio != flatHeightRatio ||
               lastConfig.flatYOffsetRatio != flatYOffsetRatio ||
               lastConfig.accidentalXOffsetRatio != accidentalXOffsetRatio;
    }

    // UI 버튼 등에서 호출할 수 있는 개별 설정 함수들
    public void SetDoubleSharpSize(float width, float height)
    {
        doubleSharpWidthRatio = width;
        doubleSharpHeightRatio = height;
        ApplySettings();
    }

    public void SetDoubleFlatSize(float width, float height, float yOffset)
    {
        doubleFlatWidthRatio = width;
        doubleFlatHeightRatio = height;
        doubleFlatYOffsetRatio = yOffset;
        ApplySettings();
    }

    public void SetNaturalSize(float width, float height)
    {
        naturalWidthRatio = width;
        naturalHeightRatio = height;
        ApplySettings();
    }

    public void ResetToDefaults()
    {
        doubleSharpWidthRatio = 1.0f;
        doubleSharpHeightRatio = 1.0f;
        doubleFlatWidthRatio = 1.2f;
        doubleFlatHeightRatio = 1.5f;
        doubleFlatYOffsetRatio = 0.1f;
        naturalWidthRatio = 0.6f;
        naturalHeightRatio = 2.2f;
        sharpWidthRatio = 0.8f;
        sharpHeightRatio = 1.8f;
        flatWidthRatio = 0.8f;
        flatHeightRatio = 1.5f;
        flatYOffsetRatio = 0.1f;
        accidentalXOffsetRatio = 1.2f;
        
        ApplySettings();
        Debug.Log("임시표 설정이 기본값으로 초기화되었습니다.");
    }
}