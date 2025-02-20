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
        //�V���O���g��
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

        //�J�n���L�^
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
#else

 /// <summary>
    /// �A�v���P�[�V�����I�����ɏI�����O
    /// </summary>
    private void OnApplicationQuit()
    {
        LogEnd();
    }

#endif





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
        string _startLog = $"\n�Q�[���J�n\n";
        File.AppendAllText(_filePath, _startLog);
    }
    public static void LogEnd()
    {
        //�C���X�^���X���Ȃ��ꍇ�̓��^�[��
        if (_instance == null) return;

        string _endLog = $"�Q�[���I��";
        File.AppendAllText(_instance._filePath, _endLog);
    }
}
