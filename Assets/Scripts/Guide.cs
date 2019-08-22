using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : MonoBehaviour {


    public Transform[] guideImages;      //新手引导图
    public Transform guidePanel;

    public void OnContinueClick(int buttonIndex)
    {
        guideImages[buttonIndex].gameObject.SetActive(true);
    }

    public void OnIKnowClick()
    {
        guidePanel.gameObject.SetActive(false);
    }

    public void OnPreButtonClick(int buttonIndex)
    {
        guideImages[buttonIndex].gameObject.SetActive(false);
    }
}
