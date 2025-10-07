using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHide : MonoBehaviour
{
    public bool mouseVisible=false;
    void Start()
    {
        Cursor.visible = mouseVisible;
    }
    void makeMouseVisible(){
        Cursor.visible = true;
    }
    void makeMouseInvisible(){
        Cursor.visible = false;
    }

}
