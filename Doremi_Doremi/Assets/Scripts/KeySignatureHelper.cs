using System.Collections.Generic;

public static class KeySignatureHelper
{
    // ��ǥ ����
    private static readonly Dictionary<string, List<string>> majorKeyAccidentals = new()
    {
        { "C", new() },
        { "G", new() { "F#" } },
        { "D", new() { "F#", "C#" } },
        { "A", new() { "F#", "C#", "G#" } },
        { "E", new() { "F#", "C#", "G#", "D#" } },
        { "B", new() { "F#", "C#", "G#", "D#", "A#" } },
        { "F#", new() { "F#", "C#", "G#", "D#", "A#", "E#" } },
        { "C#", new() { "F#", "C#", "G#", "D#", "A#", "E#", "B#" } },

        { "F", new() { "Bb" } },
        { "Bb", new() { "Bb", "Eb" } },
        { "Eb", new() { "Bb", "Eb", "Ab" } },
        { "Ab", new() { "Bb", "Eb", "Ab", "Db" } },
        { "Db", new() { "Bb", "Eb", "Ab", "Db", "Gb" } },
        { "Gb", new() { "Bb", "Eb", "Ab", "Db", "Gb", "Cb" } },
        { "Cb", new() { "Bb", "Eb", "Ab", "Db", "Gb", "Cb", "Fb" } },
    };

    /// <summary>
    /// �־��� �������� ����Ǵ� �� �Ǵ� �÷� �� �̸��� ��ȯ
    /// </summary>
    public static List<string> GetAccidentals(string key)
    {
        if (majorKeyAccidentals.TryGetValue(key, out var accidentals))
            return accidentals;
        else
            return new List<string>(); // �⺻��: C ���� ��
    }

    /// <summary>
    /// ��ǥ ���� �� �� �̸� ��ȯ (��: key=G, input=F4 �� output=F#4)
    /// </summary>
    public static string ApplyAccidental(string noteName, string key)
    {
        var accidentals = GetAccidentals(key);

        // �� �̸��� ���� (��: F4 �� F)
        string pitch = noteName[..^1];  // "F4" �� "F"
        string octave = noteName[^1..]; // "F4" �� "4"

        foreach (var acc in accidentals)
        {
            string basePitch = acc[..^1]; // "F#"
            string accidental = acc[^1..]; // "#" or "b"

            if (noteName.StartsWith(basePitch) && !noteName.Contains("#") && !noteName.Contains("b"))
                return acc + octave;
        }

        return noteName; // ���� ����
    }
}
