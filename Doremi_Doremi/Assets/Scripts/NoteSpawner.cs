using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("🎼 Clefs")]
    [SerializeField] private GameObject clefTreblePrefab;
    [SerializeField] private GameObject clefBassPrefab;

    [Header("🕓 Time Signatures")]
    [SerializeField] private GameObject timeSig_2_4_Prefab;
    [SerializeField] private GameObject timeSig_3_4_Prefab;
    [SerializeField] private GameObject timeSig_4_4_Prefab;
    [SerializeField] private GameObject timeSig_3_8_Prefab;
    [SerializeField] private GameObject timeSig_4_8_Prefab;
    [SerializeField] private GameObject timeSig_6_8_Prefab;

    [Header("🛠 Time Signature Settings")]
    [SerializeField] private Vector2 timeSignaturePosition = new Vector2(100f, 0f);
    [SerializeField] private float timeSignatureWidth = 48f;

    [Header("🎼 Clef Settings")]
    [SerializeField] private Vector2 trebleClefPosition = new Vector2(30f, -115f);
    [SerializeField] private Vector2 trebleClefSize = new Vector2(140f, 280f);
    [SerializeField] private Vector2 bassClefPosition = new Vector2(30f, -115f);
    [SerializeField] private Vector2 bassClefSize = new Vector2(140f, 280f);

    [Header("Helpers")]
    [SerializeField] private NotePrefabProvider prefabProvider;

    [Header("📋 UI Targets")]
    [SerializeField] private RectTransform linesContainer;
    [SerializeField] private RectTransform notesContainer;
    [SerializeField] private RectTransform staffPanel;
    [SerializeField] private GameObject staffLinesPanel;  // 오선을 담당하는 패널

    [Header("📂 JSON Data")]
    [SerializeField] private TextAsset songsJson;
    [SerializeField] private int selectedSongIndex = 0;

    [Header("Settings")]
    [SerializeField] private float staffHeight = 150f;
    [SerializeField] private float noteYOffset = 0f;
    [SerializeField] private float noteScale = 2f;
    [SerializeField] private float dottedNoteScale = 1f;

    [Header("🎼 Bar Line Settings")]
    [SerializeField] private GameObject barLinePrefab;
    [SerializeField] private float barLineWidth = 60f;
    [SerializeField] private float barLineHeight = 160f;
    [SerializeField] private float barLineOffsetY = -30f;

    [Header("🎼 Staff Line Settings")]
    [SerializeField] private GameObject staffLinePrefab;  // 오선 프리팹 추가
    [SerializeField] private float staffLineThickness = 2f;

    private NoteDataLoader _loader;
    private NoteMapper _mapper;
    private LedgerLineHelper _ledger;
    private KeySignatureRenderer _keysig;
    private NoteRenderer _renderer;

    private void Awake()
    {
        // RectTransform 설정 통일
        SetupRectTransforms();

        _loader = new NoteDataLoader(songsJson);
        _mapper = new NoteMapper();
        _ledger = new LedgerLineHelper(prefabProvider.LedgerLinePrefab, notesContainer, yOffsetRatio: -2.1f);
        _keysig = new KeySignatureRenderer(
            prefabProvider.SharpKeySignaturePrefab,
            prefabProvider.FlatKeySignaturePrefab,
            linesContainer,
            staffHeight / 4f,
            0f
        );
        _renderer = new NoteRenderer(
            prefabProvider,
            _mapper,
            _ledger,
            notesContainer,
            noteScale,
            dottedNoteScale,
            0f,
            barLinePrefab,
            barLineWidth,
            barLineHeight,
            0f
        );
    }

    private void OnDestroy()
    {
        // 게임 종료 시 오선 패널 비활성화
        if (staffLinesPanel != null)
        {
            staffLinesPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // RectTransform 설정 유지
        SetupRectTransforms();
    }

    private void Start()
    {
        var list = _loader.LoadSongs();
        if (list?.songs?.Length > 0)
        {
            var song = list.songs[selectedSongIndex];
            
            // Staff_Panel의 실제 높이 기준으로 계산
            float panelHeight = staffPanel.rect.height;
            float staffHeight = panelHeight * 0.4f;  // 오선이 패널 높이의 40% 차지
            float spacing = staffHeight / 4f;        // 오선 5줄 = 4칸
            float baseY = -panelHeight * 0.1f;       // 패널 상단에서 10% 아래에서 시작

            // 오선 패널 활성화 및 위치 조정
            if (staffLinesPanel != null)
            {
                staffLinesPanel.SetActive(true);
                var rt = staffLinesPanel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 0);
                    rt.pivot = new Vector2(0.5f, 0);
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = staffPanel.sizeDelta;
                }
            }

            SpawnClef(song.clef, baseY, spacing);
            _keysig.Render(song.key);
            SpawnTimeSignature(song.time, baseY, spacing);
            ClearNotes();
            _renderer.RenderSongNotes(song, baseY, spacing);
        }
    }

    private void SetupRectTransforms()
    {
        // Staff_Panel 설정 - 상단에 고정
        if (staffPanel != null)
        {
            staffPanel.anchorMin = new Vector2(0, 1);
            staffPanel.anchorMax = new Vector2(1, 1);
            staffPanel.pivot = new Vector2(0.5f, 1);
            staffPanel.anchoredPosition = Vector2.zero;
            
            // 높이가 0이면 기본값 설정
            if (staffPanel.sizeDelta.y <= 0)
            {
                staffPanel.sizeDelta = new Vector2(0, 400);
            }
        }

        // LinesContainer 설정
        if (linesContainer != null)
        {
            linesContainer.anchorMin = new Vector2(0, 1);
            linesContainer.anchorMax = new Vector2(1, 1);
            linesContainer.pivot = new Vector2(0.5f, 1);
            linesContainer.anchoredPosition = Vector2.zero;
            linesContainer.sizeDelta = staffPanel != null ? 
                staffPanel.sizeDelta : new Vector2(0, 400);
        }

        // NotesContainer 설정
        if (notesContainer != null)
        {
            notesContainer.anchorMin = new Vector2(0, 1);
            notesContainer.anchorMax = new Vector2(1, 1);
            notesContainer.pivot = new Vector2(0.5f, 1);
            notesContainer.anchoredPosition = Vector2.zero;
            notesContainer.sizeDelta = staffPanel != null ? 
                staffPanel.sizeDelta : new Vector2(0, 400);
        }
    }

    private void SpawnClef(string clef, float baseY, float spacing)
    {
        var pf = clef == "Bass" ? clefBassPrefab : clefTreblePrefab;
        if (!pf) return;
        var obj = Instantiate(pf, linesContainer);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(30f, baseY);
        rt.sizeDelta = clef == "Bass" ? bassClefSize : trebleClefSize;
    }

    private void SpawnTimeSignature(string t, float baseY, float spacing)
    {
        GameObject pf = t switch
        {
            "2/4" => timeSig_2_4_Prefab,
            "3/4" => timeSig_3_4_Prefab,
            "4/4" => timeSig_4_4_Prefab,
            "3/8" => timeSig_3_8_Prefab,
            "4/8" => timeSig_4_8_Prefab,
            "6/8" => timeSig_6_8_Prefab,
            _ => null
        };
        if (pf == null) return;
        var obj = Instantiate(pf, linesContainer);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(100f, baseY);
        rt.sizeDelta = new Vector2(timeSignatureWidth, staffHeight * 1.25f);
    }

    private void ClearStaffLines()
    {
        if (linesContainer == null) return;
        
        // LinesContainer의 모든 자식 제거
        for (int i = linesContainer.childCount - 1; i >= 0; i--)
        {
            var child = linesContainer.GetChild(i);
            if (child != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    private void ClearNotes()
    {
        if (notesContainer == null) return;
        
        // NotesContainer의 모든 자식 제거
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
        {
            var child = notesContainer.GetChild(i);
            if (child != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}
