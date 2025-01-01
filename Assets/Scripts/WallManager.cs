using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    Renderer renderer;
    private float cooltime;
    private float cooltimemax = 0.5f;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        if (renderer != null)
        {

            Color thiscolor = renderer.material.color;
            thiscolor.a = 0.5f;
            renderer.material.color = thiscolor;
        }
        else
        {
            Debug.LogWarning("Rendererがアタッチされていません");
        }
    }

    void Update()
    {
        cooltime += Time.deltaTime;
        if (renderer.material.color.a > 0.5f && cooltime > cooltimemax)
        {
            Color thiscolor = renderer.material.color;
            thiscolor.a -= 0.01f;
            renderer.material.color = thiscolor;

        }
        else if (cooltime <= cooltimemax)
        {
            
        }
        else 
        {
            Color thiscolor = renderer.material.color;
            thiscolor.a = 0.5f;
            renderer.material.color = thiscolor;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Color thiscolor = renderer.material.color;
        thiscolor = Color.green;
        renderer.material.color = thiscolor;
        cooltime = 0;
        
    }
}
