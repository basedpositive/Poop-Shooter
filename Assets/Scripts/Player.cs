using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using TMPro;

namespace Com.Zhan.SimpleFPSShoter 
{
public class Player : MonoBehaviourPunCallbacks, IPunObservable //!
{
    #region Variables

    public float speed;
    public float sprintModifier;
    public float jumpForce;
    public int max_health;
    public Camera normalCam;
    public GameObject cameraParent;
    public Transform weaponParent;
    public Transform groundDetector;
    public LayerMask ground;

    [HideInInspector] public ProfileData playerProfile;
    public TextMeshPro playerUsername;

    private Transform ui_healthbar;
    private Text ui_ammo;
    private Text ui_username;

    private Rigidbody rig;

    private Vector3 targetWeaponBobPosition;
    private Vector3 weaponParentOrigin;

    private float movementCounter;
    private float idleCounter;
    private float baseFOV;
    private float sprintFOVModifier = 1.5f;

    private int current_health;
    private Weapon weapon;

    private Manager manager;

    private float aimAngle;
    #endregion

    #region Photon Callbacks

    public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message){ //!
    if (p_stream.IsWriting) {
        p_stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100f));
    }
    else {
        aimAngle = (int)p_stream.ReceiveNext() / 100f;
    }
    } 

    #endregion

#region Monobehaviour Callbacks
    private void Start() 
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        weapon = GetComponent<Weapon>();
        current_health = max_health;

        cameraParent.SetActive(photonView.IsMine);

        if(!photonView.IsMine){ //!
             gameObject.layer = 11;
        }

        baseFOV = normalCam.fieldOfView;
        if(Camera.main) Camera.main.enabled = false;
        rig = GetComponent<Rigidbody>();
        weaponParentOrigin = weaponParent.localPosition;

        if(photonView.IsMine) {
        ui_healthbar = GameObject.Find("HUD/Health/Bar").transform;
        ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
        ui_username = GameObject.Find("HUD/Username/Text").GetComponent<Text>();

        RefreshHealthBar();
        ui_username.text = Launcher.myProfile.username;

        photonView.RPC("SyncProfile", RpcTarget.All, Launcher.myProfile.username, Launcher.myProfile.level, Launcher.myProfile.xp);
        }
    }
    
    private void Update() {
        if(!photonView.IsMine){
            RefreshMultiplayerState();
            return;
        }

        //Axis
        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");


        //Controls
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool pause = Input.GetKeyDown(KeyCode.Escape);


        //States
        bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool isJumping  = jump && isGrounded;
        bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;

        //Jumping
        if (isJumping) 
        {
            rig.AddForce(Vector3.up * jumpForce);
        }

        if(Input.GetKeyDown(KeyCode.U)) TakeDamage(100);

        //Head Bob
        if(t_hmove == 0 && t_vmove == 0) {
            HeadBob(idleCounter, 0.025f, 0.025f);
            idleCounter += Time.deltaTime;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
        else if (!isSprinting) {
            HeadBob(movementCounter, 0.035f, 0.035f); 
            movementCounter += Time.deltaTime * 3f; 
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
        else {
            HeadBob(movementCounter, 0.15f, 0.075f); 
            movementCounter += Time.deltaTime * 7f; 
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
        }
        // UI Refreshes
        RefreshHealthBar();
        weapon.RefreshAmmo(ui_ammo);
        
    }

    void FixedUpdate() 
    {
        if(!photonView.IsMine) return;

        //Axis
        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");


        // Controls
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool pause = Input.GetKeyDown(KeyCode.Escape);


        // States
        bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool isJumping  = jump && isGrounded;
        bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;

        // Pause
            if(pause)
            {
                GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
            }

            if (Pause.paused)
            {
                t_hmove = 0f;
                t_vmove = 0f;
                sprint = false;
                jump = false;
                pause = false;
                isGrounded = false;
                isJumping = false;
                isSprinting = false;
            }

        // Movement 
        Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
        t_direction.Normalize();

        float t_adjustedSpeed = speed;
        if (isSprinting) t_adjustedSpeed *= sprintModifier;

        Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * t_adjustedSpeed * Time.deltaTime;
        t_targetVelocity.y = rig.velocity.y;
        rig.velocity = t_targetVelocity;


        //Field of view
        if (isSprinting) {normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);}
        else { normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);; } 
        }
        #endregion

        #region Private Methods

        void RefreshMultiplayerState() { //!
            float cacheEulY = weaponParent.localEulerAngles.y; 

            Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
            weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

            Vector3 finalRotation = weaponParent.localEulerAngles;
            finalRotation.y = cacheEulY;

            weaponParent.localEulerAngles = finalRotation;
        }

        void HeadBob(float p_z, float p_x_intensity, float p_y_intensity) {
            float t_aim_adjust = 1f;
            if(weapon.isAiming) t_aim_adjust = 0.1f;
            targetWeaponBobPosition = weaponParentOrigin  + new Vector3(Mathf.Cos(p_z) * p_x_intensity * t_aim_adjust, Mathf.Sin(p_z * 2) * p_y_intensity * t_aim_adjust, 0);
        }

        void RefreshHealthBar() {
            float t_health_ratio =  (float)current_health / (float)max_health;
            ui_healthbar.localScale = Vector3.Lerp(ui_healthbar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
        }

        [PunRPC]
        private void SyncProfile(string p_username, int p_level, int p_xp) {
        playerProfile = new ProfileData(p_username, p_level, p_xp);
        playerUsername.text = playerProfile.username;
        }

        #endregion

        #region Public methods

        public void TakeDamage (int p_damage) {
            if (photonView.IsMine) {
            current_health -= p_damage;
            RefreshHealthBar();
            
            if(current_health <= 0) {
                manager.Spawn();
                PhotonNetwork.Destroy(gameObject);
            }

            }
        }

        #endregion
    }
}
