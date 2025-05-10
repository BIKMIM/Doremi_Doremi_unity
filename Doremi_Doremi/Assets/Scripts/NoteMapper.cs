// === 🎼 2. NoteMapper.cs ===
using System.Collections.Generic;

public class NoteMapper
{
    private Dictionary<string, float> noteToIndex;

    public NoteMapper()
    {
        noteToIndex = new Dictionary<string, float>
        {
            { "E3", -3.5f }, { "F3", -3.0f }, { "G3", -2.5f }, { "A3", -2.0f }, { "B3", -1.5f },
            { "C4", -1.0f }, { "D4", -0.5f }, { "E4",  0f  }, { "F4",  0.5f },
            { "G4",  1.0f }, { "A4",  1.5f }, { "B4",  2f  },
            { "C5",  2.5f }, { "D5",  3f  }, { "E5",  3.5f }, { "F5",  4f  },
            { "G5",  4.5f }, { "A5",  5f  }, { "B5",  5.5f }, { "C6",  6f  }
        };
    }

    public bool TryGetIndex(string pitch, out float index)
        => noteToIndex.TryGetValue(pitch, out index);
}