#ifndef headear_h
#define headear_h

#include <stdlib.h>
#include <iostream>
#include <vector>
#include <unordered_set>
#include <unordered_map>
#include <stdlib.h>
#include <queue>

#include <sys/socket.h>
#include <unistd.h>
#include<netinet/in.h>
#include<string.h>
#include<stdio.h>
#include <unordered_set>
#include <unordered_map>
#include<mutex>
#include<thread>
#include<list>
#include<sys/select.h>
#include<sys/time.h>

#include "write.h"

using std::cin;
using std::cout;
using std::vector;
using std::unordered_map;
using std::unordered_set;
using std::endl;
using std::queue;

constexpr int Hero_max_card = 24;  //玩家一共有24张牌可以选择(包含按钮）
constexpr int Max_hand_card = 10;	//玩家一共最多10张手牌
constexpr int Group_card_num = 14;	//一个卡组中有多少张牌
constexpr int Default_hand_card_num = 5;	//默认玩家最开始拥有的手牌数量

constexpr int Max_true_card = 21;		//除却按钮，玩家一共有21种可能拥有的卡牌
constexpr int Max_Waitting_time = 40;		//最长等待时间
constexpr int EmptyCardNum = 23;			//空牌编号
constexpr int PerryCardNum = 22;			//格挡牌的编号
constexpr int EnergyCardNum=21;             //蓄能卡

constexpr int Full_Blood = 20;        //
											
enum TYPE { PRE, FOLLOW, TAPPING };	//分别表示先手，后手和连击的状态
enum PICK_STAT{Still,Normal_Pick,Tapping,Shater,Exile,Ready};  //期望的人物状态

//num Player_status{Still,Pick_normal,Tap_wait,Shater,Exile}; //向客户端发送的人物状态

//全局常量
const int MAX_SIZE = 100;
const int SERV_PORT = 13240;


//处理读写相关
ssize_t readn(int fd, void *vptr, size_t n); //read函数包装
ssize_t wwriten(int,const char*,size_t);//write函数包装

void proc_off_line(int fd); //该连接断线
void new_client_proc(int sockfd);//处理新用户的连接


//on conn.c
void listen_client();


struct Integer2{
    int num1;
    int num2;
};
#endif

