#ifndef tool_h
#define tool_h
//工具函数相关
//对一个vector数组做随机
void rand_vector(int arr[],int sz);

inline int min_num(int x,int y){
    return (x<=y)?x:y;
}
inline int max_num(int x,int y){
    return (x>=y)?x:y;
}

#endif
