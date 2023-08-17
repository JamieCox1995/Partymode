using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TempRoundTimer : MonoBehaviour
{
    public TextMeshProUGUI timerTextDisplay;
    public string displayString = "<size=15><#FFC600>Time Left</size></color> {0}:{1}";

    public float timeSince = 0f;
	
	// Update is called once per frame
	void Update ()
    {
        timeSince = Time.time;

        string minutes = Mathf.Floor(timeSince / 60f).ToString("00");
        string seconds = (timeSince % 60f).ToString("00");

        timerTextDisplay.text = string.Format(displayString, minutes, seconds);
	}
}
