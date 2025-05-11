using System;  // �⺻ �ý��� ���ӽ����̽�
using System.Collections.Generic;  // Dictionary �� �����̳� ���

/// <summary>
/// NoteMapping Ŭ������ ��ǥ �̸�(��: "C4")�� MIDI ��ȣ�� ��ȯ�ϰ�,
/// �ش� MIDI�� �������� ���� �������� lineIndex(���� ��ġ)�� ����ϴ� ��ƿ��Ƽ�Դϴ�.
/// </summary>
public static class NoteMapping
{
    // ���̸��� ����(semitone)���� �����ϴ� Dictionary
    private static readonly Dictionary<string, int> noteToSemitone = new()
    {
        { "C", 0 }, { "C#", 1 }, { "Db", 1 },  // C �迭
        { "D", 2 }, { "D#", 3 }, { "Eb", 3 },  // D �迭
        { "E", 4 },                               // E
        { "F", 5 }, { "F#", 6 }, { "Gb", 6 },  // F �迭
        { "G", 7 }, { "G#", 8 }, { "Ab", 8 },  // G �迭
        { "A", 9 }, { "A#", 10 }, { "Bb", 10 },// A �迭
        { "B", 11 }                               // B
    };

    private const int referenceMidi = 67;            // ���� ��: G4�� MIDI ��ȣ (G4 = 67)
    private const float referenceLineIndex = 0f;     // ���� ��(G4)�� ��ġ�� lineIndex
    private const float lineSpacingPerSemitone = 0.5f; // �������� ������ lineIndex ����

    /// <summary>
    /// ��ǥ �̸�(noteName)�� �����ϴ� lineIndex�� ��ȯ�մϴ�.
    /// MIDI ��ȣ ���̸� �̿��� ���� ���� �������� ����մϴ�.
    /// </summary>
    /// <param name="noteName">��: "C4", "D#5"</param>
    /// <returns>���� �������� ����� ��ġ �ε���</returns>
    public static float GetLineIndex(string noteName)
    {
        // ��ǥ �̸� �� MIDI ��ȣ ��ȯ
        int midi = NoteToMidi(noteName);
        // ���� MIDI�� ���Ͽ� ������ ����ϰ�, semitone�� 0.5 ���� �̵� ����
        return (midi - referenceMidi) * lineSpacingPerSemitone + referenceLineIndex;
    }

    /// <summary>
    /// ��ǥ ���ڿ�(��: "C#4")�� MIDI ��ȣ�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="note">��ǥ ���ڿ�</param>
    /// <returns>MIDI ��ȣ(int)</returns>
    /// <exception cref="ArgumentException">�߸��� ��Ÿ�� �Ǵ� ��ġ�� �� ���� �߻�</exception>
    public static int NoteToMidi(string note)
    {
        note = note.Trim();  // �յ� ���� ����
        // ������ �� ���ڴ� ��Ÿ�� ����(��: '4'), �������� ��ġ(��: "C#")
        string pitch = note.Substring(0, note.Length - 1);
        string octaveStr = note.Substring(note.Length - 1);

        // ��Ÿ�� ���ڿ��� ������ ��ȯ
        if (!int.TryParse(octaveStr, out int octave))
            throw new ArgumentException($"Invalid octave in note: {note}");

        // ��ġ ���ڿ��� semitone ������ ��ȸ
        if (!noteToSemitone.TryGetValue(pitch, out int semitone))
            throw new ArgumentException($"Invalid pitch in note: {note}");

        // MIDI ��ȣ ����: ��Ÿ�� * 12 + semitone, �� ��Ÿ�� 0�� C-1�� ����Ű�Ƿ� +1 ����
        return 12 * (octave + 1) + semitone;
    }
}
