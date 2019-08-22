using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using DG.Tweening;


public class ButtonManager : MonoBehaviour
{

    public static ButtonManager _instance;

    public Transform startPanel;                          //开始界面
    public Transform plotSelectPanel;                     //剧情界面
    public Transform cardCheckPanel;                      //选择卡组界面
    public Transform matchPanel;                          //匹配界面
    public Transform selectPanel;                         //选人界面
    public Transform createrPanel;                        //制作人员名单                                                       //Use this for initialization
    public Material flowLight;
    public Transform settingPanelHome;                    //大厅的设置界面


    public Text notOpenText;                              //未开放文本提示

    private void Awake()
    {
        _instance = this;
    }

    /// <summary>
    /// 结束匹配按钮
    /// </summary>
    public void CancelMatchButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        matchPanel.gameObject.SetActive(false);
        Camera.main.GetComponent<MatchCtr>().CancelInvoke("MatchTimeCtr");
        MatchCtr._instance.matchTime = 0;
        NetManager.socketClient.Close();
        Camera.main.GetComponent<MatchCtr>().matchFalsePanel.gameObject.SetActive(false);
    }


    /// <summary>
    /// 剧情按钮
    /// </summary>
    public void PlotButton()
    {
        startPanel.gameObject.SetActive(false);
        plotSelectPanel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 进入剧情
    /// </summary>
    public void EnterPlotButton()
    {
        int mapIndex = GameObject.Find("Book").GetComponent<Book>().currentPage / 2;  //获得关卡序
        //   PlotMapCtr.nowPlotMapIndex = mapIndex;
        Loading.loadSceneName = "Play";
        SceneManager.LoadScene("Loading");
    }

    /// <summary>
    /// 调整卡组
    /// </summary>
    public void CardCheckButton()
    {
        cardCheckPanel.gameObject.SetActive(true);
        startPanel.gameObject.SetActive(false);
        
    }

    /// <summary>
    /// 调整卡组-选择界面
    /// </summary>
    public void CardCheckToStartButton()
    {
        cardCheckPanel.gameObject.SetActive(false);
        startPanel.gameObject.SetActive(true);
    }



    /// <summary>
    /// 进入PVP界面
    /// </summary>
    public void PVPButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        try
        {
            NetManager._instance.Connect();
            Debug.Log("连接服务器");
            char[] s = new char[3];
            s[0] = (char)NetManager.RQ_MATCH;
            s[1] = (char)0;
            s[2] = (char)GameManager.mSelectedCardGroup;

            string sendMassage = new string(s) + GameManager.MyCardStr;
    
            NetManager.SendToServer(sendMassage);
            NetManager._instance.beatCheckTime = 0;

            MatchCtr._instance.matchingImage.gameObject.SetActive(true);
            MatchCtr._instance.cancelButton.gameObject.SetActive(true);
            MatchCtr._instance.matchTimeText.text = "30s";
            MatchCtr._instance.matchTime = 30;  
         //   Camera.main.GetComponent<MatchCtr>().InvokeRepeating("MatchTimeCtr", 1.0f, 1.0f);
     
            MatchCtr._instance.mCharacter.sprite = MatchCtr._instance.matchCharacterSprite[GameManager.mSelectedCardGroup];
            NetManager._instance.StartBeatCheck();
            matchPanel.gameObject.SetActive(true);
            MatchCtr._instance.isTimeStart = true;
            // byte[] buffter = Encoding.UTF8.GetBytes(sendMassage);
            // int temp = NetManager.socketClient.Send(buffter);
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.Data);
            Debug.Log("连接服务器失败，请重新尝试！");
        }

        /*     if(NetManager.socketClient.Connected)
             {
             }

             */
        //连接接服务器
        /*   Thread thread = new Thread(NetManager.Recive);                   //接受服务器的消息
           thread.Start(NetManager.socketClient);            
            */
        //发送匹配请求
    }

    /// <summary>
    /// 确认按钮
    /// </summary>
    public void ConfirmCardButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        if (PlayManager._instance.selectedCard != null && PlayManager.state != PlayManager.State.STATE_CANTMOVE)
        {
            PlayManager._instance.selectedCard.ConfirmCard();                      //确定出牌
            PlayManager._instance.showCard.gameObject.SetActive(false);
            PlayManager._instance.ArrayHand();
        }
    }

    public void SettingButton()
    {

    }



    public void CancelSelectCardButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        if (PlayManager._instance.selectedCard != null)
        {
            Card c = PlayManager._instance.selectedCard;
            c.isSelect = false;
            c.GetComponent<Outline>().enabled = false;
            c.Array();
            PlayManager._instance.selectedCard = null;
        }
        PlayManager._instance.showCard.gameObject.SetActive(false);
    }

    /// <summary>
    /// 格挡按钮
    /// </summary>
    public void DefenceButton()
    {
        
        PlayManager PM = PlayManager._instance;
      
        if (PlayManager.state == PlayManager.State.STATE_PLAY&&PM.bitCheck(PM.mStatus,PlayManager.STATUS_BLOCKADE)==0)
        {
            AudioManager.SoundEffectPlay("se_key");
            Debug.Log("可以按");
            GameObject defenceCard = GameObject.Instantiate(PM.cards[52], PM.canvas) as GameObject;
            defenceCard.transform.position = PM.mDrawPos.position;
            defenceCard.transform.localScale = Vector3.one;
            defenceCard.GetComponent<Card>().ConfirmCard();
            if (PM.selectedCard != null)
            {
                PM.selectedCard.Array();
                PM.selectedCard = null;
            }

        }
        else
        {
            AudioManager.SoundEffectPlay("se_keymiss");
        }
        //  Destroy(uCard, 0.6f);
    }

    /// <summary>
    /// 蓄能按钮
    /// </summary>
    public void DrawCardButton()
    {
        PlayManager PM = PlayManager._instance;
        if (PlayManager.state == PlayManager.State.STATE_PLAY && PM.bitCheck(PM.mStatus, PlayManager.STATUS_BLOCKADE) == 0)
        {
            AudioManager.SoundEffectPlay("se_key");
            GameObject defenceCard = GameObject.Instantiate(PM.cards[51], PM.canvas) as GameObject;
            defenceCard.transform.position = PM.mDrawPos.position;
            defenceCard.transform.localScale = Vector3.one;
            defenceCard.GetComponent<Card>().ConfirmCard();
            if (PM.selectedCard != null)
            {
                PM.selectedCard.Array();
                PM.selectedCard = null;
            }

        }
        else
        {
            AudioManager.SoundEffectPlay("se_keymiss"); 
        }
    }

    /// <summary>
    /// 匹配按钮
    /// </summary>
    public void MatchButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        selectPanel.gameObject.SetActive(true);
        startPanel.gameObject.SetActive(false);
        MatchCtr._instance.matchingImage.gameObject.SetActive(true);
        MatchCtr._instance.cancelButton.gameObject.SetActive(true);
        // PlayerPrefs.DeleteKey("isFirstGame");
        if (PlayerPrefs.GetInt("isFirstGame") !=1)
        {
            Camera.main.GetComponent<Guide>().guidePanel.gameObject.SetActive(true);
            PlayerPrefs.SetInt("isFirstGame", 1);
        }


        AudioManager.BGMInstead("bgm_matching");
    }
    
    /// <summary>
    /// 角色选择按钮
    /// </summary>
    /// <param name="character"></param>
    public void CharacterButton(int character)
    {
        AudioManager.SoundEffectPlay("se_key");
        MatchCtr MC = MatchCtr._instance;
        MC.characterButton[GameManager.mSelectedCardGroup].GetComponent<Image>().color = MC.disableColor;

        GameManager.mSelectedCardGroup = character;
        MC.characterButton[character].GetComponent<Image>().color = Color.white;



        GameManager.MyCardStr = GameManager.CardGroupStr[character];
        if(character == 0)
        {
            GameManager.mSelectedCardGroup =GameManager.ICE;
         }
        else if(character==1)
        {
            GameManager.mSelectedCardGroup = GameManager.Demon;
        }
 
        //PlayerPrefs.SetString("MyCards", PlayerPrefs.GetString(GameInfo.characterName[character]+"Cards"));      //设置卡组各卡编号为当前卡组
    }

    /// <summary>
    /// 我方放逐区
    /// </summary>
    public void M_ExileCheckButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.mExilePanel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 我方墓地
    /// </summary>
    public void M_CemeCheckButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.mCemePanel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 对方放逐区
    /// </summary>
    public void U_ExileCheckButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.uExilePanel.gameObject.SetActive(true);
    }


    /// <summary>
    /// 对方墓地
    /// </summary>
    public void U_CemeCheckButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.uCemePanel.gameObject.SetActive(true);
    }

    public void M_CancelExileButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.mExilePanel.gameObject.SetActive(false);
    }

    public void M_CancelCemeButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.mCemePanel.gameObject.SetActive(false);
    }

    public void U_CancelCemeButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.uCemePanel.gameObject.SetActive(false);
    }

    public void U_CancelExileButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.uExilePanel.gameObject.SetActive(false);
    }


    /// <summary>
    /// 取消连击
    /// </summary>
    public void CancelCombo()
    {
        AudioManager.SoundEffectPlay("se_key");
        char[] c = new char[3];
        c[0] = (char)4;
        c[1] = (char)2;
        c[2] = (char)50;
        PlayManager.state = PlayManager.State.STATE_CANTMOVE;
        PlayManager._instance.comboPanel.gameObject.SetActive(false);
        NetManager.SendToServer(new string(c));
    }

    public void ReturnToMainButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        plotSelectPanel.gameObject.SetActive(false);                     //剧情界面
        cardCheckPanel.gameObject.SetActive(false);                        //选择卡组界面
        matchPanel.gameObject.SetActive(false);                            //匹配界面
        selectPanel.gameObject.SetActive(false);                         //选人界面
        startPanel.gameObject.SetActive(true);
    }


    /// <summary>
    /// 战斗界面的帮助按钮
    /// </summary>
    public void HelpButtonOnPlay()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.helpPanel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 战斗界面取消按钮
    /// </summary>
    public void CancelHelpButtonOnPlay()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.helpPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// 投降按钮
    /// </summary>
    public void SurrendertButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        try
        {
            string send = (char)NetManager.RQ_SURRENDERT + "";
            NetManager.SendToServer(send);
            PlayManager._instance.settingPanel.gameObject.SetActive(false);
        }
        catch
        {
            Debug.Log("发送消息失败");
        }
    }

    public void settingButtonPlay()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.settingPanel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 选项按钮
    /// </summary>
    public void ItemButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.settingPanel.gameObject.SetActive(false);
        PlayManager._instance.itemPanel.gameObject.SetActive(true);
    }


    /// <summary>
    /// 战斗场景中取消设置面板
    /// </summary>
    public void CancelSettingButtonPlay()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.settingPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// 战斗场景中取消选项面板
    /// </summary>
    public void CancelItemButtonPlay()
    {
        AudioManager.SoundEffectPlay("se_key");
        PlayManager._instance.itemPanel.gameObject.SetActive(false);
    }


    /// <summary>
    /// 结束界面返回大厅
    /// </summary>
    public void ReturnToMainButtonPlay()
    {
        AudioManager.SoundEffectPlay("se_key");
        MatchCtr.isMatchToThisScence = false;
        SceneManager.LoadScene("Start");
    }

    /// <summary>
    /// 再来一局面板
    /// </summary>
    public void PlayAgainButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        MatchCtr.isMatchToThisScence = true;
        SceneManager.LoadScene("Start");
    }

    public void LoginButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        string serverIP = transform.Find("IPAddress").GetComponent<InputField>().text;
        if (serverIP != "")
        {
            NetManager.IP2 = serverIP;
            SceneManager.LoadScene("Start");
        }
    }

    public void ConnectFalseConfirmButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        SceneManager.LoadScene("Start");
    }

    
    //查看制作人员名单
    public void CreaterButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        createrPanel.gameObject.SetActive(true);
    }

    //取消查看制作人员名单
    public void CancelCreaterPanelButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        createrPanel.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 大厅的设置按钮
    /// </summary>
    public void SettingInHomeButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        settingPanelHome.gameObject.SetActive(true);
    }

    public void CancelSettingHomeButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        settingPanelHome.gameObject.SetActive(false);
    }

    //-------------------------------------------test--------------------------------------------------
    public void PlayEffectButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        EffectManager._instance.EF_DoHurt(0, 100, 5);
        EffectManager._instance.EF_CardBegin(0, 100);
        EffectManager._instance.EF_DoHurt(0, 113, 5);
    }

    public void REButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        AudioManager.SoundEffectPlay("se_key");
        SceneManager.LoadScene("Play");
    }

    public void DRButton()
    {
        AudioManager.SoundEffectPlay("se_key");
        EffectManager._instance.UDrawCard();
    }

    public void PLButton()
    {

        EffectManager._instance.UPlayCard(4);
    }

    public void BurstButton()
    {
        EffectManager._instance.UDoBurst(2);

    }

    public void ComboButton()
    {
        EffectManager._instance.UDoCombo(3);

    }

    public void Clear()
    {
        PlayManager._instance.ClearPlayArea();
        StateTurn(1);
    }

    public void StateTurn(int state)
    {
        PlayManager.state = (PlayManager.State)state;
        if (state == (int)PlayManager.State.STATE_COMBO)
        {
            PlayManager._instance.comboPanel.gameObject.SetActive(true);
        }
        PlayManager._instance.time = 10;
    }

    public void OpenCardButton()
    {
        char[] c = new char[4];
        c[0] = 'a';
        c[1] = (char)3;
        c[2] = (char)0;
        c[3] = (char)1;
        EffectManager._instance.OpenCard(c);
    }

    public void AddFrost()
    {
        EffectManager._instance.EF_AddFrost(1, 103, 3);
    }

    public void DoIce()
    {
        EffectManager._instance.EF_DoIce(0, 103);
    }

    public void DoDizzy()
    {
        EffectManager._instance.EF_DoDizzy(0, 102);
    }


    public void ReturnButton()
    {
        NetManager.socketClient.Close();
        SceneManager.LoadScene("Start");
    }
}
