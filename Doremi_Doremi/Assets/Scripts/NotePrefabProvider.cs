using UnityEngine;  // Unity 엔진의 핵심 클래스 및 메서드를 제공하는 네임스페이스

/// <summary>
/// 🎵 NotePrefabProvider 클래스는 음표 및 쉼표에 해당하는 다양한 프리팹을 인스펙터에서 설정하고,
/// 코드(code)와 꼬리 방향(stemDown)에 따라 적절한 프리팹을 반환하는 기능을 제공합니다.
/// </summary>
public class NotePrefabProvider : MonoBehaviour
{
    [Header("🎵 음표 프리팹")]  // 인스펙터에서 음표 관련 프리팹들을 그룹화
    public GameObject wholeNotePrefab;             // 온음표 프리팹 (꼬리 없음)
    public GameObject halfNotePrefab;              // 이분음표 프리팹 (꼬리 위)
    public GameObject halfNotePrefab_Down;         // 이분음표 프리팹 (꼬리 아래)
    public GameObject quarterNotePrefab;           // 사분음표 프리팹 (꼬리 위)
    public GameObject quarterNotePrefab_Down;      // 사분음표 프리팹 (꼬리 아래)
    public GameObject eighthNotePrefab;            // 8분음표 프리팹 (꼬리 위)
    public GameObject eighthNotePrefab_Down;       // 8분음표 프리팹 (꼬리 아래)
    public GameObject sixteenthNotePrefab;         // 16분음표 프리팹 (꼬리 위)
    public GameObject sixteenthNotePrefab_Down;    // 16분음표 프리팹 (꼬리 아래)

    [Header("🔇 쉼표 프리팹")]  // 인스펙터에서 쉼표 관련 프리팹들을 그룹화
    public GameObject wholeRestPrefab;             // 온쉼표 프리팹
    public GameObject halfRestPrefab;              // 이분쉼표 프리팹
    public GameObject quarterRestPrefab;           // 사분쉼표 프리팹
    public GameObject eighthRestPrefab;            // 8분쉼표 프리팹
    public GameObject sixteenthRestPrefab;         // 16분쉼표 프리팹

    [Header("📏 기타")]  // 인스펙터에서 기타 필드를 그룹화
    public GameObject ledgerLinePrefab;            // 보조선(ledger line) 프리팹

    /// <summary>
    /// 🎵 GetPrefab 메서드는 악보 코드와 꼬리 방향에 따라 대응되는 프리팹을 반환합니다.
    /// </summary>
    /// <param name="code">"1", "2", "4", "8", "16" 등의 음표 코드 또는 "1R", "2R" 등 쉼표 코드</param>
    /// <param name="stemDown">true면 꼬리 아래, false면 꼬리 위(쉼표는 무관)</param>
    /// <returns>해당 조건에 맞는 GameObject 프리팹 또는 찾지 못한 경우 null</returns>
    public GameObject GetPrefab(string code, bool stemDown)
    {
        // C# 8.0 이상의 튜플 패턴 매칭을 사용한 switch 식
        return (code, stemDown) switch
        {
            // 음표: 코드에 따라 꼬리 방향 유/무 별로 분기
            ("1", false) => wholeNotePrefab,         // 온음표
            ("1", true) => wholeNotePrefab,         // 온음표
            ("2", false) => halfNotePrefab,          // 이분음표 (꼬리 위)
            ("2", true) => halfNotePrefab_Down,     // 이분음표 (꼬리 아래)
            ("4", false) => quarterNotePrefab,       // 사분음표 (꼬리 위)
            ("4", true) => quarterNotePrefab_Down,  // 사분음표 (꼬리 아래)
            ("8", false) => eighthNotePrefab,        // 8분음표 (꼬리 위)
            ("8", true) => eighthNotePrefab_Down,   // 8분음표 (꼬리 아래)
            ("16", false) => sixteenthNotePrefab,     // 16분음표 (꼬리 위)
            ("16", true) => sixteenthNotePrefab_Down,// 16분음표 (꼬리 아래)

            // 쉼표: 방향 상관 없이 코드만으로 분기
            ("1R", _) => wholeRestPrefab,         // 온쉼표
            ("2R", _) => halfRestPrefab,          // 이분쉼표
            ("4R", _) => quarterRestPrefab,       // 사분쉼표
            ("8R", _) => eighthRestPrefab,        // 8분쉼표
            ("16R", _) => sixteenthRestPrefab,     // 16분쉼표

            // 그 외 코드 또는 미지원 조합은 null 반환
            _ => null
        };
    }
}
