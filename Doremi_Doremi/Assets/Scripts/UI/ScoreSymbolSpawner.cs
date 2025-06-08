using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

// ScoreSymbolSpawner.cs
// 음자리표, 조표, 박자표 등 악보 초기 기호를 생성하는 스크립트

public class ScoreSymbolSpawner : MonoBehaviour
{
    [Header("음표 배치 대상 패널")]
    public RectTransform staffPanel;

    [Header("🎼 음자리표 프리팹")]
    public GameObject trebleClefPrefab;
    public GameObject bassClefPrefab;

    [Header("🎼 조표 프리팹")]
    public GameObject sharpPrefab;
    public GameObject flatPrefab;

    [Header("박자표 프리팹")]
    public GameObject timeSig2_4Prefab;
    public GameObject timeSig3_4Prefab;
    public GameObject timeSig3_8Prefab;
    public GameObject timeSig4_4Prefab;
    public GameObject timeSig4_8Prefab;
    public GameObject timeSig6_8Prefab;

    // 현재 곡의 박자표 정보 (NoteSpawner에서 설정)
    private MusicLayoutConfig.TimeSignature _currentTimeSignature;

    // 초기화 메소드 (NoteSpawner에서 호출)
    public void Initialize(RectTransform panel, MusicLayoutConfig.TimeSignature timeSignature)
    {
        staffPanel = panel;
        _currentTimeSignature = timeSignature;
    }

    // 🎼 음자리표 생성 함수 (해상도 독립적)
    public float SpawnClef(float initialX, float staffSpacing, string clefType)
    {
        GameObject clefPrefab = null;

        if (string.IsNullOrEmpty(clefType))
        {
            clefType = "treble"; // 기본값
        }

        switch (clefType.ToLower())
        {
            case "treble":
                clefPrefab = trebleClefPrefab;
                break;
            case "bass":
                clefPrefab = bassClefPrefab;
                break;
            default:
                Debug.LogWarning($"⚠️ 알 수 없는 음자리표 타입: {clefType}. treble을 사용합니다.");
                clefPrefab = trebleClefPrefab;
                break;
        }

        if (clefPrefab == null)
        {
            Debug.LogWarning($"⚠️ {clefType} 음자리표 프리팹이 설정되지 않았습니다.");
            return staffSpacing * 2f;
        }

        GameObject clefInstance = Instantiate(clefPrefab, staffPanel);
        RectTransform clefRT = clefInstance.GetComponent<RectTransform>();

        // 🎯 완전히 해상도 독립적 크기 설정 (패널 높이 기준)
        float panelHeight = staffPanel.rect.height;
        float desiredHeight;
        float desiredWidth;
        float yOffset = 0f;

        if (clefType.ToLower() == "treble")
        {
            desiredHeight = panelHeight * 0.7f;
            desiredWidth = desiredHeight * 0.3f;
        }
        else if (clefType.ToLower() == "bass")
        {
            desiredHeight = panelHeight * 0.35f;
            desiredWidth = desiredHeight * 0.6f;
            yOffset = panelHeight * 0.05f;
        }
        else
        {
            desiredHeight = panelHeight * 0.7f; // 기본값
            desiredWidth = desiredHeight * 0.375f; // 기본 비율
        }

        clefRT.sizeDelta = new Vector2(desiredWidth, desiredHeight);
        clefRT.anchorMin = new Vector2(0.5f, 0.5f);
        clefRT.anchorMax = new Vector2(0.5f, 0.5f);
        clefRT.pivot = new Vector2(0.5f, 0.5f);

        float posX = initialX + desiredWidth * 0.5f;
        clefRT.anchoredPosition = new Vector2(posX, yOffset);

        Debug.Log($"🎼 {clefType} 음자리표 (패널기준): 크기={desiredWidth:F1}x{desiredHeight:F1}, 위치=({posX:F1}, {yOffset:F1})");

        return desiredWidth + staffSpacing * 0.2f;
    }


    // 🎼 조표 생성 함수 (해상도 독립적)
    public float SpawnKeySignature(float initialX, float staffSpacing, string keySignature, string clef)
    {
        if (string.IsNullOrEmpty(keySignature))
        {
            Debug.Log("🎼 조표 없음");
            return 0f;
        }

        string[] keySignatures = keySignature.Split(',')
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrEmpty(k))
            .ToArray();

        if (keySignatures.Length == 0)
        {
            Debug.Log("🎼 유효한 조표 없음");
            return 0f;
        }

        float currentX = initialX;
        float totalWidth = 0f;

        Dictionary<string, float> positions = clef.ToLower() == "bass" ?
            NotePositioningData.bassKeySignaturePositions : NotePositioningData.trebleKeySignaturePositions;

        Debug.Log($"🎼 조표 생성 시작: {keySignature} ({clef} 음자리표)");

        foreach (string key in keySignatures)
        {
            if (!positions.ContainsKey(key))
            {
                Debug.LogWarning($"⚠️ 알 수 없는 조표: {key}");
                continue;
            }

            float noteIndex = positions[key];
            float width = SpawnSingleKeySignature(currentX, staffSpacing, key, noteIndex);

            currentX += width;
            totalWidth += width;
        }

        Debug.Log($"🎼 조표 생성 완료: 총 너비={totalWidth:F1}");
        return totalWidth + staffSpacing * 0.3f; // 조표 후 약간의 여백
    }

    // 🎼 개별 조표 생성 함수 (해상도 독립적)
    private float SpawnSingleKeySignature(float x, float staffSpacing, string keySignature, float noteIndex)
    {
        bool isSharp = keySignature.Contains("#");
        bool isFlat = keySignature.Contains("b");

        GameObject prefabToUse = null;
        float symbolWidth = 0f;
        float symbolHeight = 0f;

        if (isSharp && sharpPrefab != null)
        {
            prefabToUse = sharpPrefab;
            symbolWidth = staffSpacing * 0.8f;
            symbolHeight = staffSpacing * 1.8f;
        }
        else if (isFlat && flatPrefab != null)
        {
            prefabToUse = flatPrefab;
            symbolWidth = staffSpacing * 0.8f;
            symbolHeight = staffSpacing * 1.5f;
        }

        if (prefabToUse == null)
        {
            Debug.LogWarning($"⚠️ {keySignature} 조표 프리팹이 설정되지 않았습니다.");
            return staffSpacing * 0.5f;
        }

        GameObject keySignatureInstance = Instantiate(prefabToUse, staffPanel);
        RectTransform keyRT = keySignatureInstance.GetComponent<RectTransform>();

        keyRT.sizeDelta = new Vector2(symbolWidth, symbolHeight);
        keyRT.anchorMin = new Vector2(0.5f, 0.5f);
        keyRT.anchorMax = new Vector2(0.5f, 0.5f);
        keyRT.pivot = new Vector2(0.5f, 0.5f);

        float posX = x + symbolWidth * 0.5f;
        float posY = noteIndex * staffSpacing * 0.5f;

        if (isFlat)
        {
            posY += staffSpacing * 0.3f;
        }

        keyRT.anchoredPosition = new Vector2(posX, posY);

        Debug.Log($"   → {keySignature}: 크기={symbolWidth:F1}x{symbolHeight:F1}, 위치=({posX:F1}, {posY:F1})");

        return symbolWidth + staffSpacing * -0.2f;
    }


    // SpawnTimeSignatureSymbol 함수 (해상도 독립적)
    public float SpawnTimeSignatureSymbol(float initialX, float staffSpacing)
    {
        GameObject prefabToUse = GetTimeSignaturePrefab();

        if (prefabToUse == null)
        {
            Debug.LogError($"박자표 프리팹을 찾을 수 없습니다: {_currentTimeSignature.beatsPerMeasure}/{_currentTimeSignature.beatUnitType}");
            return staffSpacing * 1.5f;
        }

        GameObject timeSigInstance = Instantiate(prefabToUse, staffPanel);
        RectTransform tsRT = timeSigInstance.GetComponent<RectTransform>();

        float panelHeight = staffPanel.rect.height;
        float desiredHeight = panelHeight * 0.4f;
        float desiredWidth = desiredHeight * 0.4f;

        tsRT.sizeDelta = new Vector2(desiredWidth, desiredHeight);
        tsRT.anchorMin = new Vector2(0.5f, 0.5f);
        tsRT.anchorMax = new Vector2(0.5f, 0.5f);
        tsRT.pivot = new Vector2(0.5f, 0.5f);

        float posX = initialX + desiredWidth * 0.5f;
        tsRT.anchoredPosition = new Vector2(posX, 0f);

        Debug.Log($"🎵 박자표 (패널기준): 크기={desiredWidth:F1}x{desiredHeight:F1}, 위치=({posX:F1}, 0)");

        return desiredWidth + staffSpacing * 0.5f;
    }

    private GameObject GetTimeSignaturePrefab()
    {
        string tsKey = $"{_currentTimeSignature.beatsPerMeasure}/{_currentTimeSignature.beatUnitType}";

        return tsKey switch
        {
            "2/4" => timeSig2_4Prefab,
            "3/4" => timeSig3_4Prefab,
            "4/4" => timeSig4_4Prefab,
            "3/8" => timeSig3_8Prefab,
            "4/8" => timeSig4_8Prefab,
            "6/8" => timeSig6_8Prefab,
            _ => timeSig4_4Prefab // 기본값
        };
    }
}