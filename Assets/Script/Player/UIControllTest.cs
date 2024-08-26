using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Unity.UI;

public class UIControllTest : MonoBehaviour
{

    [SerializeField]
    private RectTransform MoveUI;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.W))
        {
            MoveUI.localPosition +=new Vector3( 0f,1.0f,0f);
            Debug.Log("WMoved");
        }
        else if(Input.GetKey(KeyCode.S))
        {
            MoveUI.localPosition -= new Vector3(0f, 1.0f, 0f);
            Debug.Log("SMoved");
        }
        else if( Input.GetKey(KeyCode.D))
        {
            MoveUI.localPosition += new Vector3(1.0f, 0f, 0f);
            Debug.Log("DMoved");
        }
        else if (Input.GetKey(KeyCode.A))
        {
            MoveUI.localPosition -= new Vector3(1.0f, 0f, 0f);
            Debug.Log("AMoved");
        }

    }
}
