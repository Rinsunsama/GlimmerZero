//
//  fight.cpp
//  glimmer
//
//  Created by Apple on 2018/8/7.
//  Copyright © 2018年 Hypoxa. All rights reserved.
//
#include <iostream>
#include "communi.h"
#include "header.h"
#include "vari_defini.h"  
#include "fight.h"
#include "heart_beat.h"
#include "vari.h"
#include "card.h"
#include "player.h"
#include <algorithm>
#include <stdlib.h>
#include <iostream>
#include <sys/socket.h>
#include <pthread.h>

//using namespace std;
using std::cout;
using std::cin;
using std::endl;

pthread_t mutex;


//是否已有用户在等待，等待玩家的端口号
bool player_wait=false;
int wait_player_No;


int client[FD_SETSIZE];
fd_set allset;

pthread_mutex_t fd_clienti_lock;    //fd_clineti的锁
unordered_map<int,int> fd_clienti;  //记录fd和client中下标对应关系，方便关闭连接时进行置client

//处理和client端连接相关
void conn(){
    pthread_mutex_init(&fd_clienti_lock, NULL);
    
    int listenfd,connfd;
    char buf[MAX_SIZE]; //for read client's message
    int nready;
    ssize_t n;
    fd_set rset;
    socklen_t clilen;
    sockaddr_in servaddr,cliaddr;
    
    
    
    listenfd = socket(AF_INET,SOCK_STREAM,0);
    bzero(&servaddr,sizeof(servaddr));
    servaddr.sin_family=AF_INET;
    servaddr.sin_addr.s_addr=htonl(INADDR_ANY);
    servaddr.sin_port=htons(SERV_PORT);
    
    int optval=1;
    //避免used_address used error
    setsockopt(listenfd, SOL_SOCKET, SO_REUSEADDR, (void *)&optval, sizeof(int));

    if(bind(listenfd,(struct sockaddr *)&servaddr,sizeof(servaddr))==-1)
        cout<<"bind绑定出错，errno为"<<errno<<endl;

    listen(listenfd,5); //request to set as backlog
    int maxfd = listenfd;
    int maxi=-1;
    for(int i=0;i<FD_SETSIZE;i++)
    	client[i]=-1;
    FD_ZERO(&allset);
    FD_SET(listenfd,&allset);

    

    while(true){
        rset = allset;
        nready = select(maxfd + 1, &rset, NULL, NULL, NULL);
        if(nready<=0)
            continue;
        
        if(FD_ISSET(listenfd,&rset)){
            if(Debug_mode)
                cout<<"监听到一个新玩家的连接请求，将进行后续的处理"<<endl;
            
            clilen=sizeof(cliaddr);
            
            connfd=accept(listenfd, (struct sockaddr *)&cliaddr, &clilen);
            if(connfd<0) {              // 出错
                cout<<"accept失败，出错标识："<<errno<<endl;
                continue;
            }
            //adding to new_client function
            int i=0;
            for(i=0;i<FD_SETSIZE;i++) {
                if(client[i]<0){
                    client[i]=connfd;
                    
                    pthread_mutex_lock(&fd_clienti_lock);
                    fd_clienti[connfd]=i;
                    pthread_mutex_unlock(&fd_clienti_lock);

                    break;
                }
            }

            if(i==FD_SETSIZE) {
                cout<<"因为client数组支持数量不够，无法再相应此次用户请求，关闭连接端口"<<endl;
                close(connfd); //client不够，不响应此次的用户连接需求
                continue;
            }
            if(Debug_mode)
                cout<<"将该玩家纳入心跳检测，并且listen其后续的消息请求"<<endl;
            new_client_proc(connfd);
            

            FD_SET(connfd,&allset);//	Add new descriptor

            if(connfd>maxfd)
            	maxfd=connfd;
            if(i>maxi)
            	maxi=i;
            if(--nready<=0)
                continue; //no more readable description
        }
        
        //  Cope up Cwith readable description
        for(int i=0;i<=maxi;i++) {
            int sockfd;
            if((sockfd=client[i])<0)
                continue;
            if(FD_ISSET(sockfd,&rset)){
                if((n=readn(sockfd,buf,1))==0){               //玩家关闭连接
                    player_off_line(sockfd);
                } else {                                    //  玩家发送的消息
                    int t=((int)(unsigned char)(buf[0]));
                    t = ((t<=1)?0:t-1);
                    if(t>MAX_SIZE)
                        t=MAX_SIZE;             //防止爆出缓冲区的情况出现，应对客户端无效信息的处理
                    
                    readn(sockfd,buf,t);
                    process_message(sockfd, buf, t);                //判断请求类型，判断是心跳还是出牌等
                }
                if(--nready<=0)
                    break;
            }
        }
    }
}

//心跳包相关
void new_client_proc(int sockfd){
      
      
    //新的连接
    if (Debug_mode) {
        cout << "有新的链接建立，监听端口号为" << sockfd << endl;
    }
    
    pthread_rwlock_wrlock(&Conn_lock); //lock for conn_fd and fd_count
    Conn_fd.insert(sockfd);
    fd_count[sockfd]=0;
    pthread_rwlock_unlock(&Conn_lock);
}
//关闭一个端口,更改相应状态
void shutdown(int sockfd) {
    
    if(Debug_mode)
        cout<<"关闭一个玩家端口，该玩家端口号为: "<<sockfd<<endl;
    
    pthread_mutex_lock(&fd_clienti_lock);
    int i=fd_clienti[sockfd];
    client[i]=-1;
    fd_clienti.erase(sockfd);
    pthread_mutex_unlock(&fd_clienti_lock);

    FD_CLR(sockfd,&allset);
    close(sockfd);
    
    pthread_rwlock_wrlock(&Conn_lock); //lock for conn_fd and fd_count
    //心跳检测相关，删除相应端口
    Conn_fd.erase(sockfd);
    fd_count.erase(sockfd);
    pthread_rwlock_unlock(&Conn_lock); //lock for conn_fd and fd_count
    
}

void player_off_line(int sockfd){ //当用户关闭端口或者用户掉线,此由主线程或心跳线程处理
    if (Debug_mode) {
        cout << "远程客户端关闭了一个链接或者掉线" << endl;
    }
    shutdown(sockfd);
    
    pthread_rwlock_wrlock(&mapping_lock_shared); //为映射加写锁
    
    if(socket_PlayerNo.count(sockfd) > 0) {               //  该端口号有映射到一个玩家

        int player_num=socket_PlayerNo.at(sockfd);
        socket_PlayerNo.erase(sockfd);

        if(player_fight_map.count(player_num) == 0){      //  该端口号并没有映射到战斗，故该玩家此刻是在等待页面退出
            if (Debug_mode) {
                cout << "当前关闭连接的客户端玩家位于等待界面，开始处理收尾工作" << endl;
            }
            Player_base *cur_pb = serial_player_base_dict[player_num];
            
            socket_PlayerNo.erase(sockfd);
            serial_player_base_dict.erase(player_num);
            
            pthread_rwlock_unlock(&mapping_lock_shared);
            
            player_wait = false;   //该角色即位正在等待的端口
            recycle(cur_pb);
        }
        else {                          //     玩家正处于战斗中
            int fight_num = player_fight_map[player_num];
            Fight *cur_fight=serial_fight[fight_num];
            pthread_rwlock_unlock(&mapping_lock_shared);

            if (Debug_mode) {
                cout << "当前关闭连接的客户端玩家正处于战斗状态，开始处理收尾工作" << endl;
            }
            int self_index;
            if(cur_fight->player_num[0]==player_num)
                self_index = 0;
            else
                self_index = 1;
            fight_over(fight_num, !self_index, 1);
        }
    }
}


//匹配信息错误
bool valid_match_message(char *buf,size_t len){
    if((len != Group_card_num+3) || (buf[1] != 0) || (buf[2] < 0)  || (buf[2] > 1))
        return false;
    unordered_set<int> cards;
    for(int i = 3; i < len; i++){
        if((cards.find(buf[i]) != cards.end()) ||
           buf[i] < 0 || (buf[i] > 20 && ((buf[i]<53) || (buf[i]>55))))
            return false;
    }
    return true;
}
//   处理消息
void process_message(int sockfd,char* buf,size_t len) {
    
    if (len < 1) {
        //空报文，不作处理
        return;
    }
    if(Debug_mode && buf[0]) { //屏蔽心跳包
        cout << "收到远程客户端"<<sockfd<<"发来的消息，长度为 "<<len<<", 此为远程客户端发来的消息内容";
        for(int i = 0; i < len; i++) {
            cout << (int)buf[i] << ' ';
        }
        cout << endl;
    }
    if(buf[0] == 0) { //心跳
        recv_heartbeat(sockfd);
        return;
    }
    
    if(buf[0] == 1) { //请求匹配
        pthread_rwlock_rdlock(&mapping_lock_shared);
        if(socket_PlayerNo.find(sockfd)!=socket_PlayerNo.end()) {
            if(Debug_mode)
                cout<<"玩家已在匹配队列，或在战斗中，故此次匹配失败"<<endl;
            pthread_rwlock_unlock(&mapping_lock_shared);
            return;
        }
        pthread_rwlock_unlock(&mapping_lock_shared);
        
        char wannna_match_res[3];
        wannna_match_res[0]=3;
        wannna_match_res[1]=2;
        
        if(!valid_match_message(buf,len)) {
            wannna_match_res[2]=1;     //  进入匹配队列失败
            wwriten(sockfd, wannna_match_res, 3);
            return; //用户信息格式不符合要求，不响应
        }
        else {
            wannna_match_res[2]=0;     //  进入匹配队列成功
            wwriten(sockfd, wannna_match_res, 3);
        }
        
        Player_base *player_come = player_base_block_get();
        
        if(!player_come)
            cout<<"Error player_come";
        player_come->Player_base_initialize(sockfd, buf);
        
        pthread_rwlock_wrlock(&mapping_lock_shared);
        socket_PlayerNo[sockfd] = player_come->serial_num;  //insert socket_playerNO map
        serial_player_base_dict[(player_come->serial_num)]=player_come; //  写入映射中
        pthread_rwlock_unlock(&mapping_lock_shared);

        //拆分用户卡牌信息
        
        //check if a player is waitting
        if(player_wait){
            //匹配
           // pthread_t pid;
            if(Debug_mode)
                cout << "玩家匹配成功" << endl;
            player_wait=false;
            create_fight(wait_player_No, player_come->serial_num);

        }
        else{
            player_wait=true;
            wait_player_No=player_come->serial_num;
            
            char wait_match[3];  // 向客户回送等待匹配状态
            wait_match[0]=3;
            wait_match[1]=3;
            wait_match[2] = 1;
            if(Debug_mode)
                cout << "当前没有等待玩家，当前玩家需要等待匹配" << endl;
            write(sockfd,wait_match, 3);
        }
    }
    else if(buf[0] == 4 && len > 2){ // 出牌
        if(Debug_mode)
            cout<<"收到玩家出牌"<<endl;
        
        pthread_rwlock_rdlock(&mapping_lock_shared);
        if(socket_PlayerNo.find(sockfd) == socket_PlayerNo.end()) {//    玩家并不在玩家列表中
            pthread_rwlock_unlock(&mapping_lock_shared);
            return;
        }
        
        int cur_player_No = socket_PlayerNo[sockfd];
        
        if((serial_player_dict.find(cur_player_No) != serial_player_dict.end())
           && (card_index_convert_server(buf[2]) >= 0)
           &&serial_player_dict[cur_player_No]->picking_status >= 1
           &&serial_player_dict[cur_player_No]->picking_status <= 4)  { //  牌数值有效，玩家在战斗列表中且其为等待发牌状态
            
            pthread_rwlock_unlock(&mapping_lock_shared); // 调用函数 ，在此之前解除锁
            
            process_pick_card(cur_player_No,buf[2]);
        }
        else
            pthread_rwlock_unlock(&mapping_lock_shared);
    }
    else if(buf[0]==8) {    // 玩家投降
        pthread_rwlock_rdlock(&mapping_lock_shared);
        if(socket_PlayerNo.find(sockfd) == socket_PlayerNo.end()) {      //  玩家并不在玩家列表中
            pthread_rwlock_unlock(&mapping_lock_shared);
            return;
        }
        int cur_player_No = socket_PlayerNo[sockfd];
        if(serial_player_dict.find(cur_player_No) != serial_player_dict.end()) {   //玩家在战斗列表中
            int fight_No = player_fight_map[cur_player_No];
            int opposite_index;  //对手在fight中的下标
            if(serial_fight[fight_No]->player_num[0] == cur_player_No){
                opposite_index = 1;
            }
            else
                opposite_index = 0;
            pthread_rwlock_unlock(&mapping_lock_shared);
            
            fight_over(fight_No, opposite_index, 2);    // 该场战斗，玩家对手胜
        }
        else
            pthread_rwlock_unlock(&mapping_lock_shared);

    }
    else if(buf[0] == 7)  {    //玩家战斗处理完毕，已经ready下一个状态的置位
        pthread_rwlock_rdlock(&mapping_lock_shared);
        
        if(socket_PlayerNo.find(sockfd) == socket_PlayerNo.end()) {//玩家并不在玩家列表中
            pthread_rwlock_unlock(&mapping_lock_shared);
            return;
        }
        int cur_playerNo = socket_PlayerNo[sockfd];
        if((serial_player_dict.find(cur_playerNo) != serial_player_dict.end())
           && serial_player_dict[cur_playerNo]->picking_status == 5) { //玩家正在编号列表中且正在等待7的包
            serial_player_dict[cur_playerNo]->picking_status = 0;
            serial_player_dict[cur_playerNo]->been_waitted=true;
        }
        pthread_rwlock_unlock(&mapping_lock_shared);

    }
}


    


//处理客户端的标识为2的请求
void process_pick_card(int cur_player_No,int CardNo) {
    //  出牌者编号，牌编号
    if(Debug_mode) {
        cout<<"process_pick_card:::"<<"开始处理玩家的出牌请求信息"<<endl;
        cout<<"玩家编号为"<<cur_player_No<<" ";
        cout<<"卡牌编号为"<<CardNo<<endl;
    }
    pthread_rwlock_rdlock(&mapping_lock_shared);

    Player_fight *cur_player_fight=serial_player_dict[cur_player_No];           //     玩家对象
    Fight *cur_fight = serial_fight[player_fight_map[cur_player_No]];     // 战斗对象

    int self_index = 1;
    if(cur_fight->player_num[0]==cur_player_No)
        self_index = 0;
    Player_fight *oppo_player_fight=(cur_fight->player_fight[!self_index]);   //  对方玩家
    
    pthread_rwlock_unlock(&mapping_lock_shared);

    CardNo = card_index_convert_server(CardNo);
  //  cout<<serial_player_dict[cur_player_No]->picking_status<<endl;

    
    switch(cur_player_fight->picking_status){
        case Normal_Pick: {//    正等待玩家正常出牌
            if((cur_player_fight->handCard.count(CardNo) <= 0 && (CardNo <= 20 || CardNo > 23))
               || ((cur_player_No == EnergyCardNum && cur_fight->cur_round.energy_ability_forbid[self_index]))
               || ((cur_player_No ==PerryCardNum  && cur_fight->cur_round.energy_ability_forbid[self_index]))) {
                if(Debug_mode)
                    cout<<"玩家"<<self_index<<"出牌无效"<<endl;
                return; //牌无效
            }
            break;
        }
        case Tapping:  {   //  正等待玩家连击
            if (CardNo == EmptyCardNum)   {    //   玩家不连击
                cur_player_fight->if_tap = false;
                if(Debug_mode)
                    cout<<"玩家出了空牌，即不连击"<<endl;
            }
            else {
                if(CardAll[cur_player_fight->hero_num][CardNo]->tap_hit &&
                   CardAll[cur_player_fight->hero_num][CardNo]->tap_hit_rank
                   >cur_player_fight->old_tap_rank)     //  牌有效
                    cur_player_fight->if_tap = true;
                else {
                    if(Debug_mode)
                        cout<<"玩家选择的连击牌出错"<<endl;
                    return; //连击牌不符合要求
                }
            }
            break;
        }
        case Shater: {       // 正等待玩家破灭
            if(CardNo==25)
                cur_player_fight->if_shater = false;
            else {
                if(cur_player_fight->handCard.count(CardNo) > 0) //破灭有效
                    cur_player_fight->if_shater = true;
                else {       //  牌无效
                    if(Debug_mode)
                        cout<<"玩家破灭的牌不符合要求，破灭无效"<<endl;
                    return;
                }
            }
            break;
        }
        case Exile: {
            if(oppo_player_fight->discard_card.count(CardNo) > 0) {    //放逐有效
                cur_player_fight->if_exile = true;        //牌有效
            } else {
                return;     //牌无效
            }
            break;
        }
        default:
            return;
    }
    cout<<"玩家出牌有效,玩家出的卡牌编号为"<<CardNo<<endl;
    cur_player_fight->picking_status = Still;
    cur_player_fight->pick_card_num = CardNo;
    cur_player_fight->been_waitted = true;
}


