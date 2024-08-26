using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GakiMitsukeAndOpen : MonoBehaviour
{
    [Header("道をふさいでいるオブジェクト")]
    [SerializeField]
    private GameObject BigCobo; // 道をふさいでいるオブジェクト

    [Header("道が開けた後のオブジェクト")]
    [SerializeField]
    private GameObject SmallCobo; // 道が開けた後のオブジェクト

    [Header("前の敵を殺す")]
    [SerializeField]
    private GameObject BeforeEnemy; // 
    private EnemyAI_move enemy;

    [Header("開いた時の音")]
    [SerializeField]
    private AudioClip Gomadare; // 開いた時の音

    [Header("オーディオソース")]
    [SerializeField]
    private AudioSource audioSource; // オーディオソース

    [Header("開けるのに必要なガキ")]
    [SerializeField]
    private HidingCharacter[] Gakis; // 必要なガキの配列

    // ガキが見つかったことを格納するbool配列
    private bool[] IsGakiFind;
    bool alltrue = true; // すべてのガキが見つかったかどうかのフラグ
    bool DoFindAll = false; // ガキを全て見つけた後の処理を行ったかどうかのフラグ

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        IsGakiFind = new bool[Gakis.Length]; // IsGakiFind配列をGakisの長さで初期化
        enemy = BeforeEnemy.GetComponent<EnemyAI_move>();
        for (int i = 0; i < Gakis.Length; i++)
        {
            Gakis[i].GetComponent<HidingCharacter>(); // ガキのコンポーネントを取得
            IsGakiFind[i] = false; // 初期状態ではすべてのガキが見つかっていない
        }
    }

    // Update is called once per frame
    void Update()
    {
        alltrue = true; // フラグのリセット

        for (int j = 0; j < Gakis.Length; j++)
        {
            if (Gakis[j] == null)
            {
                IsGakiFind[j] = true; // ガキがnullの場合は見つかったと見なす
            }

            if (IsGakiFind[j] == false)
            {
                alltrue = false; // 一つでも見つかっていないガキがあればフラグをfalseにする
            }
        }

        if (alltrue && !DoFindAll)
        {
            FindAll(); // すべてのガキが見つかった場合の処理
        }

         if (DoFindAll && !audioSource.isPlaying) // すべてのガキが見つかりかつ音が鳴り終わっていれば
        {
            BigCobo.SetActive(false);
            SmallCobo.SetActive(true);
            enemy.SetState(EnemyAI_move.EnemyState.Idle);
            BeforeEnemy.SetActive(false);
            //Destroy(this); // スクリプトを破棄
        }
    }

    // すべてのガキが見つかった場合の処理
    private void FindAll()
    {
        // 道を開ける

        // 音を鳴らす
        if(!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(Gomadare);
            Debug.Log("開いとる！");
        }

        DoFindAll = true; // フラグを更新
    }
}
