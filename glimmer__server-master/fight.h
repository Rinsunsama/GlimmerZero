#ifndef _fight_h
#define _fight_h

#include "header.h"
#include "player.h"

class Card;

void create_fight(int num1,int num2);

void *start_fight(void *fight);
void fight_over(int fight_num, int success_num, int type);
//  处理用户的一次出牌，可能是连击、破灭等
void process_pick_card(int cur_player_No,int cardNo);

struct Round {
	bool frozen[2];                                 //  [0] p1 冻结，[1] p2 冻结
	bool dizz[2];                                   //  [0] p1 眩晕，[1] p2 眩晕
	bool energy_ability_forbid[2];                  //  p0,p1是否格挡被禁
	bool perry_ability_forbid[2];	                //  p0,p1是否蓄能被禁
};

//  每一次出牌后客户端需要表现出的效果
struct Present_effect{
    bool presentNeed;                                //  是否有效果需要表现
    
    bool if_damage;                                 //  是否对该卡对方造成伤害
    int damage_val;                                 //  造成伤害的伤害数值
    
    bool blood_recov;                               //  是否出卡方回血
    int blood_recov_val;                            //  出卡方回血量
    
    bool interruption;                              //  是否具备打断效果
    bool dizz;                                      //  是否使对方眩晕
    bool frozing;                                   //  是否冻住该卡对方
    
    bool frozen_modify;                             //  是否变更该卡敌方霜冻值
    int frozen_val_modify;                          //  霜冻值的变更值
    
    bool lock_Adding_block;                         //  是否封锁该卡敌方的蓄能和盾牌
    
    bool AddingCard;                                //  是否蓄能
    bool blocking;                                  //  是否防御
    
    bool card_recycle;                              //  本张卡牌是否回收
    
    bool self_damage;                               //  该卡是否造成出卡方自损
    int self_damage_val;                            //  自损数值
    
    bool activated_damage_reflect;                  //  激活伤害反制
    int damage_reflect_val;                         //  激活的伤害反制的数值
    
    bool opposite_card_add_blood;                    //  激活对方卡的回血
    int opposite_card_add_blood_val;                 //  激活的对方卡回血的数值
    bool frozen_self;                                //  激活己方的冻结
    
};
struct Status {
	int damage_pre_follow[2];			            //  先手和后手的伤害量，用于计算当卡牌1为217牌时

	bool interruption_pre;				            //  先手的打断效果
    
	bool damage_reflect_pre;			            //  先手的伤害反制
    int damage_reflect_cardNo;                      //  激活先手伤害反制的卡牌编号
  
    
	bool damage_to_blood_pre;			            //  所受伤害变成回血量
    int damage_to_blood_pre_card;                   //  具伤害变回血属性的卡牌编号
    
	float block_rate_pre;				            //  先手的防御率

	bool min_blood_pre;					            //  先手的最小血量保护
	int min_blood_pre_val;				            //  先手的最小血量数值
	int min_blood_pre_hurt_multiple;	            //  先手血量保护后回血记为伤害的倍数
	int min_blood_pre_hurt_num;			            //  触发最小血量保护后的伤害值
    
    bool blood_recover;                             //  回合结束回血
    int blood_recover_roundEnd;                     //  回合结束回血血量
    int blood_recover_roundEnd_cardNo;              //  激活回合结束回血的卡牌编号

    //  以上都是only 先手有效
    
	bool block_pre;						            //  先手此次是否是格挡
	bool adding_card;					            //  先手此次是否是蓄能
    
	bool shater_pre;						        //  先手破灭属性activated,需要后手丢弃卡牌
	int shater_pre_num;					            //  valid only when shater_pre valid

	bool shater_follow;					            //  后手破灭属性activated，需要先手丢弃卡牌
	int shater_follow_num;				            //  valid only when shater_follow valid

	bool press_pre;						            //  先手压制属性activated
	bool press_follow;					            //  后手的压制属性activated

	int pre_opposite = true;

	bool tap_hit_ablity;				            //  先手可以再连击其他牌
	int tap_hit_rank;					            //  该回合目前最后连击牌的连击属性，only valid when tap_hit_ability valid

	bool tap_blood_recov;
	int tap_blood_recov_val;			            //  若有连击，附加的伤害值，因为可能先前的连击牌具备此种属性,only valid when tap_hit valid
										            //  可加在while循环中

	bool dead_protect;					            //  先手是否具备死亡保护，即若可使角色死亡，该伤害不生效，并且为角色加血，only pre player valid
	int dead_protect_bonus;				            //  触发死亡保护后，角色血量增益 only valid when dead_protect true
	bool dead_protect_frozen;			            //  触发死亡保护后，后手是否冻住？ only valid when dead_protect true

    Present_effect card_effect_present;	            //  当前卡牌所触发的效果
    
    bool damage207_follow;                             //  后手207卡牌时，伤害是否已计算过
    int damage207_follow_val;                           //  后手207卡牌时，伤害已计算过，其数值
};



/*
 *  用于在对战中储存对下一局置的状态。在每局出牌时依据这些状态判定p1,p2的牌有效与否
 */
class Fight {
public:
	static int count;
	Fight() {};
	void fight_initialize(int p1_num, int p2_num);
    void send_initia_status(Player_fight &self,int opposite_hero_num);
	void fight_round(int card1_num, int card2_num);
	void fight_process();
	//  void fight_round(int card1_num, int card2_num);

	void Process_shater(int player_index, int shater_num);

	bool Process_tap_hit(int player_index, vector<int>&card_num, int tap_hit_rank_old);
	bool Check_hit_ability(int player_index, int tap_hit_rank_old);
	//  玩家蓄能
	void Adding_card(int player_index);
	void Exile_card(int player_index);
    
   //   计算先手玩家
    int ComputePreIndex(Card &card1,Card &card2,Player_fight &p1,Player_fight &p2,bool *press);
    
    /*
     *  向玩家发送各种信息等，与客户端进行交互
     */
    
    //  向双方玩家发送编号为9的消息，即开牌消息
    void Sending9_CardShow(bool press,int card1_num,int card2_num);
    
    //  向玩家发送编号为10的消息，并作等待，即等待客户端ready
    void WaitClientReady10();
    
    //  向玩家发送一串编号为11的消息，即展示相关效果
    void PresentingEffectSend11(Present_effect &presen,int self_index,Card &card);

    //等待下标为pre_index用户选取大于rank的连击牌
    int Choose_tap_card(int pre_index);
    
    //   处理玩家的自身回血效果
    void Process_blood_recv3(int self_index,int adding_val,Card& card);
    
    //发送编号为5的消息，即对方出卡的消息
    void SendingPickCard5(int self_index,int stat,int num);
    //  玩家抽卡
    void DrawCards(int self_index,int card_num);
    
    //  发送6类型的信息，即向玩家发送状态同步信息，
    void SendPickStat6(int self_index,int self_stat,int oppo_stat);
    
    //向双方玩家发送11类型消息，即卡牌实际效果，也即发送一串编号为3的消息
    void PresentingEffectSend11(Present_effect &presen,int self_index,int card_num);
    //  发送编号为11的抽卡的卡牌效果
    void SendingEffect11_draw_card(int self_index,int card_num,vector<int> &cards_num);
    
    //向双方玩家发送类型为12的buf同步信息
    void SendingBuf12(Round &cur_round);
    
    //  将该张卡从手牌中删除并放入坟墓
    void Process_used_card(int player_index,int card_num);
    
	bool fight_running;
	int fight_num;		                            //  战斗的编号
	int player_num[2];	                            //  p1的编号，p2的编号
	//Player_fight player[2];                       //  player[0]是p1,player[1]是p2
	Player_fight *player_fight[2];	                //  双方的player_fight指针
    void pick_normal_card();                        //  双方正常出牌
    
    //  发送效果11，向客户端发送播放效果相关
    void SendingEffect11(int self_index,int cardNum,int effectNum,int val);
    void Sending9_CardShow(int pre_index,int *card_num,bool press);
    
    void SendingDefaultShater14(int pre_index,int card_num);

	Round cur_round;                                //当前待开启回合的回合状态

	//对手将该player的卡放逐一张

};

#endif
