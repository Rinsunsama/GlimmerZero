
#ifndef heart_beat_h
#define heart_beat_h
#include <unordered_map>

using std::unordered_map;
void *Timer_count(void *);

void send_hearbeat(int fd,unordered_map<int, int> &fd_count);
void recv_heartbeat(int fd);

#endif
