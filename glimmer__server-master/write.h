//
//  write.h
//  glimmer2
//
//  Created by Apple on 2018/8/14.
//  Copyright © 2018年 Hypoxa. All rights reserved.
//

#ifndef write_h
#define write_h

ssize_t                        /* Write "n" bytes to a descriptor. */
wwriten(int fd, const char *vptr, size_t n);

#endif /* write_h */
