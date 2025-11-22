using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region General Variables
    [SerializeField] float waterLeft = 100;
    [SerializeField] float foodLeft = 100;
    [SerializeField] float movementMult = 1;//cuando el player se mueva, consumirá más
    [SerializeField] Image waterBarFill;
    [SerializeField] Image foodBarFill;
    //[SerializeField] float secondsOfEnergy = 180f;//3 o 4 minutos

    [Header("Movement & Interaction")]
    [SerializeField] float speed = 5f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float maxForce = 1f; //Fuerza máxima de aceleración
    [SerializeField] float sensitivity = 0.1f;

    [SerializeField] bool isSprinting;

    [SerializeField] float interactingCooldown = 0.1f;
    [SerializeField] float interactingDistance = 1.5f;
    [SerializeField] float interactOffset = 0.5f;
    [SerializeField] bool canInteract = true;
    [SerializeField] LayerMask interactLayer;
    [SerializeField] Transform shootPos;
    [SerializeField] Vector3 interactCubeScale;
    [SerializeField] Vector3 interactCubeOffset;


    [Header("GroundCheck")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] GameObject groundCheck;
    [SerializeField] float groundCheckRadius = 0.3f;
    [SerializeField] LayerMask groundLayer;
    bool isGrounded;

    //Input Variables
    bool interacting;
    Vector2 moveInput;
    Vector2 lookInput;
    float lookRotation;
    //[SerializeField] SO_GameManager gameManager;
    [Header("Object References")]
    [SerializeField] Rigidbody playerRb;
    [SerializeField] Animator anim;
    //[SerializeField] GameObject camHolder;
    //[SerializeField] Camera cam;
    [SerializeField] AudioSource playerSpeaker;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;

    [SerializeField] Transform camTransform;
    [SerializeField] bool hasTurned = false;

    #endregion

    void Update()
    {
        //Groundcheck
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundCheckRadius, groundLayer);
        //Debug ray: visible only in Scene
        //Debug.DrawRay(camHolder.transform.position, camHolder.transform.forward * 100f, Color.red);


        StatsUpdater();
        if (interacting) StartCoroutine(InteractRoutine());

        if (waterLeft <= 0) //si la comida se agota, la bebida tmb se agotará más rápido?
        {
            //Método de perder
            //pantalla negra y se escucha un golpe en el suelo (thump)
            losePanel.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            Time.timeScale = 0f;
            Debug.Log("Game over!!");
        }
        /*//else if (condicion de ganar)
        {
            winPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            Time.timeScale = 0f;
            Debug.Log("Win!!");
        }*/

        //Debug.DrawRay(camHolder.transform.position, camHolder.transform.forward * 2f, Color.red);
        /* si llega a x bebida o x comida: animaciones de cansado
         if (energy <= 20 && energy > 10 && !isBlinking)
        {
            blinkingPanel.SetActive(true);
            isBlinking = true;
            loopedSpeaker.enabled = true;
            //StartCoroutine(Blinking(2));
        }
        else if (energy <= 10 && !isBlinking)
        {
            //StopCoroutine(Blinking(2));
            //StartCoroutine(Blinking(1));
        }
        else if (isBlinking && energy > 20)
        {
            isBlinking = false;
            blinkingPanel.SetActive(false);
            loopedSpeaker.enabled = true;
        }*/
    }
    void StatsUpdater()
    {
        foodLeft -= Time.deltaTime * (10f / 18f) * movementMult; //comprobar: que se vaya agotando la comida, ajustar tiempo o según distancia
        waterLeft -= Time.deltaTime * (10f / 18f) * movementMult; //comprobar: que se vaya agotando la bebida, ajustar tiempo o según distancia
        waterBarFill.fillAmount = waterLeft / 100;
        foodBarFill.fillAmount = foodLeft / 100;
    }
    private void FixedUpdate()
    {
        Movement();
    }

    void Movement()
    {
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;

        forward.y = 0;
        forward.Normalize();
        right.y = 0;
        right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;
        if (!hasTurned && moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }

        Vector3 currentVelocity = playerRb.linearVelocity;
        Vector3 targetVelocity = moveDirection;
        targetVelocity *= isSprinting ? sprintSpeed : speed;

        // Calcular el cambio de velocidad (aceleración)
        Vector3 velocityChange = (targetVelocity - currentVelocity);
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);
        if (moveInput.x != 0 || moveInput.y != 0)
        {
            movementMult = 2f;
            //anim.SetBool("isWalking", true);
        }
        else
        {
            movementMult = 1f;
            //anim.SetBool("isWalking", false);
        }
        playerRb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void Interact()
    {
        /*//Opción 1 OverlapSphere
        Collider[] colTouched = Physics.OverlapSphere(transform.position, interactingDistance, interactLayer); //puedes poner también layerMask y queryTriggerInteraction
        
        foreach (Collider col in colTouched)
        {
            Debug.Log("Puedes interactuar con el objeto llamado " + col.name);
            //col.SendMessage("AddDamage");//?
        }
        //Opción 2 Raycast frontal y ya
        RaycastHit hit;
        if(Physics.Raycast(shootPos.position, shootPos.forward, out hit, interactingDistance, interactLayer))
        {
            Debug.Log("Puedes interactuar con el objeto llamado " + hit.collider.name);
        }
        */

        //Opción 3 OverlapBox
        Vector3 worldOffset = transform.TransformPoint(new Vector3(0, 0, interactOffset));//transforma el offset local a global
        Collider[] colTouched = Physics.OverlapBox(worldOffset, interactCubeScale, gameObject.transform.rotation, interactLayer);
        foreach (Collider col in colTouched)
        {
            Debug.Log("Puedes interactuar con el objeto llamado " + col.name);
            //col.SendMessage("AddDamage");// creo que trygetcomponent es mejor opción
        }
        
    }
    IEnumerator InteractRoutine()
    {
        interacting = false;
        if(canInteract) Interact();
        canInteract = false;
        yield return new WaitForSeconds(interactingCooldown);
        canInteract = true;
    }
    void Jump()
    {
        if (isGrounded) playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    #region Input Methods
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) interacting = true;
    }
    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) isSprinting = true; //gestionar que deje de sprintear al dejar de moverse
        //cambiar input actions para que sea doble toque de tecla/(movimiento rápido/apretar joystick)

        //if (ctx.canceled) isSprinting = false;
    }

    #endregion
    private void OnDrawGizmosSelected()
    {
        //Debug.DrawRay(transform.position, transform.forward * interactingDistance, Color.yellow);
        Vector3 worldOffset = transform.TransformPoint(new Vector3(0, 0, interactOffset));
        Gizmos.DrawCube(worldOffset, interactCubeScale);
        //Gizmos.DrawSphere(worldOffset, interactingDistance);
        Gizmos.color = Color.blue;
    }
}