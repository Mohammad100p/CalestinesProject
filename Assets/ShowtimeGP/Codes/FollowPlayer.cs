using System;
using System.Collections;
using System.Collections.Generic;
/*using Unity.Mathematics;*/
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] Transform Player;
    [SerializeField] float MouseSpeed = 3;
    [SerializeField] float orbitDamping = 10;

    Vector3 localRot;

    public bool followPlayer=true;

    void Update()
    {
        if(followPlayer){
        transform.position = Player.position;

        localRot.x += Input.GetAxis("Mouse X") * MouseSpeed;
        localRot.y -= Input.GetAxis("Mouse Y") * MouseSpeed;

        localRot.y = Mathf.Clamp(localRot.y,-10f,80f);

        Quaternion QT = Quaternion.Euler(localRot.y*MovementTest.camIndicater, localRot.x , 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, QT, Time.deltaTime * orbitDamping);
        }

    }
}
