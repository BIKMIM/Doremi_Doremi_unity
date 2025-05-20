using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ���ڿ� "C4:4." �� NoteData ��ü�� ��ȯ

public static class NoteParser
{
    public static NoteData Parse(string raw)   
    {
        var data = new NoteData(); 

        data.isRest = raw.StartsWith("R"); 
        string[] parts = raw.Split(':'); 

        data.noteName = parts[0]; 
        data.duration = 8; // �⺻��

        if (parts.Length > 1) 
        {
            string durPart = parts[1];
            data.isDotted = durPart.EndsWith(".");
            string durVal = data.isDotted ? durPart.TrimEnd('.') : durPart;
            if (int.TryParse(durVal, out int result))
                data.duration = result; 
        }

        return data;
    }
}

