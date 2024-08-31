using UniRx;
using UnityEngine;

public class MainThreadDispatcherInitializer : MonoBehaviour
{
    void Awake()
    {
        // メインスレッドディスパッチャーがシーンに存在しない場合は作成する
        if (FindObjectOfType<MainThreadDispatcher>() == null)
        {
            var go = new GameObject("MainThreadDispatcher");
            go.AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
    }
}
