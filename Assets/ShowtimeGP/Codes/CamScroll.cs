using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScroll : MonoBehaviour
{

    public bool backCam;
    // Update is called once per frame

    private void Update() {
        if(backCam){

        if(transform.localPosition.z>=-4.54 && transform.localPosition.z<=-1.34){
            transform.localPosition += new Vector3(0,0,Input.GetAxis("Mouse ScrollWheel"));
        }
        if(transform.localPosition.z<-4.54f){
            transform.localPosition = new Vector3(0,0,-4.54f);
        }
        if(transform.localPosition.z>-1.34f){
            transform.localPosition = new Vector3(0,0,-1.34f);
        }

        }
        if(!backCam){
            
        if(transform.localPosition.z<=4.54 && transform.localPosition.z>=1.34){
            transform.localPosition -= new Vector3(0,0,Input.GetAxis("Mouse ScrollWheel"));
        }
        if(transform.localPosition.z>4.54f){
            transform.localPosition = new Vector3(0,0,4.54f);
        }
        if(transform.localPosition.z<1.34f){
            transform.localPosition = new Vector3(0,0,1.34f);
        }

        }
    }
}
