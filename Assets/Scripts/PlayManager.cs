using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;



/// <summary>
/// 管理对战逻辑
/// </summary>
public class PlayManager : MonoBehaviour {

    public static PlayManager _instance;
    GameManager GM;
    NetManager NM;
    public Color disableColor;


    //BUFF状态
    public const int STATUS_ICE     = 0;   //冰冻
    public const int STATUS_DIZZY      = 1;   //眩晕
    public const int STATUS_BLOCKADE = 2;    //封锁
    public const int STATUS_INJURY     = 3;   //减伤
    public const int STATUS_INVINCIBLE = 4;   //无敌


    delegate int DrawACard(int cardNum); 
    //操作阶段
    public enum State
    {
        STATE_CANTMOVE,    //无法行动
        STATE_PLAY,        //选择出牌
        STATE_COMBO,       //选择连击
        STATE_BURST,       //选择弃牌 破灭
        STATE_EXILE,       //选择放逐        
        STATE_ROUNDOVER    //回合结束
    }

    public static State state = State.STATE_CANTMOVE;
    public static int nowComboLevel;                           //当前连击等级
    public static string drawCardStr;                       //初始手牌


    //创建卡牌字典，根据卡牌编号
    //TODO
    public Dictionary<int, GameObject> cards = new Dictionary<int, GameObject>();

    //创建特效字典，根据特效编号
    //TODO
    public Dictionary<string, GameObject> effects = new Dictionary<string, GameObject>();


    public int cardWidth = 80;  


    public List<Card> cardPool =new List<Card>();      //本次所携带的卡                

    public int maxHandNum;                             //最大手牌数
    public Card selectedCard;                          //当前选取的卡
    public Transform handCardPanel;                    //手牌面板
    public Transform drawCardPoolPanel;                //抽卡池面板
    public Transform comboPanel;                       //连击面板
    public Transform burstPanel;                       //破灭面板
    public Image showCard;                             //展示卡牌详情
    public Transform timeProImage;                     //时间进度条
    public Transform waitingUMoveText;                 //提示等待对方行动
    public Transform buffTip;                          //buff提示信息
    public Transform cantMoveTip;                      //不可行动提示信息

    public float minReadyPosY;                         //最小判定Y轴
    public float time;                                 //计时器
    public Text timeText;                              //显示时间
    public Transform canvas;                           //画布
    public Transform cardTigPanel;                     //卡牌信息提示面板
    public Transform helpPanel;                        //帮助面板


    public Image mCharacterImage;                      //我方角色
    public Sprite[] CharacterSprite;                     //角色图片
    public Sprite[] cantMoveTipSprite;                 //无法行动图片
    public int  mStatus;                               //我方状态
    public int mHP;                                    //我方血量
    public int mFrostNum;                              //我方霜冻值
    public Card mCard;                                 //我方选取的卡牌
    public int mhandNum;                               //我方手牌数
    public Text mhandNumText;                          //我方手牌数
    public int mCemeNum;                               //我方弃牌数
    public Text mCemeNumText;                          //我方弃牌数文本
    public int mExileNum;                              //我方放逐数
    public Text mExileNumText;                         //我方放逐数文本
    public int mCardPoolNum;                           //我方抽卡池数
    public Text mCardPoolNumText;                      //我方抽卡池文本
    public Transform showCardPanel;                    //展示卡牌面板
    public Transform settingPanel;                     //设置面板
    public Transform itemPanel;                        //选项面板



    public Transform mHpTurn;                           //我方血量变化本文
    public Text mHpText;                               //我方血量
    public Transform mReadyPos;                        //我方判定区域位置
    public Transform mDrawPos;                         //我方抽卡的落点位置
    public Transform mExilePos;                        //放逐牌落点位置
    public Transform mExilePanel;                      //我方放逐面板
    public Transform mCemePanel;                       //我方墓地
    public List<Card> handCard;                        //我方手牌区
    public List<Card> drawCardPool;                    //我方抽卡池
    public List<Card> mCardCemetery;                   //我方卡墓地
    public List<Card> mExile;                          //我方放逐区
    public int mIceNum = 0;                            //我方霜冻值
    bool mStatus_fraz = false;                         //我方是否冰冻
    bool mStatus_dizz = false;                         //我方是否眩晕
    public Transform mPlayArea;                        //我方牌区

   //18813297332 联系死策划
   //18826139961 联系活策划

    public int uStatus;                                //对方状态
    public Transform uHpTurn;                          //对方血量变化文本
    public Text uHandNumText;                          //对方手牌数
    public Text uCardPoolNumText;                      //对方卡池数量文本
    public int uCardPoolNum;                           //对方卡池数量
    public Text uCemeNumText;                          //对方弃牌数文本
    public int uCemeNum;                               //对方弃牌数
    public int uExileNum;                              //对方放逐数
    public Text uExileNumText;                         //对方放逐数文本
    public int uhandNum;                               //对方手牌数
    public GameObject uCard;                           //对方选取的卡牌
    public int uHP;                                    //对方血量 
    public int uFrostNum;                              //对方霜冻值
    public Text uHpText;                               //对方血量文本
    public Transform uExilePanel;                      //对方放逐面板
    public Transform uCemePanel;                       //对方墓地
    public Transform uReadyPos;                        //对方判定区域位置
    public Transform uPlayPos;                         //对方出牌位置
    public Transform uDrawPos;                         //对方抽牌位置
    public Transform uCemePos;                         //对方墓地位置
    public List<Card> uExile;                          //对方放逐区
    public List<Card> uCardCemetery;                   //对方墓地
    public int uIceNum = 0;                            //对方霜冻值
    public bool uStatus_fraz = false;                  //对方是否冰冻
    public bool uStatus_dizz = false;                  //对方是否眩晕
    public Transform uPlayArea;                        //对方牌区

    public GameObject cardTipPrefab;                   //卡牌提示预设   

    public Transform ConnectLostPanel;                 //连接丢失时出现


    private void Awake()
    {
        _instance = this;
    }


    void Start () {

        NetManager.SendToServer((char)NetManager.RQ_STATE_SYN + "");
       // char[] c = new char[5] { (char)0, (char)13, (char)12, (char)11, (char)5 };
       // drawCardStr = new string(c);
      // Debug.Log((int)c[0]);
        GM = GameManager._instance;
        NM = NetManager._instance;
        Init();
    }

    private void FixedUpdate()
    {
        GM.TimeCtr();
    }


    void Update () {
        beatCheck();
    }

    /// <summary>
    /// 初始化卡组
    /// </summary>
    void Init()
    {
        //-------初始化字典-------------
        for (int i = 50; i < 56; i++)
        {
            cards.Add(i, Resources.Load("Prefabs/Card/Com/card_com_" + i.ToString()) as GameObject);
        }
        for (int i = 100; i < 118; i++)
        {
            cards.Add(i, Resources.Load("Prefabs/Card/Ice/card_ice_" + i.ToString()) as GameObject);
        }
        for (int i = 200; i < 218; i++)
        {
            cards.Add(i, Resources.Load("Prefabs/Card/Demon/card_demon_" + i.ToString()) as GameObject);    
        }
        // effects.Add()
        uCardPoolNum = 9;
        mCardPoolNum = 14;
        uhandNum = 5;
        state = State.STATE_PLAY;
        time = -1;
        mHP = 20;
        uHP = 20;
        uStatus = 0;
        mStatus = 0;
        /*
        if(GameManager.mSelectedCardGroup ==GameManager.ICE)
        {
            Debug.Log("Group is ICE");
            char[] iceCard = PlayerPrefs.GetString("IceCards").ToCharArray();
            int length = iceCard.Length;
            for(int i=0;i< length; i++)
            {
                GameObject card = GameObject.Instantiate(Resources.Load("Prefabs/Card/Ice/card_ice_" + ((int)(iceCard[i])+100).ToString()), drawCardPoolPanel)as GameObject;
                drawCardPool.Add(card.GetComponent<Card>());
            }
        }
        else if(GameManager.mSelectedCardGroup ==GameManager.Demon)
        {
            char[] demonCard = PlayerPrefs.GetString("DemonCards").ToCharArray();
            int length = demonCard.Length;
            for (int i = 0; i < length; i++)
            {
                GameObject card = GameObject.Instantiate(Resources.Load("Prefabs/Card/Demon/card_demon_" + ((int)(demonCard[i])+200).ToString()), drawCardPoolPanel) as GameObject;
                drawCardPool.Add(card.GetComponent<Card>());
            }
        }
        */
        StartCoroutine("DrawCard",5);                         //初始化手牌
        if(GameManager.uSelectedCardGroup ==GameManager.ICE)
        {
            //实例化对方角色
        }
        else if(GameManager.uSelectedCardGroup==GameManager.Demon)
        {
            //实例化对方角色为恶魔
        }

        mCharacterImage.sprite = CharacterSprite[GameManager.mSelectedCardGroup];


        //加载特效
        for (int cardNum = 50; cardNum < 218; cardNum++)
        {
            if (cardNum == 100 || cardNum == 101 || (cardNum >= 103 && cardNum <= 108) || cardNum==110 || cardNum == 113 || (cardNum >= 115 && cardNum <= 203) || cardNum == 207 || cardNum == 208 || cardNum == 209 || cardNum == 212 || cardNum == 213 || cardNum == 215 || cardNum == 216 || cardNum == 53 || cardNum == 54 || cardNum == 111 || cardNum == 112 || cardNum == 205 || cardNum == 206 || cardNum == 210 || cardNum == 211 || cardNum == 212||cardNum == 111 || cardNum == 112 || cardNum == 210 || cardNum == 211||cardNum==205||cardNum==206||cardNum==217)
            {
                effects.Add("ef_hurt_" + cardNum.ToString(), Resources.Load("Effect/ef_hurt_" + cardNum.ToString()) as GameObject);
            }
        }
        effects.Add("ef_fly_111",Resources.Load("Effect/ef_fly_111") as GameObject);
        effects.Add("ef_fly_112", Resources.Load("Effect/ef_fly_112") as GameObject);
        effects.Add("ef_fly_205", Resources.Load("Effect/ef_fly_205") as GameObject);
        effects.Add("ef_fly_206", Resources.Load("Effect/ef_fly_206") as GameObject);
        effects.Add("ef_fly_210", Resources.Load("Effect/ef_fly_210") as GameObject);
        effects.Add("ef_fly_211", Resources.Load("Effect/ef_fly_211") as GameObject);
        effects.Add("ef_cardBegin", Resources.Load("Effect/ef_cardBegin") as GameObject);
        effects.Add("ef_treatment", Resources.Load("Effect/ef_treatment") as GameObject);
        effects.Add("ef_break", Resources.Load("Effect/ef_break") as GameObject);
        effects.Add("ef_collide", Resources.Load("Effect/ef_collide") as GameObject);

    }

    /// <summary>
    /// 排序手牌
    /// </summary>
    public void ArrayHand()
    {
        handCardPanel.GetComponent<GridLayoutGroup>().enabled = false;
        handCardPanel.GetComponent<GridLayoutGroup>().enabled = true;
        for (int i=0;i<handCard.Count;i++)
        {
            handCard[i].Invoke("Array", 0.02f);
        }
    }

    public void SetGLGTrue()
    {
        handCardPanel.GetComponent<GridLayoutGroup>().enabled = true;
    }

    /// <summary>
    /// 初始化手牌
    /// </summary>
    /// <returns></returns>
    public IEnumerator DrawCard(int drawNum)
    {
        char[] c = drawCardStr.ToCharArray();
      //  Debug.Log((int)c[0]);
        if (GameManager.mSelectedCardGroup == GameManager.ICE)
        {
            for (int i = 0; i < drawNum; i++)
            {
                yield return new WaitForSeconds(0.2f);
                Debug.Log(100 + (int)(c[i]));
                int cardNum = ((int)(c[i]) >= 50) ? (int)(c[i]) : (int)(c[i]) + 100;
                GameObject card = GameObject.Instantiate(cards[cardNum], GameObject.Find("Canvas").transform);
                card.GetComponent<Card>().ToHand();
                mhandNum++;
               // mhandNumText.text = mhandNum.ToString();
                mCardPoolNum--;
                mCardPoolNumText.text = mCardPoolNum.ToString();
                AudioManager.SoundEffectPlay("se_draw");
            }
        }
        else if (GameManager.mSelectedCardGroup == GameManager.Demon)
        {
            for(int i=0;i< drawNum; i++)
            {
                yield return new WaitForSeconds(0.2f);
                int cardNum = ((int)(c[i]) >= 50) ? (int)(c[i]) : (int)(c[i]) + 200;
                GameObject card = GameObject.Instantiate(cards[cardNum], GameObject.Find("Canvas").transform);
                card.GetComponent<Card>().ToHand();
                mhandNum++;
             // mhandNumText.text = mhandNum.ToString();
                mCardPoolNum--;
                mCardPoolNumText.text = mCardPoolNum.ToString();
                AudioManager.SoundEffectPlay("se_draw");
            }
        }
        yield return new WaitForSeconds(0.6f);
        ArrayHand();

    }

    /*
    /// <summary>
    /// 抽卡抽卡
    /// </summary>
    public IEnumerator AddCard(int cardNum)
    {
            if (handCard.Count < maxHandNum)
            {
                yield return new WaitForSeconds(0.5f);
                GameObject card = GameObject.Instantiate(cards[cardNum], GameObject.Find("Canvas").transform);
                card.GetComponent<Card>().ToHand();
            }
    }
    */
    
    /// <summary>
    /// 状态同步
    /// </summary>
    public void StatusSYN(char[] recStr)
    {
        //TODO
        mStatus = (int)recStr[1];
        uStatus = (int)recStr[2];

        if(bitCheck(mStatus,STATUS_ICE)==0)
        {
            foreach(Transform t in EffectManager._instance.mBuffPanel.GetComponent<Transform>())
            {
                if(t.name == "buff_ice(Clone)")
                {
                    Destroy(t.gameObject);
                }
            }
        }

        if(bitCheck(mStatus,STATUS_DIZZY)==0)
        {
            foreach (Transform t in EffectManager._instance.mBuffPanel.GetComponent<Transform>())
            {
                if (t.name == "buff_dizzy(Clone)")
                {
                    Destroy(t.gameObject);
                }
            }
        }

        if(bitCheck(mStatus,STATUS_BLOCKADE)==0)
        {
           EffectManager._instance.defenceButton.GetComponent<Image>().sprite = Resources.Load("UISprite/Main/Play/defence_1", typeof(Sprite)) as Sprite;
           EffectManager._instance.drawCardButton.GetComponent<Image>().sprite = Resources.Load("UISprite/Main/Play/drawcard_1", typeof(Sprite)) as Sprite;
        }

        //---------------------------------------------

        if(bitCheck(uStatus,STATUS_ICE)==0)
        {
            foreach (Transform t in EffectManager._instance.uBuffPanel.GetComponent<Transform>())
            {
                if (t.name == "buff_ice(Clone)")
                {
                    Destroy(t.gameObject);
                }
            }
        }

        if (bitCheck(uStatus, STATUS_DIZZY) == 0)
        {
            foreach (Transform t in EffectManager._instance.uBuffPanel.GetComponent<Transform>())
            {
                if (t.name == "buff_dizzy(Clone)")
                {
                    Destroy(t.gameObject);
                }
            }
        }




    }

    /// <summary>
    /// 控制玩家操控状态
    /// </summary>
    /// <param name="recStr"></param>
    public void StateSYN(char[] recStr)
    {
        State  nextState = (State)((int)recStr[1]);
       
        int ustate = (int)recStr[2];            //对方状态
        if(nextState == State.STATE_COMBO)           //连击状态
        {
            state = State.STATE_COMBO;
            comboPanel.gameObject.SetActive(true);
            time = 30;
        }

        else if(nextState == State.STATE_EXILE)     //放逐状态
        {
            state = State.STATE_EXILE;
            time = 30;
        }

        else if(nextState == State.STATE_BURST)      //破灭状态
        {
            Debug.Log("进入破灭状态，请丢牌");
            burstPanel.gameObject.SetActive(true);
            state = State.STATE_BURST;
            time = 10;
        }

        else if(nextState == State.STATE_PLAY)      //出牌状态
        { 
            state = State.STATE_PLAY;
            Debug.Log("进入出牌状态");
            if(bitCheck(mStatus,STATUS_BLOCKADE)==0)
            {
                EffectManager._instance.defenceButton.GetComponent<Image>().color =  Color.white;
                EffectManager._instance.drawCardButton.GetComponent<Image>().color = Color.white;
            }
            waitingUMoveText.gameObject.SetActive(false);
            time = 30;
        }
        else if(nextState == State.STATE_CANTMOVE)  //无法行动状态
        {
            state = State.STATE_CANTMOVE;
          //  Debug.Log("无法行动，等待对方出牌");
            if(ustate==(int)State.STATE_COMBO||ustate ==(int)State.STATE_BURST)
            {
                waitingUMoveText.gameObject.SetActive(true);
            }
            else if(ustate==(int)State.STATE_PLAY)
            {
                if(bitCheck(mStatus,STATUS_ICE)==1)
                {
                    cantMoveTip.GetComponent<Image>().sprite = cantMoveTipSprite[0];
                }
                else if(bitCheck(mStatus,STATUS_DIZZY)==1)
                {
                    cantMoveTip.GetComponent<Image>().sprite = cantMoveTipSprite[1];
                }
                else
                {
                    cantMoveTip.GetComponent<Image>().sprite = cantMoveTipSprite[2];
                }
                cantMoveTip.gameObject.SetActive(true);
            }
        
        }
        else if(nextState ==State.STATE_ROUNDOVER)
        {
            ClearPlayArea();   //清空战场
            EffectManager._instance.RoundStart();
            
        }
    }

 
    public void M_AddCardToCeme(int cardNum)
    {
        GameObject card = GameObject.Instantiate(cards[cardNum], mCemePanel.GetChild(0).GetChild(0).GetChild(0).transform);
    }

    public void U_AddCardToCeme(int cardNum)
    {
        GameObject card = GameObject.Instantiate(cards[cardNum], uCemePanel.GetChild(0).GetChild(0).GetChild(0).transform);
    }

    /// 控制各个panel的大小
    /// </summary>
    public void CardPanelWidthCtr()
    {
      
       if(mCemePanel.GetChild(0).GetChild(0).GetChild(0).childCount>3)
        {
       //   Debug.Log(mCemePanel.GetChild(0).GetChild(0).GetChild(0).childCount);
            int count = mCemePanel.GetChild(0).GetChild(0).GetChild(0).childCount;
            mCemePanel.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(450.0f * count, 650);
        }
       else
        {
            mCemePanel.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(1350, 650);
        }

        if (uCemePanel.GetChild(0).GetChild(0).GetChild(0).childCount > 3)
        {
            int count = uCemePanel.GetChild(0).GetChild(0).GetChild(0).childCount;
            uCemePanel.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(450.0f * count, 650);
        }
        else
        {
            uCemePanel.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(1350, 650);
        }

        if (mExilePanel.GetChild(0).GetChild(0).GetChild(0).childCount > 3)
        {
            int count = mExilePanel.GetChild(0).GetChild(0).GetChild(0).childCount;
            mExilePanel.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(450.0f * count, 650);
        }
        else
        {
            mExilePanel.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(1350, 650);
        }

        if (uExilePanel.GetChild(0).GetChild(0).GetChild(0).childCount > 3)
        {
            int count = uExilePanel.GetChild(0).GetChild(0).GetChild(0).childCount;
            uExilePanel.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(450.0f * count, 650);
        }
        else
        {
            uExilePanel.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(1350, 650);
        }
    }
    

    /// <summary>
    /// 清空战场
    /// </summary>
    public void ClearPlayArea()
    {
        
        foreach(Transform t in uPlayArea.GetComponentInChildren<Transform>())
        {
            t.DOMove(uCemePos.position, 0.5f);
            Destroy(t.gameObject, 0.75f);
            int cardNum = t.GetComponent<Card>().cardNumber;
            if (cards.ContainsKey(t.GetComponent<Card>().cardNumber)&& cardNum != 51&& cardNum!=52)
            {
                U_AddCardToCeme(t.GetComponent<Card>().cardNumber);                   //添加到墓地
                uCemeNum++;
                uCemeNumText.text = uCemeNum.ToString();
                CardPanelWidthCtr();
            }

        }
        foreach(Transform t in mPlayArea.GetComponentInChildren<Transform>())
        {
            t.DOMove(mExilePos.position, 0.5f);
            Destroy(t.gameObject, 0.75f);
            int cardNum = t.GetComponent<Card>().cardNumber;
            if (cards.ContainsKey(t.GetComponent<Card>().cardNumber)&& cardNum != 51 && cardNum != 52)
            {
                mCemeNum++;
                mCemeNumText.text = mCemeNum.ToString();
                M_AddCardToCeme(t.GetComponent<Card>().cardNumber);                   //添加到墓地
                CardPanelWidthCtr();
            }
        }

        uCard = null;
        mCard = null;
    }

    /// <summary>
    /// 清空墓地
    /// </summary>
    /// <param name="player"></param>
    public void ClearCeme(int player)
    {
        if(player ==0)
        {
           foreach(Transform go in mCemePanel.GetChild(0).GetChild(0).GetChild(0).GetComponentInChildren<Transform>())
            {
                Destroy(go.gameObject);
                mCardPoolNum++;
            }
            mCemeNum = 0;
            mCemeNumText.text = mCemeNum.ToString();
            mCardPoolNumText.text = mCardPoolNum.ToString();
        }
        else if(player ==1)
        {
            foreach (Transform go in uCemePanel.GetChild(0).GetChild(0).GetChild(0).GetComponentInChildren<Transform>())
            {
                Destroy(go.gameObject);
                uCardPoolNum++;
            }
            uCemeNum = 0;
            uCemeNumText.text = uCemeNum.ToString();
            uCardPoolNumText.text = uCardPoolNum.ToString();
        }
    }

    //检查state的第pos位是否为1
    public int bitCheck(int status, int pos)
    {
        return status & 1 << pos ;
    }

    //将state的第pos位的值设为1
    public int bitAdd(int status, int pos)
    {

       // Debug.Log(bitCheck(status, STATUS_BLOCKADE));
        return status | (1 <<pos);
     
    }

    //将state的第pos位的值设为0
    public int bitDel(int status, int pos)
    {
        return status & (~(1 << pos ));
    }

    public  void beatCheck()
    {
        if(NM.beatCheckTime >10)
        {
            if(NetManager.socketClient.Connected)
            {
                try
                {
                    NetManager.socketClient.Close();
                }
                catch
                {
                    ConnectLostPanel.gameObject.SetActive(true);
                }
            }

            ConnectLostPanel.gameObject.SetActive(true);
        }


    }
}
