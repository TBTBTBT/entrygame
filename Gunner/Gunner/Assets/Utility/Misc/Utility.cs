using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ToaUtil {
    //trueが帰るまで指定回数繰り返し
    public static void LoopWhile(Func<bool> cb,int count)
    {
        for (int i = 0; i < count; i++)
        {
            if(cb())return;
        }
    }
}
