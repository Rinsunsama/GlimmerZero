//
//  fight.hpp
//  glimmer
//
//  Created by Apple on 2018/8/7.
//  Copyright © 2018年 Hypoxa. All rights reserved.
//

#ifndef communi_hpp
#define communi_hpp

//  用户发送对战相关，或许是心跳，亦或者是出牌
#include <stdio.h>
void conn();
void process_message(int sockfd,char *buf,size_t len);
void shutdown(int sockfd);          //  shutdown a socket
void player_off_line(int sockfd);   //  当用户关闭端口或者用户掉线

#endif /* fight_hpp */
