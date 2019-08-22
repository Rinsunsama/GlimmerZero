using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MatchCtr : MonoBehaviour {

    public static MatchCtr _instance;

    public Transform vsPanel;
    public float matchTime = 0;
    public Text matchTimeText;
    public Transform matchFalsePanel;                                      //匹配失败面板
    public List<Transform> characterButton;
    public Color disableColor;
    public static bool isMatchToThisScence =false;                         //进入到开始场景是否直接进入选人阶段
    public Transform matchingImage;                                        //正在匹配
    public Transform matchSucessImge;                                      //匹配成功
    public bool isTimeStart = false;
    public Image mCharacter;
    public Image uCharacter;
    public Transform uMatchingCharacter;                                    //？角色
    public Transform cancelButton;                                          //取消按钮
    public Sprite[] matchCharacterSprite;
    public Transform timeProImage;
    public Transform timeLight;

             
    private void Awake()
    {
        _instance = this;
    }
    // Use this for initialization
    void Start () {
        GameManager.mSelectedCardGroup = GameManager.ICE;
        //如果是再来一局，直接进入选人界面
        if(isMatchToThisScence)
        {
            ButtonManager._instance.selectPanel.gameObject.SetActive(true);
            AudioManager.BGMInstead("bgm_matching");
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate()
    {
        MatchTimeCtr();
    }

    /// <summary>
    /// 匹配计时
    /// </summary>
    public void MatchTimeCtr()
    {
        if(matchTime>0&&isTimeStart ==true)
        {
           matchTimeText.text = matchTime.ToString("30");
           matchTime -= Time.fixedDeltaTime;
            if (matchTime <=0)
            {
                matchTime = 0;
                isTimeStart = false;
                matchingImage.gameObject.SetActive(false);
                cancelButton.gameObject.SetActive(false);
                matchFalsePanel.gameObject.SetActive(true);            //如果还没匹配到，弹出匹配失败的消息
                try
                {
                    NetManager.socketClient.Close();                       //断开连接
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Data);
                }
            }
        }


        matchTimeText.text = matchTime.ToString("0")  ;
        timeProImage.GetComponent<Image>().fillAmount = matchTime / 30.0f;
        timeLight.rotation = Quaternion.Euler(new Vector3(0, 0, 360 * matchTime / 30));


    }

    /// <summary>
    /// 匹配成功
    /// </summary>
    public IEnumerator MatchSucess()
    {
        uCharacter.sprite = matchCharacterSprite[GameManager.uSelectedCardGroup];
        matchingImage.gameObject.SetActive(false);
        matchSucessImge.gameObject.SetActive(true);
        uMatchingCharacter.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
        yield return new WaitForSeconds(2);
        vsPanel.gameObject.SetActive(true);
        vsPanel.GetComponent<VSPanel>().StartCoroutine("ShowMatchSucess");                      //显示VS界面

    }
    

}
