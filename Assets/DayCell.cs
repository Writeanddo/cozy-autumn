using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayCell : MonoBehaviour
{
    int day = 1;
    public Text dayText;
    // Start is called before the first frame update
    void Start()
    {
        EventPool.OptIn("moveAStep", moveStep);
    }

    void moveStep()
    {
        day++;
        dayText.text = day.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}