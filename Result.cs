using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//这个类只用于存储结果的数据
public class Result {
    //存储游戏结果，可能的游戏结果为：暂时没有结果，绿赢，红赢，平局
    //这三种情况对应的数据应该是  0,1,2,3
    public static int result = 0;
}
