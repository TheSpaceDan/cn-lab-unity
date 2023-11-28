using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class PlayerController : MonoBehaviour
{
    public Transform viewPoint;
    public float mouseSens = 1f;
    private float verticalRotStore;
    private Vector2 mouseInput;
    private float horizontalInput, verticalInput;
    public float speed = 5f;
    Vector3 moveDirection;
    private Rigidbody rb;
    public Camera cam;
    public Animator anim;

    public bool invertLook;
    private bool isGrounded;
    public float jumpForce = 5f;

    public GameObject bulletImpact;
    //public float timeBetweenShots = 0.1f;
    private float shotCounter;
    public float muzzleDisplayTime = 0.1f;
    private float muzzleCounter;

    public float maxHeat = 10f, /*heatPerShot = 1f,*/ coolRate = 4f, overheatCoolRate = 5f;
    private float heatCounter;
    private bool overHeated;
    
    public Gun[] allGuns;
    private int currentGun;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        
        anim.SetBool("isShooting", false);
        UIController.instance.weaponTempSlider.maxValue = maxHeat;
        SwitchGun();
        
        Transform newTransform = SpawnManager.instance.GetSpawnPoint();
        transform.position = newTransform.position;
        transform.rotation = newTransform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSens;

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);

        if (invertLook) {
            viewPoint.rotation = Quaternion.Euler(verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }
        else
        {
            viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        if (Input.GetButtonDown("Sprint"))
        {
            speed = 7f;
        }
        if (Input.GetButtonUp("Sprint"))
        {
            speed = 4f;
        }

        if (allGuns[currentGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;
            if (muzzleCounter <= 0)
            {
                allGuns[currentGun].muzzleFlash.SetActive(false);
            }
        }

        if (!overHeated)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }

            if (Input.GetMouseButton(0) && allGuns[currentGun].isAutomatic)
            {
                shotCounter -= Time.deltaTime;
            
                if (shotCounter <= 0)
                {
                    anim.SetBool("isShooting", true);
                    Shoot();
                }
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                anim.SetBool("isShooting", false);
            }
            
            heatCounter -= coolRate * Time.deltaTime;
        }
        else
        {
            heatCounter -= overheatCoolRate * Time.deltaTime;
            if (heatCounter <= 0)
            {
                overHeated = false;
                UIController.instance.overHeatText.gameObject.SetActive(false);
            }
        }
        
        if (heatCounter < 0)
        {
            heatCounter = 0;
        }
        
        UIController.instance.weaponTempSlider.value = heatCounter;
        
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            currentGun++;
            if (currentGun >= allGuns.Length)
            {
                currentGun = 0;
            }
            SwitchGun();
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            currentGun--;
            if (currentGun < 0)
            {
                currentGun = allGuns.Length - 1;
            }
            SwitchGun();
        }
        
        //weapon switching with number keys 1-3
        for (int i = 0; i < allGuns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                currentGun = i;
                SwitchGun();
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 moveDirection = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;
        moveDirection.y = 0;

        rb.MovePosition(rb.position + moveDirection * speed * Time.deltaTime);
    }
    
    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = cam.transform.position;
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("we hit " + hit.collider.gameObject.name);
            GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.0002f), Quaternion.LookRotation(hit.normal, Vector3.up));
            Destroy(bulletImpactObject, 7f);
        }
        
        shotCounter = allGuns[currentGun].timeBetweenShots;
        
        heatCounter += allGuns[currentGun].heatPerShot;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;
            anim.SetBool("isShooting", false);
            UIController.instance.overHeatText.gameObject.SetActive(true);
        }
        
        allGuns[currentGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }
    
    private void SwitchGun()
    {
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        allGuns[currentGun].gameObject.SetActive(true);
        allGuns[currentGun].muzzleFlash.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.other.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.other.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
