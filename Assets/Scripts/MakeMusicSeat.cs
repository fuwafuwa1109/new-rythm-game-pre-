using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Make : MonoBehaviour
{
    [SerializeField] private Button Button1;
    [SerializeField] private Button Button2;
    [SerializeField] private AudioClip music;

    public string csvName = "test.csv";
    private List<string> tapData;
    private int tapcount = 0;
    private float StartTime;
    private float GameTime;
    private bool WasPlaying = false;
    

    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        tapData = new List<string>();
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = music;

    }

    // Update is called once per frame
    void Update()
    {

        bool isPlaying = audioSource.isPlaying;


        if (Input.GetKeyDown(KeyCode.D)) //左フリック
        {
            FlickLeft();
        }

        if (Input.GetKeyDown(KeyCode.F))//左タップ
        {
            StartCoroutine(TapOrLongNote(KeyCode.F));
        }

        if (Input.GetKeyDown(KeyCode.J))//右タップ
        {
            StartCoroutine(TapOrLongNote(KeyCode.J));
        }

        if (Input.GetKeyDown(KeyCode.K))//右フリック
        {
            FlickRight();
        }

        if (!isPlaying && WasPlaying)
        {
            SaveCSV();
        }

        WasPlaying = isPlaying;


    }

    void FlickLeft()
    {
        if (audioSource.isPlaying)
        {
            tapcount++;
            float TapTime = Time.realtimeSinceStartup - StartTime;
            string taptimetext = TapTime.ToString("F2");
            tapData.Add($"button1,{taptimetext},flick");
        }
        else
        {
            audioSource.Play();
            StartTime = Time.realtimeSinceStartup;

        }

    }

    void FlickRight()
    {
        if (audioSource.isPlaying)
        {
            tapcount++;
            float TapTime = Time.realtimeSinceStartup - StartTime;
            string taptimetext = TapTime.ToString("F2");
            tapData.Add($"button2,{taptimetext},flick");
        }
        else
        {
            audioSource.Play();
            StartTime = Time.realtimeSinceStartup;

        }
    }

    public void SaveCSV()
    {
        string filePath = Path.Combine(Application.persistentDataPath, csvName);
        File.WriteAllLines(filePath, tapData);
        Debug.Log($"Data saved to {filePath}");
    }

    IEnumerator TapOrLongNote(KeyCode key)
    {
        tapcount++;
        float counttime = 0;
        float starttiming = Time.realtimeSinceStartup - StartTime;
        string starttext = starttiming.ToString("F2");
        if (key == KeyCode.F)
        {
            tapData.Add(($"button1,{starttext},tap"));
        }
        else if (key == KeyCode.J)
        {
            tapData.Add(($"button2,{starttext},tap"));
        }
        int startcount = tapcount;

        while (Input.GetKey(key))
        {
            counttime += Time.deltaTime;
            yield return null;
        }

        if(key == KeyCode.F)
        {
            if (counttime > 0.2f)
            {
                Debug.Log("counttime:" + counttime);
                tapcount++;
                float finishtiming = Time.realtimeSinceStartup - StartTime;
                string finishtext = finishtiming.ToString("F2");
                tapData[startcount - 1] = ($"button1,{starttext},longstart");
                tapData.Add(($"button1,{finishtext},longfinish"));
            }
            
        }

        if (key == KeyCode.J)
        {
            if (counttime > 0.2f)
            {
                Debug.Log("counttime:" + counttime);
                tapcount++;
                float finishtiming = Time.realtimeSinceStartup - StartTime;
                string finishtext = finishtiming.ToString("F2");
                tapData[startcount - 1] = ($"button2,{starttext},longstart");
                tapData.Add(($"button2,{finishtext},longfinish"));
            }

        }
    }

    
}
