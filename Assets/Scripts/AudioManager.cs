using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AudioManager : MonoBehaviour
{

    public static AudioManager _instance;

    public static float bgmVoluem = 0.5f;
    public static float soundEffectVoluem = 0.5f;
    


    public Slider bgmSlider; //背景音乐滑动条
    public Slider soundEffectSlider;  //音效滑动tioa

    private AudioSource bgm;

    private void Awake()
    {
        _instance = this;
    }
    // Use this for initialization
    void Start()
    {
        bgm = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }



    /// <summary>
    /// 在Bgm滑动条被改变时
    /// </summary>
    public void OnBgmSliderValueChanged()
    {
        bgmVoluem = bgmSlider.value;
        bgm.volume = bgmVoluem;
    }


    /// <summary>
    /// 在音效滑动条被改变时
    /// </summary>
    public void OnSoundEffectValueChanged()
    {
        soundEffectVoluem = soundEffectSlider.value;
    }



    /// <summary>
    /// 更换背景音乐
    /// </summary>
    /// <param name="clipName"></param>
    public static void BGMInstead(string clipName)
    {
        if(_instance.bgm == null)
        {
            _instance.bgm = _instance.GetComponent<AudioSource>();
        }
        if (Resources.Load("Audio/BGM/" + clipName) as AudioClip)
        {
           _instance.bgm.clip = Resources.Load("Audio/BGM/" + clipName) as AudioClip;
           _instance.bgm.Play();

        }
    }
    /// <summary>
    /// 播放音效
    /// </summary>
    public static void SoundEffectPlay(string clipName)
    {
        //Debug.Log("Audio/" + clipName);
        AudioSource SE = _instance.gameObject.AddComponent<AudioSource>();
        if (Resources.Load("Audio/SoundEffect/" + clipName))
        {
            SE.clip = Resources.Load("Audio/SoundEffect/" + clipName) as AudioClip;
            SE.loop = false;
            SE.Play();
            SE.volume = soundEffectVoluem;
        }
        Destroy(SE, 5.0f);
    }

}
