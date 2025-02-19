using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UniRx.Diagnostics;

public sealed class LogRecorder : MonoBehaviour
{

    private static LogRecorder _instance = new LogRecorder();

    private string _filePath;

    private void Awake()
    { 
        //シングルトン
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        _filePath = Path.Combine(Application.persistentDataPath, "Playlog.csv");

        //開始を記録
        LogStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#if UNITY_EDITOR

    [InitializeOnLoadMethod]
    private static void RegisterPlayModeStateChange()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            LogEnd();
        }
    }

#endif

    /// <summary>
    /// アプリケーション終了時に終了ログ
    /// </summary>
    private void OnApplicationQuit()
    {
        LogEnd();
    }



    public static LogRecorder GetInstance()
    {
        return _instance;
    }

    public void LogEvent(string _eventData)
    {
        string _log = $"{Time.time.ToString("N2")},{_eventData}\n";
        File.AppendAllText(_filePath, _log);
    }

    public void LogStart()
    {
        string _startLog = $"\nゲーム開始";
        File.AppendAllText(_filePath, _startLog);
    }
    public void LogEnd()
    {
        string _endLog = $"ゲーム終了";
        File.AppendAllText(_filePath, _endLog);
    }
}
