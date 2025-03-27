using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class AudioLoader : MonoBehaviour
{
    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    private string csvFilePath;

    private void Start()
    {
        csvFilePath = Path.Combine(Application.streamingAssetsPath, "Audio/AudioData.csv");
        LoadAudioData().Forget();
    }

    /// <summary>
    /// CSVからAudioClipをロードする
    /// </summary>
    private async UniTaskVoid LoadAudioData()
    {
        List<string> audioNames = await ReadCSV(csvFilePath);

        foreach (string audioName in audioNames)
        {
            AudioClip clip = await LoadAudioClip(audioName);
            if (clip != null)
            {
                audioClips[audioName] = clip;
                Debug.Log($"Loaded Audio: {audioName}");
            }
            else
            {
                Debug.LogError($"Failed to load audio: {audioName}");
            }
        }
    }

    /// <summary>
    /// CSVを読み込む（StreamingAssetsから取得）
    /// </summary>
    private async UniTask<List<string>> ReadCSV(string filePath)
    {
        List<string> audioNames = new List<string>();

        if (!File.Exists(filePath))
        {
            Debug.LogError("CSVファイルが見つかりません: " + filePath);
            return null;
        }
        string fileContent = await File.ReadAllTextAsync(filePath);
        audioNames = ParseCSV(fileContent);

        return audioNames;
    }

    /// <summary>
    /// CSVテキストを解析し、AudioClipの名前リストを取得
    /// </summary>
    private List<string> ParseCSV(string csvText)
    {
        List<string> audioNames = new List<string>();
        using (StringReader reader = new StringReader(csvText))
        {
            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    audioNames.Add(line.Trim());
                }
            }
        }
        return audioNames;
    }

    /// <summary>
    /// `Resources` または `StreamingAssets` から `AudioClip` をロード
    /// </summary>
    private async UniTask<AudioClip> LoadAudioClip(string audioName)
    {
        // 1️⃣ `Resources` からロード
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{audioName}");
        if (clip != null) return clip;

        // 2️⃣ `StreamingAssets` からロード（MP3 / WAV）
        string audioPath = Path.Combine(Application.streamingAssetsPath, "Audio", $"{audioName}.mp3");

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(audioPath, AudioType.MPEG))
        {
            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                return DownloadHandlerAudioClip.GetContent(request);
            }
        }
        return null;
    }

    /// <summary>
    /// ロードしたAudioClipを再生する
    /// </summary>
    public void PlayAudio(string audioName, AudioSource _audioSource = null)
    {
        if (audioClips.TryGetValue(audioName, out AudioClip clip))
        {
            if(_audioSource != null)
            {
                _audioSource.clip = clip;
                _audioSource.PlayOneShot(clip);
            }
            else
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.PlayOneShot(clip);
            }

        }
        else
        {
            Debug.LogError($"音声が見つかりません: {audioName}");
        }
    }
}