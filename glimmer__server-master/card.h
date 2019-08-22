#ifndef _card_h
#define _card_h

#include "fight.h"
#include "header.h"
#include "player.h"
#include "tool.h"
extern bool Debug_mode;


void Initiate_card();
void Delete_card();

//将牌编号从客户端格式转换为服务端格式
int card_index_convert_server(int index);
int card_index_convert_client(int index);



class Card {
public:
	bool tap_hit; //是否具备连击属性
	int tap_hit_rank; //连击阶层，only valid when tap_hit valid
	int rank;	//卡牌等级
	int damage_val;	//卡牌伤害
	int speed_delay;		//卡牌基本速度

	Card();
	const virtual void take_effect(TYPE type, Round &cur_round, Round &next_round,
		Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        ;
    };
    
	const void defensive_effect(Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index);
    
	const virtual int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		return damage_val;
	}
    
	const virtual int get_speed_delay(Card &opposite_card, Player_fight &self, Player_fight &opposite, bool &press) {
		return speed_delay;
	}
};
class Card050 :public Card {
public:
	Card050() {
		speed_delay = 1000;
        rank=0;
	}
};

class Card051 :public Card {
public:
	Card051() {
		speed_delay = -500;
        rank=0;
	}
	const virtual void take_effect(TYPE type, Round &cur_round, Round &next_round,
                                   Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        if(Debug_mode)
            cout<<"Card51 take effect"<<endl;
        cur_status.card_effect_present.AddingCard=true;
		cur_status.adding_card = true;
	}
};
class Card052:public Card { 
public:
	Card052() {
		speed_delay = -500;
        rank=0;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        if(Debug_mode)
            cout<<"Card51 take effect"<<endl;
        cur_status.card_effect_present.blocking=true;
		cur_status.block_pre = true;
		cur_status.block_rate_pre = 0.5;
	}
};
class Card053 :public Card {
public:
	Card053() {
		damage_val = 2;
		rank = 2;
		speed_delay = 100 * rank + 10 * 2;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
        
        if (type == PRE) {
			cur_status.interruption_pre = true;
            cur_status.card_effect_present.interruption=true;
            
            cur_status.card_effect_present.damage_val=damage_val;
		}
		else
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
	}
};
class Card055 :public Card {
public:
	Card055() {
		rank = 3;
		speed_delay = 100 * rank + 2 * 10;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		//self.blood_plus(5);
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.blood_recov=true;
        cur_status.card_effect_present.blood_recov_val=5;
	}
};

class Card100 :public Card {
public:
	Card100() {
		rank = 1;
		speed_delay = rank * 100;
        damage_val=1;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
        if(type==PRE)
            cur_status.card_effect_present.damage_val=damage_val;
        else
            Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
        
        cur_status.card_effect_present.frozen_modify=true;
        cur_status.card_effect_present.frozen_val_modify=1;
	}
	const int get_speed_delay(Card &opposite_card, Player_fight &self, Player_fight &opposite,bool &press) {
        if (opposite_card.rank == 1) {
            press = true;
			return -100;
		}
		return speed_delay;
	}
};
class Card102 :public Card {
public:
	Card102() {
		rank = 1;
		speed_delay = rank * 100-10*1;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.frozen_modify=true;
        cur_status.card_effect_present.frozen_val_modify=3;
	}
};

class Card103 :public Card {
public:
	Card103() {
		rank = 4;
		speed_delay = rank * 100;
        damage_val=2;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
		//  计算伤害
		int damage = get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);
		if (type == PRE || type == TAPPING) {
            cur_status.card_effect_present.if_damage=true;
            cur_status.card_effect_present.damage_val=damage;
			if (cur_status.press_pre && (!cur_round.frozen[!self_index])) {
				//  冻结效果
                next_round.frozen[!self_index]=true;
                cur_status.card_effect_present.frozing=true;
			}
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
			if (cur_status.press_follow) {
                next_round.frozen[!self_index]=true;
                cur_status.card_effect_present.frozing=true;
			}
		}

	}
	const int get_speed_delay(Card &opposite_card, Player_fight &self, Player_fight &opposite,bool &press) {
		if ((opposite_card.rank == 2) || (opposite_card.rank == 3)) {
			press = true;
			return -200;
		}
		return speed_delay;
	}
};
class Card104 :public Card {
public:
	Card104() {
		rank = 2;
		tap_hit = true;
		tap_hit_rank = 1;
		damage_val = 1;
		speed_delay = rank * 100 - 2 * 10;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, 
		Player_fight &self, Player_fight &opposite, bool self_index) {
        cur_status.card_effect_present.presentNeed=true;
        
        cur_status.card_effect_present.if_damage=true;
 
        
        cur_status.card_effect_present.blood_recov=true;
        cur_status.card_effect_present.blood_recov_val=1;
        
        cur_status.card_effect_present.frozen_modify=true;
        cur_status.card_effect_present.frozen_val_modify=1;
        
		if (type == PRE  || type==TAPPING) { //先手打出
			cur_status.tap_hit_ablity = true;
			cur_status.tap_hit_rank = 1;
            
            cur_status.tap_blood_recov=true;
            cur_status.tap_blood_recov_val=1;
            
            cur_status.card_effect_present.damage_val=1;
		}
		else { //因为为连击1，所以只可能为先手打出或者后手打出
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite,self_index);
		}



	}
};

class Card106 :public Card {
public:
	Card106() {
		rank = 2;
		damage_val = 1;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {

        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
        cur_status.card_effect_present.frozen_modify=true;
        
		//计算伤害
		if (type == PRE || type==TAPPING) {
            cur_status.card_effect_present.damage_val=damage_val;
		}
		else {	//后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
		}
		if (opposite.get_frozen_val() >= 5) {
            if(!cur_round.frozen[!self_index]) {
                cur_status.card_effect_present.frozing=true;
                next_round.frozen[!self_index]=true;
            }
            cur_status.card_effect_present.frozen_val_modify=-2;
            
		}
        else {
			//opposite.frozen_plus(2);
            cur_status.card_effect_present.frozen_val_modify=2;
        }
	}
};
class Card107 :public Card {
public:
	Card107() {
		rank = 3;
		damage_val = 3;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
        cur_status.card_effect_present.frozen_modify=true;
        
		//  计算伤害
		if (type == PRE) {
            cur_status.card_effect_present.damage_val=damage_val;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
		}
        cur_status.card_effect_present.frozen_val_modify=1;
        if ((!cur_round.frozen[!self_index]) &&(!(rand() % 4))) {
            cur_status.card_effect_present.frozing=true;
            next_round.frozen[!self_index]=true;
        }
	}
};
class Card109 :public Card {
public:
	Card109() {
		rank = 1;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
       
        cur_status.card_effect_present.presentNeed=true;
        
        cur_status.card_effect_present.frozen_modify=true;
        cur_status.card_effect_present.frozen_val_modify=1;
		//  计算伤害
		if(type==FOLLOW) {	//  后手，需要判断先手的各种防御值状态等
			if (cur_status.block_pre || cur_status.adding_card) {
                cur_status.card_effect_present.frozen_val_modify+=1;
                if(!cur_round.frozen[!self_index]) {
                    cur_status.card_effect_present.frozing=true;
                    next_round.frozen[!self_index]=true;
                }
			}
		}
	}
};
class Card110 :public Card {
public:
	Card110() {
		rank = 1;
		speed_delay = rank * 100 + 10;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		//  计算伤害
		if (type == PRE) {
			cur_status.damage_reflect_pre = true;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
            cur_status.card_effect_present.presentNeed=true;
            cur_status.card_effect_present.frozen_modify=true;
            cur_status.card_effect_present.frozen_val_modify=1;
		}
	}
};
class Card111 :public Card {
public:
	Card111() {
		rank = 4;
		tap_hit = true;
		tap_hit_rank = 2;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {

        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
		//  计算伤害
		if (type == PRE || type == TAPPING) {
			int damage = get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);
			cur_status.tap_hit_ablity = true;
			cur_status.tap_hit_rank = 2;
            cur_status.card_effect_present.damage_val=damage;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
		}
        cur_status.card_effect_present.frozen_modify=true;
        cur_status.card_effect_present.frozen_val_modify=2;
	}
	const virtual int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		int damage = (opposite.get_frozen_val() + 1) / 2;
		if (type != TAPPING)
			return damage;
		else
			return (damage > 1) ? damage - 1 : damage;
	}
};
class Card113 :public Card {
public:
	Card113() {
		rank = 4;
		tap_hit = true;
		tap_hit_rank = 1;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		//  计算伤害
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;

		if (type == PRE) {
			int damage = get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);
			cur_status.tap_hit_ablity = true;
			cur_status.tap_hit_rank = 1;

            cur_status.card_effect_present.damage_val=damage;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
		}
	}
	const int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		return opposite.get_frozen_val() / 2;
	}
	const int get_speed_delay(Card &opposite_card, Player_fight &self, Player_fight &opposite,bool &press) {
		if (opposite.get_frozen_val() >= 5)
            return -100;
        return speed_delay;
	}
};
class Card114 :public Card {
public:
	Card114() {
		rank = 5;
		speed_delay = -300;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {

		//  计算伤害
        cur_status.card_effect_present.presentNeed=false;
		if (type == PRE) {
			cur_status.dead_protect = true;
			cur_status.dead_protect_bonus = 5;
			cur_status.dead_protect_frozen = true;
		}
		//  如若是后手，卡牌无效果
	}
};
class Card115 :public Card {
public:
	Card115() {
		rank = 5;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		int damage = get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);
		//  计算伤害
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
		if (type == PRE) {
			//opposite.blood_minus(damage,true);
            cur_status.card_effect_present.damage_val=damage;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
		}
	}
	const int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		int damage = opposite.get_frozen_val();
		if (cur_round.frozen[!self_index])
			damage += 3;
		return damage;

	}
};
class Card116 :public Card {
public:
	Card116() {
		rank = 3;
		damage_val = 2;
		tap_hit = true;
		tap_hit_rank = 2;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		//  计算伤害
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;

		int damage = get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);
		if (type == PRE || type == TAPPING) {
			cur_status.interruption_pre = true;
            cur_status.card_effect_present.interruption=true;
            cur_status.card_effect_present.damage_val=damage;
            
            cur_status.tap_hit_ablity=true;
            cur_status.tap_hit_rank=2;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
		}
        cur_status.card_effect_present.frozen_modify=true;
        cur_status.card_effect_present.frozen_val_modify=-1;
        
        if (cur_round.frozen[!self_index] || next_round.frozen[!self_index]){
            cur_status.card_effect_present.dizz=true;
            next_round.dizz[!self_index]=true;
        }
	}
	const int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status,
		Player_fight &self, Player_fight &opposite, bool self_index) {
		int damage = damage_val;
		if (type == TAPPING)
			return (damage > 1) ? damage - 1 : damage;
		return damage;
	}
};


class Card200 :public Card {
public:
	Card200() {
		rank = 2;
		damage_val = 3;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round,
		Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
		//  计算伤害
		if (type == PRE) {
            cur_status.card_effect_present.damage_val=damage_val;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
		}
        cur_status.card_effect_present.blood_recov=true;
        cur_status.card_effect_present.blood_recov_val=1;
	}

};
class Card202 :public Card {
public:
	Card202() {
		rank = 1;
		tap_hit = true;
		tap_hit_rank = 1;
		speed_delay = rank * 100;
		damage_val = 3;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
		//  计算伤害
		int damage = get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);
        //    self.blood_minus(2, false);
        
        if(type==PRE || type ==TAPPING) {
            cur_status.card_effect_present.damage_val=damage;
            cur_status.tap_hit_ablity = true;
            cur_status.tap_hit_rank = 1;
        }
        else {    //  后手，需要判断先手的各种防御值状态等
            Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
        }
        

        
        cur_status.card_effect_present.self_damage=true;
        cur_status.card_effect_present.self_damage_val=2;
		

	}
	const int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        return damage_val;
	}
};
class Card204 :public Card {
public:
	Card204() {
		rank = 1;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round,
		Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		//  计算伤害
		if (type == PRE) {
			cur_status.damage_to_blood_pre = true;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			//self.blood_plus(1);
            cur_status.card_effect_present.presentNeed=true;
            cur_status.card_effect_present.blood_recov=true;
            cur_status.card_effect_present.blood_recov_val=1;
		}
    }
};
class Card205 :public Card {
public:
	Card205() {
		rank = 1;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
		int damage = get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);

        //计算伤害
		if (type == PRE) {
            cur_status.card_effect_present.damage_val=damage;
            
            if (cur_status.press_pre) {
                cur_status.card_effect_present.blood_recov=true;
                cur_status.card_effect_present.blood_recov_val=damage;
                cur_status.card_effect_present.card_recycle=true;
            }
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
            
            if (cur_status.press_follow) {
                cur_status.card_effect_present.blood_recov=true;
                cur_status.card_effect_present.blood_recov_val=damage;
                cur_status.card_effect_present.card_recycle=true;
            }
		}
	}
	const int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		return self.get_blood() / 6;
	}
    const int get_speed_delay(Card &opposite_card, Player_fight &self, Player_fight &opposite, bool &press) {
        if (opposite_card.rank == 2) {
			press = true;
			return -200;
		}
		return speed_delay;
	}
};
class Card207 :public Card {
public:
	Card207() {
		rank = 3;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round,
		Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		int damage = get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);

		//  计算伤害
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;

		if (type == PRE) {
            cur_status.card_effect_present.damage_val=damage;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
		}
        cur_status.card_effect_present.blood_recov=true;
        cur_status.card_effect_present.blood_recov_val=cur_status.card_effect_present.damage_val/2;
	}
	const int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        if(type==FOLLOW) {
            if(!cur_status.damage207_follow) {
                cur_status.damage207_follow=true;
                cur_status.damage207_follow_val=2+rand()%5;
            }
            return cur_status.damage207_follow_val;
        }
        else
            return 2 + rand() % 5;
	}

};
class Card209 :public Card {
public:
	Card209() {
		rank = 4;
		tap_hit = true;
		tap_hit_rank = 2;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		int damage = get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);
        cur_status.card_effect_present.presentNeed=true;
		//  计算伤害
        cur_status.card_effect_present.if_damage=true;
		if (type == PRE || type == TAPPING) {
			if (type == TAPPING)
				damage = (damage > 1) ? damage - 1 : damage;

			cur_status.tap_hit_ablity = true;
			cur_status.tap_hit_rank = 2;
            cur_status.card_effect_present.damage_val=damage;
            
			cur_status.shater_pre = true;
			cur_status.shater_pre_num = 3;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
			cur_status.shater_follow = true;
			cur_status.shater_follow_num = 3;
		}
	}
	const int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		return max_num(0,8 - (int)(opposite.handCard.size()));
	}
};
class Card210 :public Card {
public:
	Card210() {
		rank = 1;
		damage_val = 2;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {

		//  计算伤害
        
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
		int damage = get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);
		if (type == PRE) {
			//opposite.blood_minus(damage,true);
            cur_status.card_effect_present.damage_val=damage;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
            if(cur_status.adding_card) {
                next_round.dizz[!self_index] = true;
                cur_status.card_effect_present.dizz=true;
            }
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
		}
	}
	const int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		if (type == FOLLOW && cur_status.adding_card) {
			return damage_val + 4;
		}
		return damage_val;
	}
};
class Card211 :public Card {
public:
	Card211() {
		rank = 3;
		damage_val = 3;
		tap_hit = true;
		tap_hit_rank = 1;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		//  计算伤害
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
        
		if (type == PRE) {
			cur_status.tap_hit_ablity = true;
			cur_status.tap_hit_rank = 1;
            
			if (cur_status.press_pre) {
				cur_status.shater_pre = true;
				cur_status.shater_pre_num = 1;
			}
            cur_status.card_effect_present.damage_val=damage_val;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
			if (cur_status.press_follow) {
				cur_status.shater_follow = true;
				cur_status.shater_follow_num = 1;
			}
		}
	}
	const int get_speed_delay(Card &opposite_card, Player_fight &self, Player_fight &opposite,bool &press) {
		if (opposite_card.rank == 1) {
			press = true;
			return -100;
		}
		return speed_delay;
	}
};
class Card212 :public Card {
public:
	Card212() {
		rank = 4;
		speed_delay = rank * 100;
        tap_hit = true;
        tap_hit_rank = 2;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, 
		Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
        
		int damage = get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);
		//  计算伤害
        
        if (self.get_blood() <= 5) {
            cur_status.shater_pre = true;
            cur_status.shater_pre_num = 2;
        }
        
		if (type == PRE || type==TAPPING) {
            if(type==TAPPING && damage>1)
                damage--;
            cur_status.card_effect_present.damage_val= damage;
            cur_status.tap_hit_ablity = true;
            cur_status.tap_hit_rank = 2;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
			if (self.get_blood() <= 5) {
				cur_status.shater_follow = true;
				cur_status.shater_follow_num = 2;
			}
		}
	}
	const int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		return (Full_Blood - self.get_blood()) / 3 + 2;
	}
};
class Card213 :public Card {
public:
	Card213() {
		rank = 5;
		damage_val = 2;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		//  计算伤害
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
        int damage=get_damage(type,cur_round, next_round,cur_status, self, opposite, self_index);
		if (type == PRE) {
			cur_status.shater_pre = true;
			cur_status.shater_pre_num = 2;

			if (cur_status.press_pre) {
				cur_status.shater_pre_num = 4;
			}
		//	opposite.blood_minus(damage_val,true);
            cur_status.card_effect_present.damage_val=damage;
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			cur_status.shater_follow = true;
			cur_status.shater_follow_num = 2;

			if (cur_status.press_follow) {
				cur_status.shater_follow_num = 4;
			}
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
		}
	}
    const virtual int get_speed_delay(Card &opposite_card, Player_fight &self, Player_fight &opposite, bool &press) {
		if (opposite_card.rank >= 3 && opposite_card.rank <= 5) {
            if(Debug_mode)
                cout<<"213压制有效"<<endl;
			press = true;
			return -200;
		}
        else
            cout<<"压制无效"<<endl;
		return speed_delay;
	}
	const int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		if ((type == PRE && cur_status.press_pre) ||
			(type == FOLLOW && cur_status.press_follow))
            return (int)opposite.handCard.size();
        else
            return damage_val;
	}
};
class Card214 :public Card {
public:
	Card214() {
		rank = 5;
		speed_delay = -200;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		//  计算伤害
		if (type == PRE) {
			cur_status.min_blood_pre = true;
			cur_status.min_blood_pre_val = 1;
			cur_status.min_blood_pre_hurt_multiple = 2;
		}
		//  后手的话无有操作
	}
};
class Card215 :public Card {
public:
	Card215() {
		rank = 3;
		damage_val = 3;

		tap_hit = true;
		tap_hit_rank = 2;
		speed_delay = rank * 100;
	}
	const void take_effect(TYPE type, Round &cur_round, Round &next_round, 
		Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
        
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
        
        cur_status.card_effect_present.lock_Adding_block=true;
		next_round.perry_ability_forbid[!self_index] = true;
        
		next_round.energy_ability_forbid[!self_index] = true;
        

		//  计算伤害
		if (type == PRE || type == TAPPING) {
			int damage = get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);
			if (type == PRE || type == TAPPING) {
				if (type == TAPPING)
					damage = (damage > 1) ? damage - 1 : damage;
			}
			cur_status.tap_hit_ablity = true;
			cur_status.tap_hit_rank = 2;
            cur_status.card_effect_present.damage_val=damage;
			//opposite.blood_minus(damage,true);
		}
		else {	//  后手，需要判断先手的各种防御值状态等
			Card::defensive_effect(cur_round, next_round, cur_status, self, opposite, self_index);
		}
	}
};
class Card217 :public Card {
public:
	Card217() {
		rank = 4;
		speed_delay = rank * 100;
	}
    
	const void take_effect(TYPE type, Round &cur_round, Round &next_round,
		Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {

        int damage=get_damage(type, cur_round, next_round, cur_status, self, opposite, self_index);
        if(Debug_mode)
            cout<<"217 damage value为"<<damage<<endl;
        cur_status.card_effect_present.presentNeed=true;
        cur_status.card_effect_present.if_damage=true;
		//  计算伤害
		if (type == PRE || type==TAPPING) {
            cur_status.card_effect_present.damage_val=damage;
            cur_status.card_effect_present.interruption=true;
			cur_status.interruption_pre = true;
		}
        else { //后手
            defensive_effect(cur_round,next_round,cur_status,self,opposite,self_index);
        }
	}
    const virtual int get_speed_delay(Card &opposite_card, Player_fight &self, Player_fight &opposite, bool &press){
        if (opposite.handCard.size() <= 5) {
			return -200;
		}
		return speed_delay;
	}
	const int get_damage(TYPE type, Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
		if (type == PRE)
			return cur_status.damage_pre_follow[1];	        //  返回后手，follow伤害量
		else
			return cur_status.damage_pre_follow[0];	        //  返回先手，pre伤害量
	}
};


#endif
