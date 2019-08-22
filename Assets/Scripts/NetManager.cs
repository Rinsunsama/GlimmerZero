using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine.SceneManagement;
using System;

public class NetManager : MonoBehaviour {

    public static NetManager _instance;
    EffectManager EM;

    public static string receiveStr;                                     //接受到的数据
    const string IP = "192.168.28.156";                                  //服务器IP地址

    //const string IP2 = "47.105.125.96";
    public static string IP2 = "10.168.5.182";
    public int port = 13240;                                             //服务器端口号

    public float beatCheckTime;


    //服务器请求类型
    public const int RQ_BEATCHECK = 0;                                   //心跳检测
    public const int RQ_MATCH = 1;                                       //请求匹配，将自己的卡组信息发送到服务器
    public const int RQ_PLAYCARD = 4;                                    //出牌，将所出牌的编号发送
                                                                         //  const int RQ_COMBO = 3;                                            //连击牌
                                                                         //  const int RQ_EXILE = 4;                                            //放逐牌
                                                                         //  const int RQ_BURST = 5;                                            //破灭牌
    public const int RQ_STATUS_SYN = 6;                                  //请求血量状态同步
    public const int RQ_STATE_SYN = 7;                                   //请求操作状态同步
    public const int RQ_SURRENDERT = 8;                                  //请求认输


    //服务器回执类型
    const int RP_BEATCHECK = 0;                                         //心跳检测                   
    const int RP_MATCHING = 2;                                          //正在匹配
    const int RP_MATCH_SUCCESS = 3;                                     //匹配成功，将对方的职业、初始手牌发送过来，
    const int RP_EFFECT_PLAY = 11;                                      //播放特效
    const int RP_STATUS_SYN = 12;                                       //BUFF状态同步
    const int RP_STATE_SYN = 6;                                         //玩家操作状态同步(出牌，连击，破灭，放逐）
    const int RP_GAME_RESULT = 13;                                      //战斗结果
    const int RP_U_PLAYCARD = 5;                                        //对方出卡
    const int RP_OPENCARD = 9;                                          //开牌
    const int RP_EF_SEND_OVER = 10;                                     //效果发送完毕

    public static Socket socketClient;

    private void Start()
    {
        EM = EffectManager._instance;
        DontDestroyOnLoad(this.gameObject);
        receiveStr = "Zero";
        beatCheckTime = int.MinValue;
    }


    private void Awake()
    {
        socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _instance = this;
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public void Connect()
    {
        IPAddress ip = IPAddress.Parse(IP2);
        IPEndPoint point = new IPEndPoint(ip, port);
        socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socketClient.Connect((EndPoint)point);
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
       if(socketClient.Connected)
        {
            RPListent();
        //  Debug.Log("Yes");
        }
        beatCheckTime += Time.fixedDeltaTime;
    }

    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="o"></param>
  /* public static void Recive(object o)
    {
        Socket send = o as Socket;
        while (true)
        {
            //获取发送过来的消息
            byte[] buffer = new byte[2048];
            int effective = send.Receive(buffer);
            if (effective == 0)
            {
                break;
            }
            else
            {
                string str = Encoding.UTF8.GetString(buffer, 0, effective);
                receiveStr = str;
            }
        }
    }
    */


    /// <summary>
    /// 心跳检测回应
    /// </summary>
    public void Listent()
    {
        while (true)
        {
            //获取发送过来的消息
            byte[] heat = new byte[1];
            
            int effective = socketClient.Receive(heat);
            if (effective == 0)
            {
                break;
            }
            else
            {
                int length =  (int)(Encoding.UTF8.GetString(heat, 0, effective).ToCharArray()[0])-1;   //获取长度
                Debug.Log("收到了长度为" + length + "的信息:");               
                string str =string.Empty;
                while (length>0)
                {
                  byte[] buffer = new byte[length];
                  int effective2 = socketClient.Receive(buffer);
                  str += Encoding.UTF8.GetString(buffer, 0, effective2);
                  length -= effective2;
                }

                string x ="";
                for(int i =0;i<str.Length;i++)
                {
                    x += (int)str[i]+" "; 
                }
                print(x);

                if ((int)str[0] == 0)                                           //如果是心跳包，回送一个
                {
                    char[] send = new char[2];
                    send[0] = (char)2;
                    send[1] = (char)0;
                    byte[] buffter = Encoding.UTF8.GetBytes(new string(send));
                    int temp = NetManager.socketClient.Send(buffter);
                    beatCheckTime = 0;
                }
                else
                {
                    receiveStr = str;  //存储接收的数据
                   // RPListent();
                }

            }
        }
        /*
        if (receiveStr == "0")
        {
            Debug.Log("0");
            byte[] buffter = Encoding.UTF8.GetBytes("0");
            int temp = NetManager.socketClient.Send(buffter);
        }*/
    }

    public void StartBeatCheck()
    {
        Thread beatCheck = new Thread(Listent);                   //开启心跳检测
        beatCheck.Start(socketClient);
    }

    /// <summary>
    /// 监听服务器消息
    /// </summary>
    public void RPListent()
    {
        EM = EffectManager._instance;
        if (receiveStr != null&&receiveStr.Length>0&&receiveStr!=string.Empty)
        {
          // Debug.Log("接受的消息不为空");
            char[] reC = receiveStr.ToCharArray();
            //-----------匹配成功，进入play场景，初始化手牌
            if ((int)reC[0] == RP_MATCH_SUCCESS && (int)reC[1] == 0)
            {
                //出现匹配成功界面
                //TODO
                Debug.Log("进入匹配成功");
                GameObject.Find("Canvas").GetComponent<ButtonManager>().CancelInvoke("MatchTimeCtr");
                GameManager.uSelectedCardGroup = (int)receiveStr[2];    //生成对方角色
                PlayManager.drawCardStr = receiveStr.Remove(0, 3);      //传输初始手牌
                receiveStr = string.Empty;
                Loading.loadSceneName = "Play";
                MatchCtr._instance.StartCoroutine("MatchSucess");                      //显示VS界面

            }
            //-----------效果发送完成---------------------------
            else if ((int)reC[0] == RP_EF_SEND_OVER && SceneManager.GetActiveScene().name == "Play")
            {
                receiveStr = string.Empty;
                Invoke("SendStateDelay", 0.1f);
            }

            //-----------处理特效消息------------------------------------------------
            else if ((int)reC[0] == RP_EFFECT_PLAY)
            {
                receiveStr = string.Empty;                             //将信息置空                    
                int player = (int)reC[1];                               //获取要播放效果的卡牌
                EM.EffectPlayCtr(reC);                                 //交付给特效控制器去执行特效播放
            }
            //-----------处理血量状态同步消息----------------------------------------
            else if ((int)reC[0] == RP_STATUS_SYN)
            {
                receiveStr = string.Empty;                              //将信息置空
                PlayManager._instance.StatusSYN(reC);                   //交付给PlayManager处理状态同步
            }
            //-----------处理操作状态同步消息-----------------------------------------
            else if ((int)reC[0] == RP_STATE_SYN)
            {
                receiveStr = string.Empty;
                PlayManager._instance.StateSYN(reC);                    //交付给PlayMananger处理玩家操作状态
            }
            //-----------开牌------------------------------------
            else if ((int)reC[0] == RP_OPENCARD)
            {
                receiveStr = string.Empty;
                EM.OpenCard(reC);
            }
            //-----------游戏结束--------------------------------------------------
            else if ((int)reC[0] == RP_GAME_RESULT&& SceneManager.GetActiveScene().name == "Play")
            {
                receiveStr = string.Empty;
                EM.StartCoroutine("GameOver", reC);
            }

            //----------如果对方出卡-----------------------------------------------
            else if ((int)reC[0] == RP_U_PLAYCARD)
            {
                //普通出卡
                if ((int)reC[1] == (int)PlayManager.State.STATE_PLAY)
                {
                    EM.UPlayCard((int)reC[2]);
                }
                //出连击卡
                else if ((int)reC[1] == (int)PlayManager.State.STATE_COMBO)
                {
                    EM.UDoCombo((int)reC[2]);
                }
                //出破灭
                else if ((int)reC[1] == (int)PlayManager.State.STATE_BURST)
                {
                    EM.UDoBurst((int)reC[2]);
                }
                //放逐
                else if ((int)reC[1] == (int)PlayManager.State.STATE_EXILE)
                {
                    char[] c = new char[2];
                    c[0] = (char)RQ_PLAYCARD;
                    c[1] = (char)(PlayManager._instance.uCemePanel.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Card>().cardNumber % 100);
                    SendToServer(new string(c));
                }
                receiveStr = string.Empty;
            }
            }

        }
    


  /*  public void C_RPListen()
    {
        EM = EffectManager._instance;
        if (receiveStr != null && receiveStr.Length > 0 && receiveStr != string.Empty)
        {
            // Debug.Log("接受的消息不为空");
            char[] reC = receiveStr.Remove(0, 1).ToCharArray();
            //-----------处理特效消息------------------------------------------------
             if ((int)reC[0] == RP_EFFECT_PLAY)
            {
                receiveStr = string.Empty;                             //将信息置空
                                                                       //处理特效
                int player = (int)reC[1];                               //获取要播放效果的卡牌
                EM.EffectPlayCtr(reC);                                 //交付给特效控制器去执行特效播放
            }
            //-----------处理血量状态同步消息----------------------------------------
            else if ((int)reC[0] == RP_STATUS_SYN)
            {
                receiveStr = string.Empty;                              //将信息置空
                PlayManager._instance.StatusSYN(reC);                   //交付给PlayManager处理状态同步
            }
            //-----------处理操作状态同步消息-----------------------------------------
            else if ((int)reC[0] == RP_STATE_SYN)
            {
                receiveStr = string.Empty;
                PlayManager._instance.StateSYN(reC);                    //交付给PlayMananger处理玩家操作状态
            }
            //-----------开牌------------------------------------
            else if ((int)reC[0] == RQ_OPENCARD)
            {
                receiveStr = string.Empty;
                EM.OpenCard(reC);
            }
            //如果对方出卡
            else if ((int)reC[0] == RP_U_PLAYCARD)
            {
                //普通出卡
                if ((int)reC[1] == (int)PlayManager.State.STATE_PLAY)
                {
                    EM.UPlayCard((int)reC[2]);
                }
                //出连击卡
                else if ((int)reC[1] == (int)PlayManager.State.STATE_COMBO)
                {
                    EM.UDoCombo((int)reC[2]);
                }
                //出破灭
                else if ((int)reC[1] == (int)PlayManager.State.STATE_BURST)
                {
                    EM.UDoBurst((int)reC[2]);
                }
                //放逐
                else if ((int)reC[1] == (int)PlayManager.State.STATE_EXILE)
                {
                    char[] c = new char[2];
                    c[0] = (char)RQ_PLAYCARD;
                    c[1] = (char)(PlayManager._instance.uCemePanel.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Card>().cardNumber % 100);
                    SendToServer(new string(c));
                }
                receiveStr = string.Empty;
            }
        }
        
    int i = 0;
    }
    */
    public static void SendToServer(string send)
    {
        int length = send.Length + 1;
        string sendF = (char)length + send;
       // Debug.Log(sendF.Length);
        byte[] buffter = Encoding.UTF8.GetBytes(sendF);

     
        try {
            int temp = socketClient.Send(buffter);
            string c = "向服务器发送了：";
            for (int i = 0; i < sendF.Length; i++)
            {
                c += ((int)sendF[i]).ToString() + " ";
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e.Data);
        }
    }

    /// <summary>
    /// 延时发送操作状态同步请求
    /// </summary>
    public void SendStateDelay()
    {
            char[] send = new char[1];
            send[0] = (char)RQ_STATE_SYN;
            SendToServer(new string(send));
    }
}

