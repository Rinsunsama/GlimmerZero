using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class HpFloat : MonoBehaviour {

    Vector3 startPos;

    public Sprite[] addNum;              //正数
    public Sprite[] subtractNum;         //负数
    public Sprite addSign;
    public Sprite subtractSign;

    public Image num1Image;
    public Image num2Image;
    public Image signImage;
	// Use this for initialization
	void Start () {
        startPos = transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    /// <summary>
    /// 飘动前更改数字
    /// </summary>
    public void SetNumber(bool isAdd,int num)
    {
        int num1 = num % 10;
        int num2 = num / 10;
        signImage.gameObject.SetActive(true);
        //加血
        if (isAdd ==true)
        {
            signImage.sprite = addSign;
            if (num2 ==0)
            {
                num2Image.sprite = addNum[num1];
                num2Image.gameObject.SetActive(true);
                num1Image.gameObject.SetActive(false);
            }
            else
            {         
                num1Image.sprite = addNum[num1];
                num2Image.sprite = addNum[num2];
                num2Image.gameObject.SetActive(true);
                num1Image.gameObject.SetActive(true);
            }
        }
        //扣血
        else if(isAdd ==false)
        {
            signImage.sprite = subtractSign;
            signImage.gameObject.SetActive(true);
            if (num2 == 0)
            {
                num2Image.sprite = subtractNum[num1];
                num2Image.gameObject.SetActive(true);
                num1Image.gameObject.SetActive(false);
            }
            else
            {
                num1Image.sprite = subtractNum[num1];
                num2Image.sprite = subtractNum[num2];
                num2Image.gameObject.SetActive(true);
                num1Image.gameObject.SetActive(true);
            }
        }

    }


    public void DoFloat()
    {
        Invoke("InvokeFloat", 0.5f);
    }

    public void ReScale()
    {
        transform.localPosition =startPos;
        transform.localScale = Vector3.zero;
    }


    public void ToHpStatus()
    {
        if(transform.parent.name=="You")
        {
            Tweener t = transform.DOMove(PlayManager._instance.uPlayPos.position, 1.0f);
            t.OnComplete(ReScale);
        }
        else if(transform.parent.name =="Me")
        {
            Tweener t = transform.DOMove(PlayManager._instance.mExilePos.position, 1.0f);
            t.OnComplete(ReScale);
        }
        
    }



    public void InvokeFloat()
    {
        transform.localScale = Vector3.one;
        Tweener t = transform.DOLocalMoveY(startPos.y + 50, 0.5f);
        t.OnComplete(ToHpStatus);
    }
}
