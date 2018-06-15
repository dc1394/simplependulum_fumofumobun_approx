/*! \file solveeomexemain.cpp
    \brief メイン関数

    Copyright © 2016-2018 @dc1394 All Rights Reserved.
    This software is released under the BSD 2-Clause License.
*/
#include "../solveeom/solveeommain.h"

int main()
{
    init(1.0f, 0.05f, 1.047197551f);
    saveresult(0.001, "deg_60.csv", 30.0);

    return 0;
}
