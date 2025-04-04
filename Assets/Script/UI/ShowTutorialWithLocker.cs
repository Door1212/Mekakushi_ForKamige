using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ShowTutorialWithLocker : MonoBehaviour
{
    [Header("こども発見チュートリアル表示")]
    public UIFade _uifade_kids;

    [Header("ロッカーに入るチュートリアル表示")]
    public UIFade uifade_Inlocker;

    [Header("身をひそめるチュートリアル表示")]
    public UIFade uifade_Hide;

    [Header("消えるまでの時間")]
    public float TimeForReset;

    [Header("敵出現までの時間")]
    public float TimeForEnemy;

    [Header("トリガーとなる子供")]
    public HidingCharacter _kid;

    [Header("ロックするドア")]
    public DoorOpen[] _Doors;

    //ロッカーの開閉取得用
    private LockerOpen _locker;
    //一回目の開閉であるかどうか
    private bool _isFirst;
    //テキスト表示コンポーネント
    private TextTalk _talk;
    //ゲームマネージャー
    private GameManager _gameManager;
    //チュートリアル用敵コントローラー
    private EnemyTutorialController _enemyTutorialController;

    // Start is called before the first frame update
    void Start()
    {
        _locker = GetComponent<LockerOpen>();
        _talk = FindObjectOfType<TextTalk>();
        _gameManager = GetComponent<GameManager>();
        _enemyTutorialController = FindObjectOfType<EnemyTutorialController>();
        _isFirst = false;
    }

    // Update is called once per frame
    void Update()
    {
        //ロッカーがなければ
        if(_locker == null)
        {
            Debug.LogError("LockerOpen is Not Found!");
        }

        if (_locker.IsOpen && !_isFirst)
        {
            _isFirst = true;
            DoFadeInFadeOut().Forget();
        }
    }

    private async UniTask DoFadeInFadeOut()
    {
        _uifade_kids.StartFadeIn();

        await UniTask.WaitForSeconds(TimeForReset);

        _uifade_kids.StartFadeOut();

        await UniTask.WaitUntil(() => _kid.IsCatched);

        await UniTask.WaitForSeconds(TimeForEnemy);

        //敵が現れる音
        _enemyTutorialController.SpawnTutoEnemy();

        //ドアを閉めてロックする
        for (int i = 0; i < _Doors.Length; i++)
        {
            _Doors[i].Doorlock = true;
            _Doors[i].ForceCloseDoor();
            Debug.Log("戸締り");
        }

        //ロッカーに入るチュートリアル出す
        uifade_Inlocker.StartFadeIn();

        Debug.Log("ロッカーに入るまで待機中");
        //プレイヤーがロッカーの中に入るまで待つ
        await UniTask.WaitUntil(() =>_locker._isPlayerIn);

        //敵にロッカーに入った事を伝える
        _enemyTutorialController._isInlocker = true;

        //ロッカーに入るチュートリアルしまう
        uifade_Inlocker.StartFadeOut();

        //隠れるチュートリアル出してしまう
        uifade_Hide.StartFadeOutIn();

        //敵が動き周り消えるまで待つ
        await UniTask.WaitUntil(() => _enemyTutorialController.GetIsDisappearEnemy()); 

        _talk.SetText("どこかに行ったみたいだ……なんだったんだあれは……", 3.0f, 0.5f);

        await UniTask.WaitForSeconds(3.0f);

        _talk.SetText("友達を探さなきゃ......", 3.0f, 0.5f);

        await UniTask.WaitForSeconds(4.0f);

        if (SceneChangeManager.Instance != null)
        {
            SceneChangeManager.Instance.LoadSceneAsyncWithFade("TrueSchool");
        }
        else
        {
            Debug.Log("シーン切り替え");
        }


    }
}
