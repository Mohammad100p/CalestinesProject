using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BarCollBool : MonoBehaviour
{
    public static bool can_hang = false;
    public static bool ready_Now;
    // Update is called once per frame
    void OnTriggerStay(Collider col){
        if(ready_Now){
        if(col.gameObject.tag == "Player"){
            can_hang = true;
            col.transform.position = new Vector3(transform.position.x,transform.position.y,transform.position.z);
            MovementTest.canMove = false;
            col.transform.forward=transform.forward;
        }
        }
    }
    /*void OnTriggerExit(Collider col){
        if(col.gameObject.tag == "Player"){
            can_hang = false;
        }
    }*/
    public void Update() {
        if(ready_Now == false){
            StartCoroutine("WaitTimeNow");
        }
    }

    public IEnumerator WaitTimeNow (){
        yield return new WaitForSeconds(0.5f);
        ready_Now = true;
    }
    
}
