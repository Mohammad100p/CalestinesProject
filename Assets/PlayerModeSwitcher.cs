using UnityEngine;
using System.Collections;

public class PlayerModeSwitcher : MonoBehaviour
{
    public GameObject groundPlayer;  // your ground player object
    public GameObject groundPlayerMesh;  // your ground player object
    public GameObject flyPlayer;     // your flying player object
    public GameObject flyPlayerMesh;     // your flying player object
    public Transform FlyCamera;
    public Transform GroundCamera;
    public string flyButton = "Fly"; // input name

    private bool isFlying = false;

    void Start()
    {
        // Ensure only ground starts active
        groundPlayer.SetActive(true);
        flyPlayer.SetActive(false);
    }

    void Update()
    {
        if (!isFlying)
        {
            FlyCamera.transform.position = GroundCamera.transform.position;
            FlyCamera.transform.rotation = GroundCamera.transform.rotation;
            flyPlayerMesh.transform.position = groundPlayerMesh.transform.position;
            flyPlayerMesh.transform.rotation = groundPlayerMesh.transform.rotation;
        }
        else
        {
            groundPlayerMesh.transform.position = flyPlayerMesh.transform.position;
            groundPlayerMesh.transform.rotation = flyPlayerMesh.transform.rotation;
            GroundCamera.transform.position = FlyCamera.transform.position;
            GroundCamera.transform.rotation = FlyCamera.transform.rotation;
        }
        if (Input.GetButtonDown(flyButton))
        {
            if (!isFlying)
            {
                // Switch to fly immediately
                flyPlayerMesh.transform.position = groundPlayerMesh.transform.position;
                flyPlayerMesh.transform.rotation = groundPlayerMesh.transform.rotation;
                //FlyCamera.transform.position = GroundCamera.transform.position;
                //FlyCamera.transform.rotation = GroundCamera.transform.rotation;

                SwitchToFly();
            }
            else
            {
                // Try to land
                groundPlayerMesh.transform.position = flyPlayerMesh.transform.position;
                groundPlayerMesh.transform.rotation = flyPlayerMesh.transform.rotation;
                //GroundCamera.transform.position = FlyCamera.transform.position;
                //GroundCamera.transform.rotation = FlyCamera.transform.rotation;

                StartCoroutine(SwitchToGroundWhenLanded());
            }
        }
    }

    void SwitchToFly()
    {
        // Copy position/rotation
        //flyPlayer.transform.position = groundPlayer.transform.position;
        //flyPlayer.transform.rotation = groundPlayer.transform.rotation;

        // Enable/disable
        groundPlayer.SetActive(false);
        flyPlayer.SetActive(true);

        isFlying = true;
    }

    IEnumerator SwitchToGroundWhenLanded()
    {
        //CapsuleCollider col = flyPlayer.GetComponent<CapsuleCollider>();

        //while (!IsGrounded(flyPlayer.transform.position, col))
        //{
        //    yield return null; // wait until we hit the ground
        //}

        // Copy position/rotation
        //groundPlayer.transform.position = flyPlayer.transform.position;
        //groundPlayer.transform.rotation = flyPlayer.transform.rotation;

        // Enable/disable
        yield return new WaitForSeconds(0.01f);
        flyPlayer.SetActive(false);
        groundPlayer.SetActive(true);

        isFlying = false;
    }

    private bool IsGrounded(Vector3 pos, CapsuleCollider col)
    {
        RaycastHit hit;
        float distance = col.height * 0.5f + 0.2f;
        return Physics.Raycast(pos, Vector3.down, out hit, distance);
    }
}
