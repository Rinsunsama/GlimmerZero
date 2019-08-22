
#ifndef _vari_h
#define _vari_h

#include "card.h"


//Fight *fight[12][14];

extern unordered_map<int, int> player_fight_map;  //玩家 map to fight
extern unordered_map<int, Fight*> serial_fight; //fight编号映射

extern unordered_map<int, Player_base*> serial_player_base_dict;	//玩家编号映射
extern unordered_map<int, Player_fight*> serial_player_dict;		//玩家战斗属性映射
extern unordered_map<int, int> socket_PlayerNo;    //端口号和玩家序号映射


extern Card *CardAll[2][Hero_max_card];
extern bool Debug_mode;

extern unordered_map<int, pthread_t>	fight_tid;


extern queue<Player_base *> player_base_block;
extern queue<Player_fight *>player_fight_block;
extern Player_base *player_base_block_get();
extern Player_fight *player_fight_block_get();


extern pthread_rwlock_t mapping_lock_shared;       //管理各种编号等映射的共享锁

//战斗编号和战斗线程号的映射
 
#endif
