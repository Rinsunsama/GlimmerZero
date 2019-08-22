
//  write.cpp
//  glimmer
//
//  Created by Apple on 2018/8/4.
//  Copyright © 2018年 Hypoxa. All rights reserved.
//

#include <stdio.h>

#include <iostream>
#include <unistd.h>


using std::cout;
using std::endl;

ssize_t                        /* Write "n" bytes to a descriptor. */
wwriten(int fd, const char *vptr, size_t n)
{
    
 //   size_t        nleft, nwritten;
    const char    *ptr;
    
    cout<<"to socket: "<< fd<<", ";
    ptr = (const char*)vptr;    /* can't do pointer arithmetic on void* */
    write(fd,vptr,n);
  //  nleft = n;
    
    cout<<"write_message: ";
    for(int i=0;i<n;i++)
        cout<<(int)vptr[i]<<' ';
    cout<<endl; /*
    while (nleft > 0) {
        if ( (nwritten = write(fd, ptr, nleft)) <= 0)
            return (nwritten);
        cout<<"nwritten"<<nwritten<<' ';
        cout<<"nleft"<<nleft<<endl;
        nleft -= nwritten;
        ptr   += nwritten;

    }*/
    return(n);
}
