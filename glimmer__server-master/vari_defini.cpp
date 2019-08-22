//
//  vari_defini.cpp
//  glimmer
//
//  Created by Apple on 2018/8/7.
//  Copyright © 2018年 Hypoxa. All rights reserved.
//
//各文件共享变量的定义
#include <unordered_map>
#include <unordered_set>
#include <pthread.h>
#include "header.h"
//using namespace std;
unordered_map<int,int> fd_count; //用于定时器
unordered_set<int> Conn_fd;     //用于心跳检测时遍历


pthread_rwlock_t Conn_lock; //为Conn_fd和fd_count加锁
pthread_rwlock_t fd_count_lock; //fd_count的读写锁

