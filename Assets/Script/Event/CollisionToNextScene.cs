using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CollisionToNextScene : MonoBehaviour
{
    [Header("�ς������V�[����")]
    public string SceneName;

    SceneChangeManager sceneChangeManager;

    // Start is called before the first frame update
    void Start()
    {
        //�V�[�����[�h�p�R���|�[�l���g
        sceneChangeManager = GameObject.Find("SceneChangeManager").GetComponent<SceneChangeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        //�V�[���؂�ւ��̍ۂ�
        if(SceneName == "SchoolMain 1")
        {
            OptionValue.SpawnSpot = SPAWNSPOT.DEFAULT;
        }

            if (other.CompareTag("Player"))//�e���^�O�ɕt�������O��()�̒��ɓ���Ă�������
            {
            sceneChangeManager.LoadSceneAsyncWithFade(SceneName);
            }

    }
}
