using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : MonoBehaviour
{

    public static GameManager _instance;

    PlayManager PM;
    public const int ICE = 0;
    public const int Demon = 1;

    public List<Card> iceCardAll;                         //所有的冰法卡
    public List<Card> demonCardAll;                       //所有的恶魔卡


   public IntArrays[] iceCardsSt;
   public IntArrays[] demonCardsSt;
  


    public static int mSelectedCardGroup;                 //玩家选择的卡组
    public static int uSelectedCardGroup;                 //对方所选择的的卡组


    public static string[] CardGroupStr = new string[2];  //恶魔卡组
    public static string MyCardStr;                       //所选卡组的字符串
    public GameObject canvas;

    private void Awake()
    {
        _instance = this;
     
    }


    // Use this for initialization
    void Start()
    {
        PM = PlayManager._instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        { 
        print("鼠标位置"+Input.mousePosition);
        print("鼠标屏幕位置" + Camera.main.ScreenToWorldPoint(Input.mousePosition));
        print("鼠标世界位置" + Camera.main.WorldToScreenPoint(Input.mousePosition));
        }
    }


    /// <summary>
    /// 初始化卡组
    /// </summary>



    /// <summary>
    /// 控制时间
    /// </summary>
    public void TimeCtr()
    {
        if(PM.time>0)
        {
            PM.timeText.text = PM.time.ToString("0");
            PM.time -= Time.fixedDeltaTime;
            if (PM.time <= 0)
            {
                //出牌阶段
                if (PlayManager.state == PlayManager.State.STATE_PLAY)
                {
                    Debug.Log("时间到了,不发送卡牌");
                    PM.cantMoveTip.GetComponent<Image>().sprite = PM.cantMoveTipSprite[3];
                    PM.cantMoveTip.gameObject.SetActive(true);
                    /*  //发送空牌
                      char[] sendnull = new char[3];
                      sendnull[0] = (char)4;
                      sendnull[1] = (char)1;
                      sendnull[2] = (char)50;
                      try
                      {
                          NetManager.SendToServer(new string(sendnull));
                      }
                      catch(System.Exception e)
                      {
                          Debug.LogError(e.Data);
                      }*/
                    ButtonManager._instance.DefenceButton();
                }
                else if (PlayManager.state == PlayManager.State.STATE_COMBO)
                {
                    Debug.Log("时间到了,发送空牌，不进行连击");
                    char[] send = new char[2];
                    send[0] = (char)2;
                    send[1] = (char)50;
                    NetManager.SendToServer(new string(send));
                    PM.comboPanel.gameObject.SetActive(false);
                }
                //破灭
                else if (PlayManager.state == PlayManager.State.STATE_BURST)
                {
                    int index = Random.Range(0, PM.mhandNum);
                    PM.handCardPanel.GetChild(index).GetComponent<Card>().ConfirmCard();
                    PM.burstPanel.gameObject.SetActive(false);
                    AudioManager.SoundEffectPlay("se_countdown");
                }
                PlayManager.state = PlayManager.State.STATE_CANTMOVE;
            }
            
            PM.timeProImage.GetComponent<Image>().fillAmount = PM.time / 30.0f;
            PM.timeText.transform.parent.GetChild(2).rotation = Quaternion.Euler(new Vector3(0, 0, 360 * PM.time / 30));
        }
       
        if (Mathf.Abs(PM.time - 5) <= 0.02f)
        {
            PM.timeText.transform.parent.DOShakePosition(5.0f, new Vector3(5.0f, 5.0f, 0));
            AudioManager.SoundEffectPlay("se_countdown");
        }
       
    }

    //
    public void GameOver()
    {

    }


    [System.Serializable]
    public class IntArrays
    {
        public int[] Array;
        public int this[int index]
        {
            get
            {
                return Array[index];
            }
        }
        public IntArrays()
        {
            this.Array = new int[1];
        }
        public IntArrays(int index)
        {
            this.Array = new int[index];
        }
    }





}
