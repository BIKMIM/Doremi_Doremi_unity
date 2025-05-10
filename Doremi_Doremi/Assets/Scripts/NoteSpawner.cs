using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public RectTransform staffPanel;
    public GameObject quarterNotePrefab;
    public GameObject ledgerLinePrefab;
    public float staffHeight = 150f;

    public TextAsset songsJson;  // 👈 JSON 연결
    public int selectedSongIndex = 0;

    private float ledgerYOffset = 4f;
    private float noteYOffset = -10f;
    private Dictionary<string, float> noteToIndex;
    private SongList songList;

    private void Awake()
    {
        LoadSongData();
        InitializeMapping();
    }

    private void Start()
    {
        SpawnSongNotes();
    }

    private void InitializeMapping()
    {
        noteToIndex = new Dictionary<string, float>
        {
            { "E3", -3.5f }, { "F3", -3.0f }, { "G3", -2.5f }, { "A3", -2.0f }, { "B3", -1.5f },
            { "C4", -1.0f }, { "D4", -0.5f }, { "E4", 0f }, { "F4", 0.5f },
            { "G4", 1.0f }, { "A4", 1.5f }, { "B4", 2f },
            { "C5", 2.5f }, { "D5", 3f }, { "E5", 3.5f }, { "F5", 4f },
            { "G5", 4.5f }, { "A5", 5f }, { "B5", 5.5f }, { "C6", 6f }
        };

    }

    private void LoadSongData()
    {
        songList = JsonUtility.FromJson<SongList>(songsJson.text);
    }

    private void SpawnSongNotes()
    {
        Song song = songList.songs[selectedSongIndex];
        float spacing = staffHeight / 4f;
        float baseY = Mathf.Round(staffPanel.anchoredPosition.y);
        float startX = -song.notes.Length * 40f;

        for (int i = 0; i < song.notes.Length; i++)
        {
            string noteName = song.notes[i];
            if (!noteToIndex.TryGetValue(noteName, out float index))
            {
                Debug.LogWarning($"Unknown note: {noteName}");
                continue;
            }

            GameObject note = Instantiate(quarterNotePrefab, staffPanel);
            RectTransform rt = note.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            float noteY = Mathf.Round(baseY + index * spacing + noteYOffset);
            rt.anchoredPosition = new Vector2(startX + i * 80f, noteY);

            if (index <= -1f)
            {
                for (float ledger = index; ledger <= -1f; ledger += 1f)
                    CreateLedgerLine(ledger, baseY, spacing, startX + i * 80f);
            }
            else if (index >= 4f)
            {
                for (float ledger = index; ledger >= 4f; ledger -= 1f)
                    CreateLedgerLine(ledger, baseY, spacing, startX + i * 80f);
            }
        }
    }

    private void CreateLedgerLine(float ledger, float baseY, float spacing, float x)
    {
        GameObject ledgerLine = Instantiate(ledgerLinePrefab, staffPanel);
        RectTransform lr = ledgerLine.GetComponent<RectTransform>();
        lr.anchorMin = new Vector2(0.5f, 0);
        lr.anchorMax = new Vector2(0.5f, 0);
        lr.pivot = new Vector2(0.5f, 0.5f);

        float ledgerY = baseY + ledger * spacing + ledgerYOffset;
        if (ledger % 1 != 0)
            ledgerY += (ledger >= 4f ? -spacing / 2f : spacing / 2f);

        lr.anchoredPosition = new Vector2(x, Mathf.Round(ledgerY));
    }
}
