using UnityEngine;

public static class AccidentalHelper
{
    [System.Serializable]
    public class AccidentalSizeConfig
    {
        public float sharpWidthRatio = 0.8f;
        public float sharpHeightRatio = 1.8f;
        public float flatWidthRatio = 0.8f;
        public float flatHeightRatio = 1.5f;
        public float flatYOffsetRatio = 0.1f;
        public float naturalWidthRatio = 0.6f;
        public float naturalHeightRatio = 2.2f;
        public float doubleSharpWidthRatio = 1.0f;
        public float doubleSharpHeightRatio = 1.0f;
        public float doubleFlatWidthRatio = 1.2f;
        public float doubleFlatHeightRatio = 1.5f;
        public float doubleFlatYOffsetRatio = 0.1f;
        public float accidentalXOffsetRatio = 1.2f;
    }

    private static AccidentalSizeConfig defaultConfig = new AccidentalSizeConfig();

    public static float SpawnAccidental(Vector2 notePosition, AccidentalType accidental, float staffSpacing, 
        RectTransform staffPanel, GameObject sharpPrefab, GameObject flatPrefab, GameObject naturalPrefab, 
        GameObject doubleSharpPrefab, GameObject doubleFlatPrefab, AccidentalSizeConfig customConfig = null)
    {
        if (accidental == AccidentalType.None)
            return 0f;

        AccidentalSizeConfig config = customConfig ?? defaultConfig;

        GameObject prefabToUse = null;
        float accidentalWidth = 0f;
        float accidentalHeight = 0f;
        float yOffset = 0f;

        switch (accidental)
        {
            case AccidentalType.Sharp:
                prefabToUse = sharpPrefab;
                accidentalWidth = staffSpacing * config.sharpWidthRatio;
                accidentalHeight = staffSpacing * config.sharpHeightRatio;
                break;
            case AccidentalType.Flat:
                prefabToUse = flatPrefab;
                accidentalWidth = staffSpacing * config.flatWidthRatio;
                accidentalHeight = staffSpacing * config.flatHeightRatio;
                yOffset = staffSpacing * config.flatYOffsetRatio;
                break;
            case AccidentalType.Natural:
                prefabToUse = naturalPrefab;
                accidentalWidth = staffSpacing * config.naturalWidthRatio;
                accidentalHeight = staffSpacing * config.naturalHeightRatio;
                break;
            case AccidentalType.DoubleSharp:
                prefabToUse = doubleSharpPrefab;
                accidentalWidth = staffSpacing * config.doubleSharpWidthRatio;
                accidentalHeight = staffSpacing * config.doubleSharpHeightRatio;
                break;
            case AccidentalType.DoubleFlat:
                prefabToUse = doubleFlatPrefab;
                accidentalWidth = staffSpacing * config.doubleFlatWidthRatio;
                accidentalHeight = staffSpacing * config.doubleFlatHeightRatio;
                yOffset = staffSpacing * config.doubleFlatYOffsetRatio;
                break;
        }

        if (prefabToUse == null)
        {
            Debug.LogWarning($"임시표 프리팹 없음: {accidental}");
            return staffSpacing * 0.3f;
        }

        GameObject accidentalInstance = Object.Instantiate(prefabToUse, staffPanel);
        RectTransform accidentalRT = accidentalInstance.GetComponent<RectTransform>();

        accidentalRT.sizeDelta = new Vector2(accidentalWidth, accidentalHeight);
        accidentalRT.anchorMin = new Vector2(0.5f, 0.5f);
        accidentalRT.anchorMax = new Vector2(0.5f, 0.5f);
        accidentalRT.pivot = new Vector2(0.5f, 0.5f);

        float accidentalX = notePosition.x - accidentalWidth * config.accidentalXOffsetRatio;
        float accidentalY = notePosition.y + yOffset;

        accidentalRT.anchoredPosition = new Vector2(accidentalX, accidentalY);

        Debug.Log($"{accidental} 임시표: 크기={accidentalWidth:F1}x{accidentalHeight:F1}, 위치=({accidentalX:F1}, {accidentalY:F1})");

        return accidentalWidth;
    }

    public static void UpdateDefaultConfig(AccidentalSizeConfig newConfig)
    {
        defaultConfig = newConfig;
    }

    public static AccidentalSizeConfig GetDefaultConfig()
    {
        return defaultConfig;
    }
}