using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using DG.Tweening;
public class Card : MonoBehaviour {

    public int cardNumber;                    //卡牌编号
    public string cardName;                   //卡牌名字
    public string description;                //卡牌描述
    public int level;                         //卡牌等级
    public int speed;                         //卡牌速度
    public int comboLevel;                    //连击等级
    public Vector3 startPos;
    public bool[] isHaveTip =new bool[12];


    string[] tips = new string[12]            //词条提示
    {
        "「打断」：当我方先攻时触发，可使对方本回合打出的卡牌失效，并将其放入弃牌区。",
        "「连击ⅠⅡⅢ」：当我方先攻时,进入连击阶段，可额外打出连击等级更高的卡牌。",
        "「X阶压制ⅠⅡⅢ」：若敌方打出的卡牌阶级为X，获得「绝对前置ⅠⅡⅢ」和「打断」。",
        "「绝对前置ⅠⅡⅢ」：具有比一阶卡牌更快的速度，前置等级越高速度越快。",
        "「绝对后置ⅠⅡⅢ」：具有比五阶卡牌更慢的速度，后置等级越高速度越慢。",
        "「X阶前置ⅠⅡⅢ」：当双方都打出同阶卡牌时触发，为卡牌加速，前置等级越高速度越快。",
        "「X阶后置ⅠⅡⅢ」：当双方都打出同阶卡牌时触发，为卡牌减速，后置等级越高速度越慢。",
        "「霜冻」：对敌方造成叠加性debuff状态，影响自身部分技能效果。",
        "「破灭X」：使敌方丢弃X张手牌。",
        "「伤害反制」：免疫敌方在本回合的后续伤害并将伤害返还给敌方。",
        "「冻结」「晕眩」：存在该debuff状态的玩家下回合无法出牌。",
        "「回收」：卡牌打出后将移回手牌。"
    };


    public Sprite cardSprite;                 //卡面
    public Sprite cardBackground;             //卡背
    public bool isSelect;                     //是否选中
    protected RectTransform RT;               //RectTransform属性
    PlayManager PM;                           //PlayManager
    Vector2 centerPos = new Vector2(375, -105);

    private void Start()
    {
       // Invoke("Array", 0.1f);
        PM = PlayManager._instance;
        RT = GetComponent<RectTransform>();
    }
    private void Update()
    {
       
    }
    /// <summary>
    /// 展示效果
    /// </summary>
    public static void Effect(int cardNum)
    {
        Debug.Log("卡牌编号为" + cardNum + "的卡发动了效果！");
    }


    /* public void OnPointerEnter()
     {
         Debug.Log("进入长按");
     }

     public void OnPointExit()
     {
         Debug.Log("退出长按");
     }
     */
    /// <summary>
    /// 点击
    /// </summary>
    public virtual void OnPointDown()
    {
        startPos = transform.position;
        if (transform.parent.name == "handCardPanel")      //当该卡在手牌中时
        {
            //如果该卡未被选中
            if (!isSelect)
            {
                Tweener tweener = RT.DOLocalMoveY(RT.anchoredPosition.y - centerPos.y + 50, 0.2f);
                Tweener scaleTween = RT.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f);
                //   GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 20.0f);
                isSelect = true;
                GetComponent<Outline>().enabled = true;
                PM.showCardPanel.gameObject.SetActive(true);
                PM.showCard.transform.position = transform.position + new Vector3(0, 4, 0);

                Tweener t = PM.showCard.transform.DOMove(transform.position + new Vector3(0, 4.2f, 0), 0.3f);
                // t.SetEase(Ease.);
                PM.showCard.sprite = cardSprite;
                if (PM.selectedCard != null)
                {
                    PM.selectedCard.GetComponent<Outline>().enabled = false;
                    PM.selectedCard.Array();
                    PM.selectedCard.isSelect = false;
                }
                PM.selectedCard = GetComponent<Card>();
            }
            else if (isSelect)
            {
                isSelect = false;
                PM.selectedCard = null;
                GetComponent<Outline>().enabled = false;
                Array();
                PM.showCardPanel.gameObject.SetActive(false);
            }
        }

        // ---------显示词条-------------------


        for (int i = 0; i < 12; i++)
        {
            if (isHaveTip[i])
            {
                GameObject tip = Instantiate(PM.cardTipPrefab, PM.cardTigPanel);
                tip.transform.GetChild(0).GetComponent<Text>().text = tips[i];
            }
        }
        PM.cardTigPanel.position = transform.position + new Vector3(-4f, 3.0f, 0);
        Debug.Log(PM.cardTigPanel.transform.position);
        if(PM.cardTigPanel.GetComponent<RectTransform>().anchoredPosition.x<-710) PM.cardTigPanel.position = transform.position + new Vector3(4f, 3.0f, 0);
        PM.cardTigPanel.gameObject.SetActive(true);
    }

    public virtual void OnPointUp()
    {
        if(transform.parent.name == "handCardPanel")
        {
            if (PlayManager._instance.selectedCard != null)
            {
                Card c = PlayManager._instance.selectedCard;
                c.isSelect = false;
                c.GetComponent<Outline>().enabled = false;
                c.Array();
                PlayManager._instance.selectedCard = null;
            }
            PM.showCardPanel.gameObject.SetActive(false);
            PM.showCard.transform.DOKill();
        }

        for (int i = 0; i < PM.cardTigPanel.childCount; i++)
        {
            Destroy(PM.cardTigPanel.GetChild(i).gameObject);
           
        }
        PM.cardTigPanel.gameObject.SetActive(false);
 
    }
    /// <summary>
    /// 拖拽
    /// </summary>
    public virtual void OnDrag()
    {
        if (transform.parent.name == "handCardPanel")
        {
            PM.showCardPanel.gameObject.SetActive(false);
                                
            if (PlayManager.state == PlayManager.State.STATE_PLAY)                                      //如果是出牌状态
            {
                float tiltX = Input.GetAxis("Mouse X");
                float tileY = Input.GetAxis("Mouse Y");
             //   Debug.Log(transform.position);
                //   transform.position += new Vector3(tiltX/3 , tileY/3 , 0);
                GetComponent<RectTransform>().anchoredPosition = new Vector3(Input.mousePosition.x - 550, Input.mousePosition.y - 105, 0);
             //   transform.localScale += new Vector3(tileY / 20, tileY / 20, tileY / 20);              //卡牌随着Y轴逐渐变大
                transform.eulerAngles = Vector3.zero;
            }

            else if (PlayManager.state == PlayManager.State.STATE_COMBO)
            {
                if (comboLevel > PlayManager.nowComboLevel)
                {
                    float tiltX = Input.GetAxis("Mouse X");
                    float tileY = Input.GetAxis("Mouse Y");
                    GetComponent<RectTransform>().anchoredPosition = new Vector3(Input.mousePosition.x - 550, Input.mousePosition.y - 105, 0);
                //    transform.localScale += new Vector3(tileY / 20, tileY / 20, tileY / 20);              //卡牌随着Y轴逐渐变大
                    transform.eulerAngles = Vector3.zero;
                }
            }

            else if (PlayManager.state == PlayManager.State.STATE_BURST)
            {
                float tiltX = Input.GetAxis("Mouse X");
                float tileY = Input.GetAxis("Mouse Y");
                GetComponent<RectTransform>().anchoredPosition = new Vector3(Input.mousePosition.x - 550, Input.mousePosition.y-105, 0);
             //   transform.localScale += new Vector3(tileY / 20, tileY / 20, tileY / 20);              //卡牌随着Y轴逐渐变大
                transform.eulerAngles = Vector3.zero;
            }
        }
    }
    
    /// <summary>
    /// 完成拖拽
    /// </summary>
    public virtual void OnEndDrag()
    {
        if (transform.parent.name == "handCardPanel")
        {
            if (GetComponent<RectTransform>().anchoredPosition.y > PM.minReadyPosY)
            {
                if (PlayManager.state != PlayManager.State.STATE_CANTMOVE)
                {
                    ConfirmCard();
                }
            }
            transform.localScale = Vector3.one;
            PM.showCardPanel.gameObject.SetActive(false);
            GetComponent<Outline>().enabled = false;
            PM.selectedCard = null;
            transform.position = startPos;
         //   transform.SetParent(PM.canvas);
         //  transform.SetParent(PM.handCardPanel);
            PM.StartCoroutine("ArrayHand");
        }
    }

    /// <summary>
    /// 卡牌排序
    /// </summary>
    public void Array()
    {  
        transform.eulerAngles = new Vector3(0, 0, (GetComponent<RectTransform>().anchoredPosition.x - centerPos.x) / -30.0f);
        float yInc = -105 - Mathf.Abs(GetComponent<RectTransform>().anchoredPosition.x - centerPos.x) / 25f;
        Tweener tweener = RT.DOLocalMove(new Vector2(GetComponent<RectTransform>().anchoredPosition.x - centerPos.x, yInc - centerPos.y), 0.5f);
        Tweener scaleTween = RT.DOScale(Vector3.one, 0.5f);
        GetComponent<Outline>().enabled = false;
    }

    /// <summary>
    /// 确定卡牌
    /// </summary>
    public void ConfirmCard()
    {
        PM = PlayManager._instance;
  
     /*   if(cardNumber ==202||cardNumber==203)
        {
            //跳出选项，让玩家选择
            //TODO
        }*/

        //出的是连击牌时
        if(PlayManager.state ==PlayManager.State.STATE_COMBO)
        {
            //发送COMBO信息
            char[] send = new char[3];
            send[0] = (char)NetManager.RQ_PLAYCARD;
            send[1] = (char)2;
            send[2] = (char)(cardNumber % 100);
            NetManager.SendToServer(new string(send));

            Tweener tweener = transform.DOMove(PM.mReadyPos.position, 0.75f);
            transform.DOScale(Vector3.one, 0.5f);
            tweener.OnComplete(M_ToPlayPanel);
            Effect(cardNumber);
                                                             //手牌数量减少
            PM.comboPanel.gameObject.SetActive(false);
            PlayManager.state = PlayManager.State.STATE_CANTMOVE;                 //出牌结束，置为不可行动
            PlayManager.nowComboLevel = comboLevel;
            PM.mCard = this;
            //本回合出的牌为当前卡牌
            PM.mhandNum--;
        //    PM.mhandNumText.text= PM.mhandNum.ToString();.
        }
        //出的是普通牌时
        else if(PlayManager.state == PlayManager.State.STATE_PLAY)
        {
            //发送出牌信息
            EffectManager._instance.drawCardButton.GetComponent<Image>().color = PM.disableColor;
            EffectManager._instance.defenceButton.GetComponent<Image>().color = PM.disableColor;
            GetComponent<Image>().sprite = cardBackground;   
            char[] send = new char[3];
            send[0] = (char)NetManager.RQ_PLAYCARD;
            send[1] = (char)1;
            send[2] = (char)(cardNumber % 100);
            NetManager.SendToServer(new string(send));
            Tweener tweenerR = transform.DORotate(new Vector3(0, 180, 0), 0.5f);
            transform.DOScale(new Vector3(1.4f,1.4f,1), 0.5f);
            Tweener tweener = transform.DOMove(PM.mReadyPos.position, 0.75f);
            tweener.OnComplete(M_ToPlayPanel);                                    //将选择的卡牌放置到判定区
          //  Debug.Log(transform.localScale);
                                                                 //手牌数量减少
            PlayManager.state = PlayManager.State.STATE_CANTMOVE;                 //出牌结束，置为不可行动

            if(PM.uPlayArea.childCount ==0)
            {
                PM.waitingUMoveText.gameObject.SetActive(true);
            }
            if(comboLevel>0)
            {
                PlayManager.nowComboLevel = comboLevel;
            }
            PM.mCard = this;                                  //本回合出的牌为当前卡牌

            if(cardNumber!=51&&cardNumber!=52)
            {
                PM.mhandNum--;
            }
           
        }

        //出的是破灭牌时
        else if(PlayManager.state ==PlayManager.State.STATE_BURST)
        {
            Tweener tweenP = transform.DOMove(PM.mExilePos.position, 0.5f);
            Destroy(this.gameObject, 1.0f);
            PM.M_AddCardToCeme(cardNumber);
            PM.CardPanelWidthCtr();
            char[] send = new char[3];
            send[0] = (char)NetManager.RQ_PLAYCARD;
            send[1] = (char)3;
            send[2] = (char)(cardNumber % 100);
            NetManager.SendToServer(new string(send));
            PlayManager.state = PlayManager.State.STATE_CANTMOVE;                 //出牌结束，置为不可行动
            PlayManager._instance.burstPanel.gameObject.SetActive(false);
            PM.mCemeNum++;
            PM.mCemeNumText.text = PM.mCemeNum.ToString();
        }
        AudioManager.SoundEffectPlay("se_shuffle");
        PM.time = -1;
        PM.timeText.text = "";
        GetComponent<Outline>().enabled = false;       
        isSelect = false;
        PM.selectedCard = null;
        PM.handCard.Remove(this);
      


        //发送所出牌的序号
        /* char[] message = new char[2];
           message[0] = '2';
           message[1] = (char)(cardNumber % 100);
           byte[] buffter = Encoding.UTF8.GetBytes(message);
           int temp = NetManager.socketClient.Send(buffter);*/
    }

    /// <summary>
    /// 回到手牌
    /// </summary>
    public void ToHand()
    {
        PlayManager._instance.handCard.Add(this);
        Tweener tweener = GetComponent<RectTransform>().DOMove(PlayManager._instance.mDrawPos.position, 0.5f);
        tweener.OnComplete(SetParent);
    }

    public void M_ToPlayPanel()
    {
      //  transform.rotation = Quaternion.identity;
        transform.SetParent(PM.mPlayArea.transform);      //将选择的卡牌放置到判定区
        transform.localScale = Vector3.one;
        
    }

    //设置在战场
    public void U_ToPlayPanel()
    {
        transform.localScale = Vector3.one;              
        transform.SetParent(PM.uPlayArea.transform);
    }

    //设置父节点
    public void  SetParent()
    {
       transform.SetParent(PM.handCardPanel);
    }



}
