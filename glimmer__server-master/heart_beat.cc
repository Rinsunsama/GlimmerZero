#include "heart_beat.h"
#include "communi.h"
#include "vari_defini.h"
#include "communi.h"
#include "vari.h"
#include "header.h"

#include <pthread.h>
#include <stdlib.h>
#include <unistd.h>

#include <iostream>


char heart_message[2];

//  心跳检测，每一秒向所有连接端口发送心跳包
/*
 *  如若连续十次均未收到心跳回复，关闭玩家连接，处理玩家下线逻辑，否则继续发送心跳包
 *  Param int fd    对应客户端连接端口
 */
void *Timer_count(void *){
  //  signal(SIGPIPE,SIG_IGN);

    heart_message[0]=2;
    heart_message[1]=0;
    if(Debug_mode)
        cout<<"心跳检测启动"<<endl;
    while(true) {
        
        //用一个vector保存失效了的连接端口，一轮结束后，集中处理
        vector<int> play_offline_connfd;    //掉线的连接套接字
        pthread_rwlock_rdlock(&Conn_lock);
        for(auto fd:Conn_fd){
            if((fd_count.count(fd)>0) && (fd_count[fd]++>10)) {    //超时未回复心跳包，关闭端口
                if(Debug_mode)
                    cout<<"玩家超时未回复心跳包，关闭该用户连接，该连接端口号为："<<fd<<endl;
                play_offline_connfd.push_back(fd);
            }
            else
                write(fd, heart_message, 2);
        }
        pthread_rwlock_unlock(&Conn_lock);
        
        //处理掉线玩家
        if(!play_offline_connfd.empty()) {
            pthread_rwlock_wrlock(&Conn_lock);
            for(auto fd:play_offline_connfd)
                player_off_line(fd);
            pthread_rwlock_unlock(&Conn_lock);
        }
        sleep(1);
    }
    return NULL;
}


/*  接收心跳包，并重置相关状态位
 *  Param int fd    对应客户端连接端口
 */
void recv_heartbeat(int fd){
        pthread_rwlock_rdlock(&Conn_lock);
		if(fd_count.find(fd)!=fd_count.end()) //	该玩家可能并不在等待序列中
    		fd_count[fd]=0;
      //  if(Debug_mode)
        //    cout<<"收到心跳包"<<endl;
        pthread_rwlock_unlock(&Conn_lock);
}
