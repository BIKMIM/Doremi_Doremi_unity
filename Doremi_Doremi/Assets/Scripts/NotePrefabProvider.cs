using UnityEngine;

/// <summary>
/// 🎵 음표 및 쉼표 프리팹 제공
/// </summary>
public class NotePrefabProvider : MonoBehaviour
{
    [Header("🎵 음표 프리팹")]
    public GameObject wholeNotePrefab;
    public GameObject halfNotePrefab;
    public GameObject halfNotePrefab_Down;
    public GameObject quarterNotePrefab;
    public GameObject quarterNotePrefab_Down;
    public GameObject eighthNotePrefab;
    public GameObject eighthNotePrefab_Down;
    public GameObject sixteenthNotePrefab;
    public GameObject sixteenthNotePrefab_Down;

    [Header("🔇 쉼표 프리팹")]
    public GameObject wholeRestPrefab;
    public GameObject halfRestPrefab;
    public GameObject quarterRestPrefab;
    public GameObject eighthRestPrefab;
    public GameObject sixteenthRestPrefab;

    [Header("📏 기타")]
    public GameObject ledgerLinePrefab;

    /// <summary>
    /// 🎵 코드와 방향에 따라 프리팹 반환
    /// </summary>
    public GameObject GetPrefab(string code, bool stemDown)
    {
        return (code, stemDown) switch
        {
            ("1", false) => wholeNotePrefab,
            ("2", false) => halfNotePrefab,
            ("2", true) => halfNotePrefab_Down,
            ("4", false) => quarterNotePrefab,
            ("4", true) => quarterNotePrefab_Down,
            ("8", false) => eighthNotePrefab,
            ("8", true) => eighthNotePrefab_Down,
            ("16", false) => sixteenthNotePrefab,
            ("16", true) => sixteenthNotePrefab_Down,
            // 쉼표는 방향 관계 없음
            ("1R", _) => wholeRestPrefab,
            ("2R", _) => halfRestPrefab,
            ("4R", _) => quarterRestPrefab,
            ("8R", _) => eighthRestPrefab,
            ("16R", _) => sixteenthRestPrefab,
            _ => null
        };
    }
}
