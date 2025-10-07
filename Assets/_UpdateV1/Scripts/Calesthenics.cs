using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calesthenics : MonoBehaviour
{
    public CalesthenicsType calesthenicsType;
    public bool hasStartedCalesthenics;
    // Start is called before the first frame update
    void Start()
    {
        
    }
}

public enum CalesthenicsType
{
    Pushups,
    PullUps,
}
