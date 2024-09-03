using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//OnBecameInvisible/OnBecameVisibleはmeshrendererがないと機能しないので必ずenemybodyにアタッチすること

public class EnemyAI_checkRendering : MonoBehaviour
{
    //private EnemyAI_move eMove = default;
    //void Start()
    //{
    //    eMove = GetComponentInParent<EnemyAI_move>();
    //    eMove.SetisRendered(true);
    //}
    ////レンダリングされ始めた
    //private void OnBecameVisible()
    //{
    //    eMove.SetisRendered(true);
    //}
    ////レンダリングされなくなった
    //private void OnBecameInvisible()
    //{

    //    eMove.SetisRendered(false);
    //}
}
