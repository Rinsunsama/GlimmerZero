using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NotOpenButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    /// <summary>
    /// 点击按钮
    /// </summary>
    public void OnClick()
    {

        DOTween.Kill();
        ButtonManager._instance.notOpenText.color = new Color(1, 1, 1, 1);
        DOTween.To(() => ButtonManager._instance.notOpenText.color, x => ButtonManager._instance.notOpenText.color = x, new Color(1, 1, 1, 0), 2);

        if (transform.parent.name == "Character")
        {
            ButtonManager._instance.notOpenText.transform.position = transform.position + new Vector3(160f, 0, 0);
        }
        else if(transform.parent.name == "ButtonBar")
        {
            ButtonManager._instance.notOpenText.transform.position = transform.position + new Vector3(0, 100.0f, 0);
        }
        else if(transform.parent.name== "RightBar")
        {
            ButtonManager._instance.notOpenText.transform.position = transform.position + new Vector3(-100f, 65.0f, 0);
        }
    }

    public void OnClickLogin()
    {
        DOTween.Kill();
        ButtonManager._instance.notOpenText.color = new Color(1, 1, 1, 1);
        DOTween.To(() => ButtonManager._instance.notOpenText.color, x => ButtonManager._instance.notOpenText.color = x, new Color(1, 1, 1, 0), 2);
        ButtonManager._instance.notOpenText.transform.position = transform.position + new Vector3(-1.2f, 0, 0);
    }
}
