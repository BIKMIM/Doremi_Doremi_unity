using UnityEngine;  // Unity 엔진의 핵심 클래스 및 메서드를 제공하는 네임스페이스

public class NoteSpawner : MonoBehaviour  // MonoBehaviour를 상속하여 Unity 씬에서 컴포넌트로 사용 가능한 클래스 정의
{
    [Header("Helpers")]  // 인스펙터에서 헬퍼 관련 필드를 그룹화하는 헤더
    [SerializeField] private NotePrefabProvider prefabProvider;  // NotePrefabProvider 참조: 음표 프리팹을 가져오기 위한 헬퍼

    [Header("🎹 UI")]  // 인스펙터에서 UI 관련 필드를 그룹화하는 헤더
    [SerializeField] private RectTransform linesContainer;  // 오선(StaffLineRenderer)이 포함된 RectTransform 컨테이너
    [SerializeField] private RectTransform notesContainer;  // 생성된 음표(GameObject)를 자식으로 두는 RectTransform 컨테이너

    [Header("📄 Data")]  // 인스펙터에서 데이터 관련 필드를 그룹화하는 헤더
    [SerializeField] private TextAsset songsJson;  // JSON 형식의 악보 데이터를 담는 TextAsset
    [SerializeField] private int selectedSongIndex = 0;  // 재생할 곡을 선택하는 인덱스, 기본값은 0

    [Header("⚙ Settings")]  // 인스펙터에서 설정 관련 필드를 그룹화하는 헤더
    [SerializeField] private float staffHeight = 150f;  // 오선 전체 높이 (픽셀 단위)
    [SerializeField] private float beatSpacing = 80f;  // 음표 간 가로 간격 (픽셀 단위)
    [SerializeField] private float noteYOffset = 0f;  // 음표의 수직 오프셋 (추가 높이 조정)
    [SerializeField] private float ledgerYOffset = 0f;  // 보조선의 수직 오프셋
    [SerializeField] private float noteScale = 2f;  // 생성된 음표 프리팹의 기본 스케일 값
    [SerializeField] private float wholeNoteYOffset = 0f;  // 추가

    private NoteDataLoader dataLoader;  // 악보 데이터를 파싱하는 헬퍼 클래스 인스턴스
    private NoteMapper noteMapper;  // 음이름을 라인/공간 인덱스로 변환하는 헬퍼 클래스 인스턴스
    private LedgerLineHelper ledgerHelper;  // 보조선을 생성하는 헬퍼 클래스 인스턴스

    private void Awake()  // 컴포넌트가 활성화될 때 가장 먼저 호출되는 초기화 메서드
    {
        dataLoader = new NoteDataLoader(songsJson);  // JSON 데이터 로더 초기화
        noteMapper = new NoteMapper();  // 노트 매퍼 초기화
        // 보조선 프리팹과 음표 컨테이너를 전달하여 보조선 헬퍼 초기화
        ledgerHelper = new LedgerLineHelper(prefabProvider.ledgerLinePrefab, notesContainer);
    }

    private void Start()  // Awake 다음에 한 프레임 후 호출되는 메서드
    {
        ClearNotes();  // 기존에 생성된 음표를 모두 제거
        SpawnSongNotes();  // 노래에 맞춰 음표를 생성
    }

    private void ClearNotes()  // notesContainer 아래의 모든 자식 오브젝트 삭제
    {
        // 자식 개수만큼 루프: 뒤에서부터 삭제하여 인덱스 문제 방지
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
            Destroy(notesContainer.GetChild(i).gameObject);  // GameObject 바로 파괴
    }

    private void SpawnSongNotes()  // 악보 데이터를 읽어 음표 및 보조선을 생성하는 메인 로직
    {
        // 1) 악보 데이터 파싱 및 선택된 곡 정보 가져오기
        var songList = dataLoader.LoadSongs();  // JSON 파싱하여 SongList 객체 반환
        var song = songList.songs[selectedSongIndex];  // 선택된 인덱스의 Song 객체
        var notes = song.notes;  // 음표 토큰 배열
        int count = notes.Length;  // 음표 개수

        // 2) 생성될 음표들의 전체 가로 폭 계산 및 초기 X 위치 중앙 정렬
        float totalSpan = beatSpacing * (count - 1);  // 음표 사이 간격 * (개수-1)
        float currentX = -totalSpan / 2f;  // 화면 중앙 기준으로 좌측 시작점 설정

        // 3) 세로 기준선(baseline) 계산: 오선 5줄 중 3번째 줄 사용
        var midLineRT = linesContainer.GetChild(2).GetComponent<RectTransform>();  // 0~4 중 인덱스 2는 가운데 선
        float baselineY = midLineRT.anchoredPosition.y;  // 기준 Y 좌표
        float spacing = staffHeight / 4f;  // 오선 5줄 간격 수직 스페이싱 = 높이 / 4

        // 4) 음표 토큰별로 루프를 돌며 음표 생성
        foreach (var token in notes)
        {

           

            // 토큰 형식: "음이름:코드" (예: "C4:4"), 코드 미존재 시 기본 "4"
            var parts = token.Split(':');  // ':'로 분리
            string pitch = parts[0];  // 음이름(R은 휴지표)
            string code = parts.Length > 1 ? parts[1] : "4";  // 음표 길이 코드
            bool isRest = pitch == "R";  // 음이름이 R이면 쉼표

            // 높이 인덱스 가져오기 및 꼬리 방향 결정
            float index = 0f;  // 0 기반 라인/공간 인덱스
            bool stemDown = false;  // 꼬리 방향 (위로 또는 아래로)

            // 이렇게 바꿔 주세요
            if (!isRest && noteMapper.TryGetIndex(pitch, out index))
            {
                // 온음표(코드 "1")는 항상 꼬리 없음
                if (code == "1")
                    stemDown = false;
                else
                    stemDown = index >= -1f;
            }


            Debug.Log($"[NoteSpawner] token={token}, code='{code}'");


            // 해당 코드와 방향에 맞는 음표 프리팹 가져오기
            var prefab = prefabProvider.GetPrefab(code, stemDown);
            if (prefab == null)  // 알 수 없는 코드일 경우 경고 로그 출력
            {


                Debug.LogWarning($"Unknown code: '{code}'");
                Debug.Log($"  index={index}, baselineY={baselineY}, spacing={spacing}");
                // continue 전에 필요한 로그를 남김
                
                currentX += beatSpacing * GetBeatLength(code);  // X 위치만 이동 후 다음 음표로
                continue;
            }


            


            // 음표 오브젝트 생성 및 위치/회전/스케일 설정
            var note = Instantiate(prefab, notesContainer);  // notesContainer의 자식으로 생성
            var rt = note.GetComponent<RectTransform>();  // RectTransform 컴포넌트 참조
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);  // 앵커 중앙 고정
            rt.pivot = new Vector2(0.5f, 0.5f);  // 피벗 중앙 고정
            rt.localScale = Vector3.one * noteScale;  // 스케일 적용
            rt.localRotation = stemDown  // 꼬리 방향에 따라 회전
                               ? Quaternion.Euler(0, 0, 180f)
                               : Quaternion.identity;


            // 1) 기본 Y 계산
            float y = baselineY + index * spacing + noteYOffset;

            // 2) 보조선
            if (!isRest)
                ledgerHelper.GenerateLedgerLines(index, baselineY, spacing, currentX, ledgerYOffset);

            // 3) 온음표만 추가 보정
            if (code == "1")
            {

                // 피벗 보정
                float spriteHeight = rt.rect.height * rt.localScale.y;
                y += wholeNoteYOffset;  // Inspector에서 +5, +10 등으로 미세 조절

            }

            // 4) 단 한 번만 위치 적용
            rt.anchoredPosition = new Vector2(currentX, y);

            // 5) X 좌표 갱신
            currentX += beatSpacing * GetBeatLength(code);

            // (디버그 로그)
            Debug.Log($"finalY(after all) = {y}");


        }
    }

    // 음표 길이 코드에 따른 실제 가로 길이 반환 메서드
    private float GetBeatLength(string code)
    {
        return code switch
        {
            "1" => 2f,  // 온음표 길이
            "2" => 2f,  // 이분음표 길이
            "4" => 1.5f, // 사분음표 길이
            "8" => 1f,  // 8분음표 길이
            "16" => 1f,  // 16분음표 길이
            "1R" => 2f,  // 온음표 쉼표
            "2R" => 2f,  // 이분음표 쉼표
            "4R" => 1.5f, // 사분음표 쉼표
            "8R" => 1f,  // 8분음표 쉼표
            "16R" => 1f,  // 16분음표 쉼표
            _ => 1f   // 그 외 기본 1
        };
    }
}
