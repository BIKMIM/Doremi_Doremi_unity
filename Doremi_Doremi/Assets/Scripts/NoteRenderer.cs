using System;
using UnityEngine;
using Object = UnityEngine.Object;


/// <summary>
/// NoteRenderer는 주어진 Song 정보에 따라
/// 음표 및 쉼표, 덧줄, 조립된 프리팹을 생성합니다.
/// </summary>
public class NoteRenderer
{
    /// <summary>
    /// 생성된 음표 오브젝트들을 모두 제거합니다.
    /// </summary>
    public void Clear(RectTransform notesContainer)
    {
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
            Object.Destroy(notesContainer.GetChild(i).gameObject);
    }

    /// <summary>
    /// 주어진 곡 정보를 기반으로 음표/쉼표/덧줄을 렌더링합니다.
    /// </summary>
    public void SpawnNotes(
        string[] notes,
        RectTransform linesContainer,
        RectTransform notesContainer,
        NoteMapper noteMapper,
        LedgerLineHelper ledgerHelper,
        NotePrefabProvider prefabProvider,
        float staffHeight,
        float beatSpacing,
        float noteYOffset,
        float ledgerYOffset,
        float noteScale,
        float wholeNoteYOffset)
    {
        float spacing = staffHeight / 4f;
        float currentX = -(beatSpacing * (notes.Length - 1)) / 2f;

        // 기준선 (3번째 줄) 위치
        var midLineRT = linesContainer.GetChild(2).GetComponent<RectTransform>();
        float baselineY = midLineRT.anchoredPosition.y;

        foreach (var token in notes)
        {
            var parts = token.Split(':');
            string pitch = parts[0];
            string code = parts.Length > 1 ? parts[1] : "4";
            bool isRest = pitch == "R";

            float index = 0f;
            bool stemDown = false;

            if (!isRest && noteMapper.TryGetIndex(pitch, out index))
                stemDown = code == "1" ? false : index >= 1.5f;

            float y = baselineY + index * spacing + noteYOffset;

            if (!isRest)
                ledgerHelper.GenerateLedgerLines(index, baselineY, spacing, currentX, ledgerYOffset);

            // 온음표는 보정
            if (code == "1")
            {
                y += wholeNoteYOffset;
                y += 20f;
                y -= spacing * 2f;
            }

            if (isRest)
            {
                var rest = Object.Instantiate(prefabProvider.GetRest(code), notesContainer);
                var rt = rest.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localScale = Vector3.one * noteScale;
                rt.anchoredPosition = new Vector2(currentX, baselineY);
            }
            else
            {
                var head = prefabProvider.GetNoteHead(code);
                var stem = (code == "1") ? null : prefabProvider.noteStemPrefab;
                GameObject flag = null;
                if (code == "8") flag = prefabProvider.noteFlag8Prefab;
                if (code == "16") flag = prefabProvider.noteFlag16Prefab;

                NoteFactory.CreateNoteWrap(
                    notesContainer,
                    head,
                    stem,
                    flag,
                    null,
                    stemDown,
                    new Vector2(currentX, y),
                    noteScale
                );
            }

            currentX += GetBeatLength(code) * beatSpacing;
        }
    }

    private float GetBeatLength(string code) => code switch
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
