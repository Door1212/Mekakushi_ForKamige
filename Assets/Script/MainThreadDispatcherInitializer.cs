using UniRx;
using UnityEngine;

public class MainThreadDispatcherInitializer : MonoBehaviour
{
    void Awake()
    {
        // ���C���X���b�h�f�B�X�p�b�`���[���V�[���ɑ��݂��Ȃ��ꍇ�͍쐬����
        if (FindObjectOfType<MainThreadDispatcher>() == null)
        {
            var go = new GameObject("MainThreadDispatcher");
            go.AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
    }
}
