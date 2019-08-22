//
//  vari_defini.hpp
//  glimmer
//
//  Created by Apple on 2018/8/7.
//  Copyright © 2018年 Hypoxa. All rights reserved.
//

//各文件共享变量的声明
#ifndef vari_defini_hpp
#define vari_defini_hpp

#include <unordered_map>
#include <unordered_set>
#include <pthread.h>
//using namespace std;

using std::unordered_map;
using std::unordered_set;
extern unordered_map<int,int> fd_count;
extern unordered_set<int> Conn_fd;

extern pthread_rwlock_t Conn_lock; //conn的锁

#endif /* vari_defini_hpp */
