using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲームのオプション情報の保持に用いるグローバル変数群
/// </summary>
public static class OptionValue
{
    // どのシーンからでもアクセスできる変数
    //顏を使うかどうか
    public static bool IsFaceDetecting = true;

    public static string DeathScene = "SchoolMain 1";
}
