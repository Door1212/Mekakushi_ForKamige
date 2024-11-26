using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲームのオプション情報の保持に用いるグローバル変数群
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
    // どのシーンからでもアクセスできる変数
    //顏を使うかどうか
    public static bool IsFaceDetecting = false;

    public static string DeathScene = "SchoolMain 1";

    public static SPAWNSPOT SpawnSpot = SPAWNSPOT.DEFAULT;

    public static Vector3[] _SpawnPoint = new Vector3[(int)SPAWNSPOT.MAX_SPOT];

    public static bool InStealth = false;
}
