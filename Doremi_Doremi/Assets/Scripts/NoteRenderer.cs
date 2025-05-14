using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer
{
    // 생성자 주입받은 의존성들
    private readonly NotePrefabProvider _prefabs;
    private readonly NoteMapper _mapper;
    private readonly LedgerLineHelper _ledger;
    private readonly RectTransform _container;
    private readonly float _noteScale;
    private readonly float _dottedScale;
    private readonly float _noteYOffset;
    private readonly GameObject _barLinePrefab;
    private readonly float _barLineWidth;
    private readonly float _barLineHeight;
    private readonly float _barLineOffsetY;

    public NoteRenderer(
        NotePrefabProvider prefabs,
        NoteMapper mapper,
        LedgerLineHelper ledger,
        RectTransform container,
        float noteScale,
        float dottedScale,
        float noteYOffset,
        GameObject barLinePrefab,
        float barLineWidth,
        float barLineHeight,
        float barLineOffsetY)
    {
        _prefabs = prefabs;
        _mapper = mapper;
        _ledger = ledger;
        _container = container;
        _noteScale = noteScale;
        _dottedScale = dottedScale;
        _noteYOffset = noteYOffset;
        _barLinePrefab = barLinePrefab;
        _barLineWidth = barLineWidth;
        _barLineHeight = barLineHeight;
        _barLineOffsetY = barLineOffsetY;
    }

    public void RenderSongNotes(Song song, float baseY, float spacing)
    {
        float beatsPerMeasure = GetBeatsPerMeasure(song.time);
        float beatLimit = beatsPerMeasure * 2f;  // 2마디만 표시

        // 1) 두 마디 분량만 자르기
        var limited = new List<string>();
        float acc = 0f;
        foreach (var n in song.notes)
        {
            var parts = n.Split(':');
            if (parts.Length != 2) continue;
            float b = GetBeatLength(parts[1]) * (parts[1].Contains(".") ? 1.5f : 1f);
            if (acc + b > beatLimit) break;
            limited.Add(n);
            acc += b;
        }

        // 2) 좌우 여백, 센터 위치 계산
        float W = _container.rect.width;
        float leftPad = 150f;       // 왼쪽 여백 증가 (100f에서 150f로)
        float rightPad = 40f;
        float availableWidth = W - leftPad - rightPad;
        float measureWidth = availableWidth / 2f;  // 2마디로 균등 분할
        float staffHeight = spacing * 4f;
        
        // 3) 마디선 위치 계산
        float startX = -W/2f + leftPad;  // 첫 마디 시작점
        float centerX = startX + measureWidth;  // 중앙 마디선
        float endX = centerX + measureWidth;    // 마지막 마디선

        // 4) 마디별 노트 분리
        var m1 = new List<string>();
        var m2 = new List<string>();
        float sumBeats = 0f;
        foreach (var n in limited)
        {
            float b = GetBeatLength(n.Split(':')[1]) * (n.Contains(".") ? 1.5f : 1f);
            if (sumBeats < beatsPerMeasure)
                m1.Add(n);
            else
                m2.Add(n);
            sumBeats += b;
        }

        // 5) 마디선 그리기
        // DrawBarLine(startX, baseY);     // 시작 마디선
        DrawBarLine(centerX, baseY, staffHeight, spacing);    // 중앙 마디선
        // DrawBarLine(endX, baseY);       // 끝 마디선

        // 6) 첫 번째 마디 배치
        float beatAcc = 0f;
        foreach (var n in m1)
        {
            float x = startX + (beatAcc / beatsPerMeasure) * measureWidth;
            PlaceNoteOrRest(n, x, baseY, measureWidth, beatsPerMeasure, spacing, 1.2f);
            beatAcc += GetBeatLength(n.Split(':')[1]) * (n.Contains(".") ? 1.5f : 1f);
        }

        // 7) 두 번째 마디 배치 - 시작 위치를 약간 오른쪽으로 조정
        beatAcc = 0f;
        foreach (var n in m2)
        {
            float x = centerX + 50f + (beatAcc / beatsPerMeasure) * measureWidth;
            PlaceNoteOrRest(n, x, baseY, measureWidth, beatsPerMeasure, spacing, 1.2f);
            beatAcc += GetBeatLength(n.Split(':')[1]) * (n.Contains(".") ? 1.5f : 1f);
        }
    }

    private void DrawBarLine(float x, float baseY, float staffHeight, float spacing)
    {
        var bl = UnityEngine.Object.Instantiate(_barLinePrefab, _container);
        var rt = bl.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(x, baseY);
        rt.sizeDelta = new Vector2(_barLineWidth, staffHeight);
        rt.localScale = Vector3.one;
    }

    private void PlaceNoteOrRest(string noteStr, float x, float baseY, float measureWidth, float beatsPerMeasure, float spacing, float noteScale)
    {
        var parts = noteStr.Split(':');
        string pitch = parts[0];
        string dur = parts[1];
        string code = dur.Replace("R", "").Replace(".", "");
        bool dotted = dur.Contains(".");
        bool isRest = pitch == "R";
        float beat = GetBeatLength(dur) * (dotted ? 1.5f : 1f);

        // 컨테이너의 실제 높이 가져오기
        float containerHeight = _container.rect.height;

        if (isRest)
        {
            var r = UnityEngine.Object.Instantiate(_prefabs.GetRest(code), _container);
            var rt = r.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(x, baseY + 2 * spacing);
            rt.localScale = Vector3.one * _noteScale * 0.8f * noteScale;
        }
        else if (_mapper.TryGetIndex(pitch, out float idx))
        {
            float y = baseY + idx * spacing;
            Debug.Log($"[NoteDebug] pitch: {pitch}, idx: {idx}, spacing: {spacing}, baseY: {baseY}, containerHeight: {containerHeight}, y: {y}");
            
            var wrap = NoteFactory.CreateNoteWrap(
                _container,
                _prefabs.GetNoteHead(code),
                code == "1" ? null : _prefabs.NoteStemPrefab,
                GetFlag(code),
                null,
                idx < 1.0f,
                new Vector2(x, y),
                _noteScale * 0.8f * noteScale,
                spacing
            );

            if (dotted)
            {
                var dot = UnityEngine.Object.Instantiate(_prefabs.NoteDotPrefab, wrap.transform);
                var dr = dot.GetComponent<RectTransform>();
                dr.anchorMin = dr.anchorMax = new Vector2(0.5f, 0);
                dr.pivot = new Vector2(0.5f, 0);
                dr.anchoredPosition = new Vector2(spacing * 0.5f, 0f);
                dr.localScale = Vector3.one * _dottedScale * 0.8f * noteScale;
            }

            _ledger.GenerateLedgerLines(idx, spacing, x, baseY, spacing * 3f); // 덧줄 너비를 spacing의 3배로
        }
    }

    private GameObject GetFlag(string c) => c switch
    {
        "8" => _prefabs.NoteFlag8Prefab,
        "16" => _prefabs.NoteFlag16Prefab,
        _ => null
    };

    private float GetBeatLength(string d) => d switch
    {
        string s when s.Contains("1") => 4f,
        string s when s.Contains("2") => 2f,
        string s when s.Contains("4") => 1f,
        string s when s.Contains("8") => 0.5f,
        string s when s.Contains("16") => 0.25f,
        _ => 1f
    };

    private float GetBeatsPerMeasure(string t) => t switch
    {
        "2/4" => 2f,
        "3/4" => 3f,
        "4/4" => 4f,
        "3/8" => 1.5f,
        "4/8" => 2f,
        "6/8" => 3f,
        _ => 4f
    };
}
