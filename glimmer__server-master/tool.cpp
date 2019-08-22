#include "header.h"
#include "tool.h"
//using namespace std;
using std::swap;
//对一个vector数组做随机
void rand_vector(int arr[],int sz) {
	for (int i = 0; i < sz; i++) {
		int target_index = rand() % sz;
		swap(arr[i], arr[target_index]);
	}
}


