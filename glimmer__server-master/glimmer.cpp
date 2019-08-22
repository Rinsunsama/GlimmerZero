// glimmer.cpp: 定义控制台应用程序的入口点。
//
#include "stdafx.h"
#include "header.h"
#include "card.h"
#include "fight.h"
#include "vari.h"

void Initiate_card();
void Delete_card();

int main()
{

	Initiate_card();		//对CardAll做初始化

	Delete_card();			//删除CardAll数组
    return 0;
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
	CardAll[0][10] = new Card100;
	CardAll[0][11] = new Card111;
	CardAll[0][12] = new Card111;
	CardAll[0][13] = new Card113;
	CardAll[0][14] = new Card114;
	CardAll[0][15] = new Card115;
	CardAll[0][16] = new Card116;
	CardAll[0][17] = new Card116;
	CardAll[0][18] = new Card100;
	CardAll[0][19] = new Card100;
	CardAll[0][20] = new Card100;
	CardAll[0][21] = new Card100;
	CardAll[0][22] = new Card100;
	CardAll[0][23] = new Card100;
	CardAll[0][24] = new Card100;

	CardAll[1][0] = new Card200;
	CardAll[1][1] = new Card200;
	CardAll[1][2] = new Card202;
	CardAll[1][3] = new Card202;
	CardAll[1][4] = new Card204;
	CardAll[1][5] = new Card205;
	CardAll[1][6] = new Card205;
	CardAll[1][7] = new Card207;
	CardAll[1][8] = new Card207;
	CardAll[1][9] = new Card209;
	CardAll[1][10] = new Card210;
	CardAll[1][11] = new Card211;
	CardAll[1][12] = new Card212;
	CardAll[1][13] = new Card213;
	CardAll[1][14] = new Card214;
	CardAll[1][15] = new Card215;
	CardAll[1][16] = new Card215;
	CardAll[1][17] = new Card217;
}

void Delete_card() {
	for (int i = 0; i<2; i++)
		for (int j = 0; j < Hero_max_card; j++) {
			delete CardAll[i][j];
		}
}

