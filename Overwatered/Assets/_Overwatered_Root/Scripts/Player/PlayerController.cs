using System.Collections;
using Unity.Cinemachine;
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

    [SerializeField] float rowingForce = 0.5f;
    [SerializeField] float rowingTurningForce = 0.5f;

    [SerializeField] bool isSprinting;

    [SerializeField] float interactingCooldown = 0.1f;
    [SerializeField] bool canInteract = true;
    [SerializeField] LayerMask interactLayer;
    [SerializeField] LayerMask NPCLayer;
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
    bool isInsideBoat = false;
    [SerializeField] bool isNearBoat;
    public bool isNearLand;
    [SerializeField] bool canRow = true;
    Vector2 moveInput;
    //[SerializeField] SO_GameManager gameManager;
    [Header("Player References")]
    [SerializeField] Rigidbody playerRb;
    [SerializeField] Animator anim;
    //[SerializeField] GameObject camHolder;
    //[SerializeField] Camera cam;
    [SerializeField] AudioSource playerSpeaker;

    [SerializeField] Transform camTransform;
    [SerializeField] bool hasTurned = false;
    [SerializeField] float rotationTime = 20f;

    [Header("Boat References")]
    [SerializeField] GameObject boat;
    [SerializeField] BoatController boatController;
    [SerializeField] Rigidbody boatRb;
    [SerializeField] Transform exitWaterPoint;

    [SerializeField] float timePassed;
    public Vector3 shorePoint;
    #endregion
    private void Start()
    {
        if (GameManager.Instance.gameData.gameHasStarted)
        {
            GameManager.Instance.gameData.lastPlayerPos = gameObject.transform.position;
            GameManager.Instance.gameData.lastPlayerRot = gameObject.transform.rotation;
        }
    }
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

        foodLeft -= Time.deltaTime * (10f / 24f) * movementMult; //ajustar tiempo o según distancia
        waterLeft -= Time.deltaTime * (10f / 24f) * movementMult; //ajustar tiempo o según distancia
        timePassed += Time.deltaTime;
        if (timePassed >= 5f)
        {
            timePassed = 0;
            StatsUpdater();
        }

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
    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Boat"))
        {
            if (!isInsideBoat && !isNearBoat) isNearBoat = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Boat"))
        {
            if (isNearBoat) isNearBoat = false;
        }
    }
    void StatsUpdater()
    {
        //foodLeft -= Time.deltaTime * (10f / 24f) * movementMult; //ajustar tiempo o según distancia
        //waterLeft -= Time.deltaTime * (10f / 24f) * movementMult; //ajustar tiempo o según distancia
        waterBarFill.fillAmount = waterLeft / 100;
        foodBarFill.fillAmount = foodLeft / 100;
    }
    private void FixedUpdate()
    {
        if (!playerPaused)
        {
            if (!isInsideBoat)
            {
                Movement();
            }
            else
            {
                if(canRow)BoatMovement();
            }
        }
    }

    void Movement() //añadir que tolere escalones ligeros (raycasts? u otra cosa)
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
    void BoatMovement()
    {
        //moveInput.x rota la barca y moveInput.y acelera o mueve hacia atrás
        //poner preferencia en alguna si son pulsadas a la vez?
        float forceDirection = rowingForce * moveInput.y; //quizá poner directamente si 1 o -1
        float forceRotation = rowingTurningForce * moveInput.x;

        if (moveInput.sqrMagnitude > 0.001f)
        {
            if (moveInput.x != 0 || moveInput.y != 0)
            {
                if (movementMult != 2) movementMult = 2f;
                if (!GameManager.Instance.camController.zoomReseted) GameManager.Instance.camController.ResetZoom();
                //anim.SetBool("isWalking", true);
            }
            if (moveInput.y != 0)//ver si se puede poner easy in y out, no solo easyout
            {
                boatRb.AddForce(boat.transform.forward * forceDirection, ForceMode.Impulse);// o velocity change
                boatRb.AddForce(boat.transform.up * Random.Range(0.2f, 0.7f), ForceMode.Impulse);// o velocity change
            }
            else if (moveInput.x != 0)
            {
                boatRb.AddTorque(Vector3.up * forceRotation, ForceMode.Impulse);
            }
            canRow = false;
            StartCoroutine(RowingCoroutine());
        }
        else
        {
            movementMult = 1f;
            isSprinting = false;
            //anim.SetBool("isWalking", false);
        }
    }
    IEnumerator RowingCoroutine()
    {
        yield return new WaitForSeconds(1);
        canRow = true;
        yield break;
    }
    void Interact()
    {
        if(!isNearBoat && !isNearLand) // && !isInsideBoat? ya veremos
        {
            Vector3 worldOffset = transform.TransformPoint(interactCubeOffset);//transforma el offset local a global
            Collider[] colTouched = Physics.OverlapBox(worldOffset, interactCubeScale, gameObject.transform.rotation, interactLayer);
            foreach (Collider col in colTouched)
            {
                Debug.Log("Puedes interactuar con el objeto llamado " + col.name);
                //col.SendMessage("AddDamage");// creo que trygetcomponent es mejor opción
            }
            colTouched = Physics.OverlapBox(worldOffset, interactCubeScale, gameObject.transform.rotation, NPCLayer);
            if (colTouched[0]!= null) //aqui sale algun error
            {
                colTouched[0].GetComponent<NPCAI>().Talk(gameObject.transform);
                GameManager.Instance.ChangeCamera();
                GameManager.Instance.SetNPCTarget(colTouched[0].gameObject.transform);
            }
        }
        else
        {
            if(isNearBoat && !isInsideBoat)
            {
                isNearBoat = false;
                //sentarte en el bote
                gameObject.GetComponent<Collider>().enabled = false;
                playerRb.isKinematic = true;
                playerRb.useGravity = false;
                gameObject.transform.position = boat.transform.position;
                gameObject.transform.rotation = boat.transform.rotation;
                gameObject.transform.SetParent(boat.transform);
                isInsideBoat = true;
                boatController.RegisterPlayer(this);
            }
            else if(isNearLand && isInsideBoat)//este no va
            {
                isNearLand = false;
                //sentarte en el bote
                gameObject.GetComponent<Collider>().enabled = true;
                playerRb.isKinematic = false;
                playerRb.useGravity = true;
                //poner que sea más flexible la bajada
                boatController.SendClosestPoint();
                gameObject.transform.position = shorePoint;
                gameObject.transform.parent = null; 
                isInsideBoat = false;
                boatController.hasPlayer = false;
            }
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
                GameManager.Instance.cinemachineCamera.gameObject.GetComponent<ThirdPersonCamController>().enabled = true;
                GameManager.Instance.cinemachineCamera.gameObject.GetComponent<CinemachineInputAxisController>().enabled = true;
                //poner que mínimo la cámara lerpee hasta su posición actual (o que se mantenga en esa posición aunque muevas el ratón)
                playerPaused = false;
                //quitar animación pensativa
            }
            else
            {
                GameManager.Instance.inventoryPanel.SetActive(true);
                GameManager.Instance.cinemachineCamera.enabled = false;
                GameManager.Instance.cinemachineCamera.gameObject.GetComponent<ThirdPersonCamController>().enabled = false;
                GameManager.Instance.cinemachineCamera.gameObject.GetComponent<CinemachineInputAxisController>().enabled = false;
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