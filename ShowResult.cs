using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowResult : MonoBehaviour {

	// Use this for initialization
	void Start () {
        string showText = "游戏结束\n";

        if (Result.result == 1)
            showText += "恭喜绿方获胜";
        else if (Result.result == 2)
            showText += "恭喜红方获胜";
        else if (Result.result == 3)
            showText += "双方平局";
        else
            showText += "对局结果异常";


        this.GetComponent<Text>().text = showText;

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
