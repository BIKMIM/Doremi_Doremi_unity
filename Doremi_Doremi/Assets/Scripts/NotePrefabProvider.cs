using UnityEngine;

/// <summary>
/// 🎵 음표 및 쉼표 프리팹을 제공하는 클래스
/// </summary>
public class NotePrefabProvider : MonoBehaviour
{
    [Header("🎵 음표 머리 프리팹")]
    public GameObject wholeNoteHeadPrefab;
    public GameObject halfNoteHeadPrefab;
    public GameObject quarterNoteHeadPrefab;

    [Header("📏 공통 스템 및 플래그")]
    public GameObject noteStemPrefab;
    public GameObject noteFlag8Prefab;
    public GameObject noteFlag16Prefab;

    [Header("🎯 점음표용 점 프리팹")]
    public GameObject noteDotPrefab;

    [Header("🔇 쉼표 프리팹")]
    public GameObject wholeRestPrefab;
    public GameObject halfRestPrefab;
    public GameObject quarterRestPrefab;
    public GameObject eighthRestPrefab;
    public GameObject sixteenthRestPrefab;

    [Header("📌 보조선 프리팹")]
    public GameObject ledgerLinePrefab;

    /// <summary>
    /// 코드에 따라 음표 머리 프리팹을 반환
    /// </summary>
    public GameObject GetNoteHead(string code)
    {
        return code switch
        {
            "1" => wholeNoteHeadPrefab,
            "2" => halfNoteHeadPrefab,
            "4" or "8" or "16" => quarterNoteHeadPrefab,
            _ => null
        };
    }

    /// <summary>
    /// 쉼표 프리팹 반환
    /// </summary>
    public GameObject GetRest(string code)
    {
        return code switch
        {
            "1R" => wholeRestPrefab,
            "2R" => halfRestPrefab,
            "4R" => quarterRestPrefab,
            "8R" => eighthRestPrefab,
            "16R" => sixteenthRestPrefab,
            _ => null
        };
    }
}
