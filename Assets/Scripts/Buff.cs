using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Buff : MonoBehaviour {


    public string tipContent;                //提示内容                                            
    Transform buffTip;                //提示框

    // Use this for initialization
    void Start() {
        buffTip = PlayManager._instance.buffTip;
    }

    // Update is called once per frame
    void Update()
    {

    }



    /// <summary>
    /// 长按触发
    /// </summary>
    public void OnPointDown()
    {
        buffTip.GetComponentInChildren<Text>().text = tipContent;
        buffTip.gameObject.SetActive(true);

        if (transform.parent.name == "M_BuffPanel")
        {
            buffTip.transform.position = transform.position + new Vector3(2.0f, 1.25f, 0);
        }
        else if (transform.parent.name == "U_BuffPanel")
        {
            buffTip.transform.position = transform.position + new Vector3(-2.0f, 1.25f, 0);
        }
    }


    /// <summary>
    /// 退出长按
    /// </summary>
    public void OnPointUp()
    {
        buffTip.gameObject.SetActive(false);
    }
}
