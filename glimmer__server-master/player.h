#ifndef _player_h
#define _player_h

#include "header.h"


//只包含用户的基本属性
class Player_base {
public:
	static int count;

	int serial_num;
	int socket_num;
	int hero_num;
	int choose_card[Group_card_num];

	Player_base() {

	}
	void Player_base_initialize(int socket_user, char *message);
};
//

//用户战斗属性
class Player_fight {
private:
	int frozen_val; //自身霜冻值
	int blood_val;  //自身血量 xx
public:
	int serial_num;		//用户的序号
	int hero_num;       //  该玩家选定的英雄
	int socket;         //  该玩家的连接端口


	 //卡牌状态相关
    unordered_set<int> handCard;       //  用户手牌

	int wait_card[Group_card_num];	//用户待抽取的卡牌组
	int wait_card_size = 0;	//卡牌组剩余数目

	unordered_set<int> discard_card;	//弃牌区
	vector<int> exile_card;				//放逐区
public:
    int picking_status;         // 0表示玩家此时不处于出牌状态，1表示玩家处于等待玩家正常出牌的状态
                                // 2表示玩家处于选取连击牌的状态，3表示玩家处于选取放逐牌的状态
                                // 4表示玩家处于选取破灭牌的状态，5表示处于等待玩家发送特效结束状态
    
	bool been_waitted;	        //  是否等到了玩家所发送的消息，2或者7消息
	int pick_card_num;			//  如果是该消息是出牌，那么该消息中玩家所选的牌

	Player_fight() {};
	void Player_fight_initialize(const Player_base& play_base);
	//关于破灭
    bool if_shater;         //用户是否要破灭
	int waitting_shater_num;	//待选取的破灭牌的数量，only valid when waitting_shater valid
    

	//关于卡组重置和放逐
    bool if_exile;          //用户是否要放逐
    bool if_tap;            //用户是否连击
    int  old_tap_rank;        //等待选取的连击牌的连击属性至少应大于此值
    
    
//	bool exile_choose_ok;	//用户选好了放逐的对方的卡牌
//	int exile_choose_num;	//用户选取的放逐的对方卡牌的编号

	//关于连击

	//bool tap_choose_ok;		//用户是否要连击

//	vector<int> tap_choose_card;		//用户选取的valid连击牌的编号

	const int get_frozen_val() {
		return frozen_val;
	}
	const int get_blood() {
		return blood_val;
	}
	//回血
	void blood_plus(int num);
	void blood_minus(int num);
	void frozen_plus(int num);
	void frozen_minus(int num);
	void process_dead(int fightNo);
    
    //检查用户手中是否有连击等级大于rank的牌
    bool tap_ability(int tap_rank);
            
};

Player_base *player_base_block_get();
Player_fight *player_fight_block_get();


void recycle(Player_base* player_base);
void recycle(Player_fight *player_fight);
#endif
