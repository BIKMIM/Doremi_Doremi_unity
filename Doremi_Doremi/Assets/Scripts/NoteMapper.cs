using System.Collections.Generic;
using UnityEngine;

public class NoteMapper
{
    // 🎯 Treble Clef 조표 위치 (-1.0 ~ +1.0 정규화된 상대 비율)
    private static readonly Dictionary<string, float> trebleKeySigIndex = new()
    {
        { "F#", -0.4f }, { "C#", -0.6f }, { "G#", -0.3f }, { "D#", -0.5f },
        { "A#", -0.7f }, { "E#", -0.45f }, { "B#", -0.65f },
        { "Bb", -0.6f }, { "Eb", -0.4f }, { "Ab", -0.65f },
        { "Db", -0.5f }, { "Gb", -0.7f }, { "Cb", -0.55f }, { "Fb", -0.75f }
    };

    // ✅ 조표 위치 반환 (Treble 기준)
    public static float GetKeySignatureIndex(string accidentalNote)
    {
        return trebleKeySigIndex.TryGetValue(accidentalNote, out float index) ? index : 0f;
    }

<<<<<<< HEAD
    // 🎵 오선 음높이 기준값 (-1.0 ~ +1.0 정규화된 상대 비율)
    // - 첫 번째 줄을 0.0으로 기준
    // - 줄 사이 간격을 정확히 0.5로 설정
    // - 각 음의 정확한 위치 조정
    private readonly Dictionary<string, float> _noteToIndex = new()
    {
        { "A3", -1.5f },    // 첫 번째 줄 아래 3칸
        { "B3", -1.0f },    // 첫 번째 줄 아래 2칸
        { "C4", -0.5f },    // 첫 번째 줄 아래 1칸
        { "D4", -0.25f },   // 첫 번째 줄 아래 반 칸
        { "E4", 0.0f },     // 첫 번째 줄 정확히
        { "F4", 0.25f },    // 첫째와 둘째 줄 사이 정확히
        { "G4", 0.5f },     // 두 번째 줄 정확히
        { "A4", 1.0f },     // 세 번째 줄 정확히
        { "B4", 1.5f },     // 네 번째 줄 정확히
        { "C5", 2.0f },     // 다섯 번째 줄 정확히
        { "D5", 2.25f },    // 다섯 번째 줄 위 반 칸
        { "E5", 2.5f },     // 다섯 번째 줄 위 한 칸
        { "F5", 2.75f },    // 다섯 번째 줄 위 한 칸 반
        { "G5", 3.0f },     // 다섯 번째 줄 위 두 칸
        { "A5", 3.25f },    // 다섯 번째 줄 위 두 칸 반
        { "B5", 3.5f },     // 다섯 번째 줄 위 세 칸
        { "C6", 3.75f },    // 다섯 번째 줄 위 세 칸 반
=======
    // 🎵 오선 음높이 기준값 - C4부터 시작
    private readonly Dictionary<string, float> _noteToIndex = new()
    {
        { "A3", -3.0f },   // 첫 번째 보조선 아래 2칸
        { "B3", -2.5f },   // 첫 번째 보조선 아래 1칸
        { "C4", -2.0f },   // 첫 번째 보조선
        { "D4", -1.5f },   // 보조선과 첫 줄 사이
        { "E4", 0.0f },    // 첫 번째 줄
        { "F4", 0.5f },    // 첫~두 번째 줄 사이
        { "G4", 1.0f },    // 두 번째 줄
        { "A4", 1.5f },    // 두~세 번째 줄 사이
        { "B4", 2.0f },    // 세 번째 줄
        { "C5", 2.5f },    // 세~네 번째 줄 사이
        { "D5", 3.0f },    // 네 번째 줄
        { "E5", 3.5f },    // 네~다섯 번째 줄 사이
        { "F5", 4.0f },    // 다섯 번째 줄
        { "G5", 4.5f },    // 다섯 번째 줄 위
        { "A5", 5.0f },    // 다섯 번째 줄 위 칸
        { "B5", 5.5f },    // 다섯 번째 줄 위 두 칸
        { "C6", 6.0f },    // 다섯 번째 줄 위 세 칸
>>>>>>> e07dfb26bf7734208eb8a21c3791426be1698ca0
    };

    public bool TryGetIndex(string note, out float index)
    {
        return _noteToIndex.TryGetValue(note, out index);
    }
<<<<<<< HEAD
}

public class NoteHeadGLDrawer : MonoBehaviour
{
    public Material mat;
    public float staffStartX = 100f;
    public float staffStartY = 200f;
    public float staffSpacing = 20f;
    public float noteHeadWidth = 24f;
    public float noteHeadHeight = 16f;

    string[] notes = { "C4", "D4", "E4", "F4", "G4" };
    Dictionary<string, float> noteYOffsets = new Dictionary<string, float>
    {
        { "C4", -2f },
        { "D4", -1f },
        { "E4",  0f },
        { "F4",  1f },
        { "G4",  2f },
    };

    void OnPostRender()
    {
        mat.SetPass(0);

        // 오선 그리기
        GL.Begin(GL.LINES);
        GL.Color(Color.black);
        for (int i = 0; i < 5; i++)
        {
            float y = staffStartY + i * staffSpacing;
            GL.Vertex(new Vector3(staffStartX, y, 0));
            GL.Vertex(new Vector3(staffStartX + 400, y, 0));
        }
        GL.End();

        // 음표 그리기
        foreach (var note in notes)
        {
            float yOffset = noteYOffsets[note];
            float x = staffStartX + 60 + System.Array.IndexOf(notes, note) * 60;
            float y = staffStartY + 4 * staffSpacing - yOffset * (staffSpacing / 2);

            DrawEllipse(x, y, noteHeadWidth, noteHeadHeight, 32);
        }
    }

    void DrawEllipse(float cx, float cy, float width, float height, int segments)
    {
        GL.Begin(GL.TRIANGLES);
        GL.Color(Color.black);
        for (int i = 0; i < segments; i++)
        {
            float angle0 = 2 * Mathf.PI * i / segments;
            float angle1 = 2 * Mathf.PI * (i + 1) / segments;
            float x0 = Mathf.Cos(angle0) * width / 2;
            float y0 = Mathf.Sin(angle0) * height / 2;
            float x1 = Mathf.Cos(angle1) * width / 2;
            float y1 = Mathf.Sin(angle1) * height / 2;

            // 중심점
            GL.Vertex3(cx, cy, 0);
            // 현재 점
            GL.Vertex3(cx + x0, cy + y0, 0);
            // 다음 점
            GL.Vertex3(cx + x1, cy + y1, 0);
        }
        GL.End();
    }
=======
>>>>>>> e07dfb26bf7734208eb8a21c3791426be1698ca0
}
