using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Cysharp.Threading.Tasks;
/// <summary>
/// CSVファイルの読み込みを行う
/// </summary>
public class CSVReader : MonoBehaviour
{
    //ロードが完了しているか
    public bool _isLoadDone;

    // Start is called before the first frame update
    void Start()
    {
        _isLoadDone = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async UniTask<List<MetaAI.HeartRateValue>> ReadCSV(string _filePath)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, _filePath);
        List<MetaAI.HeartRateValue> heartRateValues = new List<MetaAI.HeartRateValue>();


        if (!File.Exists(fullPath))
        {
            Debug.LogError("CSVファイルが見つかりません: " + fullPath);
            return null;
        }

        string fileContent = await File.ReadAllTextAsync(fullPath);
        ParseCSV(fileContent, heartRateValues);

        _isLoadDone = true;
        Debug.Log("ロード完了状態"+_isLoadDone);

        return heartRateValues;
    }

    private void ParseCSV(string csvText, List<MetaAI.HeartRateValue> heartRateValues)
    {
        using (StringReader reader = new StringReader(csvText))
        {
            reader.ReadLine(); // ヘッダーをスキップ

            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                if (values.Length < 3) continue;

                MetaAI.HeartRateValue heartRate = new MetaAI.HeartRateValue
                {
                    _valueName = values[0],
                    _HopeValue = float.TryParse(values[1], out float hope) ? hope : 0f,
                    _FearValue = float.TryParse(values[2], out float fear) ? fear : 0f
                };

                heartRateValues.Add(heartRate);
            }
        }
    }
}
