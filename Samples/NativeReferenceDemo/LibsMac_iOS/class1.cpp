//
//  class1.cpp
//  libLibStatic_iOS
//
//  Created by Alex on 7/5/24.
//

#include "class1.hpp"
extern "C" {
int clickCount = 666;
int getClickCount() {
    return clickCount++;
}
}
