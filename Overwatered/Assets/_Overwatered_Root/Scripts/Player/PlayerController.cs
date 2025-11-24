using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines.Interpolators;
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

    [SerializeField] bool isSprinting;

    [SerializeField] float interactingCooldown = 0.1f;
    [SerializeField] bool canInteract = true;
    [SerializeField] LayerMask interactLayer;
    //[SerializeField] Transform shootPos;
    [SerializeField] Vector3 interactCubeScale;
    [SerializeField] Vector3 interactCubeOffset;


    [Header("GroundCheck")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] GameObject groundCheck;
    [SerializeField] float groundCheckRadius = 0.3f;
    [SerializeField] LayerMask groundLayer;
    bool isGrounded;

    //Input Variables
    bool playerPaused;//quitar esta o la siguiente? o no?
    bool menuOpened;
    bool interacting;
    Vector2 moveInput;
    //[SerializeField] SO_GameManager gameManager;
    [Header("Object References")]
    [SerializeField] Rigidbody playerRb;
    [SerializeField] Animator anim;
    //[SerializeField] GameObject camHolder;
    //[SerializeField] Camera cam;
    [SerializeField] AudioSource playerSpeaker;

    [SerializeField] Transform camTransform;
    [SerializeField] bool hasTurned = false;
    [SerializeField] float rotationTime = 20f;

    #endregion

    void Update()
    {
        //Groundcheck
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundCheckRadius, groundLayer);
        //Debug ray: visible only in Scene
        //Debug.DrawRay(camHolder.transform.position, camHolder.transform.forward * 100f, Color.red);

        if (!playerPaused)//congelar también las stats? o solo en las cabinas telefónicas
        {
            if (interacting) StartCoroutine(InteractRoutine());
        }
        

        StatsUpdater();

        if (waterLeft <= 0) //si la comida se agota, la bebida tmb se agotará más rápido?
        {
            //Método de perder
            //pantalla negra y se escucha un golpe en el suelo (thump)
            GameManager.Instance.losePanel.SetActive(true);
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
    }
    void StatsUpdater()
    {
        foodLeft -= Time.deltaTime * (10f / 24f) * movementMult; //ajustar tiempo o según distancia
        waterLeft -= Time.deltaTime * (10f / 24f) * movementMult; //ajustar tiempo o según distancia
        waterBarFill.fillAmount = waterLeft / 100;
        foodBarFill.fillAmount = foodLeft / 100;
    }
    private void FixedUpdate()
    {
        if (!playerPaused) Movement();
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
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationTime * Time.deltaTime);
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
            if(movementMult != 2) movementMult = 2f;
            if(!GameManager.Instance.camController.zoomReseted) GameManager.Instance.camController.ResetZoom();
            //anim.SetBool("isWalking", true);
        }
        else
        {
            movementMult = 1f;
            isSprinting = false;
            //anim.SetBool("isWalking", false);
        }
        playerRb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void Interact()
    {
        Vector3 worldOffset = transform.TransformPoint(interactCubeOffset);//transforma el offset local a global
        Collider[] colTouched = Physics.OverlapBox(worldOffset, interactCubeScale, gameObject.transform.rotation, interactLayer);
        foreach (Collider col in colTouched)
        {
            if(col.gameObject.transform.localScale.x == 0.5f) col.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            else col.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
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

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) interacting = true;
    }
    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) isSprinting = true; //gestionar que deje de sprintear al dejar de moverse
        //cambiar input actions para que sea doble toque de tecla/(movimiento rápido/apretar joystick)
    }
    public void OnMenuInteraction(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if(menuOpened)
            {
                GameManager.Instance.inventoryPanel.SetActive(false);
                GameManager.Instance.cinemachineCamera.enabled = true;
                //poner que mínimo la cámara lerpee hasta su posición actual (o que se mantenga en esa posición aunque muevas el ratón)
                playerPaused = false;
                //quitar animación pensativa
            }
            else
            {
                GameManager.Instance.inventoryPanel.SetActive(true);
                GameManager.Instance.cinemachineCamera.enabled = false;
                playerPaused = true;
                //poner animación pensativa
            }
            menuOpened = !menuOpened;
        }
    }

    #endregion
    private void OnDrawGizmosSelected()
    {
        Vector3 worldOffset = transform.TransformPoint(interactCubeOffset);
        Gizmos.DrawCube(worldOffset, interactCubeScale);
        //Gizmos.DrawSphere(worldOffset, interactingDistance);
        Gizmos.color = Color.blue;
    }
}