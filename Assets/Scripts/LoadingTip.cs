using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LoadingTip : MonoBehaviour {

    private string[] tips = new string[5]
    {
        "手牌越多你的可选择性就越高，但手牌数量不能超过十张上限。",
        "绝对前置也不一定能让你获得先攻。",
        "善用连击牌可以在单回合打出极强的效果。",
        "多预测对方的出牌，打出对应的压制牌，可以助你一击必胜。",
        "高阶卡牌效果虽强，但速度很慢，也具有被打断的风险。"
    };
	// Use this for initialization
	void Start () {
        int tipIndex = Random.Range(0, 5);
        GetComponent<Text>().text = tips[tipIndex];
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
