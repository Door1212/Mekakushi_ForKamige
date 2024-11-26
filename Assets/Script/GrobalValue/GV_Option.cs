using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �Q�[���̃I�v�V�������̕ێ��ɗp����O���[�o���ϐ��Q
/// </summary>
/// 

public enum SPAWNSPOT
{
    DEFAULT,
    TUTO_STEALTH,
    MAX_SPOT
}

public static class OptionValue
{
    // �ǂ̃V�[������ł��A�N�Z�X�ł���ϐ�
    //�����g�����ǂ���
    public static bool IsFaceDetecting = false;

    public static string DeathScene = "SchoolMain 1";

    public static SPAWNSPOT SpawnSpot = SPAWNSPOT.DEFAULT;

    public static Vector3[] _SpawnPoint = new Vector3[(int)SPAWNSPOT.MAX_SPOT];

    public static bool InStealth = false;
}
