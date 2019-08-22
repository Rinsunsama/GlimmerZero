using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
public class VSPanel : MonoBehaviour {

     

    public Transform vsPanel;           //VS面板
    public Image mCharacter;        //我方角色
    public Image uCharacter;        //对方角色
    public Transform vsImage;       //VS图标
    public Transform light;
    public Transform vsLight;

    public Sprite[] mCharacterSprite;   //我方角色sprite
    public Sprite[] uCharacterSprite;   //对方角色sprite

    public Transform mFinalPos;        //我方角色终点位置            
    public Transform uFinalPos;        //对方角色终点位置
     
    public Transform mStartPos;        //我方角色初始位置
    public Transform uStartPos;        //对方角色初始位置


	// Use this for initialization
	void Start () {
     //   StartCoroutine("ShowMatchSucess");
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public  IEnumerator ShowMatchSucess()
    {
        
        mCharacter.sprite = mCharacterSprite[GameManager.mSelectedCardGroup];
        uCharacter.sprite = uCharacterSprite[GameManager.uSelectedCardGroup];
       // mCharacter.sprite = mCharacterSprite[0];
       // uCharacter.sprite = uCharacterSprite[1];
        Tweener mTTweener = mCharacter.transform.DOMove(mFinalPos.position, 0.6f);
        mTTweener.SetEase(Ease.InCirc);
        AudioManager.SoundEffectPlay("se_headportrait");
        

        yield return new WaitForSeconds(0.6f);
        mCharacter.transform.DOMove(mFinalPos.position + new Vector3(100, 0, 0), 10.0f);
        mCharacter.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 20.0f);
        Tweener uTTweener = uCharacter.transform.DOMove(uFinalPos.position, 0.6f);
        uTTweener.SetEase(Ease.InCirc);
        AudioManager.SoundEffectPlay("se_headportrait");

        yield return new WaitForSeconds(0.6f);
        uCharacter.transform.DOMove(uFinalPos.position + new Vector3(-100, 0, 0), 10.0f);
        uCharacter.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 20.0f);
        vsImage.localScale = Vector3.one * 3;
        Tweener vsSTweener = vsImage.DOScale(Vector3.one, 0.5f);
        vsSTweener.SetEase(Ease.InOutBack);
        AudioManager.SoundEffectPlay("se_headportrait");

        yield return new WaitForSeconds(0.5f);
        AudioManager.SoundEffectPlay("se_vs");
        light.gameObject.SetActive(true);
        light.transform.DORotate(new Vector3(20, 20, 180), 20f);
        vsLight.localScale = Vector3.one * 2;
        Tweener vsLSTweener = vsLight.DOScale(Vector3.one, 0.2f);
        vsLSTweener.SetEase(Ease.InBounce);
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("Loading");
      
    }

    public void StartShowMatchSucess()
    {
        StartCoroutine("ShowMatchSucess");
    }
}
