#include <pthread.h>
#include "heart_beat.h"
#include "vari_defini.h"
#include "communi.h"
#include "vari.h"
#include "unistd.h"
#include "signal.h"
#include "card.h"

#include <iostream>

//using namespace std;

extern pthread_mutex_t fight_over_mutex;
extern pthread_mutex_t block_lock;     //用于块分配的锁

int main() {
    Initiate_card();                                    //  初始化卡牌数组
    srand((int)time(0));                                     //  随机化种子
    
    //初始化相关锁
    pthread_rwlock_init(&Conn_lock, NULL);

    pthread_mutex_init(&fight_over_mutex, NULL);
    pthread_mutex_init(&block_lock,NULL);
    pthread_rwlock_init(&mapping_lock_shared,NULL);
    

    //阻止SIGPIPE信号
    sigset_t set;
    sigemptyset(&set);
    sigaddset(&set,SIGPIPE);
    sigprocmask(SIG_BLOCK, &set, NULL);
    
    pthread_t tid;
    pthread_create(&tid,NULL,Timer_count,NULL);         //  启动定时器，心跳函数

    
    cout<<"main start"<<endl;
    conn();                                             //  陷入conn的infinite loop中
    
    Delete_card();                                      // 卡牌对象资源的回收
    return 0;
}
