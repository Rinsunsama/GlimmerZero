using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGroup : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
        InitCardGroup();
        if (GameManager.mSelectedCardGroup == GameManager.ICE)
        {
            PlayerPrefs.SetString("MyCards", PlayerPrefs.GetString("IceCards"));
        }
        else if(GameManager.mSelectedCardGroup == GameManager.Demon)
        {
            PlayerPrefs.SetString("MyCards", PlayerPrefs.GetString("DemonCards"));
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// 初始化卡组
    /// </summary>
    public void InitCardGroup()
    {
        int cardGroupIndex = Random.Range(0, 3);
        if (PlayerPrefs.GetString("IceCards") =="")
        {
            Debug.Log("Enter IceCards Set");
            char[] iceCard = new char[14];
            for (int i = 0; i < 14; i++)
            {
                iceCard[i] = (char)GameManager._instance.iceCardsSt[cardGroupIndex][i];
            }
            //     string iceCardStr = iceCard.ToString();
            //     Debug.Log((int)iceCardStr.ToCharArray()[2]);
            //     PlayerPrefs.SetString("IceCards",new string(iceCard));
            GameManager.CardGroupStr[0] = new string(iceCard);

        //    Debug.Log(PlayerPrefs.GetString("IceCards").ToCharArray().Length);

        }

        if (PlayerPrefs.GetString("DemonCards") == "")
        {
            Debug.Log("Enter DemonCard Set");
            char[] demonCard = new char[14];
            for (int i = 0; i < 14; i++)
            {
                demonCard[i] = (char)GameManager._instance.demonCardsSt[cardGroupIndex][i];
            }
            // PlayerPrefs.SetString("IceCards", new string(demonCard));
            GameManager.CardGroupStr[1] = new string(demonCard);
        }

        GameManager.MyCardStr = GameManager.CardGroupStr[GameManager.ICE];
    }



}
