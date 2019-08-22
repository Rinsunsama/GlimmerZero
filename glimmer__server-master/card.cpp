#include "card.h"

// glimmer.cpp: 定义控制台应用程序的入口点。
//
#include "header.h"
#include "card.h"
#include "fight.h"
#include "vari.h"

//将牌编号从客户端格式转换为服务端格式
int card_index_convert_server(int index){
        //if(Debug_mode)
        //cout<<"服务端牌号为"<<index<<endl;
    if(index>=0 && index<=17)
        return index;
    if(index>=53 && index<=55)
        return index-35;
    if(index>=51 && index<=52)
        return index-30;
    if(index==50)
        return 23;
    
    if(Debug_mode)
        cout<<"Index 不合法，该下标为"<<index<<endl;
    return -1; //表示出错，不合法index
    
}
//转换为客户端格式
int card_index_convert_client(int index){
    if(index>=0 && index<=17)
        return index;
    if(index>=18 && index<=20)
        return index+35;
    if(index>=21 && index<=22)
        return index+30;
    if(index==23)
        return 50;
    
    if(Debug_mode)
        cout<<"Index 不合法，该下标为"<<index<<endl;
    return -1; //表示出错，不合法index
    
}

void Initiate_card() {
    CardAll[0][0] = new Card100;
    CardAll[0][1] = new Card100;
    CardAll[0][2] = new Card102;
    CardAll[0][3] = new Card103;
    CardAll[0][4] = new Card104;
    CardAll[0][5] = new Card104;
    CardAll[0][6] = new Card106;
    CardAll[0][7] = new Card107;
    CardAll[0][8] = new Card107;
    CardAll[0][9] = new Card109;
    CardAll[0][10] = new Card110;
    CardAll[0][11] = new Card111;
    CardAll[0][12] = new Card111;
    CardAll[0][13] = new Card113;
    CardAll[0][14] = new Card114;
    CardAll[0][15] = new Card115;
    CardAll[0][16] = new Card116;
    CardAll[0][17] = new Card116;
    CardAll[0][18] = new Card053;
    CardAll[0][19] = new Card053;
    CardAll[0][20] = new Card055;
    CardAll[0][21] = new Card051;
    CardAll[0][22] = new Card052;
    CardAll[0][23] = new Card050;
    
    CardAll[1][0]  = new Card200;
    CardAll[1][1]  = new Card200;
    CardAll[1][2]  = new Card202;
    CardAll[1][3]  = new Card202;
    CardAll[1][4]  = new Card204;
    CardAll[1][5]  = new Card205;
    CardAll[1][6]  = new Card205;
    CardAll[1][7]  = new Card207;
    CardAll[1][8]  = new Card207;
    CardAll[1][9]  = new Card209;
    CardAll[1][10] = new Card210;
    CardAll[1][11] = new Card211;
    CardAll[1][12] = new Card212;
    CardAll[1][13] = new Card213;
    CardAll[1][14] = new Card214;
    CardAll[1][15] = new Card215;
    CardAll[1][16] = new Card215;
    CardAll[1][17] = new Card217;
    CardAll[1][18] = new Card053;
    CardAll[1][19] = new Card053;
    CardAll[1][20] = new Card055;
    CardAll[1][21] = new Card051;
    CardAll[1][22] = new Card052;
    CardAll[1][23] = new Card050;
}

void Delete_card() {
    for (int i = 0; i<2; i++)
        for (int j = 0; j < Hero_max_card; j++) {
            delete CardAll[i][j];
        }
}


Card::Card() :tap_hit(false), rank(0), damage_val(0), speed_delay(1000) {

}
//基类无产生效果

//该牌的后手效果
const void Card::defensive_effect(Round &cur_round, Round &next_round, Status &cur_status, Player_fight &self, Player_fight &opposite, bool self_index) {
    int damage = get_damage(FOLLOW, cur_round, next_round, cur_status, self, opposite, self_index);
    if (cur_status.block_pre) { //盾牌
      //  opposite.blood_minus((int)((float)damage*(cur_status.block_rate_pre)), true);
        cur_status.card_effect_present.damage_val=(int)((float)damage*(cur_status.block_rate_pre));
    }
    else if (cur_status.damage_reflect_pre && damage>0) {               //      激活伤害反制
            cur_status.card_effect_present.activated_damage_reflect=true;
            cur_status.card_effect_present.damage_reflect_val=damage;
    }
    else if (cur_status.damage_to_blood_pre && damage>0) { //触发伤害变回血
            cur_status.card_effect_present.opposite_card_add_blood=true;
            cur_status.card_effect_present.opposite_card_add_blood_val+=damage;
    }
    else if (cur_status.dead_protect && damage >= opposite.get_blood()) {	//     死亡保护触发
        cur_status.card_effect_present.opposite_card_add_blood=true;
        cur_status.card_effect_present.opposite_card_add_blood_val=cur_status.dead_protect_bonus;
        if (cur_status.dead_protect_frozen) {
            cur_status.card_effect_present.frozen_self=true;
        }
    }
    else if (cur_status.min_blood_pre) {  //最小血量保护
        cur_status.blood_recover=true;
        cur_status.blood_recover_roundEnd=damage*cur_status.min_blood_pre_hurt_multiple;
        
        int hurt_num = damage;
        if (opposite.get_blood() - damage < cur_status.min_blood_pre_val) {  //触发了最小血量保护
            hurt_num = opposite.get_blood() - cur_status.min_blood_pre_val;
            cur_status.card_effect_present.damage_val=hurt_num;
        }
        else                //  最小血量保护未触发
            cur_status.card_effect_present.damage_val=hurt_num;
    }
    else //先手并没有其他附加属性了
        cur_status.card_effect_present.damage_val=damage;
}
