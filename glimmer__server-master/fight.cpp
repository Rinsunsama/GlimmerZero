#include "fight.h"
#include "vari.h"
#include "header.h"
#include "player.h"
#include "string.h"
#include "tool.h"
#include "unistd.h"
#include "math.h"
#include "stdlib.h"
#include <time.h>
#include <iostream>
#include "communi.h"
//socket_num1 ,socket_num2

using std::cerr;
using std::swap;
unordered_set<int>  valid_socket_num;

pthread_mutex_t fight_over_mutex; //战斗结束的锁
pthread_mutex_t block_lock;     //用于块分配的锁

queue<Fight*> fight_block;	//有没有空余的fight_block块
int Fight::count = 0;

Fight *Fight_block_get() {
	Fight *cur;
    pthread_mutex_lock(&block_lock);
	if (!fight_block.empty()) {
		cur = fight_block.front();
		fight_block.pop();
	}
	else
		cur = new Fight();
    pthread_mutex_unlock(&block_lock);

    if(!cur)
        cerr<<"Wrong fight_block malloc"<<endl;
	return cur;
}


void recycle(Fight *fight_target) {
    pthread_mutex_lock(&block_lock);
	fight_block.push(fight_target);
    pthread_mutex_unlock(&block_lock);

}

void create_fight(int seria_1,int seria_2) {
		
    Fight *cur_fight = Fight_block_get();
    cur_fight->fight_initialize(seria_1, seria_2);

    cout<<"客户端初始化完毕"<<endl;
    
    pthread_t pthd;
    
    if(!pthread_create(&pthd, NULL,start_fight, (void *)cur_fight)) { //线程创建成功,返回0
        pthread_detach(pthd);   //设定线程分离
        cout<<"线程创建成功"<<endl;
        pthread_rwlock_wrlock(&mapping_lock_shared);
        cout<<"线程编号为"<<cur_fight->fight_num<<endl;
        fight_tid[cur_fight->fight_num]=pthd;    //建立线程号的映射
        pthread_rwlock_unlock(&mapping_lock_shared);
    }
    else {
        cout<<"线程创建失败"<<endl;
    }

}

void *start_fight(void *fight) {
    pthread_setcancelstate(PTHREAD_CANCEL_ENABLE, NULL);
    pthread_setcanceltype(PTHREAD_CANCEL_ASYNCHRONOUS, NULL);   //设定cancel 函数的异步取消
    ((Fight*)fight)->fight_process();   //-------------!!!!!!
    
    
    
    
    
    return ((void *)0);
}

//结束某场战斗 0表示p1获胜，1表示p2获胜
void fight_over(int fight_num, int success_index,int type) {  //获胜者的下标，0表示p1获胜，1表示p2获胜

    //type 0表示是正常战斗结束，1表示是玩家掉线，2表示是玩家投降
    if(Debug_mode)
        cout<< "战斗结束,结束类型是 "<< type<<endl;
    
    pthread_mutex_lock(&fight_over_mutex);
    pthread_rwlock_rdlock(&mapping_lock_shared);
    if(serial_fight.count(fight_num)<=0){   //该战斗资源已经被释放，处理结束
        //关掉相关锁
        cout<<"Error:战斗资源之前已释放"<<endl;
        pthread_rwlock_unlock(&mapping_lock_shared);
        pthread_mutex_unlock(&fight_over_mutex);
        return;
    }
    
    //必须判断当前线程是在主进程中运行还是在战斗线程中运行，这样才能决定结束时是退出线程还是杀死线程
    //因为该函数可能由心跳检测等引起，，亦有可能是正常战斗结束
    //玩家认输时和掉线处理相仿，都需要进行fight线程的打断和终止线程的操作

    Fight *cur_fight=serial_fight[fight_num];

    if(fight_tid.find(fight_num)==fight_tid.end()){
        cout<<"can't find the fight_tid"<<endl;
        pthread_rwlock_unlock(&mapping_lock_shared);
        pthread_mutex_unlock(&fight_over_mutex);
        return;
    }
    pthread_t tid = fight_tid.at(fight_num);   // 获取战斗线程的id
    int win_num = cur_fight->player_num[success_index];
    int lose_num = cur_fight->player_num[!success_index];
    
    Player_base *pwin=serial_player_base_dict[win_num],*plose=serial_player_base_dict[lose_num];
    Player_fight *pf_win=serial_player_dict[win_num],*pf_lose=serial_player_dict[lose_num];
    
    pthread_rwlock_unlock(&mapping_lock_shared); //资源读取完毕，释放映射读锁

    
    int winsock=pf_win->socket;
    int losesock=pf_lose->socket;
    
    //   通知双方客户端胜败信息
    char str_over[4];
    str_over[0]=4;
    str_over[1]=13;
    str_over[2]=0;
    if (type == 1 || type == 2) {        //战斗非正常结束，认输和掉线是由主线程或者心跳检测线程做相关处理
       // pthread_testcancel();
        if(pthread_cancel(tid))
            cout<<"战斗线程未能正常关闭"<<endl;
        else
            cout<<"战斗线程已关闭"<<endl;
        if(type==1){ //掉线
            str_over[3]=1;
            wwriten(winsock,str_over,4);
        }
        else {  //认输
            str_over[3]=2;
            wwriten(winsock,str_over,4);
            str_over[2]=1;
            wwriten(losesock,str_over,4);
        }
    }
    else if (type==0){  //正常战斗结束
        str_over[3]=0;
        wwriten(winsock, str_over,4);
        str_over[2]=1;      //敌方获胜
        wwriten(losesock, str_over,4);
    }
    else {
        //type类型出错，直接返回
        pthread_mutex_unlock(&fight_over_mutex);
        return;
    }

   //堆资源的回收等
    recycle(pf_win);
    recycle(pf_lose);
    recycle(pwin);
    recycle(plose);
    recycle(cur_fight);
    //相关资源的处理和释放
    if(type==0 || type==2)
        shutdown(losesock);
    shutdown(winsock);
    
    //以下将改变相关映射关系，故加写锁
    pthread_rwlock_wrlock(&mapping_lock_shared);
    
    fight_tid.erase(fight_num);
    serial_player_base_dict.erase(win_num);
    serial_player_base_dict.erase(lose_num);
    serial_player_dict.erase(win_num);
    serial_player_dict.erase(lose_num);
    
    socket_PlayerNo.erase(winsock);
    socket_PlayerNo.erase(losesock);
    serial_fight.erase(fight_num);
    
    player_fight_map.erase(win_num);
    player_fight_map.erase(lose_num);
    
    
    pthread_rwlock_unlock(&mapping_lock_shared);
    pthread_mutex_unlock(&fight_over_mutex);
    
    cout<<"fight_over 处理结束"<<endl;
    if(type==0)
        pthread_exit(NULL);  //战斗正常结束，关闭当前战斗线程
}

//用于在对战中储存对下一局置的状态。在每局出牌时依据这些状态判定p1,p2的牌有效与否
void Fight::fight_initialize(int p1_num, int p2_num) {
    fight_num = count++;
    
    
    
    player_fight[0]=player_fight_block_get();
    player_fight[1]=player_fight_block_get();
    
    pthread_rwlock_rdlock(&mapping_lock_shared);
    Player_base *pb1=serial_player_base_dict[p1_num];
    Player_base *pb2=serial_player_base_dict[p2_num];
    pthread_rwlock_unlock(&mapping_lock_shared);
    
    player_fight[0]->Player_fight_initialize(*pb1);
    player_fight[1]->Player_fight_initialize(*pb2);
    
    
    player_num[0] = p1_num;
    player_num[1] = p2_num;
    
    pthread_rwlock_wrlock(&mapping_lock_shared);
    player_fight_map[p1_num]=fight_num;
    player_fight_map[p2_num]=fight_num;
    
    serial_fight[fight_num]=this;
    serial_player_dict[p1_num]=player_fight[0];
    serial_player_dict[p2_num]=player_fight[1];
    
    pthread_rwlock_unlock(&mapping_lock_shared);

    
   // fight_running = true;
    memset(&cur_round, 0, sizeof(cur_round));
}


//  处理整场战斗信息，负责p1,p2整场的战斗计算和处理
void Fight::fight_process() {
    
    //  战斗开始，向客户端发送己方手牌和对方所选择的英雄等信息
    send_initia_status(*player_fight[0], player_fight[1]->hero_num);
    send_initia_status(*player_fight[1], player_fight[0]->hero_num);

    
    sleep(1);
    while (true) {                  //  开启一轮战斗
        //  处理完成，等待客户端处理完成，以开启新一回合操刚才作
        WaitClientReady10();
        if(Debug_mode) {
            cout<<"双方玩家手牌数量分别为: ";
            cout<<player_fight[0]->handCard.size()<<' '<<player_fight[1]->handCard.size()<<endl;
        }
        
        if(Debug_mode) {
            cout<<"p0 手牌数量"<<endl;
            cout<<"当前玩家手牌数量为"<<player_fight[0]->handCard.size()<<endl;
            cout<<"当前玩家卡组剩余卡牌数量为"<<player_fight[0]->wait_card_size<<endl;
            cout<<"当前玩家坟墓区卡牌数量为"<<player_fight[0]->discard_card.size()<<endl;
            cout<<endl;
            
            cout<<"p1 手牌数量"<<endl;
            cout<<"当前玩家手牌数量为"<<player_fight[1]->handCard.size()<<endl;
            cout<<"当前玩家卡组剩余卡牌数量为"<<player_fight[1]->wait_card_size<<endl;
            cout<<"当前玩家坟墓区卡牌数量为"<<player_fight[1]->discard_card.size()<<endl;
        }
        
        SendPickStat6(0, 5, 5);     //向双方玩家发送开始新一回合的操作状态
        sleep(1);

        if(Debug_mode)
            cout<<"同步双方玩家buf信息"<<endl;
        SendingBuf12(cur_round);              //  为双方玩家同步buf状态
        sleep(1);
        pick_normal_card();         //  双方正常选择出牌
        if((player_fight[0]->pick_card_num==EmptyCardNum) &&
           (player_fight[1]->pick_card_num==EmptyCardNum)) {
            memset(&cur_round,0,sizeof(cur_round));
            continue;
        }
        fight_round(player_fight[0]->pick_card_num, player_fight[1]->pick_card_num);
        sleep(1);
        if(Debug_mode) {
            cout<<endl;
            cout<<"双方编号为";
            cout<<player_fight[0]->serial_num<<' ';
            cout<<player_fight[1]->serial_num<<endl;
            cout<<"一个回合结束，当前双方血量各为"<<endl;
            cout<<player_fight[0]->get_blood()<<"   "<<player_fight[1]->get_blood()<<endl;
            cout<<"当前双方霜冻值为"<<endl;
            cout<<player_fight[0]->get_frozen_val() <<" "<<player_fight[1]->get_frozen_val()<<endl;
        }
    }
}

//发送双方初始卡牌和对方英雄等
void Fight::send_initia_status(Player_fight &self,int opposite_hero_num){
    char str[15];
    str[0]=9;                    //     长度
    str[1]=3;                    //     类型标识
    str[2]=0;                   //      匹配成功
    str[3]=opposite_hero_num;   //      对方英雄
    int index = 4;
    
    cout<<"发送玩家初始手牌"<<endl;
    for(auto i:self.handCard)
        str[index++] = card_index_convert_client(i);  //  玩家初始的卡牌
     
    if(Debug_mode) {
        cout<<"现在此为抽卡区卡牌: ";
        for(int i=0;i<self.wait_card_size;i++)
            cout<<self.wait_card[i]<<' ';
        cout<<endl;
        cout<<"玩家初始手牌发送完毕"<<endl;
    }
    wwriten(self.socket, str, 9);
}
/*
 *   处理双方的正常出牌，置相关状态等，向客户端发送要求出牌请求
 */
void Fight::pick_normal_card(){
    if(Debug_mode)
        cout<<"进入双方正常出牌阶段"<<endl;
    
    //  重置双方出牌状态
    player_fight[0]->been_waitted = false;
    player_fight[1]->been_waitted = false;
    
    bool send[2]={false,false};
    
    
    //若格挡被禁，则默认手牌为空牌，否则默认为防御
    player_fight[0]->pick_card_num=(cur_round.perry_ability_forbid[0])?EmptyCardNum:EnergyCardNum;
    player_fight[1]->pick_card_num=(cur_round.perry_ability_forbid[1])?EmptyCardNum:EnergyCardNum;
    
    //  检查冻结或眩晕效果
    if(cur_round.dizz[0] || cur_round.frozen[0] ||
       (cur_round.perry_ability_forbid[0] && cur_round.energy_ability_forbid[0] &&player_fight[0]->handCard.empty())) {  //  玩家处于冻结或眩晕的状态
        player_fight[0]->pick_card_num=EmptyCardNum;
        player_fight[0]->been_waitted = true;
        send[1]=true;
    }
    else {                                          //  未冻结玩家需要出牌
        player_fight[0]->picking_status = 1;        //  玩家1可出牌
        if (Debug_mode) {
            cout << "等待玩家0出牌" << endl;
        }
    }
    
    if(cur_round.dizz[1] || cur_round.frozen[1] ||
       (cur_round.perry_ability_forbid[1] && cur_round.energy_ability_forbid[1] &&player_fight[1]->handCard.empty())){   //  玩家2处于冻结或眩晕的状态
        player_fight[1]->pick_card_num=EmptyCardNum;    //冻结或眩晕，应为空牌
        player_fight[1]->been_waitted = true;
        send[0]=true;
    }
    else {
        if (Debug_mode) {
            cout << "等待玩家1出牌" << endl;
        }
        player_fight[1]->picking_status = 1;          //玩家2等待出牌,打开p1出牌状态
    }
    
    if(Debug_mode)
        cout<<"通知客户端出牌"<<endl;
    SendPickStat6(0, player_fight[0]->picking_status, player_fight[1]->picking_status);
    
    for (int i = 0; i < Max_Waitting_time; i++) {
        sleep(1);
        //通知双方对方出卡消息
        if(player_fight[0]->been_waitted && !send[1]) { //尚未通知对方0出牌
            if(Debug_mode)
                cout<<"玩家0出牌，卡牌编号为"<< player_fight[0]->pick_card_num<<endl;
            SendingPickCard5(1, 1, 0);
            send[1]=true;
        }
        if(player_fight[1]->been_waitted && !send[0]) {
            if(Debug_mode)
                cout<<"玩家1出牌，卡牌编号为"<< player_fight[1]->pick_card_num<<endl;
            SendingPickCard5(0, 1, 0);
            send[0]=true;
        }
        if (player_fight[0]->been_waitted && player_fight[1]->been_waitted)
            break;      //双方出牌成功
    }
    //双方再次置为不可出卡状态
    player_fight[0]->picking_status = 0;
    player_fight[1]->picking_status = 0;
    

    
    //超时默认出牌时，表示对方出盾牌效果
    if(!player_fight[0]->been_waitted && player_fight[0]->pick_card_num==PerryCardNum)
        SendingPickCard5(1, 1, 0);
    if(!player_fight[1]->been_waitted && player_fight[1]->pick_card_num==PerryCardNum)
        SendingPickCard5(0, 1, 0);
    if (Debug_mode)
    {
        cout << "双方出牌完成出牌，开始处理牌面逻辑。" << endl;
    }
    
}

/*
 * Comments:依据玩家的出牌信息，处理当前战斗回合
 * Param card1_num : 玩家1出牌的编号 int
 * Param card2_num : 玩家2出牌的编号 int
 */
void Fight::fight_round(int card1_num, int card2_num) {
    if(Debug_mode)
        cout<<"回合开始，双方的卡牌分别为: "<<card1_num<<' '<<card2_num<<endl;

    //  将两张卡移入坟墓区
    Process_used_card(0, card1_num);
    Process_used_card(1, card2_num);
    
    Status cur_status;                    //    当前回合进行时的状态
    memset(&cur_status, 0, sizeof(cur_status));
    
    Round next_round;                    //    当前回合进行完毕后所置的下一轮状态
    memset(&next_round, 0, sizeof(next_round));
    
    int pre_index;                    //    先手玩家的下标，0代表玩家0先手，1代表玩家1先手
    Card *player_cards[2];                //    p1、p2 玩家当前回合的出牌
    int card_num[2];
    
    //    获取卡牌信息
    card_num[0]=card1_num;
    card_num[1]=card2_num;
    
    player_cards[0] = CardAll[player_fight[0]->hero_num][card1_num];
    player_cards[1] = CardAll[player_fight[1]->hero_num][card2_num];
    
    bool press[2];                        //p1压制位，p2压制位
    pre_index=ComputePreIndex(*player_cards[0],*player_cards[1],*player_fight[0],*player_fight[1],press);
    //    计算先后手信息，并置相关的压制状态位
    
    //    置相关压制位
    cur_status.press_pre = press[pre_index];
    cur_status.press_follow = press[!pre_index];
    if(Debug_mode) {
        cout<<"本回合先手下标为"<<pre_index<<endl;
        if(cur_status.press_pre)
            cout<<"本回合存在先手压制"<<endl;
        if(cur_status.press_follow)
            cout<<"本回合存在后手压制"<<endl;
    }
    //  压制附带打断
    if(cur_status.press_pre)
        cur_status.interruption_pre=true;
    //  发送开牌信息，是否压制取决于是否有先手压制
    sleep(2);
    Sending9_CardShow(pre_index,card_num, cur_status.press_pre);
    sleep(1);
    
    //处理No217的卡牌,即卡牌等于对方卡牌伤害
    if (player_fight[!pre_index]->hero_num == 1 && card_num[!pre_index] == 17) { // 后手也为217
        cur_status.damage_pre_follow[1] = 0;        //后手牌的伤害
    }
    else    //  后手不为207，计算后手卡牌伤害
        cur_status.damage_pre_follow[1] =
        CardAll[player_fight[!pre_index]->hero_num][card_num[!pre_index]]->get_damage(
                                                FOLLOW, cur_round, next_round, cur_status, *player_fight[!pre_index], *player_fight[pre_index], !pre_index);
    //  先手的牌伤害先置为0，待后续累加,217卡牌为后手
    cur_status.damage_pre_follow[0] = 0;   //先手牌伤害应为0
    
    if(Debug_mode)
        cout<<"处理先手卡牌效果"<<endl;
    //  先手卡生效
    player_cards
    [pre_index]->take_effect(PRE, cur_round, next_round, cur_status, *player_fight[pre_index], *player_fight[!pre_index], pre_index);
    //  伤害累加
    cur_status.damage_pre_follow[0]+=cur_status.card_effect_present.damage_val;
    
    // 压制附带打断效果
    if(cur_status.press_pre){
        if(Debug_mode)
            cout<<"客户端将展示由压制引起的打断效果"<<endl;
        cur_status.card_effect_present.interruption=true;
    }
    
    //如果后手没有牌，则屏蔽打断效果
    if(card_num[!pre_index]==EmptyCardNum) {
        cur_status.card_effect_present.interruption=false;
    }
    //  客户端展示相应效果
    PresentingEffectSend11(cur_status.card_effect_present,pre_index,card_num[pre_index]);

    

    //  处理破灭
    if(cur_status.shater_pre)
        Process_shater(!pre_index, cur_status.shater_pre_num);
    
 //   sleep(1);
    //  处理可能的先手的连击操作
    bool first_tap=true;
    while(cur_status.tap_hit_ablity && player_fight[pre_index]->tap_ability(cur_status.tap_hit_rank)){
        player_fight[pre_index]->old_tap_rank=cur_status.tap_hit_rank;
        //进入选择连击牌阶段，若返回值-1，不连击，否则连击牌生效
        if(Debug_mode)
            cout<<"进入连击阶段，用户选取连击牌"<<endl;
        int card_num_tap=Choose_tap_card(pre_index);
        
        if(!player_fight[pre_index]->if_tap) {  // 用户选择不连击
            if(Debug_mode)
                cout<<"玩家选择不连击，退出连击阶段"<<endl;
            break;
        }
        if(card_num_tap>=0){//玩家选择了连击牌
            //开始相关置位
            if(Debug_mode) {
                cout<<"用户选取的连击牌，卡牌编号为"<<card_num_tap<<endl;
                cout<<"处理连击卡牌效果"<<endl;
            }
            cur_status.shater_pre=false;
            memset(&cur_status.card_effect_present,0,sizeof(Present_effect));
            
            SendingPickCard5(!pre_index, Tapping,card_num_tap);     //  向对手发送该玩家的出牌信息
            if(first_tap && cur_status.tap_blood_recov) {       //  玩家有连击回血状态，且此为第一次连击，触发连击回血
                first_tap=false;
                sleep(1);
                SendingEffect11(pre_index, card_num[pre_index], 2, cur_status.tap_blood_recov_val);
                player_fight[pre_index]->blood_plus(cur_status.tap_blood_recov_val);
            }
            //  卡牌的数据的更改
            Process_used_card(pre_index,card_num_tap);
            //  连击卡生效
            CardAll [player_fight[pre_index]->hero_num] [card_num_tap]->
            take_effect(TAPPING, cur_round, next_round, cur_status, *player_fight[pre_index], *player_fight[!pre_index], pre_index);
            //  伤害累加
            cur_status.damage_pre_follow[0]+=cur_status.card_effect_present.damage_val;
        
            //如果后手没有牌，则屏蔽打断效果
            if(card_num[!pre_index]==EmptyCardNum) {
                cur_status.card_effect_present.interruption=false;
            }
            
            //  展现效果
            PresentingEffectSend11(cur_status.card_effect_present, pre_index,card_num_tap);
            //  处理破灭
            if(cur_status.shater_pre)
                Process_shater(!pre_index, cur_status.shater_pre_num);
       //     sleep(1);
        }
        else
            break;      //  退出连击
    }
    
    //    处理后手的卡牌        无打断，且后手牌不为空牌，处理之
    if (!cur_status.interruption_pre && (card_num[!pre_index]!=EmptyCardNum)) {
        if(Debug_mode)
            cout<<"处理后手卡牌效果"<<endl;
        //  置相关位
        memset(&cur_status.card_effect_present,0,sizeof(Present_effect));
        //  卡牌生效
        player_cards[!pre_index]->take_effect(FOLLOW, cur_round, next_round, cur_status, *player_fight[!pre_index], *player_fight[pre_index], !pre_index);
        PresentingEffectSend11(cur_status.card_effect_present, !pre_index,card_num[!pre_index]);
        //    处理破灭
        if (cur_status.shater_follow)
            Process_shater(pre_index, cur_status.shater_follow_num);
        if(cur_status.card_effect_present.activated_damage_reflect) { //伤害反制被激活
         //   sleep(2);
            if(Debug_mode) {
                cout<<"处理伤害反制"<<endl;
                cout<<"伤害反制的数值为"<< cur_status.card_effect_present.damage_reflect_val;
            }
            SendingEffect11(pre_index, card_num[pre_index], 1, cur_status.card_effect_present.damage_reflect_val);
            player_fight[!pre_index]->blood_minus(cur_status.card_effect_present.damage_reflect_val);
            if(player_fight[!pre_index]->get_blood()<=0){
                player_fight[!pre_index]->process_dead(fight_num);
            }
        }
        if(cur_status.card_effect_present.opposite_card_add_blood) {      //先手的回血相关被激活
            sleep(1);
            SendingEffect11(pre_index, card_num[pre_index], 2, cur_status.card_effect_present.opposite_card_add_blood_val);
            player_fight[pre_index]->blood_plus(cur_status.card_effect_present.opposite_card_add_blood_val);
        }
        if(cur_status.card_effect_present.frozen_self && (!cur_round.frozen[!pre_index]) ) {                //  先手的冰冻后手被激活
            if(Debug_mode)
                cout<<"先手的冰冻后手操作被激活,发动冰冻后手效果"<<endl;
            sleep(1);
            SendingEffect11(pre_index, card_num[pre_index], 5, 0);
            next_round.frozen[!pre_index]=true;
        }
        //  处理回合结束的相关回血
        if (cur_status.blood_recover) {
            sleep(1);
            SendingEffect11(pre_index,card_num[pre_index],2,cur_status.blood_recover_roundEnd);
            player_fight[pre_index]->blood_plus(cur_status.blood_recover_roundEnd);
        }
    }
  //  sleep(2);
    cur_round = next_round;
}
//比较谁先手，并置相关压制打断位,返回先手index(0/1)
int Fight::ComputePreIndex(Card &card1,Card &card2,Player_fight &p1,Player_fight &p2,bool *press){
    int speed_delay[2];                    //    玩家当前出牌的速度值，越小越快
    
    int pre_index=1;
    press[0] = false;
    press[1] = false;
    speed_delay[0] = card1.get_speed_delay(card2, p1, p2, press[0]);
    speed_delay[1] = card2.get_speed_delay(card1, p2, p1, press[1]);
    if(Debug_mode) {
        cout<<"p0,p1双方速度分别为";
        cout<<speed_delay[0]<<"  "<<speed_delay[1]<<endl;
    }
    if ((speed_delay[0] < speed_delay[1]) || ((speed_delay[0] == speed_delay[1]) && (rand() % 2)))
        pre_index = 0;
    if(Debug_mode)
        cout<<"先手下标为"<<pre_index<<endl;
    return pre_index;
}

//  将出手的卡牌从手牌中删除，放入坟墓区
void Fight::Process_used_card(int player_index,int card_num){
    if(Debug_mode)
        cout<<"将用户"<<player_index<<"的编号为"<<card_num<<"的手牌删除"<<endl;
    if (card_num>=0 && card_num<=20) {
        player_fight[player_index]->handCard.erase(card_num);
        player_fight[player_index]->discard_card.insert(card_num);
    }
}

//关于连击，等待下标为self_index用户选取符合要求的连击牌
int Fight::Choose_tap_card(int self_index){
    WaitClientReady10();
    player_fight[self_index]->if_tap=false;
    player_fight[self_index]->been_waitted=false;
    player_fight[self_index]->picking_status=Tapping;
    
    if(Debug_mode)
        cout<<"向客户端发送连击请求"<<endl;
    SendPickStat6(self_index, Tapping, Still);
    if(player_fight[self_index]->if_tap)
        return player_fight[self_index]->pick_card_num;
    return -1;
}




//处理破灭
void Fight::Process_shater(int self_index, int shater_num) {
	//  发送状态，等待破灭
    shater_num=min_num((int)player_fight[self_index]->handCard.size(),shater_num);   //实际破灭数量不能多于手牌数量
    if(Debug_mode)
        cout<<"Process_shater："<<"等待玩家选取"<<shater_num<<"张破灭牌"<<endl;
    WaitClientReady10();
    for(int i=0;i<shater_num;i++) {
        player_fight[self_index]->been_waitted=false;
        player_fight[self_index]->picking_status=Shater;

        SendPickStat6(self_index, Shater, Still);  //置状态
        
        //等待出牌完毕
        for(int i=0;i<30;i++) {
            sleep(1);
            if(player_fight[self_index]->been_waitted) {  //默认的shater
                cout<<"proc shater"<<"玩家选取了破灭牌"<<endl;
                break;
            }
        }
        
        
        if(!player_fight[self_index]->been_waitted) {
            //  客户端超时未选取破灭牌,系统随机丢弃破灭牌,并通知客户端破灭了哪些牌
            
            //随机抽取破灭牌
            if(Debug_mode)
                cout<<"玩家未选取破灭牌，随机丢弃破灭牌"<<endl;
            int card=*(player_fight[self_index]->handCard.begin());
            player_fight[self_index]->pick_card_num=card;
            
            SendingDefaultShater14(self_index, card);
            // 向该客户端发送系统随机选取了破灭牌的消息
        }

        int cardNo=player_fight[self_index]->pick_card_num;
        Process_used_card(self_index, cardNo);
        SendingPickCard5(!self_index,Shater, cardNo);
        if(Debug_mode)
            cout<<"玩家"<<self_index<<"破灭的卡牌编号为"<<cardNo<<endl;
     //   sleep(1);
    }
}
/*
* Comments:处理玩家的连击
* Param player_index :  玩家编号 int
* Param &card_num : 卡牌编号 vector<int>
* Param int i : 第几个页面
* @Return bool : 是否连击成功 
*/
//check 玩家手中是否有可连击的牌
/*
bool Fight::Check_hit_ability(int player_index, int tap_hit_rank_old) {
    for(auto &card_index:player_fight[player_index]->handCard) {
        if ((CardAll[player_fight[player_index]->hero_num][card_index]->tap_hit) &&
            (CardAll[player_fight[player_index]->hero_num][card_index]->tap_hit_rank>tap_hit_rank_old))
                return true;
    }
    return false;
}
*/
/*  sending message
 *  向双方客户端发送相应出牌信息
 */
void Fight::SendingPickCard5(int self_index,int stat,int num){
    char str[4];
    str[0]=4;
    str[1]=5;
    str[2]=stat;
    str[3]=card_index_convert_client(num);
    wwriten(player_fight[self_index]->socket, str, 4);
}
//向双方玩家发送类型为12的buf同步信息
void Fight::SendingBuf12(Round &cur_round){
    char str[4];
    str[0]=4;
    str[1]=12;
    str[2]=0;
    str[3]=0;
    
    
    int fro=cur_round.frozen[0];
    int dizz=cur_round.dizz[0];
    int perry_abili1=cur_round.perry_ability_forbid[0];
    
    int fro2=cur_round.frozen[1];
    int dizz2=cur_round.dizz[1];
    int perry_abili2=cur_round.perry_ability_forbid[1];
    
    str[2]|=fro;
    str[2]|=(dizz<<1);
    str[2]|=(perry_abili1<<2);
    
    str[3]|=fro2;
    str[3]|=(dizz2<<1);
    str[3]|=(perry_abili2<<2);
    wwriten(player_fight[0]->socket, str, 4);
    
    swap(str[2],str[3]);
    wwriten(player_fight[1]->socket, str, 4);
}



// 发送编号为10的消息
void Fight::WaitClientReady10(){
    char str[2];
    str[0]=2;
    str[1]=10;
    
    player_fight[0]->been_waitted=false;
    player_fight[0]->picking_status=Ready;
    
    player_fight[1]->been_waitted=false;
    player_fight[1]->picking_status=Ready;
    
    wwriten(player_fight[0]->socket,str,2);
    wwriten(player_fight[1]->socket,str,2);
    
    
    for(int i=0;i<20;i++) {
        if(player_fight[0]->been_waitted && player_fight[1]->been_waitted)
            break;
            //等到了p1,p2的消息
        sleep(1);
    }
    player_fight[0]->picking_status=Still;
    player_fight[1]->picking_status=Still;
}


//  向双方玩家发送编号为9的消息，即开牌消息
void Fight::Sending9_CardShow(int pre_index,int *card_num,bool press){
    if(Debug_mode)
        cout<<"发送开牌信息"<<endl;
    if(press)
        cout<<"先手压制"<<endl;
    char str[5];
    str[0]=5;
    str[1]=9;
    str[2]=card_index_convert_client(card_num[!pre_index]);
    str[3]=0;
    str[4]=press;
    
    //  发送给先手玩家
    wwriten(player_fight[pre_index]->socket,str,5);
    
    str[2]=card_index_convert_client(card_num[pre_index]);
    str[3]=1;
    wwriten(player_fight[!pre_index]->socket,str,5);
}



//向双方玩家发送卡牌实际效果，也即发送一串编号为11的消息
void Fight::PresentingEffectSend11(Present_effect &presen,int self_index,int card_num){
    if(Debug_mode)
        cout<<"向双方玩家发送卡牌效果"<<endl;
    
    if(Debug_mode)
        cout<<"卡牌发动效果"<<endl;
   // sleep(2);
    SendingEffect11(self_index,card_num,0,0);
        
    if(presen.AddingCard) {              // 玩家蓄能
        if(Debug_mode)
            cout<<"蓄能效果"<<endl;
        sleep(1);
        DrawCards(self_index, 5);
    }
    
    //实际无卡牌效果需要发送，直接返回即可
    if(!presen.presentNeed)
        return;
   // sleep(1);
    if(presen.if_damage){       //      伤害
        if(Debug_mode)
            cout<<"卡牌有伤害"<<endl;
        sleep(2);
        SendingEffect11(self_index, card_num, 1, presen.damage_val);
        player_fight[!self_index]->blood_minus(presen.damage_val);
        if(player_fight[!self_index]->get_blood()<=0){
            player_fight[!self_index]->process_dead(fight_num);
        }
    }
    if(presen.blood_recov){     //     回血
        if(Debug_mode)
            cout<<"卡牌有回血"<<endl;
        sleep(1);
        SendingEffect11(self_index, card_num, 2, presen.blood_recov_val);
        player_fight[self_index]->blood_plus(presen.blood_recov_val);
    }
  
    if(presen.dizz){        // 眩晕效果
        if(Debug_mode)
            cout<<"卡牌有眩晕"<<endl;
        sleep(1);
        SendingEffect11(self_index, card_num, 4, 0);
    }
    if(presen.frozing){        //  冰冻效果
        if(Debug_mode)

            cout<<"卡牌有冰冻"<<endl;
        sleep(1);
        SendingEffect11(self_index, card_num, 5, 0);
    }
    if(presen.frozen_modify){        //   变更霜冻值
        sleep(1);
        if(Debug_mode)
            cout<<"霜冻有变更，变更值为"<<presen.frozen_val_modify;
        player_fight[!self_index]->frozen_plus(presen.frozen_val_modify);
        if(presen.frozen_val_modify>=0)
            SendingEffect11(self_index, card_num, 6, presen.frozen_val_modify);
        else
            SendingEffect11(self_index, card_num, 7, (-presen.frozen_val_modify));
    }
    if(presen.lock_Adding_block) {      //  封锁蓄能／盾牌
        sleep(1);
        if(Debug_mode)
            cout<<"卡牌有封锁"<<endl;
        SendingEffect11(self_index, card_num, 8, 0);
    }
    if(presen.self_damage){              //  自损效果
        sleep(2);
        if(Debug_mode)
            cout<<"自损"<<endl;
        SendingEffect11(self_index, card_num, 11, presen.self_damage_val);
        player_fight[self_index]->blood_minus(presen.self_damage_val);
        if(player_fight[self_index]->get_blood()<=0){
            player_fight[self_index]->process_dead(fight_num);
        }
    }
    if(presen.interruption){    //  打断效果
        if(Debug_mode)
            cout<<"卡牌有打断"<<endl;
        sleep(1);
        SendingEffect11(self_index, card_num, 3, 0);
    }
    
    if(presen.card_recycle){            //  卡牌回收
        sleep(1);
        if(Debug_mode)
            cout<<"卡牌回收"<<endl;
        player_fight[self_index]->handCard.insert(card_num);
        player_fight[self_index]->discard_card.erase(card_num);
        
        SendingEffect11(self_index, card_num, 9, 0);
    }
   
}

//分别向双方发送相应卡牌效果11
//卡牌持有者编号，卡牌编号，要写入的效果和对应数值
void Fight::SendingEffect11(int self_index,int cardNum,int effectNum,int val){
    char str_self[6];
    char str_opposite[6];
    
    str_self[0]=6;
    str_self[1]=11;
    str_self[2]=0;                                                      //      发牌玩家
    str_self[3]=card_index_convert_client(cardNum);                     //      卡牌编号
    
    str_opposite[0]=6;
    str_opposite[1]=11;
    str_opposite[2]=1;                                                  //      对方的卡
    str_opposite[3]=card_index_convert_client(cardNum);                     //      卡牌编号
    
    str_opposite[4]=effectNum;
    str_self[4]=effectNum;
    str_self[5]=val;
    str_opposite[5]=val;
    
   // sleep(2);
    wwriten(player_fight[self_index]->socket,str_self,6);
    wwriten(player_fight[!self_index]->socket, str_opposite, 6);
}

//发送11类型的抽卡效果信息
void Fight::SendingEffect11_draw_card(int self_index,int cardNum,vector<int> &cards_num){
    char str_self[11];
    
    cout<<"抽取卡牌数量"<<cards_num.size()<< endl;
    str_self[1]=11;
    str_self[2]=0;                                                      //      发牌玩家
    str_self[3]=card_index_convert_client(cardNum);                                                     //      卡牌编号,在此必为51，以引起抽卡操作
    str_self[4]=10;                                                     //      抽卡效果
    str_self[5]=cards_num.size();
    int index=6;
    for(auto c:cards_num)
        str_self[index++]=card_index_convert_client(c);
    str_self[0]=index;
    cout<<"index"<<index<<endl;
    wwriten(player_fight[self_index]->socket,str_self,index);
    
    str_self[2]=1;                                                      //      对方出牌
    str_self[0]=6;
    wwriten(player_fight[!self_index]->socket, str_self, 6);
}
//  发送6类型的信息，即向玩家发
//送状态同步信息，并等待客户端回复
void Fight::SendPickStat6(int self_index,int self_stat,int oppo_stat){
    if(Debug_mode)
        cout<<"和客户端进行状态同步,并等待期待的客户端回复"<<endl;
    char str[4];
    str[0]=4;
    str[1]=6;
    str[2]=self_stat;
    str[3]=oppo_stat;
    wwriten(player_fight[self_index]->socket,str,4);
    swap(str[3],str[2]);
    wwriten(player_fight[!self_index]->socket,str,4);
    
    if(self_stat==1 || oppo_stat==1 ||
       ((self_stat==0) && oppo_stat==0) || (self_stat==5))    //表示此处无需等待,由调用函数处理等待
        return;
    
    
    for(int i=0;i<Max_Waitting_time;i++) {
        if(player_fight[self_index]->been_waitted)
            break;
        sleep(1);
    }
    player_fight[self_index]->picking_status=Still;
}





//玩家抽卡
void Fight::DrawCards(int self_index,int card_num){
    if(Debug_mode) {
        cout<<"玩家开始抽卡"<<endl;
        cout<<"当前玩家手牌数量为"<<player_fight[self_index]->handCard.size()<<endl;
        cout<<"当前玩家卡组剩余卡牌数量为"<<player_fight[self_index]->wait_card_size<<endl;
        cout<<"当前玩家坟墓区卡牌数量为"<<player_fight[self_index]->discard_card.size()<<endl;
        cout<<endl;
    }
    int to_draw=min_num(5,(int)(Max_hand_card-player_fight[self_index]->handCard.size()));
    int draw_num=min_num((int)player_fight[self_index]->wait_card_size,to_draw);
    vector<int> cards;
    if(draw_num>0){ //用户从抽卡区抽卡
        for (int i = 0; i <draw_num; i++) {
            int card_num = player_fight[self_index]->wait_card[--player_fight[self_index]->wait_card_size];
            player_fight[self_index]->handCard.insert(card_num);
            cards.push_back(card_num);
        }
        cout<<"抽取卡牌的数量"<<draw_num<<endl;
        SendingEffect11_draw_card(self_index,card_num,cards);
    }
    
    
    int remain=to_draw-draw_num;
    if(remain>0){       //未抽足足够的卡
        if(Debug_mode)
            cout<<"未抽足足够的卡片，继续抽卡"<<endl;
        //check if 放逐
        if(player_fight[self_index]->exile_card.size()<3) {     //放逐区未满，对手放逐
            if(Debug_mode)
                cout<<"对手玩家开始卡牌的放逐"<<endl;
            WaitClientReady10();
  //          sleep(1);
            SendPickStat6(!self_index, Exile, Still);
            if(player_fight[!self_index]->if_exile) {       //玩家选择了放逐牌
                //进行放逐处理
                player_fight[self_index]->discard_card.erase(player_fight[!self_index]->pick_card_num);
                player_fight[self_index]->exile_card.push_back(player_fight[!self_index]->pick_card_num);
                SendingPickCard5(self_index, Exile, player_fight[!self_index]->pick_card_num);  //向另一方发送出卡信息
                sleep(1);
            }
        }
        if(Debug_mode)
            cout<<"进行卡组重置操作"<<endl;
        //进行卡组重置，即把坟墓区的牌清洗后放入待抽卡区，再次进行抽卡
        for (auto card_i : player_fight[self_index]->discard_card)
            player_fight[self_index]->wait_card[(player_fight[self_index]->wait_card_size)++] = card_i;
        player_fight[self_index]->discard_card.clear();
        //对卡组进行洗牌
        rand_vector(player_fight[self_index]->wait_card, player_fight[self_index]->wait_card_size);
        
   //     sleep(2);
        if(Debug_mode)
            cout<<"发送洗牌效果"<<endl;
        SendingEffect11(self_index, EnergyCardNum, 12, 0);        //发送洗牌效果
        
        usleep(300000);
        //再次进行抽卡
        cards.clear();
        if(Debug_mode)
            cout<<"此次玩家抽卡的卡牌数量为"<<remain<<endl;
        for (int i = 0; i <remain; i++) {
            int card_num = player_fight[self_index]->wait_card[--player_fight[self_index]->wait_card_size];
            player_fight[self_index]->handCard.insert(card_num);
            cards.push_back(card_num);
        }
    //    sleep(2);
        SendingEffect11_draw_card(self_index,EnergyCardNum,cards);
    }
    
}
void Fight::SendingDefaultShater14(int pre_index,int card_num){
    if(Debug_mode)
        cout<<"客户端超时未选择破灭牌，此为系统为玩家随机破灭的牌"<<endl;
    char s[3];
    s[0]=3;
    s[1]=14;
    s[2]=card_num;
    int socket=player_fight[pre_index]->socket;
    wwriten(socket,s,3);
}


 




