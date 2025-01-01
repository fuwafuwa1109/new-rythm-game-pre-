using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotesManager : MonoBehaviour
{

    public float speed = 6.0f; //物体の速度
    public Rigidbody2D rb2d;
    private Vector2 InitialDirection = new Vector2(1, 3).normalized; //正規化された物体の速度ベクトル

    public float cooltime = 0;
    private float cooltimemax = 0.5f;
    private Renderer renderer;
    public bool iskeeping = false;
    Renderer parentRenderer;

    void Start()
    {
        
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.velocity = speed * InitialDirection;

        Transform childTransform = transform.GetChild(0); 
        renderer = childTransform.GetComponent<Renderer>();
        parentRenderer = GetComponent<Renderer>();
        
        ChangeNoteColor(Color.white, 0.5f, false);
        ChangeParentColor(Color.white, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (speed != 0)
        {
            Vector2 CurrentVelocity = rb2d.velocity;
            rb2d.velocity = CurrentVelocity.normalized * speed;
        }


        cooltime += Time.deltaTime;
        if (renderer.material.color.a > 0.4f && cooltime > cooltimemax && !iskeeping)
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
            thiscolor.a = 0.4f;
            renderer.material.color = thiscolor;
        }




    }

    public void ChangeNoteColor(Color newColor, float transparency, bool keeping)
    {
        newColor.a = transparency;
        renderer.material.color = newColor;
        cooltime = 0;
        iskeeping = keeping;
    }


    private void ChangeParentColor(Color newColor, float transparency)
    {
        newColor.a = transparency;
        parentRenderer.material.color = newColor;
    }

}
