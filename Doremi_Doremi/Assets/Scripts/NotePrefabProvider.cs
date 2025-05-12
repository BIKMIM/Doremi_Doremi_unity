using UnityEngine;

/// <summary>
/// 🎵 음표, 쉼표, Accidentals, 보조선 프리팹을 제공하는 클래스
/// </summary>
public class NotePrefabProvider : MonoBehaviour
{
    #region Dots (점음표)
    [Header("🎯 Dots")]
    [SerializeField] private GameObject noteDotPrefab;
    public GameObject NoteDotPrefab => noteDotPrefab;
    #endregion

    #region Note Heads (음표 머리)
    [Header("🎵 Note Heads")]
    [SerializeField] private GameObject wholeNoteHeadPrefab;
    [SerializeField] private GameObject halfNoteHeadPrefab;
    [SerializeField] private GameObject quarterNoteHeadPrefab;
    #endregion

    #region Stems & Flags (스템 및 깃발)
    [Header("📏 Stems & Flags")]
    [SerializeField] private GameObject noteStemPrefab;
    [SerializeField] private GameObject noteFlag8Prefab;
    [SerializeField] private GameObject noteFlag16Prefab;
    #endregion

    #region Rests (쉼표)
    [Header("🔇 Rests")]
    [SerializeField] private GameObject wholeRestPrefab;
    [SerializeField] private GameObject halfRestPrefab;
    [SerializeField] private GameObject quarterRestPrefab;
    [SerializeField] private GameObject eighthRestPrefab;
    [SerializeField] private GameObject sixteenthRestPrefab;
    #endregion

    #region Accidentals (♯♭ 부호)
    [SerializeField] private GameObject sharpPrefab;  // accidentals 용
    [SerializeField] private GameObject flatPrefab;
    #endregion

    #region Key Signatures (조표용)
    [SerializeField] private GameObject sharpKeySignaturePrefab;  // ✅ 조표 전용 샵
    [SerializeField] private GameObject flatKeySignaturePrefab;   // ✅ 조표 전용 플랫
    #endregion


    public GameObject SharpAccidentalPrefab => sharpPrefab;
    public GameObject FlatAccidentalPrefab => flatPrefab;
    public GameObject SharpKeySignaturePrefab => sharpKeySignaturePrefab;
    public GameObject FlatKeySignaturePrefab => flatKeySignaturePrefab;


    #region Ledger Lines (보조선)
    [Header("📌 Ledger Lines")]
    [SerializeField] private GameObject ledgerLinePrefab;
    #endregion

    #region Public API

    public GameObject LedgerLinePrefab => ledgerLinePrefab;
    public GameObject NoteStemPrefab => noteStemPrefab;
    public GameObject NoteFlag8Prefab => noteFlag8Prefab;
    public GameObject NoteFlag16Prefab => noteFlag16Prefab;
    

    public GameObject GetNoteHead(string code) => code switch
    {
        "1" => wholeNoteHeadPrefab,
        "2" => halfNoteHeadPrefab,
        "4" or "8" or "16" => quarterNoteHeadPrefab,
        _ => null
    };

    public GameObject GetRest(string code) => code switch
    {
        "1" or "1R" => wholeRestPrefab,
        "2" or "2R" => halfRestPrefab,
        "4" or "4R" => quarterRestPrefab,
        "8" or "8R" => eighthRestPrefab,
        "16" or "16R" => sixteenthRestPrefab,
        _ => null
    };


    public GameObject GetAccidental(string symbol) => symbol switch
    {
        "#" => sharpPrefab,
        "b" => flatPrefab,
        _ => null
    };
    #endregion



}
