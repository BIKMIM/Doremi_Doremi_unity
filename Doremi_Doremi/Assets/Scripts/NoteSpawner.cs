using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    // === 🎼 기본 설정 ===
    public RectTransform staffPanel;         // 오선 영역
    public RectTransform notesContainer;     // 음표 생성될 부모
    public TextAsset songsJson;              // JSON으로 된 악보 데이터
    public int selectedSongIndex = 0;        // 선택된 곡 인덱스

    // === 🎵 음표 프리팹들 ===
    [Header("Note Prefabs")]
    [SerializeField] private GameObject wholeNotePrefab;   // 🎵 1분음표 (온음표)
    [SerializeField] private GameObject halfNotePrefab;// 🎵 2분음표 (온음표)
    [SerializeField] private GameObject quarterNotePrefab;// 🎵 4분음표 (온음표)
    [SerializeField] private GameObject eighthNotePrefab;// 🎵 8분음표 (온음표)
    [SerializeField] private GameObject sixteenthNotePrefab;// 🎵 16분음표 (온음표)

    [SerializeField] private GameObject halfNotePrefab_Down;// 🎵 2분음표-아래 (온음표)
    [SerializeField] private GameObject quarterNotePrefab_Down;// 🎵 4분음표-아래 (온음표)
    [SerializeField] private GameObject eighthNotePrefab_Down;// 🎵 8분음표-아래 (온음표)
    [SerializeField] private GameObject sixteenthNotePrefab_Down;// 🎵 16분음표-아래 (온음표)

    // === 🔇 쉼표 프리팹들 ===
    [Header("Rest Prefabs")]
    [SerializeField] private GameObject wholeRestPrefab;   // 🔇 1분쉼표 (온쉼표)
    [SerializeField] private GameObject halfRestPrefab;// 🔇 2분쉼표 (온쉼표)
    [SerializeField] private GameObject quarterRestPrefab;// 🔇 4분쉼표 (온쉼표)
    [SerializeField] private GameObject eighthRestPrefab;// 🔇 8분쉼표 (온쉼표)
    [SerializeField] private GameObject sixteenthRestPrefab;// 🔇 16분쉼표 (온쉼표)

    // === 📏 기타 설정 ===
    [Header("Other")]
    public GameObject ledgerLinePrefab;      // 덧줄 프리팹
    public float staffHeight = 150f;         // 오선 높이

    // === 내부 변수 ===
    private float ledgerYOffset = 4f;        // 덧줄 위치 조정 값
    private float noteYOffset = -10f;        // 음표 Y 오프셋
    private Dictionary<string, float> noteToIndex;  // 음 이름 → 위치 매핑
    private SongList songList;               // 로드된 곡 데이터

    // === 초기화 ===
    private void Awake()
    {
        LoadSongData();
        InitializeMapping();
    }

    private void Start()
    {
        ClearNotes();         // 기존 음표 제거
        SpawnSongNotes();     // 새로 음표 그리기
    }

    // === 음 높이 매핑 설정 ===
    private void InitializeMapping()
    {
        noteToIndex = new Dictionary<string, float>
        {
            { "E3", -3.5f }, { "F3", -3.0f }, { "G3", -2.5f }, { "A3", -2.0f }, { "B3", -1.5f },
            { "C4", -1.0f }, { "D4", -0.5f }, { "E4",  0f  }, { "F4",  0.5f },
            { "G4",  1.0f }, { "A4",  1.5f }, { "B4",  2f  },
            { "C5",  2.5f }, { "D5",  3f  }, { "E5",  3.5f }, { "F5",  4f  },
            { "G5",  4.5f }, { "A5",  5f  }, { "B5",  5.5f }, { "C6",  6f  }
        };
    }

    // === JSON 악보 데이터 불러오기 ===
    private void LoadSongData()
    {
        songList = JsonUtility.FromJson<SongList>(songsJson.text);
    }

    // === 음표 생성 ===
    private void SpawnSongNotes()
    {
        Song song = songList.songs[selectedSongIndex];

        float spacing = staffHeight / 4f;                       // 오선 간격
        float baseY = Mathf.Round(staffPanel.anchoredPosition.y);
        float startX = -200f;                                   // 시작 위치 왼쪽으로 살짝
        float currentX = startX;                                // 음표 위치 누적값

        for (int i = 0; i < song.notes.Length; i++)
        {
            string token = song.notes[i];
            string[] parts = token.Split(':');
            string pitch = parts[0];
            string code = parts.Length > 1 ? parts[1].Trim() : "4";
            bool isRest = pitch == "R";

            // 🔸 음 높이에 따른 꼬리 방향 판단
            bool stemDown = false;
            float index = 0f;
            if (!isRest && noteToIndex.TryGetValue(pitch, out index))
                stemDown = index > 2f;

            // ✅ 올바른 방식: 방향 포함한 프리팹 가져오기
            GameObject prefab = GetPrefab(code, stemDown);
            if (prefab == null)
            {
                Debug.LogWarning($"Unknown duration code: {code}");
                continue;
            }

            GameObject note = Instantiate(prefab, notesContainer);
            RectTransform rt = note.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);

            float y;
            if (isRest)
            {
                y = baseY + noteYOffset;
            }
            else
            {
                y = Mathf.Round(baseY + index * spacing + noteYOffset);

                // 덧줄
                if (index <= -1f)
                {
                    for (float ledger = index; ledger <= -1f; ledger += 1f)
                        CreateLedgerLine(ledger, baseY, spacing, currentX);
                }
                else if (index >= 4f)
                {
                    for (float ledger = index; ledger >= 4f; ledger -= 1f)
                        CreateLedgerLine(ledger, baseY, spacing, currentX);
                }
            }

            rt.anchoredPosition = new Vector2(currentX, y);

            float beatSpacing = 80f;
            float beatLength = GetBeatLength(code);
            currentX += beatSpacing * beatLength;
        }

    }

    // === 박자 길이 계산 (음표 간격에 사용) ===
    private float GetBeatLength(string code)
    {
        return code switch
        {
            "1" => 2f,
            "2" => 2f,
            "4" => 1.5f,
            "8" => 1f,
            "16" => 1f,
            "1R" => 2f,
            "2R" => 2f,
            "4R" => 1.5f,
            "8R" => 1f,
            "16R" => 1f,
            _ => 1f
        };
    }

    // === 프리팹 매핑 ===
    private GameObject GetPrefab(string code, bool stemDown)
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
            // 쉼표는 방향 무관
            ("1R", _) => wholeRestPrefab,
            ("2R", _) => halfRestPrefab,
            ("4R", _) => quarterRestPrefab,
            ("8R", _) => eighthRestPrefab,
            ("16R", _) => sixteenthRestPrefab,
            _ => null
        };
    }


    // === 덧줄 생성 ===
    private void CreateLedgerLine(float ledger, float baseY, float spacing, float x)
    {
        GameObject ledgerLine = Instantiate(ledgerLinePrefab, notesContainer);
        RectTransform lr = ledgerLine.GetComponent<RectTransform>();
        lr.anchorMin = lr.anchorMax = new Vector2(0.5f, 0);
        lr.pivot = new Vector2(0.5f, 0.5f);

        float ledgerY = baseY + ledger * spacing + ledgerYOffset;
        if (ledger % 1 != 0)
            ledgerY += (ledger >= 4f ? -spacing / 2f : spacing / 2f);

        lr.anchoredPosition = new Vector2(x, Mathf.Round(ledgerY));
    }

    // === 음표 초기화 ===
    private void ClearNotes()
    {
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(notesContainer.GetChild(i).gameObject);
        }
    }
}
