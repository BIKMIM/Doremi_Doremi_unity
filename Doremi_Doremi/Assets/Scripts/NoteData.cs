using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �ϳ��� ��ǥ ���� ���� Ŭ����

public class NoteData
{
    public string noteName;   // ��: "C4"
    public int duration;      // ��: 1, 2, 4, 8, 16
    public bool isDotted;     // ����ǥ ����
    public bool isRest;       // ��ǥ ����
    public AccidentalType accidental; // �ӽ�ǥ Ÿ�� (Sharp, Flat, Natural ��)


    public override string ToString() // ��ǥ ������ ���ڿ��� ��ȯ�ϴ� �޼���
    {
        return $"{noteName} | {duration}����ǥ | ����ǥ: {isDotted} | ��ǥ: {isRest}"; 
    }
}

