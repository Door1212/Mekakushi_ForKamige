using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;
//using UnityEditor.Experimental.GraphView;

//グラフ情報
public class GraphPoint
{
    public string label; // 点の名前
    public Vector2 position; // X-Y 座標
    public Color color = Color.clear; // 点の色
}

/// <summary>
/// 数の制限を持たせたList<T>
/// </summary>
/// <typeparam name="T"></typeparam>
public class LimitedQueueList<T>
{
    private List<T> _list = new List<T>();
    private int _maxCapacity;

    public LimitedQueueList(int maxCapacity)
    {
        _maxCapacity = maxCapacity;
    }

    public void Add(T item)
    {
        if (_list.Count >= _maxCapacity)
        {
            _list.RemoveAt(0); // 一番古いデータを削除
        }

        _list.Add(item);
    }

    public List<T> GetList() => _list;
}

[RequireComponent(typeof(CSVReader))]


public class MetaAI : MonoBehaviour
{
    //データ保存用ファイルパス
    string filePath;

    //データ取得用ファイルパス
    string _dataFilePath;

    //したいこと
    //動的な敵の生成
    //動的な難易度変更
    //チュートリアルの提示

    //感情の二次元マップと
    //ゲームへの習熟度
    //の二軸でAIは判断する

    //感情マッピングの為の値
    private const float _MaxFear = 1.0f;    //敗北への恐怖の最大値
    private const float _MinFear = -1.0f;   //敗北への恐怖の最小値
    private const float _MaxHope = 1.0f;    //勝利への希望の最大値
    private const float _MinHope = -1.0f;   //勝利への希望のの最小値
    [Header("敗北への恐怖の初期値")]
    [SerializeField] private float _InitFear = -1.0f; //敗北初期値
    [Header("勝利への希望の初期値")]
    [SerializeField] private float _InitHope = -1.0f; //希望初期値
    [Header("敗北への恐怖の現在の値")]
    [SerializeField] private float _CurrentFear; //現在の敗北への恐怖の値
    [Header("勝利への希望の現在の値")]
    [SerializeField] private float _CurrentHope; //現在の勝利への希望の値
    [Header("恐怖の移動可能範囲")]
    [SerializeField] private Vector2 _FearRange;
    [Header("希望の移動可能範囲")]
    [SerializeField] private Vector2 _HopeRange;
    [Header("保存しておく過去EPの最大数")]
    [SerializeField] public const int _MaxPreEPDataNum = 50;

    [Header("プレイヤーの行動状態")]
    TaskState _PlayerTask;

    [Header("AIが次にして欲しい行動状態")]
    TaskState _AITask;

    [Header("子供を見つけた数から希望の値を求めるためのカーブ")]
    public AnimationCurve _KidsFoundHopeNumCurve = new AnimationCurve();

    private List<HeartRateValue> _rateValue = new List<HeartRateValue>();

    private LimitedQueueList<Vector2> _PreEP = new LimitedQueueList<Vector2>(_MaxPreEPDataNum);

    //EmotionalPoint(感情の位置)
    private Vector2 _CurrentEP;//現在の感情の位置
    private Vector2 _NextEP;//次の感情の位置
    private Vector2 _TargetEP;//目標の感情の位置

    //習熟度の値
    private const float _MaxProfi = 1.0f;   //習熟度の最大
    private const float _MinProfi = -1.0f;  //習熟度の最小

    public float graphSize = 1f; // グラフの最大値
    public GraphPoint[] points = new GraphPoint[3]; // データポイント

    //子供の人数を取得するため
    private GameManager _manager;

    //心拍の取得用
    private HeartRate _heartRate;

    //CSV読み込み用
    private CSVReader _csvReader;

    //行動状態
    enum TaskState
    {
        None,           //特に何もない状態
        Search_kids,    //子供の位置を見つける
        Catch_Kids,     //子供を捕まえる
        HideFrom_Enemy, //敵から隠れる
        RunFrom_Enemy,  //敵から逃げる
        FeelSafe,       //安全を感じてほしい
        FeelDanger,     //危険を感じてほしい
    }

    public class HeartRateValue
    {
        public string _valueName;
        public float _HopeValue;
        public float _FearValue;
    }



    // Start is called before the first frame update
    async void Start()
    {
        //値の初期化
        _CurrentFear = _InitFear;
        _CurrentHope = _InitHope;

        _CurrentEP = new Vector2(_CurrentHope, _CurrentFear);
        _NextEP = new Vector2(_CurrentHope, _CurrentFear);
        _TargetEP = new Vector2(_CurrentHope, _CurrentFear);

        _HopeRange = new Vector2(_MinHope, _MaxHope);
        _FearRange = new Vector2(_MinFear, _MaxFear);

        //過去感情リスト初期化
        _PreEP = new LimitedQueueList<Vector2>(_MaxPreEPDataNum);

        //ファイルパスを取得
        filePath = Path.Combine(Application.persistentDataPath, "MetaAIData.csv");

        _dataFilePath = Path.Combine(Application.streamingAssetsPath, "MetaAI/MetaAIValue.csv");

        _manager =GameObject.Find("GameManager").GetComponent<GameManager>();
        _heartRate = GameObject.Find("Player").GetComponent<HeartRate>();
        _csvReader = GetComponent<CSVReader>();

        _rateValue = await _csvReader.ReadCSV(_dataFilePath);

        //グラフ情報の初期化
        InitGraph();

    }

    // Update is called once per frame
    void Update()
    {
        //Ai関連のアップデート
        AIUpdate();
        //グラフ用データの更新
        UpdateGraph();
    }

    /// <summary>
    /// AI関連処理のアップデート
    /// </summary>
    private void AIUpdate()
    {
        //CurrentEPの推測
        GuessCurrentEP();
        //GoalEPのプランニング
        PlanGoalEP();
        //NextEPの設定
        SetNextEP();
        //オーダーの生成
        GenerateOrder();

    }

    /// <summary>
    /// 現在の感情の位置を推測する
    /// </summary>
    private void GuessCurrentEP()
    {
        //現在置かれている状況から感情の位置を推測する
        //まずは初期化
        _CurrentFear = 0.0f;
        _CurrentHope = 0.0f;

        //見つけるべき子供の位置を把握しているか(恐れ)


        //隠れているか(恐れ)


        //敵に追われているか(恐れ+,希望の値にも影響)

        //心拍数の状態(恐れ)
        if(_csvReader._isLoadDone)
        {
            switch (_heartRate._heartRateState)
            {
                case HeartRate.HeartRateState.Zone1:
                    {
                        Debug.Log(_rateValue[0]._valueName);
                        _CurrentHope += _rateValue[0]._HopeValue;
                        _CurrentFear += _rateValue[0]._FearValue;
                        break;
                    }
                case HeartRate.HeartRateState.Zone2:
                    {
                        Debug.Log(_rateValue[1]._valueName);
                        _CurrentHope += _rateValue[1]._HopeValue;
                        _CurrentFear += _rateValue[1]._FearValue;
                        break;
                    }
                case HeartRate.HeartRateState.Zone3:
                    {
                        Debug.Log(_rateValue[2]._valueName);
                        _CurrentHope += _rateValue[2]._HopeValue;
                        _CurrentFear += _rateValue[2]._FearValue;
                        break;
                    }
                case HeartRate.HeartRateState.Zone4:
                    {
                        Debug.Log(_rateValue[3]._valueName);
                        _CurrentHope += _rateValue[3]._HopeValue;
                        _CurrentFear += _rateValue[3]._FearValue;
                        break;
                    }
                case HeartRate.HeartRateState.Zone5:
                    {
                        Debug.Log(_rateValue[4]._valueName);
                        _CurrentHope += _rateValue[4]._HopeValue;
                        _CurrentFear += _rateValue[4]._FearValue;
                        break;
                    }
            }
        }
        

        //あと何人見つければクリアか(希望+)
        _CurrentHope += _KidsFoundHopeNumCurve.Evaluate(Mathf.Lerp(0,10.0f,((float)_manager.isFindpeopleNum/ (float)_manager.PeopleNum)/10));

        //値のクランプ
        _CurrentHope = Mathf.Clamp(_CurrentHope,_HopeRange.x, _HopeRange.y);
        _CurrentFear = Mathf.Clamp(_CurrentFear,_FearRange.x, _FearRange.y);

        //前の値を格納
        _PreEP.Add(_CurrentEP);

        _CurrentEP = new Vector2(_CurrentFear, _CurrentHope);
    }

    /// <summary>
    /// 目標の感情の位置を設定する
    /// </summary>
    private void PlanGoalEP()
    {
        //現在のプレイヤーの状況から向かって欲しい感情を決める
        switch(_PlayerTask)
        {

        }

        //興奮状態
        _TargetEP = new Vector2(1, 1);

    }

    /// <summary>
    /// 次の感情の位置を設定する
    /// </summary>
    private void SetNextEP()
    {
        switch(_AITask)
        {
            case TaskState.None:
            {
                    return;
            }
            case TaskState.Search_kids:
            {
                    return;
            }
        }

        _NextEP = new Vector2(0, 0);
    }

    /// <summary>
    /// 感情からオーダーの生成
    /// </summary>
    private void GenerateOrder()
    {
        //AIのやりたいことと感情の差からやることと変更の強度を決定する


    }

    private void OnApplicationQuit()
    {
        //データの保存
        SaveData();
    }

    private void SaveData()
    {
        //ファイルが存在しない場合は作成
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }

        //ファイルへの書き込み
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine(_CurrentFear);
            writer.WriteLine(_CurrentHope);
        }
    }

    /// <summary>
    /// グラフ情報の初期化
    /// </summary>
    private void InitGraph()
    {
        if (points == null || points.Length < 3)
        {
            points = new GraphPoint[3]; // 配列を初期化
        }

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null)
            {
                points[i] = new GraphPoint();
            }
        }
        //現在
        points[0].label = "CurrentEP";
        points[0].position = _CurrentEP;
        points[0].color = Color.blue;
        //次のポイント
        points[1].label = "NextEP";
        points[1].position = _NextEP;
        points[1].color = Color.green;
        //ゴールのポイント
        points[2].label = "TargetEP";
        points[2].position = _TargetEP;
        points[2].color = Color.yellow;
    }

    /// <summary>
    /// グラフ情報用のアップデート
    /// </summary>
    private void UpdateGraph()
    {
        if (points == null || points.Length < 3) return; //nullチェック

        points[0].position = _CurrentEP;
        Debug.Log("CurrentEP"+_CurrentEP);
        points[1].position = _NextEP;
        Debug.Log("NextEP" + _NextEP);
        points[2].position = _TargetEP;
        Debug.Log("TargetEP" + _TargetEP);
    }


    //ゲッターセッター

    /// <summary>
    /// 敗北への恐れの値を直接設定する
    /// </summary>
    /// <param name="fear">セットしたい恐れの値</param>
    public void SetFear(float fear)
    {

        _CurrentFear = Mathf.Clamp(fear, _FearRange.x, _FearRange.y);


    }
    /// <summary>
    /// 現在の敗北への恐怖の値を取得する
    /// </summary>
    /// <returns>現在の敗北への恐怖の値を取得する</returns>
    public float GetFear()
    {
        return _CurrentFear;
    }


    /// <summary>
    /// 現在の勝利への希望の値から加算する
    /// </summary>
    /// <param name="fear">加算したい恐怖の値</param>
    public void AddFear(float fear)
    {
        _CurrentFear = Mathf.Clamp(_CurrentFear + fear, _FearRange.x, _FearRange.y); ;

    }

    /// <summary>
    /// 敗北への恐れの値を直接設定する
    /// </summary>
    /// <param name="Hope">セットしたい希望の値</param>
    public void SetHope(float Hope)
    {
        _CurrentHope = Mathf.Clamp(Hope, _HopeRange.x, _HopeRange.y);
    }
    /// <summary>
    /// 現在の勝利への希望の値を取得する
    /// </summary>
    /// <returns>現在の勝利への希望の値を取得する</returns>
    public float GetHope()
    {
        return _CurrentHope;
    }


    /// <summary>
    /// 現在の勝利への希望の値から加算する
    /// </summary>
    /// <param name="Hope">加算したい希望の値</param>
    public void AddHope(float Hope)
    {
        _CurrentHope = Mathf.Clamp(Hope + _CurrentHope, _HopeRange.x, _HopeRange.y);
    }
}
