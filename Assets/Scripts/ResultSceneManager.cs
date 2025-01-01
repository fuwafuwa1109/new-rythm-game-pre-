using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ResultSceneManager : MonoBehaviour
{
    [SerializeField] private Text PerfectNumber;
    [SerializeField] private Text GreatNumber;
    [SerializeField] private Text GoodNumber;
    [SerializeField] private Text MissNumber;

    void Start()
    {
        PerfectNumber.text = GameManager.PlayResult["Perfect"].ToString();
        GreatNumber.text = GameManager.PlayResult["Great"].ToString();
        GoodNumber.text = GameManager.PlayResult["Good"].ToString();
        MissNumber.text = GameManager.PlayResult["Miss"].ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
