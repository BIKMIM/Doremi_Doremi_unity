using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public RectTransform staffPanel;
    public GameObject quarterNotePrefab;
    public GameObject ledgerLinePrefab;
    public GameObject measureLinePrefab;  // 마디선 프리팹
    public float staffHeight = 150f;

    public enum ClefType { Treble, Bass }
    public ClefType clefType = ClefType.Treble;

    float ledgerYOffset = 4f;
    float noteYOffset = -10f;
    public float[] lineIndexes;

    public void LoadSong(SongLoader.SongData song)
    {
        clefType = song.clef;
        lineIndexes = song.notes;  // 이제 바로 float[]로 사용

        for (int m = 0; m < song.notes.Length; m++)
        {
            float note = song.notes[m]; // 음표 처리
            float noteY = GetNoteYPosition(note);
            float xPosition = m * 100f;
            SpawnNote(note, noteY, xPosition);  // SpawnNote 호출
            SpawnMeasureLine(m, 0); // 마디선 생성
        }
    }

    // 🎵 음표 위치 계산
    public float GetNoteYPosition(float note)
    {
        return note * 10f;  // 예시로 조정 (각 음표에 대한 Y 위치 계산 필요)
    }

    public void SpawnNote(float note, float noteY, float xPosition)
    {
        GameObject noteObject = Instantiate(quarterNotePrefab, staffPanel);
        RectTransform rt = noteObject.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(xPosition, noteY);
    }

    public void SpawnMeasureLine(int measureIndex, int noteIndex)
    {
        GameObject measureLine = Instantiate(measureLinePrefab, staffPanel);
        RectTransform rt = measureLine.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(measureIndex * 100f + noteIndex * 80f, 0f);
    }
}
