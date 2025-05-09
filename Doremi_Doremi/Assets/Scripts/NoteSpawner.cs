using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public RectTransform staffPanel;           // 오선 패널
    public GameObject quarterNotePrefab;       // 음표 프리팹
    public GameObject ledgerLinePrefab;        // 덧줄 프리팹
    public float staffHeight = 150f;           // 오선 높이

    float ledgerYOffset = 4f;                  // 덧줄 위치 보정
    float noteYOffset = -10f;                  // 음표 위치 보정

    // 🎵 G3(-4f) ~ G5(+5f)까지 포함한 전체 라인 인덱스
    float[] lineIndexes = new float[]
    {
        -4f, -3.5f, -3f, -2.5f, -2f, -1.5f, -1f, -0.5f,
         0f, 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f, 4.5f, 5f
    };

    void Start()
    {
        float spacing = staffHeight / 4f;
        float baseY = Mathf.Round(staffPanel.anchoredPosition.y);
        float startX = -lineIndexes.Length * 40f; // 왼쪽 여유 공간 확보

        for (int i = 0; i < lineIndexes.Length; i++)
        {
            float index = lineIndexes[i];

            // 🎵 음표 생성
            GameObject note = Instantiate(quarterNotePrefab, staffPanel);
            RectTransform rt = note.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            float noteY = Mathf.Round(baseY + index * spacing + noteYOffset);
            rt.anchoredPosition = new Vector2(startX + i * 80f, noteY);

            // 🎵 덧줄 생성 (아래 음표)
            if (index <= -1f)
            {
                for (float ledger = index; ledger <= -1f; ledger += 1f)
                {
                    GameObject ledgerLine = Instantiate(ledgerLinePrefab, staffPanel);
                    RectTransform lr = ledgerLine.GetComponent<RectTransform>();
                    lr.anchorMin = new Vector2(0.5f, 0);
                    lr.anchorMax = new Vector2(0.5f, 0);
                    lr.pivot = new Vector2(0.5f, 0.5f);

                    float ledgerY = baseY + ledger * spacing + ledgerYOffset;
                    if (ledger % 1 != 0) ledgerY += spacing / 2f;

                    lr.anchoredPosition = new Vector2(startX + i * 80f, Mathf.Round(ledgerY));
                }
            }

            // 🎵 덧줄 생성 (위 음표)
            else if (index >= 4f)
            {
                for (float ledger = index; ledger >= 4f; ledger -= 1f)
                {
                    GameObject ledgerLine = Instantiate(ledgerLinePrefab, staffPanel);
                    RectTransform lr = ledgerLine.GetComponent<RectTransform>();
                    lr.anchorMin = new Vector2(0.5f, 0);
                    lr.anchorMax = new Vector2(0.5f, 0);
                    lr.pivot = new Vector2(0.5f, 0.5f);

                    float ledgerY = baseY + ledger * spacing + ledgerYOffset;
                    if (ledger % 1 != 0) ledgerY -= spacing / 2f;

                    lr.anchoredPosition = new Vector2(startX + i * 80f, Mathf.Round(ledgerY));
                }
            }
        }
    }
}
