/******************************************************************************
 *
 *                      GeaVR
 *                https://www.geavr.eu/
 *             https://github.com/GeaVR/GeaVR
 * 
 * GeaVR is an open source software that allows the user to experience a wide 
 * range of geological and geomorphological sites in immersive virtual reality,
 * including data collection.
 *
 * Main Developers:      
 * 
 *     Fabio Luca Bonali (fabio.bonali@unimib.it)
 *     Martin Kearl (martintkearl@gmail.com)
 *     Fabio Roberto Vitello (fabio.vitello@inaf.it)
 * 
 * Developed thanks to the contribution of following projects:
 *
 *     ACPR15T4_ 00098 “Agreement between the University of Milan Bicocca and the 
 *     Cometa Consortium for the experimentation of cutting-edge interactive 
 *     technologies for the improvement of science teaching and dissemination” of 
 *     Italian Ministry of Education, University and Research (ARGO3D)
 *     PI: Alessandro Tibaldi (alessandro.tibaldi@unimib.it)
 *     
 *     Erasmus+ Key Action 2 2017-1-UK01-KA203- 036719 “3DTeLC – Bringing the  
 *     3D-world into the classroom: a new approach to Teaching, Learning and 
 *     Communicating the science of geohazards in terrestrial and marine 
 *     environments”
 *     PI: Malcolm Whitworth (malcolm.Whitworth@port.ac.uk)
 * 
 ******************************************************************************
 * Copyright (c) 2016-2022
 * GPL-3.0 License
 *****************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityStandardAssets.Characters.FirstPerson;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class CustomFPCharacter : MonoBehaviour
{
    [SerializeField] private bool m_IsWalking;
    [SerializeField] public float m_WalkSpeed;
    [SerializeField] public float m_RunSpeed;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private MouseLook m_MouseLook;
    [SerializeField] private bool m_UseFovKick;
    [SerializeField] private FOVKick m_FovKick = new FOVKick();
    [SerializeField] private bool m_UseHeadBob;
    [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
    [SerializeField] private float m_StepInterval;
    [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    public Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    public Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;


    public Vector3 pos;
    public int HEIGHT = 0;
    public bool isOculusTouch = false;
    public GameObject OculusTouchRight, OculusTouchLeft;
    public Vector3 desiredMove;

    private bool isMovingY = false;
    private bool isTooHigh = false;
    public float flySpeed = 100.0f;
    public float flyYVectorTrigger = 0.3f;  //if the input vector.y exceeds this value then the character will start flying 
    public float maxFlyHeight = 3.0f;

    public float ROTATIONSMOOTHINGSCALINGFACTOR = 3.0f, ROTATIONSMOOTHINGTIME = 0.5F, ROTATIONCAP = 360.0f, ROTATION_SPEED_VALUE=1;
    public bool isRWTCrunning = false;

    public float joystickDeadzone = 0.2f;

    // Use this for initialization
    public void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
        m_MouseLook.Init(transform, m_Camera.transform);


    }


    // Update is called once per frame
    private void Update()
    {
        if (Time.deltaTime > 0 && !isRWTCrunning) RotateView();
        // the jump state needs to read here to make sure it is not missed
        if (!m_Jump)
        {
            //m_Jump = CrossPlatformInputManager.GetButtonDown("Jump"); // we can fly, do we need to jump?
        }

        // just landed
        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            StartCoroutine(m_JumpBob.DoBobCycle());
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
            isMovingY = false;
        }

        // On the ground for consecutive ticks
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded && !isMovingY)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }


    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }


    private void FixedUpdate()
    {
        float speed;

        //Get input returns local translation values. horizontal and vertical, realtive to the character transform
        GetInput(out speed);

        // always move along the camera forward as it is the direction that it being aimed at        
        desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;               

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                            m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        // get input does this, projecting hear removes variable joystick values
        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;

        // low level flying
        if (isMovingY)
        {            
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, maxFlyHeight + (HEIGHT * 0.5f)))
            {
                isTooHigh = false;                
            }
            else
            {
                if (!isTooHigh)
                {
                    m_MoveDir.y = 0f;
                    isTooHigh = true;
                }
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }            
        }
        // walking
        else if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
            }
        }
        // falling
        else 
        {    
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }

        // Do movement
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);

        m_MouseLook.UpdateCursorLock();

        if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.3f && isRWTCrunning == false)
        {
            StartCoroutine(RotateWithTriggerCoroutine());
        }
    }


    private void PlayJumpSound()
    {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }


    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                            Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }


    private void PlayFootStepAudio()
    {
    }


    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;
        if (!m_UseHeadBob)
        {
            return;
        }
        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
        {
            m_Camera.transform.localPosition =
                m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                    (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
        }
        else
        {
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
        }
        m_Camera.transform.localPosition = newCameraPosition;
    }


    private void GetInput(out float speed)
    {
        // Read input

        float horizontal = 0.0f;
        float vertical = 0.0f;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {            
            // XZ movement
            Vector2 move2d = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            Vector3 desiredDirection = Vector3.forward * move2d.y + Vector3.right * move2d.x;

            desiredDirection = OculusTouchRight.transform.TransformDirection(desiredDirection);

            if (desiredDirection.y > flyYVectorTrigger)
            {
                isMovingY = true;
                m_Jump = false;
                m_Jumping = false;
            }

            if (!isMovingY)
            {
                desiredDirection = Vector3.ProjectOnPlane(desiredDirection, Vector3.up);
            }

            desiredDirection = transform.worldToLocalMatrix * desiredDirection;
            horizontal = desiredDirection.x;
            vertical = desiredDirection.z;
        }
        else
        {
            horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            vertical = CrossPlatformInputManager.GetAxis("Vertical");
        }
        
        bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
        m_Input = new Vector2(horizontal, vertical);


        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }
    }


    private void RotateView()
    {
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {
            if (Mathf.Abs(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x) > joystickDeadzone)
            {
                transform.Rotate(0.0f, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x * m_MouseLook.XSensitivity*ROTATION_SPEED_VALUE, 0.0f);
            }
        }
        else
        {
            m_MouseLook.LookRotation(transform, m_Camera.transform);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }

    IEnumerator RotateWithTriggerCoroutine()
    {
        isRWTCrunning = true;
        float startAngle = transform.eulerAngles.y;

        float angleToSet = 0;
        float diffAngle = 0;
        float prevDiffAngle = 0;
        Vector3 startTouchVector = transform.worldToLocalMatrix * OculusTouchRight.transform.forward;

        float smoothingVelocity = 0.0F;

        while (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.3f)
        {
            angleToSet = startAngle - diffAngle;
            angleToSet = (angleToSet < 0) ? (360 + angleToSet) : angleToSet;

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, angleToSet, transform.eulerAngles.z);
            m_Camera.transform.eulerAngles = new Vector3(m_Camera.transform.eulerAngles.x, angleToSet, m_Camera.transform.eulerAngles.z);

            // interpolate to smooth flickering due to input noise
            prevDiffAngle = diffAngle;
            float targetAngle = Vector3.SignedAngle(startTouchVector, transform.worldToLocalMatrix * OculusTouchRight.transform.forward, Vector3.up);
            diffAngle = Mathf.SmoothDampAngle(prevDiffAngle, targetAngle * ROTATIONSMOOTHINGSCALINGFACTOR, ref smoothingVelocity, ROTATIONSMOOTHINGTIME);

            diffAngle = (diffAngle > 180) ? (180 - diffAngle) : diffAngle;
            diffAngle = Mathf.Clamp(diffAngle, -ROTATIONCAP, ROTATIONCAP);
            yield return new WaitForEndOfFrame();
        }
        isRWTCrunning = false;

        // re- initialise MouseLook
        m_MouseLook.Init(transform, m_Camera.transform);

        print("Rotated");
    }
}

