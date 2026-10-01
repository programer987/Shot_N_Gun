using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.WSA;

public class Player_Controller : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;
    public float airDrag;

    public float jumpForce;
    public float airMultiplier;
    private bool readyToJump;
    public float jumpCooldown;

    public float launchForce;
    public float superLaunchForce;
    
    private bool readyToLaunch;
    private bool readyToReload;
    public float launchCooldown;
    public float reloadCooldown;

    public int charges;
    private bool superCharged;
    private bool superChargeFired;
    public float chargingTimer;
    public float timeToCharge;
    public float timeToOverheat;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask isGround;
    private bool grounded;

    [Header("Keybinds")]
    public KeyCode jumpKey;
    public KeyCode launchKey;
    public KeyCode reloadKey;

    public Transform player_orientation;
    public Transform player_camera;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
    private Vector3 launchDirection;

    private Rigidbody rb;

    //Visual Variables
    [SerializeField] private MeshRenderer[] buttons = new MeshRenderer[3];
    [SerializeField] private Material chargedMat;
    [SerializeField] private Material unchargedMat;
    [SerializeField] private Material superchargeMat;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        readyToLaunch = true;
        readyToReload = true;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.1f, isGround);

        MyInput();

        if (grounded)
        {
            rb.drag = groundDrag;
        } else
        {
            rb.drag = airDrag;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        //what have you done
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown); //Why is this necessary?
        }
        
        #region Launch Code
        if (Input.GetKeyDown(launchKey) && charges > 0 && readyToLaunch && !superCharged)
        {
            readyToLaunch = false;

            Launch(launchForce);

            Invoke(nameof(ResetLaunch), launchCooldown);

        } else if (Input.GetKeyUp(reloadKey) && readyToLaunch && superCharged)
        {
            readyToLaunch = false;
            chargingTimer = 0;
            superChargeFired = true;

            Launch(superLaunchForce);

            Invoke(nameof(ResetLaunch), launchCooldown);
        }
        #endregion
        #region Reload Code
        if (Input.GetKeyUp(reloadKey) && readyToReload && charges < 3)
        {
            readyToReload = false;
            chargingTimer = 0;
            superChargeFired = false;

            Reload();

            Invoke(nameof(ResetLaunch), reloadCooldown);
            Invoke(nameof(ResetReload), reloadCooldown);

        } else if (Input.GetKeyUp(reloadKey) && readyToReload)
        {
            Dead();
            chargingTimer = 0;
            superChargeFired = false;
        }

        if (Input.GetKey(reloadKey) && !superChargeFired)
        {
            chargingTimer += Time.deltaTime;

            if (chargingTimer < timeToCharge)
            {
                readyToLaunch = false;
            } else
            {
                readyToLaunch = true;
            }
        }
        #endregion
        //idek what this means man
        if (chargingTimer >= timeToOverheat)
        {
            Dead(); //???
        }
        else if (chargingTimer >= timeToCharge)
        {
            UpdateGun(true);
            superCharged = true;
        }
        else
        {
            superCharged = false;
        }
    }

    private void MovePlayer()
    {
        moveDirection = player_orientation.forward * verticalInput + player_orientation.right * horizontalInput;

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        } else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
        
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void Launch(float force)
    {
        launchDirection = -player_camera.forward;

        rb.AddForce(launchDirection.normalized * force, ForceMode.Impulse);

        charges--;
        UpdateGun(false);
    }

    private void Reload()
    {
        charges++;
        UpdateGun(false);
    }

    private void ResetLaunch()
    {
        readyToLaunch = true;
    }

    private void ResetReload()
    {
        readyToReload = true;
    }

    public void Dead()
    {
        Debug.Log("You Ded");
    }

    private void UpdateGun(bool superCharged)
    {
        for(int i = 0; i < 3; i++)
        {
            if(superCharged) 
            {
                buttons[i].material = superchargeMat;
                continue;
            } 

            if(i >= charges)
            {
                buttons[i].material = unchargedMat;
                continue;
            }
            buttons[i].material = chargedMat;
        }
    }
}
