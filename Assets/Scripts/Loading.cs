using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Loading : MonoBehaviour {


    public static string loadSceneName;
    AsyncOperation async;
	// Use this for initialization
	void Start () {
        StartCoroutine(loadScene(loadSceneName));

    }

//注意这里返回值一定是 IEnumerator
IEnumerator loadScene(string loadName)
{
    //异步读取场景。
    //Globe.loadName 就是A场景中需要读取的C场景名称。
    yield return new WaitForSeconds(3);
    async = SceneManager.LoadSceneAsync(loadName);
    //读取完毕后返回， 系统会自动进入C场景
    yield return async;

}

}
