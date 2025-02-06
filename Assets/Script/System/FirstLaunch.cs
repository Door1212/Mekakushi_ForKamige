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
        string destinationPath = Path.Combine(Application.persistentDataPath, "MetaAIData.csv");

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


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
