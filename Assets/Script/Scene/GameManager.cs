using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
//

public class GameManager : MonoBehaviour
{
    //�G�^�C�v
    EnemyTypeSelector enemyType;

    SceneChangeManager sceneChangeManager;

    //��������
    [Tooltip("��������")]
    [SerializeField]
    [Range(0f,300f)]
    private float TimeLimit;

    //������l��
    [Tooltip("������l��")]
    [SerializeField]
    [Range(0, 10)]
    private int PeopleNum;

    public int isFindpeopleNum;

    //[SerializeField]
    //TextMeshProUGUI PeopleNumTMP;

    [Tooltip("�������ԂŏI���Ȃ�`�F�b�N")]
    public bool isTimeLim = false;

    private float NowTime = 0.0f;

    //���ꂪtrue�ɂȂ�ƃQ�[���I�[�o�[
    public bool isGameOver = false;

    //���ꂪtrue�ɂȂ�ƃQ�[���N���A
    public bool isGameClear = false;

    //�I�v�V�������g�p�\�ł��邩?
    public bool isEnableToOpenOption;

    //�V�[���@�ۂɓ��������Ƃ𔻒�
    private bool isGameOverClear = false;
    [SerializeField]
    //�ꊇ�ł��낢��~�߂�ׂ̕ϐ�
    private bool StopAll = false;
    //�ꊇ�ł��낢��~�߂�ׂ̕ϐ��̕⏕
    private bool PreStopAll = false;

    [Header("PlayerMove")]
    [SerializeField]
    private PlayerMove playerMove;
    [Header("CameraMove")]
    [SerializeField]
    private CameraMove CameraMove;
    [Header("EnemyAIMove")]
    [SerializeField]
    private EnemyAI_move[] EnemyAI_Moves;

    private DoorOpen[] AllDoor;

    private playSound PlaySound;

    private DirectionalSound[] directionalSound; 

    private SoundWall soundWall;

    // Start is called before the first frame update
    void Start()
    {
        isGameOverClear = false;
        isGameOver = false;
        isGameClear = false;
        //�I�v�V�������g�p�\�ł��邩?
        isEnableToOpenOption = true;

    //�e�L�X�g�̐F��ς���
    //PeopleNumTMP.color = Color.red;
    playerMove.GetComponent<PlayerMove>();
        CameraMove.GetComponent<CameraMove>();
   
        //for(int i = 0; i < EnemyAI_Moves.Length; i++)
        //{
        //    EnemyAI_Moves[i] = new EnemyAI_move();
        //    EnemyAI_Moves[i].GetComponent<EnemyAI_move>();
        //}

        // �V�[�����̂��ׂẴh�A���擾
        AllDoor = FindObjectsOfType<DoorOpen>();

        PlaySound = FindObjectOfType<playSound>();
        
        //�q���Ə��̉�
        directionalSound = FindObjectsOfType<DirectionalSound>();
        soundWall = FindObjectOfType<SoundWall>();
    }

    // Update is called once per frame
    void Update()
    {
        NowTime += Time.deltaTime;

        if(StopAll == true && PreStopAll == false)
        {
            DoStopAll();
        }

        if (StopAll == false && PreStopAll == true)
        {
            DoStopAll();
        }

#if UNITY_EDITOR

        if(Input.GetKey(KeyCode.Tab) && Input.GetKey(KeyCode.F1))
        {
            OptionValue.InStealth = false;
            if (SceneChangeManager.Instance != null) {
                SceneChangeManager.Instance.LoadSceneAsyncWithFade("SchoolMain 1");
            }
            else
            {
                SceneManager.LoadScene("SchoolMain 1");
            }
        }

        if (Input.GetKey(KeyCode.Tab) && Input.GetKey(KeyCode.F2))
        {
            OptionValue.InStealth = false;
            if (SceneChangeManager.Instance != null)
            {
                SceneChangeManager.Instance.LoadSceneAsyncWithFade("SchoolMain 2");
            }
            else
            {
                SceneManager.LoadScene("SchoolMain 2");
            } 
        }

        if (Input.GetKey(KeyCode.Tab) && Input.GetKey(KeyCode.F3))
        {
            OptionValue.InStealth = false;
            if (SceneChangeManager.Instance != null)
            {
                SceneChangeManager.Instance.LoadSceneAsyncWithFade("SchoolMain 3");
            }
            else
            {
                SceneManager.LoadScene("SchoolMain 3");
            }
        }
#endif

        //UpdatePeopleText();
        if (!isGameOverClear)
        {
            //�Q�[���I�[�o�[�Ń��U���g�Ɉڍs������(�ق�Ƃɂ��ꂾ��)
            if (isGameOver)
            {
                Cursor.lockState = CursorLockMode.None;
                OptionValue.InStealth = false;
                OptionValue.DeathScene = SceneManager.GetActiveScene().name;
                isGameOverClear = true;
                if (SceneChangeManager.Instance != null)
                {
                    SceneChangeManager.Instance.LoadSceneAsyncWithFade("GameOver");
                }
                else
                {
                    SceneManager.LoadScene("GameOver");
                }

            }

            if (isGameClear)
            {
                Cursor.lockState = CursorLockMode.None;
                isGameOverClear = true;
                if (SceneChangeManager.Instance != null)
                {
                    SceneChangeManager.Instance.LoadSceneAsyncWithFade("ResultHonban");
                }
                else
                {
                    SceneManager.LoadScene("ResultHonban");
                }

            }
        }
        if (isTimeLim)
        {
            //�w�莞�Ԍo�ƃQ�[���N���A
            if (NowTime > TimeLimit)
            {
                 isGameClear = true;
            }
        }
        else
        {
            //���ׂČ���������
            if(isFindpeopleNum == PeopleNum)
            {
                isGameClear = true;
            }
        }

        PreStopAll = StopAll;
    }

    //������l�����擾���邽�߂�Get�֐�
    public int GetPeopleNum()
    {
        return PeopleNum - isFindpeopleNum;
    }

    //��L��Get�֐��𗘗p����UI��text�X�V�֐�
    void UpdatePeopleText()
    {
       // PeopleNumTMP.SetText(GetPeopleNum().ToString());

    }

    public void SetStopAll(bool Set)
    {
        StopAll = Set;
    }
    /// <summary>
    /// �I�v�V�������Ȃǂ�Update���~�߂������̂������ɒǉ�
    /// </summary>
    private void DoStopAll()
    {
        playerMove.SetCanMove(!StopAll);
        CameraMove.SetCanMove(!StopAll);
        PlaySound.SetCanMove(!StopAll);
        soundWall.SetCanMove(!StopAll);

        for (int i = 0; i < EnemyAI_Moves.Length; i++)
        {
            EnemyAI_Moves[i].SetCanMove(!StopAll);
        }

        for(int i = 0; i < AllDoor.Length; i++)
        {
            AllDoor[i].SetCanMove(!StopAll);
        }

        for (int i = 0; i < directionalSound.Length; i++)
        {
            directionalSound[i].SetCanMove(!StopAll);
        }
    }
}
