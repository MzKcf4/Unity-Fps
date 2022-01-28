using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UiFpsCounter : MonoBehaviour
{
    private float timer, refresh, avgFramerate;
    private TextMeshProUGUI text;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        float timelapse = Time.smoothDeltaTime;
        timer = timer <= 0 ? refresh : timer -= timelapse;

        if (timer <= 0) avgFramerate = (int)(1f / timelapse);
        text.text = avgFramerate.ToString();
        
    }
}
