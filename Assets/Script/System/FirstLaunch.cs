using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FirstLaunch : MonoBehaviour
{

    private const string FirstLaunchKey = "isFirstLaunch";


    // Start is called before the first frame update
    void Start()
    {

        string sourcePath = Path.Combine(Application.streamingAssetsPath, "MetaAI/MetaAIData.csv");
        string sourcePathForPlaylog = Path.Combine(Application.streamingAssetsPath, "MetaAI/Playlog.csv");
        string destinationPath = Path.Combine(Application.persistentDataPath, "MetaAIData.csv");
        string destinationPathForPlaylog = Path.Combine(Application.persistentDataPath, "Playlog.csv");

        if (!PlayerPrefs.HasKey(FirstLaunchKey))
        {
            // 初回起動
            Debug.Log("初回起動です！");
            PlayerPrefs.SetInt(FirstLaunchKey, 1);
            PlayerPrefs.Save(); // データを保存
        }
        else
        {
            Debug.Log("2回目以降の起動");
        }

       //メタAI用の
       if(!File.Exists(destinationPath))
        {
            // ファイルが存在しない場合
            Debug.Log("ファイルが存在しません");
            File.Copy(sourcePath, destinationPath);
        }
        else
        {
            Debug.Log("ファイルが存在します");
        }

        if (!File.Exists(destinationPathForPlaylog))
        {
            // ファイルが存在しない場合
            Debug.Log("プレイログファイルが存在しません");
            File.Copy(sourcePathForPlaylog, destinationPathForPlaylog);
        }
        else
        {
            Debug.Log("プレイログファイルが存在します");
        }

        Debug.Log("ファイルの保存先: " + destinationPathForPlaylog);


    }

}
