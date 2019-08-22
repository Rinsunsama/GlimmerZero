#include "card.h"


pthread_rwlock_t mapping_lock_shared;       //管理各种编号映射的共享锁

unordered_map<int, int> player_fight_map;  //玩家 map to fight
unordered_map<int, Fight*> serial_fight; //fight编号映射
unordered_map<int, Player_base*> serial_player_base_dict;	//玩家编号映射
unordered_map<int, Player_fight*> serial_player_dict;		//玩家战斗属性映射

unordered_map<int, int> socket_PlayerNo;    //端口号和玩家序号映射


bool Debug_mode = true;

Card *CardAll[2][Hero_max_card];		//const Card数组

unordered_map<int, pthread_t>	fight_tid;	//战斗编号和线程的映射  //Tag_unix


queue<Player_base *> player_base_block;
queue<Player_fight *>player_fight_block;
Player_base *player_base_block_get();
Player_fight *player_fight_block_get();



