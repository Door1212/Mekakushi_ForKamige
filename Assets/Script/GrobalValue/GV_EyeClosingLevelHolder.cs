using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ★どのシーンからでもアクセスできるクラス★
public static class EyeClosingLevel
{
    // どのシーンからでもアクセスできる変数
    public static float REyeClosingLevelValue = 0.5f;
    public static float LEyeClosingLevelValue = 0.5f;
}

//すでにチュートリアルを受けたかを保持する変数群
public static class IsEndTutorial
{
    public static bool IsEyeTutorial = false;
    public static bool IsGameTutorial = false;
}