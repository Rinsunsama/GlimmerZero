using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;
/// <summary>
/// 管理卡牌所生成的效果
/// </summary>
/// 
delegate void OnCompleteTween();
public class EffectManager : MonoBehaviour {

    const int EF_CARD_BEGIN = 0;            //卡牌发动效果
    const int EF_HURT = 1;                  //伤害效果
    const int EF_TREATMENT = 2;             //治疗效果
    const int EF_BREAK = 3;                 //打断效果
    const int EF_DIZZY = 4;                 //附加眩晕
    const int EF_ICE = 5;                   //附加冰冻
    const int EF_ADD_FROST = 6;             //增加霜冻点数
    const int EF_REDUCE_FROST = 7;          //减少霜冻点数
    const int EF_BLOCKADE = 8;              //封锁效果
    const int EF_RECOVER = 9;               //回收效果
    const int EF_DRAW_CARD = 10;            //抽卡效果
    const int EF_HURT_SELF = 11;            //自损效果
    const int EF_WASH_CARD = 12;            //洗牌效果
    const int EF_BURST = 13;                //破灭效果

    private int cardNumIn = 0;              //计算

   

    public static EffectManager _instance;
    public GameObject rootPanel;
    PlayManager PM;  
    GameManager GM;

    public Transform uEffectPos;                    //对方的特效播放位置
    public Transform mEffectPos;                    //我方的特效播放位置

    public Transform openCardStartPos;              //先攻判定起始位置    
    public Transform openCardFinalPos;              //压制判定结束位置

   
    public Transform openCardImage;                 //先攻或压制特效

    public GameObject uCardPfb;                     //对方卡                     
    public Transform  uBuffPanel;                   //对方buff面板
    public Transform  mBuffPanel;                   //己方buff面板
    public Dictionary<string, GameObject> buff = new Dictionary<string, GameObject>();

    public Text mFrostNumText;                      //我方霜冻点数文本
    public Text uFrostNumText;                      //对方霜冻点数文本
    public Transform drawCardButton;                //蓄能按钮
    public Transform defenceButton;                 //格挡按钮
    public AnimatorCtr uCharacter;
    public Transform uCardColPos;                   //对方卡牌碰撞位置
    public Transform mCardColPos;                   //我方卡牌碰撞位置
    public Color breakColor;                        //被打断卡牌的颜色
    public Transform overPanel;                     //游戏结束面板
    public Image roundStartImage;                   //回合开始图片
    public GameObject frostImage;                   //霜冻buff图片

    public float shakeSen =2.0f;                          //屏幕抖动系数




    private void Awake()
    {
        _instance = this;
    }
    // Use this for initialization
    void Start () {
        PM = PlayManager._instance;
        GM = GameManager._instance;
        
        cardNumIn = (GameManager.uSelectedCardGroup + 1) * 100;
        buff.Add("dizzy", Resources.Load("Prefabs/Buff/buff_dizzy")as GameObject);
        buff.Add("ice",Resources.Load("Prefabs/Buff/buff_ice") as GameObject);
        buff.Add("frost", Resources.Load("Prefabs/Buff/buff_frost") as GameObject);

        //test
       // EF_DoBlockade(1, 100);
    }
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.W))
        {
            UPlayCard(1);
        }
        if(Input.GetKeyDown(KeyCode.A))
        {
            UDrawCard();
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            MDoTreatMent(2);
            MDoHurt(3);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            
                char[] c = new char[4];
            c[0] = (char)0;
            c[1] = (char)51;
            c[2] = (char)0;
            c[3] = (char)1;
            OpenCard(c);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            char[] c = new char[2];
            c[0] = (char)1;
            c[1] = (char)0;
            StartCoroutine("GameOver", c);
        }
    }

    /// <summary>
    /// 回合开始
    /// </summary>
    public void RoundStart()
    {
        roundStartImage.gameObject.SetActive(false);
        roundStartImage.gameObject.SetActive(true);
        AudioManager.SoundEffectPlay("se_roundstart");

    }
    /// <summary>
    /// 造成伤害
    /// </summary>
    /// <param name="player"></param>
    /// <param name="num"></param>
    public void UDoHurt(int num)
    {
        PM.mHP -= num;
        PM.mHpText.text = PM.mHP.ToString();
        if (PM.mHP<=0)
            {
                PM.mHP = 0;
            //播放死亡的动画
            //TODO
         //   uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Win);
            //游戏结束
         //   GM.GameOver();
            }
        //播放扣血的动画
        //TODO
        PM.mHpTurn.GetComponent<HpFloat>().SetNumber(false, num);
        PM.mHpTurn.GetComponent<HpFloat>().DoFloat();
        rootPanel.transform.DOShakePosition(1.0f, shakeSen * num);
            
    }

    public void MDoHurt(int num)
    {
        PM.uHP -= num;
        PM.uHpText.text = PM.uHP.ToString();
        if (PM.uHP <= 0)
        {
            PM.uHP = 0;
            //播放死亡的动画
            //TODO
            //游戏结束
            uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Die);
            GM.GameOver();
        }
        //播放扣血的动画
        //TODO
        PM.uHpTurn.GetComponent<HpFloat>().SetNumber(false, num);
        PM.uHpTurn.GetComponent<HpFloat>().DoFloat();
    }

    /// <summary>
    /// 进行治疗
    /// </summary>
    /// <param name="player"></param>
    /// <param name="num"></param>
    public void MDoTreatMent(int num)
    {
        PM.mHP += num;
        PM.mHpText .text= PM.mHP.ToString();
        //播放加血的动画
        //TODO
        PM.mHpTurn.GetComponent<HpFloat>().SetNumber(true, num);
        PM.mHpTurn.GetComponent<HpFloat>().DoFloat();
    }

    public void UDoTreatMent(int num)
    {
        PM.uHP += num;
        PM.uHpText.text = PM.uHP.ToString();
        //播放加血的动画
        //TODO
        PM.uHpTurn.GetComponent<HpFloat>().DoFloat();
        PM.uHpTurn.GetComponent<HpFloat>().SetNumber(true, num);
    }
    /// <summary>
    /// 播放特效
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="playPos"></param>
    public void PlayEffect(string effectName,Vector3 playPos)
    {
        if(PM.effects.ContainsKey(effectName))
        {
            Debug.Log("播放特效"+effectName);
            GameObject ef = Instantiate(PM.effects[effectName]) as GameObject;
            ef.transform.position = new Vector3(playPos.x,playPos.y,ef.transform.position.z);
            Destroy(ef, 3.0f);
        }

    }

    /// <summary>
    /// 播放飞出特效
    /// </summary>
    /// <param name="effectName"></param>
    /// <param name="playPos"></param>
    /// <param name="toPos"></param>
    public void FlyEffect(string effectName,string effectBName, Vector3 playPos,Vector3 toPos)
    {
        GameObject ef = GameObject.Instantiate(Resources.Load("Effect/" + effectName, typeof(GameObject))) as GameObject;
        ef.transform.position = playPos;
        Tweener t = ef.transform.DOMove(toPos, 1.0f);
        TweenCallback flyBoom = delegate ()                      //到达
         {
             PlayEffect(effectBName, toPos);
         };
        t.OnComplete(flyBoom);
        Destroy(ef, 5.0f);
    }
    /// <summary>
    /// 对方盖牌
    /// </summary>
    public void UPlayCard(int num)
    {

        if(num!=50)
        {
            GameObject playCard = GameObject.Instantiate(uCardPfb, PM.uPlayPos.position, Quaternion.identity, rootPanel.transform);
            PM.uCard = playCard;
            Tweener tweenerR = playCard.transform.DORotate(new Vector3(0, 180, 0), 0.8f);
            tweenerR.OnComplete(playCard.GetComponent<Card>().U_ToPlayPanel);
            playCard.transform.DOMove(PM.uReadyPos.position, 0.8f);
            playCard.transform.DOScale(new Vector3(1.4f, 1.4f, 1f), 0.7f);
            PM.uHandNumText.text = PM.uhandNum.ToString();
            PM.waitingUMoveText.gameObject.SetActive(false);
            AudioManager.SoundEffectPlay("se_shuffle");
        }
       // GameObject uCard = GameObject.Instantiate(PM.cards[(GameManager.uSelectedCardGroup + 1) * 100 +num],PM.uPlayPos.position, Quaternion.identity,GameObject.Find("playArea").transform);
       
        //uCard.GetComponent<Image>().sprite = uCard.GetComponent<Card>().cardBackground;
        //StartCoroutine("DoShake", rootPanel.transform);
    }

    /// <summary>
    /// 对方连击
    /// </summary>
    public void UDoCombo(int num)
    {
        GameObject uCard = GameObject.Instantiate(PM.cards[cardNumIn + num], PM.uPlayPos.position, Quaternion.identity, rootPanel.transform);

        Tweener tweenerR = uCard.transform.DORotate(new Vector3(20, 0, 0), 0.6f);
        tweenerR.OnComplete(uCard.GetComponent<Card>().U_ToPlayPanel);
        uCard.transform.DOMove(PM.uReadyPos.position+new Vector3(300,0,0), 0.6f);
        PM.uCard = uCard;
        PM.uhandNum--;
        PM.uHandNumText.text = PM.uhandNum.ToString();
        uCard.transform.DOScale(new Vector3(1.2f, 1.2f, 1f), 0.5f);
        //  uCard.GetComponent<Image>().sprite = uCard.GetComponent<Card>().cardBackground;
        //  StartCoroutine("DoShake", rootPanel.transform);
        AudioManager.SoundEffectPlay("se_shuffle");
    }

    /// <summary>
    /// 对方破灭
    /// </summary>
    /// <param name="num"></param>
    public void UDoBurst(int num)
    {
        GameObject burstCard = GameObject.Instantiate(uCardPfb, PM.uPlayPos.position, Quaternion.identity, rootPanel.transform);
        if (num < 50) num = cardNumIn + num;
        burstCard.GetComponent<Image>().sprite = PM.cards[num].GetComponent<Card>().cardSprite;
        PM.U_AddCardToCeme(num);
        PM.CardPanelWidthCtr();
        Tweener tweener = burstCard.transform.DOMove(PM.uPlayPos.position , 0.5f);
        PM.uhandNum--;
        PM.uHandNumText.text = PM.uhandNum.ToString();
        PM.uCemeNum++;
        PM.uCemeNumText.text = PM.uCemeNum.ToString();
        Destroy(burstCard, 1.0f);
        AudioManager.SoundEffectPlay("se_shuffle");
    }



    public IEnumerator DoShake(Transform  it)
    {
        yield return new WaitForSeconds(1);
        it.DOShakePosition(0.5f, new Vector3(20, 20, 0),10,20,true);
    }
    /// <summary>
    /// 对方抽牌
    /// </summary>
    public void UDrawCard()
    {
        GameObject uCard = GameObject.Instantiate(uCardPfb, PM.uDrawPos.position, Quaternion.identity, rootPanel.transform);
        uCard.transform.DOMove(PM.uPlayPos.position, 0.5f);
        uCard.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        uCard.transform.DOScale(new Vector3(0.8f,0.8f,0.8f), 0.5f);
        PM.uhandNum++;
        PM.uHandNumText.text = PM.uhandNum.ToString();
        PM.uCardPoolNum--;
        PM.uCardPoolNumText.text = PM.uCardPoolNum.ToString();
        Destroy(uCard, 0.6f);
        AudioManager.SoundEffectPlay("se_draw");
    }

    /// <summary>
    /// 翻牌动画
    /// </summary>
    /// 

    public IEnumerator Open()
    {
        if (PM.uCard!=null&&PM.mCard!=null)
        {
            Tweener ut = PM.uCard.transform.DOMove(uCardColPos.position, 0.5f);
            Tweener mt = PM.mCard.transform.DOMove(mCardColPos.position, 0.5f);
            mt.SetEase(Ease.InBack);
            ut.SetEase(Ease.InBack);
         
            yield return new WaitForSeconds(0.5f);
            AudioManager.SoundEffectPlay("se_cardcollision");
            Tweener uTweenR = PM.uCard.transform.DORotate(new Vector3(0, 0, 0), 0.8f);
            Tweener mtweenR = PM.mCard.transform.DORotate(new Vector3(0, 0, 0), 0.8f);
            GameObject collide = Instantiate(PM.effects["ef_collide"], PM.canvas);
            Destroy(collide, 1.0f);
            yield return new WaitForSeconds(0.25f);
            PM.uCard.GetComponent<Image>().sprite = PM.uCard.GetComponent<Card>().cardSprite;
            PM.mCard.GetComponent<Image>().sprite = PM.mCard.GetComponent<Card>().cardSprite;
       
           
            yield return new WaitForSeconds(0.25f);
            Tweener utR = PM.uCard.transform.DOMove(PM.uReadyPos.position, 0.3f);
            PM.uCard.transform.DORotate(new Vector3(0, 0, 0), 0.3f);
            Tweener mtR = PM.mCard.transform.DOMove(PM.mReadyPos.position, 0.3f);
            PM.mCard.transform.DORotate(new Vector3(0, 0, 0), 0.3f);
        }
        else if(PM.uCard!=null)
        {
            PM.uCard.GetComponent<Image>().sprite = PM.uCard.GetComponent<Card>().cardSprite;
            Tweener uTweenR = PM.uCard.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        }
        else if(PM.mCard!=null)
        {
            PM.mCard.GetComponent<Image>().sprite = PM.mCard.GetComponent<Card>().cardSprite;
            Tweener mtweenR = PM.mCard.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        }
        else
        {
            Debug.Log("双方未出卡");
        }
    }



    public void OpenCard(char[] receive)
    {

        PM.cantMoveTip.gameObject.SetActive(false);              //隐藏不可行动面板
        PM.waitingUMoveText.gameObject.SetActive(false);         //隐藏等待对方行动面板

        int cardNum =  ((int)receive[1] >= 50)? (int)receive[1]: (int)receive[1] + cardNumIn;  //所出牌的序号
        int player = (int)receive[2];                                                          //先攻方
        int isSuppress = (int)receive[3];                                                      //是否压制先攻
        PM.waitingUMoveText.gameObject.SetActive(false);
        Debug.Log("对方出牌的序号为" + cardNum);
        if(PM.uCard!=null)
        {
            PM.uCard.GetComponent<Card>().cardSprite = PM.cards[cardNum].GetComponent<Card>().cardSprite;
            PM.uCard.GetComponent<Card>().cardNumber = PM.cards[cardNum].GetComponent<Card>().cardNumber;
            if(PM.uCard.GetComponent<Card>().cardNumber != 51 && PM.uCard.GetComponent<Card>().cardNumber != 52)
            {
                PM.uhandNum--;
                PM.uHandNumText.text = PM.uhandNum.ToString();
            }     
        }
      
        if(PM.mCard!=null)
        {
          //  PM.mCard.GetComponent<Image>().sprite = PM.mCard.GetComponent<Card>().cardSprite;
        }
        StartCoroutine("Open");   //开牌效果
        StartCoroutine("WaitForSomeSeconds", 1.2f);
        TweenCallback Over = delegate
        {
            openCardImage.position = openCardStartPos.position;
           
        };
        TweenCallback Stay = delegate
         {
             Tweener t2 = openCardImage.DOMove(openCardFinalPos.position+new Vector3(0.1f,0,0), 1.0f);
             DOTween.To(() => openCardImage.GetComponent<CanvasGroup>().alpha, x => openCardImage.GetComponent<CanvasGroup>().alpha = x, 0, 1.0f);
             t2.OnComplete(Over);
         };

        openCardImage.GetComponent<CanvasGroup>().alpha = 1;
        openCardImage.position = openCardStartPos.position;

        //我方先攻
        if (player==0&&isSuppress==0)
        {
            openCardImage.GetChild(0).GetComponent<Image>().sprite = Resources.Load("UISprite/Main/Combo/font_first", typeof(Sprite)) as Sprite;
            Tweener t = openCardImage.DOMove(openCardFinalPos.position, 0.5f);
            t.OnComplete(Stay);
        }
        
        //压制先攻
        else if(isSuppress==1)
        {
            openCardImage.GetChild(0).GetComponent<Image>().sprite = Resources.Load("UISprite/Main/Combo/font_stifle", typeof(Sprite)) as Sprite;
            Tweener t = openCardImage.DOMove(openCardFinalPos.position, 0.5f);
            t.OnComplete(Stay);

        }
      
        AudioManager.SoundEffectPlay("se_headportrait"); //播放音效
        
    }

    /// <summary>
    /// 处理服务器传输过来的特效信息
    /// </summary>
    /// <param name="effectNum"></param>
    /// <param name="recStr"></param>
    public void  EffectPlayCtr(char[] recStr)
    {
        int player = (int)recStr[1];                      //获取发动方，1为己方，2为友方
        int cardNum=51;
        if (player==0)
        {
            if (PM.mCard != null)
            {
                cardNum = PM.mCard.GetComponent<Card>().cardNumber;
            }
        }
        else
        {
            if (PM.uCard != null)
            {
                cardNum = PM.uCard.GetComponent<Card>().cardNumber;
            }
        }
  
        int effectNum = recStr[3];           //获取效果编号

        switch (effectNum)
        {
            //发动效果
            case EF_CARD_BEGIN:
                EF_CardBegin(player, cardNum);
                break;
            //造成伤害  ----》还没做完--->EF_DoHurt()
            case EF_HURT:
                int hurtNum = (int)recStr[4];
                EF_DoHurt(player, cardNum, hurtNum);
                break;
            //治疗效果
            case EF_TREATMENT:
                int treatNum = (int)recStr[4];
                EF_DoTreatment(player, cardNum, treatNum);
                break;
            //抽卡效果
            case EF_DRAW_CARD:
                string drawStr = new string(recStr).Remove(0, 5);
                EF_DrawCards(player, (int)recStr[4], drawStr);
                break;
            //打断效果
            case EF_BREAK:
                EF_Break(player);
                break;
            //增加霜冻点数
            case EF_ADD_FROST:
                int addNum = (int)recStr[4];
                EF_AddFrost(player, cardNum, addNum);
                break;
            //减少霜冻点数
            case EF_REDUCE_FROST:
                int reduceNum = (int)recStr[4];
                EF_ReduceFrost(player, cardNum, reduceNum);
                break;
            //附加霜冻效果
            case EF_ICE:
                EF_DoIce(player, cardNum);
                break;
            //附加眩晕效果
            case EF_DIZZY:
                EF_DoDizzy(player, cardNum);
                break;
            //附加封锁效果
            case EF_BLOCKADE:
                EF_DoBlockade(player, cardNum);
                break;
            //回收效果
            case EF_RECOVER:
                EF_Recover(player);
                break;
            //自损
            case EF_HURT_SELF:
                int num = (int)recStr[4];
                EF_HurtSelf(player, num);
                break;
           //洗牌效果
            case EF_WASH_CARD:
                EF_WashCard(player);
                break;
        }
    }


    /// <summary>
    /// 打断效果
    /// </summary>
    public void EF_Break(int player)
    {
        if(player==0)
        {
            GameObject efBreak = Instantiate(PM.effects["ef_break"] as GameObject, PM.canvas);
            Destroy(efBreak, 2.0f);
            efBreak.transform.position = PM.uCard.transform.position;
            PM.uCard.GetComponent<Image>().color = breakColor;
           
        }

        else if(player ==1)
        {
            GameObject efBreak = Instantiate(PM.effects["ef_break"] as GameObject, PM.canvas);
            Destroy(efBreak, 2.0f);
            efBreak.transform.position = PM.mCard.transform.position;
            PM.mCard.GetComponent<Image>().color = breakColor;
        }
        AudioManager.SoundEffectPlay("se_break");

    }




    /// <summary>
    /// xipauxiaoguo
    /// </summary>
    /// <param name="player"></param>
    public void EF_WashCard(int player)
    {
        Debug.Log("洗牌效果");
        if (player==0)
        {
            Debug.Log("我方洗牌");
            PM.ClearCeme(player);
        }
        else if(player ==1)
        {
            Debug.Log("对方洗牌");
            PM.ClearCeme(player);
        }
    }

    /// <summary>
    /// 自损效果
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cardNum"></param>
    /// <param name="num"></param>
    public void  EF_HurtSelf(int player, int num)
    {
        Debug.Log("玩家" + (player + 1).ToString() + "发动了卡牌的自损效果");
        if (player == 0)
        {
            UDoHurt(num);
        }
        else if(player ==1)
        {
            MDoHurt(num);
        }
    }

    /// <summary>
    /// 回收效果
    /// </summary>
    /// <param name="player"></param>
    public void EF_Recover(int player)
    {
        Debug.Log("玩家" + (player + 1).ToString() + "发动了卡牌的回收效果");
        if (player ==0)
        {
            PM.mCard.ToHand();
            PM.mhandNum++;
            PM.mhandNumText.text = PM.mhandNum.ToString();
        }
        else if(player ==1)
        {
            PM.uCard.transform.DOMove(PM.uDrawPos.position, 0.5f);
            TweenCallback over = delegate
            {
                Destroy(PM.uCard);
                PM.uCard = null;
                PM.uhandNum++;
                PM.uHandNumText.text = PM.uhandNum.ToString();
            };
            PM.uhandNum++;
        }
    }


    /// <summary>
    /// 封锁效果
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cardNum"></param>
    public void EF_DoBlockade(int player,int cardNum)
    {
        Debug.Log("玩家" + (player + 1).ToString() + "发动了卡牌的封锁效果");
        if (player==0)
        {
            PM.bitAdd(PM.uStatus, PlayManager.STATUS_BLOCKADE);
        }
        else if(player ==1)
        {
            GameObject blockade1 = Instantiate(Resources.Load("Effect/ef_blockade") as GameObject, PM.canvas);
            GameObject blockade2 = Instantiate(Resources.Load("Effect/ef_blockade") as GameObject, PM.canvas);
            blockade1.transform.position = defenceButton.position;
            blockade2.transform.position = drawCardButton.position;
            Destroy(blockade1, 2.0f);
            Destroy(blockade2, 2.0f);
         
            defenceButton.GetComponent<Image>().sprite = Resources.Load("UISprite/Main/Play/defence_2", typeof(Sprite)) as Sprite;
            drawCardButton.GetComponent<Image>().sprite = Resources.Load("UISprite/Main/Play/drawcard_2", typeof(Sprite)) as Sprite;
        
            PM.mStatus = PM.bitAdd(PM.mStatus, PlayManager.STATUS_BLOCKADE);
        
        }
    }


    /// <summary>
    /// 附加眩晕效果
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cardNum"></param>
    public void EF_DoDizzy(int player, int cardNum)
    {
        Debug.Log("玩家" + (player + 1).ToString() + "发动了卡牌的眩晕效果");
        //施法方是我方
        if (player == 0)
        {
            GameObject dizzy = Instantiate(buff["dizzy"], PM.canvas);
            dizzy.transform.position = PM.mCard.transform.position;
            Tweener t = dizzy.transform.DOMove(uBuffPanel.transform.position, 0.8f);
            t.SetEase(Ease.InBack);
            dizzy.transform.localScale = Vector3.zero;
            dizzy.transform.DOScale(Vector3.one, 0.6f);
            TweenCallback over = delegate
            {
                dizzy.transform.SetParent(uBuffPanel);
                PM.uStatus = PM.bitAdd(PM.uStatus, PlayManager.STATUS_DIZZY);
            };
            t.OnComplete(over);
        }
        //施法方是对方
        else if (player == 1)
        {
            GameObject dizzy = Instantiate(buff["dizzy"], PM.canvas);
            dizzy.transform.position = PM.uCard.transform.position;
            Tweener t = dizzy.transform.DOMove(mBuffPanel.transform.position,0.8f);
            t.SetEase(Ease.InBack);
            dizzy.transform.DOScale(Vector3.one, 0.6f);
            TweenCallback over = delegate
            {
                dizzy.transform.SetParent(mBuffPanel);
                PM.mStatus = PM.bitAdd(PM.mStatus, PlayManager.STATUS_DIZZY);
            };
            t.OnComplete(over);
        }
    }

    /// <summary>
    /// 附加霜冻效果
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cardNum"></param>
    public void EF_DoIce(int player,int cardNum)
    {
        Debug.Log("玩家" + (player + 1).ToString() + "发动了卡牌的冰冻效果");
        //施法方是我方
        if (player ==0)
        {
            GameObject ice = Instantiate(buff["ice"], PM.canvas);
            ice.transform.position = PM.mCard.transform.position;
            Tweener t = ice.transform.DOMove(uBuffPanel.transform.position, 0.5f);
            t.SetEase(Ease.InBack);
            ice.transform.localScale = Vector3.zero;
            ice.transform.DOScale(Vector3.one, 0.6f);
            TweenCallback over = delegate
            {
                ice.transform.SetParent(uBuffPanel);
               PM.uStatus= PM.bitAdd(PM.uStatus, PlayManager.STATUS_ICE);
            };
            t.OnComplete(over);
        }
        //施法方是对方
        else if(player ==1)
        {
            GameObject ice = Instantiate(buff["ice"], PM.canvas);
            ice.transform.position = PM.uCard.transform.position;
            Tweener t = ice.transform.DOMove(mBuffPanel.transform.position, 0.5f);
            t.SetEase(Ease.InBack);
            ice.transform.localScale = Vector3.zero;
            ice.transform.DOScale(Vector3.one, 0.6f);
            TweenCallback over = delegate
            {
                ice.transform.SetParent(mBuffPanel);
                PM.mStatus = PM.bitAdd(PM.mStatus, PlayManager.STATUS_ICE);
            };
            t.OnComplete(over);
        }
    }

    /// <summary>
    /// 减少霜冻
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cardNum"></param>
    /// <param name="num"></param>
    public void EF_ReduceFrost(int player,int cardNum,int num)
    {
        Debug.Log("玩家" + (player + 1).ToString() + "发动了卡牌的减少霜冻效果");
        if (player == 0)
        {
            GameObject frost = Instantiate(buff["frost"], PM.canvas);
            frost.transform.position = uBuffPanel.transform.position;
            Tweener t = frost.transform.DOMove(PM.mCard.transform.position, 0.5f);
            t.SetEase(Ease.InBack);
            TweenCallback over = delegate
            {
                Destroy(frost);
                if (PM.uFrostNum - num >= 0)
                {
                    PM.uFrostNum -= num;
                    uFrostNumText.text = PM.uFrostNum.ToString();
                }
                else
                {
                    PM.uFrostNum = 0;
                    Destroy(uFrostNumText.transform.parent.gameObject);
                }
               
            };
            t.OnComplete(over);

        }
        else if (player == 1)
        {
            GameObject frost = Instantiate(buff["frost"], PM.canvas);
            frost.transform.position = mBuffPanel.transform.position;
            Tweener t = frost.transform.DOMove(PM.uCard.transform.position, 0.5f);
            t.SetEase(Ease.InBack);
            TweenCallback over = delegate
            {
                Destroy(frost);
                if (PM.mFrostNum - num >= 0)
                {
                    PM.mFrostNum -= num;
                    mFrostNumText.text = PM.mFrostNum.ToString();
                }
                else
                {
                    PM.mFrostNum = 0;
                    Destroy(mFrostNumText.transform.parent.gameObject);
                }
              
            };
            t.OnComplete(over);
        }
    }

    /// <summary>
    /// 增加霜冻
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cardNum"></param>
    /// <param name="forst"></param>
    public void EF_AddFrost(int player,int cardNum,int num)
    {
        Debug.Log("玩家" + (player + 1).ToString() + "发动了霜冻增加的效果");
        if (player ==0)
        {
            GameObject frost = Instantiate(frostImage, PM.canvas);
            if(PM.uFrostNum==0)
            {
                GameObject frostBuff = Instantiate(buff["frost"], uBuffPanel)as GameObject;
                Debug.Log("生成buff");
                uFrostNumText = frostBuff.transform.GetChild(0).GetComponent<Text>();
            }
            frost.transform.position = PM.mCard.transform.position;
            Tweener t= frost.transform.DOMove(uBuffPanel.transform.position+new Vector3(0,-1,0), 0.8f);
            t.SetEase(Ease.InBack);
            frost.transform.localScale = Vector3.zero;
            frost.transform.DOScale(Vector3.one,0.6f);
            TweenCallback over = delegate
             {
                 Destroy(frost, 0.3f);
                 PM.uFrostNum += num;
                 if (uFrostNumText != null)
                 {
                     uFrostNumText.text = PM.uFrostNum.ToString();
                 }
                 
             };
            t.OnComplete(over);
            
        }
        else if(player ==1)
        {
            GameObject frost = Instantiate(frostImage, PM.canvas);
            if (PM.mFrostNum == 0)
            {
                GameObject frostBuff = Instantiate(buff["frost"], mBuffPanel) as GameObject;
                mFrostNumText = frostBuff.transform.GetChild(0).GetComponent<Text>();
            }
            frost.transform.position = PM.uCard.transform.position;
            Tweener t = frost.transform.DOMove(mBuffPanel.transform.position + new Vector3(0, -1, 0), 0.8f);
            t.SetEase(Ease.InBack);
            frost.transform.localScale = Vector3.zero;
            frost.transform.DOScale(Vector3.one, 0.6f);
            TweenCallback over = delegate
            {
                Destroy(frost,0.3f);
                PM.mFrostNum += num;
                if (mFrostNumText != null)
                {
                    mFrostNumText.text = PM.mFrostNum.ToString();
                }
             
            };
            t.OnComplete(over);
        }
    }

    /// <summary>
    /// 发牌发动效果
    /// </summary>
    public void EF_CardBegin(int player, int cardNum)
    {
        Debug.Log("玩家" + (player + 1).ToString() + "发动了卡牌的发动效果");

        GameObject ef = GameObject.Instantiate(Resources.Load("Effect/" + "ef_cardBegin", typeof(GameObject))) as GameObject;
      //  ef.GetComponent<ParticleSystem>().Play();
        Destroy(ef, 3.0f);
        if (player == 0&&PM.mCard!=null)
        {
           ef.transform.position = PM.mCard.transform.position+new Vector3(0,0,-PM.mCard.transform.position.z+99);
            TweenCallback reScale = delegate
            {
                Tweener tScale1 = PM.mCard.transform.DOScale(Vector3.one, 0.2f);
            };
            Tweener tScale = PM.mCard.transform.DOScale(new Vector3(1.2f, 1.2f, 1), 0.3f);
            tScale.OnComplete(reScale);
        }
        else if (player == 1&&PM.uCard!=null)
        {
           ef.transform.position = PM.uCard.transform.position+ new Vector3(0,0, -PM.uCard.transform.position.z + 99);
            TweenCallback reScale = delegate
            {
                Tweener tScale1 = PM.uCard.transform.DOScale(Vector3.one, 0.2f);
            };
            Tweener tScale = PM.uCard.transform.DOScale(new Vector3(1.2f, 1.2f, 1), 0.3f);
            tScale.OnComplete(reScale);
        }
    }


    public IEnumerator WaitForSomeSeconds(float n)
    {
        yield return new WaitForSeconds(n);
    }

    public IEnumerator DelayPlayEffect()
    {
        uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Attack);
        Debug.Log("置对方行动为攻击状态");
        yield return new WaitForSeconds(3);
    }

    public IEnumerator DelayMDoHurt(int num)
    {
        yield return new WaitForSeconds(2);
        MDoHurt(num);
    }

    public IEnumerator DelayUDoHurt(int num)
    {
        yield return new WaitForSeconds(2);
        UDoHurt(num);
    }

    public IEnumerator DalayPEMPos(int cardNum)
    {
        yield return new WaitForSeconds(2);
        PlayEffect("ef_hurt_" + cardNum.ToString(), mEffectPos.transform.position);
    }


    public IEnumerator DalayPEUPos(int cardNum)
    {
        yield return new WaitForSeconds(2);
        PlayEffect("ef_hurt_" + cardNum.ToString(), uEffectPos.transform.position);
    }

    /// <summary>
    /// 伤害特效
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cardNum"></param>
    /// <param name="hurtNum"></param>
    public void EF_DoHurt(int player, int cardNum, int hurtNum)
    {
        Debug.Log("玩家" + (player + 1).ToString() + "发动了卡牌的伤害效果");
        //当伤害特效在敌人身上产生时
        if (cardNum == 100 || cardNum == 101 || (cardNum >= 103 && cardNum <= 108) || cardNum == 113 || (cardNum >= 115 && cardNum <= 203) || cardNum == 207 || cardNum == 208 || cardNum == 209 || cardNum == 212 || cardNum == 213 || cardNum == 215 || cardNum == 216)
        {
            if (player == 0)
            {

                PlayEffect("ef_hurt_" + cardNum.ToString(), uEffectPos.position);               
                MDoHurt(hurtNum);
                uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Attacked);
            }
            else if (player == 1)
            {
                StartCoroutine("DelayUDoHurt", hurtNum);
                StartCoroutine("DalayPEMPos", cardNum);
                uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Attack);
              //  PlayEffect("ef_hurt_" + cardNum.ToString(), mEffectPos.transform.position);
              //  UDoHurt(hurtNum);
            }
        }
        //当伤害特效在卡牌自身产生时
        else if (cardNum == 53||cardNum==54)
        {
            if(player==0)
            {
                PlayEffect("ef_hurt_" + cardNum.ToString(), mEffectPos.position);
                MDoHurt(hurtNum);
                uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Attacked);
            }
            else if(player ==1)
            {
                uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Attack);
                StartCoroutine("DelayUDoHurt", hurtNum);
                StartCoroutine("DalayPEUPos", cardNum);
                // PlayEffect("ef_hurt_" + cardNum.ToString(), uEffectPos.position);        
                // UDoHurt(hurtNum);

            }
        }

        //当伤害特效从卡牌位置飞到对方脸上时
        else if(cardNum==111|| cardNum == 112 || cardNum == 205 || cardNum == 206 || cardNum == 210 || cardNum == 211)
        {
            if(player ==0)
            {
                FlyEffect("ef_fly_" + cardNum, "ef_hurt_" + cardNum, PM.mCard.transform.position + new Vector3(0, 0, 50), uEffectPos.position);
               
                MDoHurt(hurtNum);
                uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Attacked);
            }
            else if (player ==1)
            {
                uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Attack);
                FlyEffect("ef_fly_" + cardNum, "ef_hurt_" + cardNum, PM.uCard.transform.position + new Vector3(0, 0, 50), mEffectPos.position);
                
                UDoHurt(hurtNum);
                
            }
        }

        //当伤害特效在自身产生时
        else if(cardNum==110||cardNum==217)
        {
            if (player == 0)
            {
                PlayEffect("ef_hurt_" + cardNum.ToString(), mEffectPos.position);
              
                MDoHurt(hurtNum);
                uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Attacked);
            }
            else if (player == 1)
            {
                uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Attack);
                PlayEffect("ef_hurt_" + cardNum.ToString(), uEffectPos.position);
                UDoHurt(hurtNum);
              
            }
        }

        AudioManager.SoundEffectPlay("se_ce_attack_" + cardNum.ToString());
    }

    /// <summary>
    /// 治疗特效
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cardNum"></param>
    /// <param name="treatNum"></param>
    public void EF_DoTreatment(int player,int cardNum,int treatNum)
    {
        Debug.Log("玩家" + (player + 1).ToString() + "发动了卡牌的治疗效果");
        if (player==0)
        {
            PlayEffect("ef_treatment", mEffectPos.position);
            MDoTreatMent(treatNum);
        }
        else if(player ==1)
        {
            PlayEffect("ef_treatment", uEffectPos.position);
            UDoTreatMent(treatNum);
        }
        AudioManager.SoundEffectPlay("se_ce_treatment" + cardNum.ToString());
    }

    /// <summary>
    /// 打断效果
    /// </summary>
    /// <param name="player"></param>
    public void EF_Burst(int player)
    {
        Debug.Log("玩家" + (player + 1).ToString() + "发动了卡牌的打断效果");
        if (player==0)    //我方打断对手
        {
            if(PM.uCard!=null)
            { 
                PlayEffect("ef_burst", PM.uCard.transform.position);
                PM.uCard.transform.GetComponent<Image>().color = Color.gray;
            }
        }
        else if(player ==1)  //对方打断我方
        {
            if (PM.mCard != null)
            {
                PlayEffect("ef_burst", PM.mCard.transform.position);
                PM.mCard.transform.GetComponent<Image>().color = Color.gray;
            }
        }
    }

    /// <summary>
    /// 抽牌效果
    /// </summary>
    /// <param name="player"></param>
    /// <param name="drawNum"></param>
    /// <param name="drawStr"></param>
    public void EF_DrawCards(int player, int drawNum,string  drawStr)
    {
        Debug.Log("玩家" + (player + 1).ToString() + "发动了卡牌的抽牌效果果");
        //我方抽牌
        if (player==0)
        {
            Debug.Log("我方抽牌");
            PlayManager.drawCardStr = drawStr;
            PM.StartCoroutine("DrawCard", drawNum);                       
        }
        //对方抽卡
        else if(player==1)
        {
            for(int i=0;i<drawNum;i++)
            {
                Invoke("UDrawCard", 0.5f * i);
            }
        }

    }
    

    /// <summary>
    /// 游戏结束
    /// </summary>
    public IEnumerator GameOver(char[] reC)
    {

        NetManager._instance.beatCheckTime = int.MinValue;
        yield return new WaitForSeconds(2.5f);
        Debug.Log("游戏结束了");
        int winner = reC[1];                   //胜利方
        Transform result = null;
        if(winner ==0) //我方获胜
        {
            result = overPanel.GetChild(0);
            uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Die);
            Debug.Log("胜利");
            AudioManager.SoundEffectPlay("se_win");
        }
        else if(winner ==1)                   //我方失败
        {
            result = overPanel.GetChild(1);
            uCharacter.SetAnimatorState((int)AnimatorCtr.UState.Win);
            Debug.Log("失败");
            AudioManager.SoundEffectPlay("se_fail");
            
        }
        else
        {
            result = overPanel.GetChild(0);
        }

        yield return new WaitForSeconds(2.0f);

        overPanel.gameObject.SetActive(true); 
        result.gameObject.SetActive(true);


    }
    
    
    
    /// <summary>
    /// 特效是否播完
    /// </summary>
    /// <returns></returns>
    public bool IsEffectPlayOver()
    {
        return false;
    }
}
