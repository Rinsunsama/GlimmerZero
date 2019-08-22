using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
public class AnimatorCtr : MonoBehaviour {

    public RuntimeAnimatorController[] animatorControllers;

    public enum UState       //对方角色动作状态
    {
        Idle,      //0
        Attack,    //1
        Attacked,  //2
        Die,       //3
        Win,       //4
        Win2,
    }
    private int nowState;
    public CustomArrays[] animationPos;
    public Sprite iceIdleImage0;

    private Animator uAnimator;
	// Use this for initialization
	void Start () {
        uAnimator = GetComponent<Animator>();
        nowState = (int)UState.Idle;
        if(GameManager.uSelectedCardGroup>=0)
         uAnimator.runtimeAnimatorController = animatorControllers[GameManager.uSelectedCardGroup];
    }
	
    public void SetAnimatorState(int state)
    {

        if (state >= 0 && state <= 5)
        { 
            nowState = state;
            uAnimator.SetInteger("state", state);
           
        }
      

        //  GetComponent<Image>().SetNativeSize();
        //  Invoke("SetIdle", 3.0f);
    }

    private void Update()
    {
        //  transform.position = Vector3.Lerp(transform.position, animationPos[GameManager.uSelectedCardGroup][nowState].transform.position, 0.02f);
        GetComponent<Image>().SetNativeSize();
    }

    // Update is called once per frame
    void FixedUpdate () {
       
    }

    public void SetPos()
    {
     // Debug.Log("SetPos");
     //    Debug.Log("2");
        GetComponent<Image>().SetNativeSize();
        if(GameManager.mSelectedCardGroup==0)
        {
            uAnimator.transform.GetComponent<Image>().sprite = iceIdleImage0;
        }
       
        GetComponent<Image>().SetNativeSize();
        transform.position = animationPos[GameManager.uSelectedCardGroup][nowState].transform.position;
        
    }




    [System.Serializable]
    public class CustomArrays
    {
        public GameObject[] Array;
        public GameObject this[int index]
        {
            get
            {
                return Array[index];
            }
        }
        public CustomArrays()
        {
            this.Array = new GameObject[1];
        }
        public CustomArrays(int index)
        {
            this.Array = new GameObject[index];
        }
    }

}

