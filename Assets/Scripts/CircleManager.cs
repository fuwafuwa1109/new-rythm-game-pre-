using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleManager : MonoBehaviour
{
    float ShrinkTime = 1f; //輪っかが縮むスピード
    float NowTime = 0; //輪っかが作られてからの時間
    [SerializeField] private GameObject Note;
    GameObject TrackObject;
    int whichbutton = 0;
    GameManager gameManager;



    void Start()
    {
        transform.localScale = new Vector3(6f, 6f, 1);
        gameManager = GameManager.Instance;
        
    }

    void Update()
    {
        if (TrackObject != null)
        {
            transform.position = TrackObject.transform.position; // TrackObjectと同じ座標にする
        }
        NowTime += Time.deltaTime;
        ShrinkCircle();
    }

    

    public void SetTrackButton(GameObject Note, int index)
    {
        TrackObject = Note;
        whichbutton = index;
    }

    void ShrinkCircle()
    {
        Vector3 CircleScale = transform.localScale; //輪っかを時間経過で縮ませる
        if (CircleScale.x > 1.0f) //輪っかが小さくなったらdestroy
        {
            CircleScale.x = 6f - 3.5f * NowTime / ShrinkTime;
            CircleScale.y = 6f - 3.5f * NowTime / ShrinkTime;
            transform.localScale = CircleScale;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void OnDestroy()
    {
        if (transform.localScale.x < 3.0f && transform.localScale.x > 2.0f)
        {
            GameManager.PlayResult["Perfect"]++;
        }
        else if ((transform.localScale.x < 3.5f && transform.localScale.x > 1.5f))
        {
            GameManager.PlayResult["Great"]++;
        }
        else if ((transform.localScale.x < 4.0f && transform.localScale.x > 1.0f))
        {
            GameManager.PlayResult["Good"]++;
        }
        else if (transform.localScale.x <= 1.0f)
        {
            GameManager.PlayResult["Miss"]++;
            gameManager.Combo = 0;
            gameManager.comboText.text = gameManager.Combo.ToString();
        }
        gameManager.Indexes[whichbutton]++;
    }
}
