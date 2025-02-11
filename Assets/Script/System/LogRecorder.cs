using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public sealed class LogRecorder : MonoBehaviour
{

    private static LogRecorder instance = new LogRecorder();

    private string filePath;

    private void Awake()
    { 

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "Playlog.csv");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static LogRecorder GetInstance()
    {
        return instance;
    }

    public void LogEvent(string eventData)
    {
        string log = $"{Time.time},{eventData}\n";
        File.AppendAllText(filePath, log);
    }
}
