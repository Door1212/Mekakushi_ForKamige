using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassroomController : MonoBehaviour
{
    [Header("�g�������̐�")]
    public const int _UseClassroomNum = 20;

    private GameObject[] ClassroomParents;

    private GameObject[] Classroom;

    private GameObject[] Doors;

    private const string FindWords = "TheClassroom";

    private bool[] usedNumbers; // �����g�p���鋳����True�ŋL������



    // Start is called before the first frame update
    void Start()
    {
        ClassroomParents = GameObject.FindGameObjectsWithTag("Classroom");

        usedNumbers = new bool[ClassroomParents.Length];

        //UsedNumbers�̏�����
        for (var i = 0; i < ClassroomParents.Length; i++)
        {
            usedNumbers[i] = false;
        }

        //�g�����������Ԃ�Ȃ��Ō���
        for (var i = 0; i < _UseClassroomNum; i++)
        {
            GetUniqueRandomNumber();
        }

        // �����̐����̃��X�g���쐬
        Classroom = new GameObject[ClassroomParents.Length];
        Doors = new GameObject[ClassroomParents.Length];

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

            SetClassroomActive(i,usedNumbers[i]);
        }

        // �m�F���O
        for (int i = 0; i < ClassroomParents.Length; i++)
        {
            Debug.Log($"���� {i + 1}: {Classroom[i]?.name}, �h�A: {Doors[i]?.name}");
        }


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
}
