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

    public bool invertLook;
    private bool isGrounded;
    public float jumpForce = 5f;

    public GameObject bulletImpact;
    public float timeBetweenShots = 0.1f;
    private float shotCounter;


    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
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

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        if (Input.GetMouseButton(0))
        {
            shotCounter -= Time.deltaTime;
            
            if (shotCounter <= 0)
            {
                Shoot();
            }
        }
    }

    private void FixedUpdate()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

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
        
        shotCounter = timeBetweenShots;
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
