using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ClassroomController : MonoBehaviour
{
    [Header("�g�������̐�")]
    public const int _UseClassroomNum = 20;

    private int _UseKidsNum;//�g���q���̐�(GameManager����擾)

    private int HitNumsCnt;//�A�N�e�B�u��Ԃ̋��������J�E���g����

    private GameObject[] ClassroomParents;

    private GameObject[] Classroom;

    private GameObject[] Doors;

    private GameObject[] Friends;

    private const string FindWords = "TheClassroom";

    private bool[] usedNumbers; // �����g�p���鋳����True�ŋL������0

    private bool[] usedKids;

    private GameManager _gameManager;




    // Start is called before the first frame update
    void Start()
    {
        UniTask.SwitchToMainThread();
        ClassroomLoad();
    }
    //�����ƃh�A�̃A�N�e�B�u��Ԃ�ݒ肷��
    private void SetClassroomActive(int _num,bool _isActive)
    {
            Doors[_num].SetActive(!_isActive);
            Classroom[_num].SetActive(_isActive);
    }

    //���Ԃ�Ȃ��Ŏg�����������߂�
    int GetUniqueRandomNumber()
    {
        int num;
        do
        {
            num = Random.Range(0, usedNumbers.Length);
        } while (usedNumbers[num]); // ���łɎg�p�ς݂Ȃ��蒼��

        usedNumbers[num] = true; // �擾�ς݂Ƃ��ă}�[�N
        return num;
    }
    int GetUniqueRandomNumberKids()
    {
        int num;
        do
        {
            num = Random.Range(0, usedKids.Length);
        } while (usedKids[num]); // ���łɎg�p�ς݂Ȃ��蒼��

        usedKids[num] = true; // �擾�ς݂Ƃ��ă}�[�N

        return num;
    }
    //�^�O���������I�u�W�F�N�g�̐���Ԃ�
    int GetTaggedObjectCount(string tag)
    {
        return GameObject.FindGameObjectsWithTag(tag).Length;
    }

    /// <summary>
    /// �����̃����_���z�u
    /// </summary>
    private void ClassroomLoad()
    {
        //�J�E���g��������
        HitNumsCnt = 0;

        _gameManager = FindObjectOfType<GameManager>();
        _UseKidsNum = _gameManager.PeopleNum;

        //���������ׂĎ擾
        ClassroomParents = GameObject.FindGameObjectsWithTag("Classroom");

        //�z��̏�����
        usedNumbers = new bool[ClassroomParents.Length];

        usedKids = new bool[_UseClassroomNum];


        //UsedNumbers�̏�����
        for (var i = 0; i < ClassroomParents.Length; i++)
        {
            usedNumbers[i] = false;

        }

        //�g�����������Ԃ�Ȃ��Ō���
        for (var i = 0; i < _UseClassroomNum; i++)
        {
            GetUniqueRandomNumber();
            usedKids[i] = false;
        }

        for (var i = 0; i < _UseKidsNum; i++)
        {
            GetUniqueRandomNumberKids();
        }

        // �����̐����̃��X�g���쐬
        Classroom = new GameObject[ClassroomParents.Length];
        Doors = new GameObject[ClassroomParents.Length];
        Friends = new GameObject[ClassroomParents.Length];

        for (var i = 0; i < ClassroomParents.Length; i++) // `<=` �ł͂Ȃ� `<`
        {
            Transform parentTransform = ClassroomParents[i].transform;

            // �q�I�u�W�F�N�g�����ׂĎ擾
            foreach (Transform child in parentTransform)
            {
                // "Doors" �Ƃ������O�̃I�u�W�F�N�g���擾
                if (child.name == "Doors")
                {
                    Doors[i] = child.gameObject;
                }
                // "TheClassroom" ���܂ރI�u�W�F�N�g���擾�i������v�j
                else if (child.name.Contains(FindWords))
                {
                    Classroom[i] = child.gameObject;
                }

            }

            // "Friend" ���܂ރI�u�W�F�N�g���擾
            Friends[i] = Classroom[i].transform.Find("Friend").gameObject;
            Friends[i].SetActive(false);

            SetClassroomActive(i, usedNumbers[i]);

            if (usedNumbers[i])
            {
                //�q�����g�������̎q�����A�N�e�B�u��Ԃ�
                if (HitNumsCnt < usedKids.Length && usedKids[HitNumsCnt])
                {
                    if (Friends[i] != null)
                    {
                        Friends[i].SetActive(true);
                    }
                }

                HitNumsCnt++;

            }


        }
        int targetCount = GetTaggedObjectCount("Target");
        Debug.Log($"�^�O 'Target' �����I�u�W�F�N�g�̐�: {targetCount}");

    }
}
