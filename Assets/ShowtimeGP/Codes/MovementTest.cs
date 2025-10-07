 using System;
using System.Collections;
/*using Cinemachine.Utility;*/
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovementTest : MonoBehaviour
{
    public float moveSpeed = 5f;         // Normal speed of movement
    private float moveSpeedmem;
    public float sprintSpeed = 10f;      // Speed when sprinting
    private float sprintSpeedmem;
    public float jumpForce = 7f;         // Force applied when jumping
    public Transform groundCheck;        // Position to check if the player is grounded
    public LayerMask groundLayer;        // Layer mask to define what is considered ground
    private Rigidbody rb;                // Reference to the Rigidbody component
    public bool isGrounded;             // Is the player on the ground?
    private float groundCheckRadius = 0.2f;  // Radius of the ground check sphere
    public Animator playerAnim;
    private CapsuleCollider cp;
    

    public LayerMask Ledge;
    public LayerMask PBar;

    private bool walkingRnfrwd;
    private bool walkingbk;

    private bool hanging;
    public static bool canMove = true;

    public bool Bflipin, Fflipin;
    private bool didajump;

    private bool walkingfrwd;

    public bool walkS2S;
    private bool nowJumpinOnHit;
    private SphereCollider sp;
    private float timeToHit=1;
    public bool CamMoveStoper;





    [SerializeField] Transform cam;


    void Start()
    {
        sp=GetComponent<SphereCollider>();
        rb = GetComponent<Rigidbody>();
        cp = GetComponent<CapsuleCollider>();
        moveSpeedmem=moveSpeed;
        sprintSpeedmem = sprintSpeed;
        
    }

    void Update()
    {

        if(!isGrounded&&!hanging){
            rb.AddForce(new Vector3(0,-10.81f,0));
        }

        if(canMove){
        Move();
        }
        CheckGrounded();
        Jump();
        Flip();
        LedgeGrab();
        CamMove();
        AnimateAll();
        Crawling();
        CamSwitch();
        //PBGrab();
        PusGra();


    }

    void Move()
    {
        // Get input for movement
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        


        // Determine the current speed, disable sprinting when moving backward
        float currentSpeed = (moveVertical > 0 && Input.GetKey(KeyCode.LeftShift) && moveHorizontal==0) ? sprintSpeed : moveSpeed;

        // Calculate movement direction relative to the character's facing direction
        Vector3 movement = transform.forward * moveVertical * currentSpeed * verMult + transform.right * moveHorizontal * currentSpeed * horiMult;

        // Apply movement to the Rigidbody
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        // Update walking state
        walkingRnfrwd = currentSpeed > moveSpeed;
        walkingfrwd = moveSpeed == moveVertical * currentSpeed;
        
    }

    void CheckGrounded()
    {
        // Check if the player is on the ground
        isGrounded = !Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer)==false;
    }



    void Jump()
    {


        if(hanging && Input.GetButtonDown("Jump")){
            rb.useGravity = true;
            hanging = false;
            StartCoroutine(EnableCanMove(0.25f));
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        if(PBGrabing && Input.GetButtonDown("Jump")){
            rb.useGravity = true;
            PBGrabing = false;
            StartCoroutine(EnableCanMove(0.25f));
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            cp.isTrigger = false;
        }
        if(BarCollBool.ready_Now&& PBGrabing && Input.GetButtonDown("Jump")){
            rb.AddForce(Vector3.up * jumpForce*Time.deltaTime, ForceMode.Impulse);
        }


        // Jump if the player is on the ground and the jump button is pressed
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            stopThatFlip=false;
        }

        // Handle jump animation
        if(isGrounded){
            didajump=false;
        }else{
            didajump=true;
        }
        
        
        
    }

    void OnDrawGizmosSelected()
    {
        // Draw a sphere at the ground check position to visualize the ground check radius
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);
    }

public float flipJumpmult;
    void Flip()
    {
        float flipTwrds = Input.GetAxis("Flip");
        if (flipTwrds > 0 && !Fflipin && !Bflipin && !isGrounded)
        {
            Fflipin = true;
        }
        if (flipTwrds < 0 && !Fflipin && !Bflipin && !isGrounded)
        {

            Bflipin = true;
        }
    }

    void LedgeGrab()
    {
        if (rb.velocity.y < 0 && !hanging)
        {
            RaycastHit downHit;

            Vector3 lineDownStart = (transform.position + Vector3.up * .95f) + transform.forward;
            Vector3 lineDownEnd = (transform.position + Vector3.up * .7f) + transform.forward*-.25f;

            if (Physics.Linecast(lineDownStart, lineDownEnd, out downHit, Ledge))
            {
                Debug.DrawLine(lineDownStart, lineDownEnd, Color.red,2,false);

                if (downHit.collider != null)
                {
                    RaycastHit fwdHit;

                    Vector3 lineFwdStart = new Vector3(transform.position.x, downHit.point.y - 0.1f, transform.position.z);
                    Vector3 lineFwdEnd = lineFwdStart + transform.forward;

                    if (Physics.Linecast(lineFwdStart, lineFwdEnd, out fwdHit, Ledge))
                    {
                        Debug.DrawLine(lineFwdStart, lineFwdEnd, Color.blue,2,false);

                        if (fwdHit.collider != null)
                        {
                            
                            // Ledge grab detected, disable gravity and stop movement
                            rb.useGravity = false;
                            rb.velocity = Vector3.zero;

                            hanging = true;

                            // Adjust player position and orientation
                            Vector3 hangPos = new Vector3(fwdHit.point.x, downHit.point.y, fwdHit.point.z);
                            transform.position = hangPos;
                            transform.forward = -fwdHit.normal;
                            canMove = false;
                        }
                    }
                }
            }
        }
    }
    private bool PBGrabing;
    /*void PBGrab()
    {
        if(BarCollBool.can_hang){
        if (rb.velocity.y < 0 && !PBGrabing)
        {
            RaycastHit downHit;

            Vector3 lineDownStart = (transform.position + Vector3.up * .95f) + transform.forward;
            Vector3 lineDownEnd = (transform.position + Vector3.up * .7f) + transform.forward*-.25f;

            if (Physics.Linecast(lineDownStart, lineDownEnd, out downHit, PBar))
            {
                Debug.DrawLine(lineDownStart, lineDownEnd, Color.red,60,false);

                if (downHit.collider != null)
                {
                    RaycastHit fwdHit;
                    

                    Vector3 lineFwdStart = new Vector3(transform.position.x, downHit.point.y - 0.1f, transform.position.z);
                    Vector3 lineFwdEnd = lineFwdStart + transform.forward;

                    if (Physics.Linecast(lineFwdStart, lineFwdEnd, out fwdHit, PBar))
                    {
                        Debug.DrawLine(lineFwdStart, lineFwdEnd, Color.blue,2,false);

                        if (fwdHit.collider != null)
                        {
                            
                            // Ledge grab detected, disable gravity and stop movement
                            rb.useGravity = false;
                            rb.velocity = Vector3.zero;

                            PBGrabing = true;

                            // Adjust player position and orientation
                            Vector3 hangPos = new Vector3(fwdHit.point.x, downHit.point.y-0.06f, fwdHit.point.z);
                            transform.position = hangPos;
                            transform.forward = -fwdHit.normal;
                            canMove = false;
                            
                        }
                    }
                }
            } 
        }
        if(PBGrabing){
            cp.isTrigger = true;
        }
        }
    }*/

    IEnumerator EnableCanMove(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        canMove = true;
        nowJumpinOnHit = false;
    }

    void CamMove()
    {

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right*0;

        camForward.y = 0;
        camRight.y = 0;


        Vector3 moveDir = camForward + camRight;

        if(!hanging && !PBGrabing &&!CamMoveStoper &&!pushupswitch){
        transform.forward = new Vector3(moveDir.x,0,moveDir.z);
        }

    }


private bool stopThatFlip;
private float timeAfterFlip=.3f;
    void AnimateAll(){
    // Handle animation for everything

    float moveHorizontal = Input.GetAxis("Horizontal");
    float moveVertical = Input.GetAxis("Vertical");

    if(!PBGrabing && !hanging && isGrounded && !isCrawling && moveHorizontal == 0f && moveVertical == 0f){

        playerAnim.SetBool("idle",true);
        Bflipin = false;
        Fflipin = false;


    }else{

        playerAnim.SetBool("idle",false);

    }

    playerAnim.SetBool("pullup",PBGrabing);

    if(PBGrabing){

            playerAnim.SetBool("Pulling Up",moveVertical==1);

    }

    playerAnim.SetFloat("VSpeed",moveVertical);
    playerAnim.SetFloat("HSpeed",moveHorizontal);


    if(!isCrawling) playerAnim.SetBool("Jump",didajump);
    

    playerAnim.SetBool("Running",walkingRnfrwd);

    playerAnim.SetBool("Hang",hanging);

    if(didajump && !isGrounded){

        if(Bflipin && !Fflipin){

            playerAnim.SetBool("Bflip",!stopThatFlip);

            if(!stopThatFlip){
            timeAfterFlip -= Time.deltaTime;

            if(timeAfterFlip < 0){
                timeAfterFlip = .3f;
                stopThatFlip = true;
                Fflipin = false;
                Bflipin = false;
                playerAnim.SetBool("Bflip",false);
            }
            }

        }

        if(Fflipin && !Bflipin){

            playerAnim.SetBool("Fflip",!stopThatFlip);

            if(!stopThatFlip){
            timeAfterFlip -= Time.deltaTime;

            if(timeAfterFlip < 0){
                
                timeAfterFlip = .3f;
                stopThatFlip = true;
                Fflipin = false;
                Bflipin = false;
                playerAnim.SetBool("Fflip",false);

            }
            }

        }

        if(walkingRnfrwd && isGrounded){
            Fflipin = false;
            Bflipin = false;
        }

    }else{

        playerAnim.SetBool("Jump",false);
        playerAnim.SetBool("Bflip",false);
        playerAnim.SetBool("Fflip",false);

    }



    if(isCrawling){

    playerAnim.SetBool("Crawl",true);


    playerAnim.SetFloat("VSpeed",moveVertical);

    }else{

        playerAnim.SetBool("Crawl",false);

    }

    playerAnim.SetBool("PushUpStay",pushupswitch);
    playerAnim.SetBool("DoingPushUp",moveVertical==1);

    }


public bool isCrawling;
private float horiMult;
private float verMult;

    void Crawling(){
        
    float moveHorizontal = Input.GetAxis("Horizontal");
    float moveVertical = Input.GetAxis("Vertical");

        if(!hanging && moveHorizontal==0 && moveVertical==0 && pushupswitch ==false){
            if(Input.GetKeyDown(KeyCode.Z)){
                isCrawling = !isCrawling;
            }
            if(isCrawling && !hanging && pushupswitch == false){

            cp.isTrigger = true;
            moveSpeed = moveSpeedmem/3;
            sprintSpeed = 1;
            groundCheckRadius = 0;
            horiMult = 0;
            verMult = 1;
        }
            if(!isCrawling && !hanging && pushupswitch ==false){

                horiMult = 1;
                cp.isTrigger = false;
                moveSpeed = moveSpeedmem;
                sprintSpeed = sprintSpeedmem;
                groundCheckRadius=0.2f;
                verMult = 1f;

            }
        }
        if(Input.GetKeyDown(KeyCode.X)){
            pushupswitch = !pushupswitch;
        }
        if(isCrawling && !hanging && pushupswitch){
            moveSpeed=moveSpeedmem*0;
            sprintSpeed = 1;
            groundCheckRadius = 0;
            verMult = 0;
            horiMult = 0;
            cp.isTrigger = true;
        }
        
    }

    private bool pushupswitch=false;

public static float camIndicater=1;
public GameObject cam1;
public GameObject cam2;

    void CamSwitch(){

        if(Input.GetButtonDown("Camera Switch")){
            camIndicater = -camIndicater;
        }

        if(camIndicater == 1){
             cam1.SetActive(true); cam2.SetActive(false);
        }

        if(camIndicater == -1){

        cam1.SetActive(false); cam2.SetActive(true);
        }
    }

    
    void PusGra(){
        if(BarCollBool.can_hang){
            rb.velocity = Vector3.zero;
            cp.isTrigger = true;
            rb.useGravity = false;
            PBGrabing = true;
            if(Input.GetKey(KeyCode.Space)){
                rb.useGravity = true;
                cp.isTrigger = false;
                PBGrabing = false;
                BarCollBool.can_hang = false;
                canMove = true;
                BarCollBool.ready_Now = false;
            }
        }
    }

}
