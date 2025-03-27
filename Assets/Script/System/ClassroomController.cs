using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ClassroomController : MonoBehaviour
{
    [Header("使う教室の数")]
    public const int _UseClassroomNum = 20;

    private int _UseKidsNum;//使う子供の数(GameManagerから取得)

    private int HitNumsCnt;//アクティブ状態の教室数をカウントする

    private GameObject[] ClassroomParents;

    private GameObject[] Classroom;

    private GameObject[] Doors;

    private GameObject[] Friends;

    private const string FindWords = "TheClassroom";

    private bool[] usedNumbers; // 教室使用する教室をTrueで記憶する0

    private bool[] usedKids;

    private GameManager _gameManager;




    // Start is called before the first frame update
    void Start()
    {
        UniTask.SwitchToMainThread();
        ClassroomLoad();
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
    int GetUniqueRandomNumberKids()
    {
        int num;
        do
        {
            num = Random.Range(0, usedKids.Length);
        } while (usedKids[num]); // すでに使用済みならやり直し

        usedKids[num] = true; // 取得済みとしてマーク

        return num;
    }
    //タグを持ったオブジェクトの数を返す
    int GetTaggedObjectCount(string tag)
    {
        return GameObject.FindGameObjectsWithTag(tag).Length;
    }

    /// <summary>
    /// 教室のランダム配置
    /// </summary>
    private void ClassroomLoad()
    {
        //カウントを初期化
        HitNumsCnt = 0;

        _gameManager = FindObjectOfType<GameManager>();
        _UseKidsNum = _gameManager.PeopleNum;

        //教室をすべて取得
        ClassroomParents = GameObject.FindGameObjectsWithTag("Classroom");

        //配列の初期化
        usedNumbers = new bool[ClassroomParents.Length];

        usedKids = new bool[_UseClassroomNum];


        //UsedNumbersの初期化
        for (var i = 0; i < ClassroomParents.Length; i++)
        {
            usedNumbers[i] = false;

        }

        //使う教室をかぶりなしで決定
        for (var i = 0; i < _UseClassroomNum; i++)
        {
            GetUniqueRandomNumber();
            usedKids[i] = false;
        }

        for (var i = 0; i < _UseKidsNum; i++)
        {
            GetUniqueRandomNumberKids();
        }

        // 教室の数分のリストを作成
        Classroom = new GameObject[ClassroomParents.Length];
        Doors = new GameObject[ClassroomParents.Length];
        Friends = new GameObject[ClassroomParents.Length];

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

            // "Friend" を含むオブジェクトを取得
            Friends[i] = Classroom[i].transform.Find("Friend").gameObject;
            Friends[i].SetActive(false);

            SetClassroomActive(i, usedNumbers[i]);

            if (usedNumbers[i])
            {
                //子供を使う教室の子供をアクティブ状態に
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
        Debug.Log($"タグ 'Target' を持つオブジェクトの数: {targetCount}");

    }
}
