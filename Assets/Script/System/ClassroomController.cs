using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassroomController : MonoBehaviour
{
    [Header("使う教室の数")]
    public const int _UseClassroomNum = 20;

    private GameObject[] ClassroomParents;

    private GameObject[] Classroom;

    private GameObject[] Doors;

    private const string FindWords = "TheClassroom";

    private bool[] usedNumbers; // 教室使用する教室をTrueで記憶する



    // Start is called before the first frame update
    void Start()
    {
        ClassroomParents = GameObject.FindGameObjectsWithTag("Classroom");

        usedNumbers = new bool[ClassroomParents.Length];

        //UsedNumbersの初期化
        for (var i = 0; i < ClassroomParents.Length; i++)
        {
            usedNumbers[i] = false;
        }

        //使う教室をかぶりなしで決定
        for (var i = 0; i < _UseClassroomNum; i++)
        {
            GetUniqueRandomNumber();
        }

        // 教室の数分のリストを作成
        Classroom = new GameObject[ClassroomParents.Length];
        Doors = new GameObject[ClassroomParents.Length];

        for (var i = 0; i < ClassroomParents.Length; i++) // `<=` ではなく `<`
        {
            Transform parentTransform = ClassroomParents[i].transform;

            // 子オブジェクトをすべて取得
            foreach (Transform child in parentTransform)
            {
                // "Doors" という名前のオブジェクトを取得
                if (child.name == "Doors")
                {
                    Doors[i] = child.gameObject;
                }
                // "TheClassroom" を含むオブジェクトを取得（部分一致）
                else if (child.name.Contains(FindWords))
                {
                    Classroom[i] = child.gameObject;
                }
            }

            SetClassroomActive(i,usedNumbers[i]);
        }

        // 確認ログ
        for (int i = 0; i < ClassroomParents.Length; i++)
        {
            Debug.Log($"教室 {i + 1}: {Classroom[i]?.name}, ドア: {Doors[i]?.name}");
        }


    }
    //教室とドアのアクティブ状態を設定する
    private void SetClassroomActive(int _num,bool _isActive)
    {
            Doors[_num].SetActive(!_isActive);
            Classroom[_num].SetActive(_isActive);
    }

    //かぶりなしで使う教室を決める
    int GetUniqueRandomNumber()
    {
        int num;
        do
        {
            num = Random.Range(0, usedNumbers.Length);
        } while (usedNumbers[num]); // すでに使用済みならやり直し

        usedNumbers[num] = true; // 取得済みとしてマーク
        return num;
    }
}
