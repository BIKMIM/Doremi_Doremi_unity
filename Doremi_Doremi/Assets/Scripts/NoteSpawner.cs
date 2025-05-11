using System.Collections.Generic;
using UnityEngine;

// 악보 데이터(JSON)를 기반으로 음표 프리팹을 생성하고 배치하는 컴포넌트
public class NoteSpawner : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────────
    // 🎼 스태프(오선지) 전체를 담고 있는 RectTransform 참조
    public RectTransform staffPanel;

    // 🎵 4분음표 프리팹: NotePlacer 컴포넌트가 반드시 함께 붙어 있어야 함
    public GameObject quarterNotePrefab;

    // ➖ 덧줄(ledger line) 프리팹
    public GameObject ledgerLinePrefab;

    // 📄 JSON 악보 파일 참조
    public TextAsset songsJson;

    // 🔢 불러온 곡 목록 중 재생할 곡의 인덱스
    public int selectedSongIndex = 0;

    // 📏 스태프 전체 높이 (픽셀 단위). 5줄 간격을 4칸으로 나누는 기준값.
    public float staffHeight = 150f;

    // ─────────────────────────────────────────────────────────────────────────────
    // 덧줄 위치를 약간 보정하기 위한 Y 오프셋
    private float ledgerYOffset = 4f;

    // 음표 이미지 중심 위치 보정을 위한 Y 오프셋 (NotePlacer에도 동일)
    private float noteYOffset = -10f;

    // JSON 파싱 후 저장되는 곡 목록
    private SongList songList;

    // ─────────────────────────────────────────────────────────────────────────────
    // Awake: 컴포넌트가 활성화될 때 한 번 실행되어 JSON 로드
    private void Awake()
    {
        // JSON 텍스트를 SongList 구조체로 파싱
        songList = JsonUtility.FromJson<SongList>(songsJson.text);
    }

    // Start: 게임 시작 시점에 음표 생성/배치를 수행
    private void Start()
    {
        SpawnSongNotes();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 악보의 모든 음표를 순회하며 프리팹 인스턴스화 및 배치
    private void SpawnSongNotes()
    {
        // 1) 곡 정보 가져오기
        Song song = songList.songs[selectedSongIndex];

        // 2) Y축 간격 계산: staffHeight / (라인 개수-1) = staffHeight / 4
        float spacing = staffHeight / 4f;

        // 3) 스태프 기준 Y 좌표 (anchoredPosition.y)를 소수점 반올림
        float baseY = Mathf.Round(staffPanel.anchoredPosition.y);

        // 4) X축 시작 위치 설정: 음표 개수 * 40px 만큼 왼쪽으로 이동
        float startX = -song.notes.Length * 40f;

        // 5) 곡의 각 음표에 대해 반복
        for (int i = 0; i < song.notes.Length; i++)
        {
            string noteName = song.notes[i];

            // 5-1) NoteMapping 유틸로 'lineIndex' 계산
            float lineIndex = NoteMapping.GetLineIndex(noteName);

            // 5-2) 4분음표 프리팹 인스턴스화
            GameObject noteGO = Instantiate(quarterNotePrefab, staffPanel);
            RectTransform rt = noteGO.GetComponent<RectTransform>();

            // 5-3) X 축 배치: startX + (i * 80px)
            float xPos = startX + i * 80f;
            // 현재 Y 위치는 NotePlacer가 담당하므로 유지
            Vector2 anchored = rt.anchoredPosition;
            rt.anchoredPosition = new Vector2(xPos, anchored.y);

            // 5-4) NotePlacer 컴포넌트를 통해 Y축 배치
            NotePlacer placer = noteGO.GetComponent<NotePlacer>();
            placer.staffPanel = staffPanel;      // 스태프 참조 전달
            placer.staffHeight = staffHeight;    // 간격 계산용 높이 전달
            placer.noteYOffset = noteYOffset;    // 중심 보정값 전달
            placer.lineIndex = lineIndex;        // 계산된 칸 인덱스 설정
            placer.PlaceNote();                  // 실제 위치 이동 호출

            // 5-5) 덧줄(ledger line) 필요 시 생성
            if (lineIndex <= -1f)
            {
                // 오선 아래쪽 덧줄
                for (float ledger = lineIndex; ledger <= -1f; ledger += 1f)
                    CreateLedgerLine(ledger, baseY, spacing, xPos);
            }
            else if (lineIndex >= 4f)
            {
                // 오선 위쪽 덧줄
                for (float ledger = lineIndex; ledger >= 4f; ledger -= 1f)
                    CreateLedgerLine(ledger, baseY, spacing, xPos);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 덧줄(ledger line) 생성 및 위치 지정 헬퍼 메서드
    private void CreateLedgerLine(float ledger, float baseY, float spacing, float x)
    {
        // 1) 덧줄 프리팹 인스턴스화
        GameObject ledgerGO = Instantiate(ledgerLinePrefab, staffPanel);
        RectTransform lr = ledgerGO.GetComponent<RectTransform>();

        // 2) 앵커/피벗 설정: X 중앙, Y 아래 기준
        lr.anchorMin = new Vector2(0.5f, 0);
        lr.anchorMax = new Vector2(0.5f, 0);
        lr.pivot = new Vector2(0.5f, 0.5f);

        // 3) Y 좌표 계산: 기준 Y + (덧줄 인덱스 * 간격) + 보정
        float ledgerY = baseY + ledger * spacing + ledgerYOffset;
        // 반단위 인덱스(줄과 줄 사이)일 때 위치 보정
        if (ledger % 1 != 0)
            ledgerY += (ledger >= 4f ? -spacing / 2f : spacing / 2f);

        // 4) 최종 위치 지정 (X는 note와 동일)
        lr.anchoredPosition = new Vector2(x, Mathf.Round(ledgerY));
    }
}
