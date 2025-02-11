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
            // ����N��
            Debug.Log("����N���ł��I");
            PlayerPrefs.SetInt(FirstLaunchKey, 1);
            PlayerPrefs.Save(); // �f�[�^��ۑ�
        }
        else
        {
            Debug.Log("2��ڈȍ~�̋N��");
        }

       //���^AI�p��
       if(!File.Exists(destinationPath))
        {
            // �t�@�C�������݂��Ȃ��ꍇ
            Debug.Log("�t�@�C�������݂��܂���");
            File.Copy(sourcePath, destinationPath);
        }
        else
        {
            Debug.Log("�t�@�C�������݂��܂�");
        }

        if (!File.Exists(destinationPathForPlaylog))
        {
            // �t�@�C�������݂��Ȃ��ꍇ
            Debug.Log("�v���C���O�t�@�C�������݂��܂���");
            File.Copy(sourcePathForPlaylog, destinationPathForPlaylog);
        }
        else
        {
            Debug.Log("�v���C���O�t�@�C�������݂��܂�");
        }

        Debug.Log("�t�@�C���̕ۑ���: " + destinationPathForPlaylog);


    }

}
