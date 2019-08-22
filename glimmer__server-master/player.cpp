
#include "vari.h"
#include "header.h"
#include "player.h"
#include "fight.h"
#include "tool.h"
#include <pthread.h>
//只包含用户的基本属性

extern pthread_mutex_t block_lock;     //用于块分配的锁

extern pthread_mutex_t fight_over_mutex; //战斗结束的锁

Player_base *player_base_block_get() {
    Player_base *cur;
    pthread_mutex_lock(&block_lock);     //加锁

    if (!player_base_block.empty()) {
        cur = player_base_block.front();
        player_base_block.pop();
    }
    else
        cur = new Player_base();
    pthread_mutex_unlock(&block_lock);     //解锁

    if(!cur)
        cout<<"Error player_base";
    return cur;
}
Player_fight *player_fight_block_get() {
    Player_fight *cur;
    
    pthread_mutex_lock(&block_lock);
    if (!player_fight_block.empty()) {
        cur = player_fight_block.front();
        player_fight_block.pop();
    }
    else
        cur = new Player_fight();
    pthread_mutex_unlock(&block_lock);

    if(!cur)
        cout<<"Error malloc"<<endl;
    return cur;
}

void recycle(Player_base * player_base) {
    pthread_mutex_lock(&block_lock);
    player_base_block.push(player_base);
    pthread_mutex_unlock(&block_lock);

}
void recycle(Player_fight *player_fight) {
    pthread_mutex_lock(&block_lock);
    player_fight_block.push(player_fight);
    pthread_mutex_unlock(&block_lock);

}
void Player_base::Player_base_initialize(int socket_user, char *message) {
	serial_num = count++;
	socket_num = socket_user;
	//message第一位标志，第二位是请求进入的战斗类型（暂只有0)，第三位是英雄，之后14位是玩家选择的卡牌
	hero_num = (unsigned char)message[2];
	for (int i = 0; i <Group_card_num; i++)
        choose_card[i] = card_index_convert_server((unsigned char)message[i+3]);
}
int Player_base::count = 0;

void Player_fight::Player_fight_initialize(const Player_base& play_base) {
	serial_num = play_base.serial_num;
	hero_num = play_base.hero_num;
	socket = play_base.socket_num;
	frozen_val = 0;
	blood_val = Full_Blood;
    been_waitted=false;
    picking_status=Still;
    
    handCard.clear();
    discard_card.clear();
    exile_card.clear();
    
    
    //等待抽取的卡牌组
	for (int i = 0; i < Group_card_num; i++) {
		wait_card[i] = play_base.choose_card[i];
	}
	wait_card_size = Group_card_num;

	//初始化用户手牌
	rand_vector(wait_card,wait_card_size);
    
    //  初始的抽取5张卡的操作
	for (int i = 0; i < Default_hand_card_num; i++) {
        
        handCard.insert(wait_card[--wait_card_size]);
	}
}

	//回血
void  Player_fight::blood_plus(int num) {
    //回血等
    cout<<endl<<"玩家";
    cout<<serial_num<<"回血前血量是";
    cout<<get_blood()<<", ";
	blood_val += num;
    cout<<endl<<"玩家";
    cout<<serial_num<<"回血后血量是"<<blood_val<<endl;
    
}
void Player_fight::blood_minus(int num) {
    cout<<"玩家受伤前血量";
    cout<<get_blood()<<", ";
	blood_val -= num;
    if(Debug_mode) {
        cout<<serial_num;
        cout<<"角色受伤后血量";
        cout<<get_blood()<<endl;
    }
}
void Player_fight::frozen_plus(int num) {
	frozen_val += num;
}
void Player_fight::frozen_minus(int num) {
	frozen_val -= num;
    if(frozen_val<0)
        frozen_val=0;
}

void Player_fight::process_dead(int fight_No) {
    pthread_mutex_lock(&fight_over_mutex);
    if(Debug_mode)
        cout<<"角色死亡，结束战斗"<<endl;

	bool success_index;
    sleep(1);
	if (serial_num == (serial_fight[fight_No]->player_num[0]))
		success_index = 1;
    else
        success_index = 0;

    pthread_mutex_unlock(&fight_over_mutex);
    
	fight_over(fight_No, success_index, 0);  //战斗正常结束
}
//检查用户手中是否有连击等级大于rank的牌
bool Player_fight::tap_ability(int tap_rank){
    cout<<"当前连击属性为"<<tap_rank<<endl;
    for(auto card_num:handCard)
        if( (CardAll[hero_num][card_num]->tap_hit) &&
           (CardAll[hero_num][card_num]->tap_hit_rank)>tap_rank)
            return true;
    return false;
}

