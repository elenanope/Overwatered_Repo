using System.Collections;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region General Variables
    [SerializeField] GameObject walkDust;
    public float waterLeft = 150;
    [SerializeField] float maxWater = 150;
    public float foodLeft = 150;
    [SerializeField] float maxFood = 150;
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
    [SerializeField] bool isGrounded;

    [SerializeField] bool willBeGrounded;
    [SerializeField] GameObject futureGroundCheck;
    [SerializeField] GameObject futureGroundCheck2;
    [SerializeField] Vector3 futureGroundCheckBox;

    //Input Variables
    [Header("References for UI")]
    [SerializeField] GameObject menuCamera;

    [SerializeField] GameObject adviceDialogue;//pasar a dialogueManager? o notification manager
    [SerializeField] TMP_Text adviceText;

    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] GameObject movablePoint;
    [SerializeField] GameObject breadObject;
    [SerializeField] GameObject waterObject;
    [SerializeField] ItemClass water;
    [SerializeField] ItemClass bread;
    [SerializeField]bool playerPaused;//quitar esta o la siguiente? o no?
    bool interacting;
    bool isInsideBoat = false;
    [SerializeField] bool isNearBoat;
    public bool isNearLand;
    [SerializeField] bool canRow = true;
    Vector2 moveInput;
    [Header("Player References")]
    [SerializeField] Rigidbody playerRb;
    [SerializeField] Animator anim;
    [SerializeField] Animator animHand;
    [SerializeField] Animator animatorL;
    [SerializeField] Animator animatorR;
    [SerializeField] RectTransform camCompass;
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
    [SerializeField] float timeSinceMove;
    public Vector3 shorePoint;
    Quaternion mapRotation;
    bool maintainedRow;
    GameObject consumable;//cambiar esto como pueda
    #endregion
    private void Awake()
    {
        GameManager.Instance.inventoryPanel.SetActive(true);
        GameManager.Instance.inventoryPanel.SetActive(false);
    }
    private void Start()
    {
            StatsUpdater();
        //GameManager.Instance.StartFade(0);
    }
    void Update()
    {
        if(GameManager.Instance.playerInDialogue) playerPaused = true;
        else playerPaused = false;
        //Groundcheck
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundCheckRadius, groundLayer);
        if(isSprinting)
        {
            if (Physics.CheckBox(futureGroundCheck.transform.position, futureGroundCheckBox * 0.2f, this.gameObject.transform.rotation, groundLayer) && Physics.CheckBox(futureGroundCheck2.transform.position, futureGroundCheckBox * 0.2f, this.gameObject.transform.rotation, groundLayer))
                willBeGrounded = true;
            else willBeGrounded = false;
        }
        else
        {
            if (Physics.CheckBox(futureGroundCheck.transform.position, futureGroundCheckBox, this.gameObject.transform.rotation, groundLayer) && Physics.CheckBox(futureGroundCheck2.transform.position, futureGroundCheckBox, this.gameObject.transform.rotation, groundLayer))
                willBeGrounded = true;
            else willBeGrounded = false;
        }
        //Debug ray: visible only in Scene
        //Debug.DrawRay(camHolder.transform.position, camHolder.transform.forward * 100f, Color.red);

        if (!playerPaused)//congelar también las stats? o solo en las cabinas telefónicas
        {
            if (interacting) StartCoroutine(InteractRoutine());
        }

        foodLeft -= Time.deltaTime * (10f / 24f) * movementMult; //ajustar tiempo o según distancia
        waterLeft -= Time.deltaTime * (10f / 24f) * movementMult; //ajustar tiempo o según distancia
        timePassed += Time.deltaTime;
        if(!isInsideBoat)
        {
            timeSinceMove += Time.deltaTime;

            if (timeSinceMove >= 20f)
            {
                anim.SetTrigger("varyIdle");
                timeSinceMove = -10;
            }
        }
        if (timePassed >= 5f)
        {
            timePassed = 0;
            StatsUpdater();
        }

        if (waterLeft <= 0 && !GameManager.Instance.gameOver) //si la comida se agota, la bebida tmb se agotará más rápido?
        {
            GameManager.Instance.gameOver = true;
            StartCoroutine(GameOver());
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
        waterBarFill.fillAmount = waterLeft / maxWater;
        foodBarFill.fillAmount = foodLeft / maxFood;
    }
    IEnumerator GameOver()
    {
        playerPaused = true;
        anim.SetTrigger("faint");
        yield return new WaitForSeconds(1.2f);
        //pantalla negra y se escucha un golpe en el suelo (thump)
        GameManager.Instance.losePanel.SetActive(true);//añadirle fade a esto
        //GameManager.Instance.menuOpened = true;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 0f;
        Debug.Log("Game over!!");
    }
    public void Consume(bool isDrinkable ,int itemNumber, int waterAdded, int foodAdded)//añadir el tipo de objeto para spawnear ese
    {
        string consumeTrigger = "";
        GameManager.Instance.isEating = true;
        if (!isDrinkable)
        {
            animHand.SetTrigger("eat");
            consumeTrigger = "eat";
            //animHand.ResetTrigger("eat");
            foodLeft += foodAdded;
            consumable = breadObject;
            //consumable = Instantiate(bread.itemPrefab);
        }
        else
        {
            animHand.SetTrigger("drink");
            consumeTrigger = "drink";
            //animHand.ResetTrigger("drink");
            consumable = waterObject;
        }
        consumable.SetActive(false);
    waterLeft += waterAdded;
        timePassed = 0;
        //+ spawneo de objeto
        StartCoroutine(LunchTime(consumeTrigger));
        
    }
    IEnumerator LunchTime(string order)
    {
        yield return new WaitForSeconds(0.1f);//para que se coordinen lo máximo posible las anims
        anim.SetTrigger(order);
        consumable.SetActive(true);
        yield return new WaitForSeconds(3f);
        StatsUpdater();
        consumable.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        GameManager.Instance.isEating = false;
    }
    private void FixedUpdate()
    {
        if (!playerPaused || !GameManager.Instance.playerInDialogue)
        {
            if (!isInsideBoat)
            {
                Movement();
            }
            else
            {
                if (!anim.GetBool("inBoat"))
                {
                    anim.SetBool("inBoat", true);
                    walkDust.SetActive(false);
                    animatorL.gameObject.SetActive(true);
                    animatorR.gameObject.SetActive(true);
                }
                if (anim.GetInteger("playerState") > 0) anim.SetInteger("playerState", 0);
                if(canRow)BoatMovement();
            }
            //poner que la siga a la camara en vez de al personaje
            if(camCompass != null)
            {
                //rota flecha
                camCompass.rotation = Quaternion.Euler (0f, 0f, GameManager.Instance.camController.transform.eulerAngles.y);
                //rota cámara
                mapRotation = GameManager.Instance.mapCamera.rotation;
                GameManager.Instance.mapCamera.rotation = Quaternion.Euler (90f, GameManager.Instance.camController.transform.eulerAngles.y, 0f);
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

        if(willBeGrounded)
        {
            // Calcular el cambio de velocidad (aceleración)
            Vector3 velocityChange = (targetVelocity - currentVelocity);
            velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);
            velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);
            if (moveInput.x != 0 || moveInput.y != 0)
            {
                if (movementMult != 2) movementMult = 2f;
                if (!GameManager.Instance.camController.zoomReseted) GameManager.Instance.camController.ResetZoom();
                timeSinceMove = 0;
                anim.SetInteger("playerState", isSprinting ? 2 : 1);
            }
            else
            {
                movementMult = 1f;
                isSprinting = false;
                if (anim.GetInteger("playerState") >= 0)
                {
                    timeSinceMove = 0;
                    anim.SetInteger("playerState", 0);
                }

            }
            playerRb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        else
        {
            if(isGrounded)playerRb.linearVelocity = Vector3.zero;
            movementMult = 1f;
            isSprinting = false;
            if (anim.GetInteger("playerState") >= 0)
            {
                timeSinceMove = 0;
                anim.SetInteger("playerState", 0);
            }
        }
        
    }
    void BoatMovement()
    {
        if (moveInput.sqrMagnitude > 0.001f)
        {
            //maintainRow = true;

            canRow = false;
            StartCoroutine(RowingCoroutine());
        }
        else
        {
            movementMult = 1f;
            isSprinting = false;
            //anim.SetBool("maintainRow", false);
        }
    }
    IEnumerator RowingCoroutine()
    {
        //moveInput.x rota la barca y moveInput.y acelera o mueve hacia atrás
        //poner preferencia en alguna si son pulsadas a la vez?
        float forceDirection = rowingForce * moveInput.y; //quizá poner directamente si 1 o -1
        float forceRotation = rowingTurningForce * moveInput.x;
        float lastMoveInputX = moveInput.x;
        float lastMoveInputY = moveInput.y;

        //anim.SetBool("maintainRow", true);

        if (moveInput.y != 0)
        {
            if (moveInput.y == 1)
            {
                anim.SetInteger("rowDirection", 0);
                animatorL.SetBool("moveForward", true);
                animatorR.SetBool("moveForward", true);
                animatorL.SetTrigger("row");
                animatorR.SetTrigger("row");
            }
            else
            {
                anim.SetInteger("rowDirection", 2);
                animatorL.SetBool("moveForward", false);
                animatorR.SetBool("moveForward", false);
                animatorL.SetTrigger("row");
                animatorR.SetTrigger("row");
            }
        }
        else if (moveInput.x != 0)
        {
            if (moveInput.x == 1)
            {
                anim.SetInteger("rowDirection", 1);

                animatorR.SetBool("moveForward", true);
                animatorR.SetTrigger("row");
            }
            else
            {
                anim.SetInteger("rowDirection", 3);

                animatorL.SetBool("moveForward", true);
                animatorL.SetTrigger("row");
            }
        }

        anim.SetTrigger("row");
        yield return new WaitForSeconds(0.3f);
        anim.ResetTrigger("row");
        animatorL.ResetTrigger("row");
        animatorR.ResetTrigger("row");
        if(lastMoveInputX != 0) yield return new WaitForSeconds(0.7f);
        else if(lastMoveInputY > 0) yield return new WaitForSeconds(0.5f);
        else if (lastMoveInputY < 0) yield return new WaitForSeconds(1.1f);
        //rema
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("AN_Player_RowIdle") && !anim.GetCurrentAnimatorStateInfo(0).IsName("AN_Player_RowTired"))
        {
            if (lastMoveInputX != 0 || lastMoveInputY != 0)
            {
                if (movementMult != 2) movementMult = 2f;
                if (!GameManager.Instance.camController.zoomReseted) GameManager.Instance.camController.ResetZoom();
            }
            if (lastMoveInputY != 0)//ver si se puede poner easy in y out, no solo easyout
            {
                boatRb.AddForce(boat.transform.forward * forceDirection, ForceMode.Impulse);// o velocity change
                boatRb.AddForce(boat.transform.up * Random.Range(0.2f, 0.7f), ForceMode.Impulse);// o velocity change
            }
            else if (lastMoveInputX != 0)
            {
                boatRb.AddTorque(Vector3.up * forceRotation, ForceMode.Impulse);
            }
        }

        if (lastMoveInputX != 0) yield return new WaitForSeconds(1f);
        else if (lastMoveInputY > 0) yield return new WaitForSeconds(1.2f);
        else if (lastMoveInputY < 0) yield return new WaitForSeconds(0.9f);
        //hasta aqui verá si se sigue manteniendo o no

        //puede volver a remar
        //canRow = true;

        if(maintainedRow)
        {
            StartCoroutine(ResetRow());
        }
        else
        {
            canRow = true; //hace falta poner esto en más sitios

        }
            yield break;
    }

    IEnumerator ResetRow()
    {
        yield return new WaitForSeconds(0.1f); //se reproduce idle de row (transición entre barridos)
        canRow = true;
        yield break;
    }
    void Interact()
    {
        if (!GameManager.Instance.menuOpened)
        {
            if (!isNearBoat && !isNearLand) // && !isInsideBoat? ya veremos
            {
                Vector3 worldOffset = transform.TransformPoint(interactCubeOffset);//transforma el offset local a global
                Collider[] colTouched = Physics.OverlapBox(worldOffset, interactCubeScale, gameObject.transform.rotation, interactLayer);
                foreach (Collider col in colTouched)
                {//si no está lleno el inventario
                    ItemClass item = null;
                    if (col.gameObject.name == "SM_Bread") item = bread; 
                    else if (col.gameObject.name == "SM_Water") item = water; 
                    if(item != null)
                    {
                        if (inventoryManager.Add(item, 1))
                        {
                            col.gameObject.SetActive(false);
                            anim.SetTrigger("pocketSearch");
                        }
                        else
                        {
                            //Debug.Log("No tienes espacio");//sacar nube de diálogo que te diga eso, mientras no se apague, no puede volver a aparecer
                            adviceDialogue.SetActive(false);
                            adviceDialogue.SetActive(true);
                            adviceText.text = "No te queda espacio en el inventario!";
                            //después apagar
                        }
                    }
                }
                colTouched = Physics.OverlapBox(worldOffset, interactCubeScale, gameObject.transform.rotation, NPCLayer);
                if (colTouched.Length > 0) //aqui sale algun error
                {
                    colTouched[0].GetComponent<NPCAI>().Talk(gameObject.transform);
                }
            }
            else
            {
                if (isNearBoat && !isInsideBoat)
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
                else if (isNearLand && isInsideBoat)//este no va
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
                    //MinigameUpdater.Instance.SaveBoatPos(boatController.gameObject.transform);
                    if (anim.GetBool("inBoat"))
                    {
                        anim.SetBool("inBoat", false);
                        animatorL.gameObject.SetActive(false);
                        animatorR.gameObject.SetActive(false);
                    }
                    boatController.hasPlayer = false;
                    boatController.sticks.SetActive(true);
                    walkDust.SetActive(true);
                }
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
        if(isInsideBoat)
        {
            if (moveInput.x != 0 || moveInput.y != 0)
            {
                maintainedRow = true;
            }
            else maintainedRow = false;
        }
        else maintainedRow = false;
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !playerPaused) interacting = true;
    }
    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !playerPaused) isSprinting = true; //gestionar que deje de sprintear al dejar de moverse
        //cambiar input actions para que sea doble toque de tecla/(movimiento rápido/apretar joystick)
    }
    public void OnMenuInteraction(InputAction.CallbackContext ctx) //pasar al gameManager
    {
        if (ctx.performed && !GameManager.Instance.playerInDialogue && !GameManager.Instance.isEating)//a menos que esté tradeando, que el tradeo puede handlear abrir y cerrar el menú)
        {
            if(GameManager.Instance.menuOpened)
            {
                anim.SetInteger("playerState", 0);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {

                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                anim.SetInteger("playerState", -1);
            }
            GameManager.Instance.inventoryPanel.SetActive(!GameManager.Instance.menuOpened);
            GameManager.Instance.SetNPCTarget(null);
            menuCamera.SetActive(!GameManager.Instance.menuOpened);
            GameManager.Instance.cinemachineCamera.enabled = GameManager.Instance.menuOpened;
            GameManager.Instance.cinemachineCamera.gameObject.GetComponent<ThirdPersonCamController>().enabled = GameManager.Instance.menuOpened;
            GameManager.Instance.cinemachineCamera.gameObject.GetComponent<CinemachineInputAxisController>().enabled = GameManager.Instance.menuOpened;
            GameManager.Instance.menuOpened = !GameManager.Instance.menuOpened;
            //playerPaused = GameManager.Instance.menuOpened;
        }
    }

    #endregion
    private void OnDrawGizmosSelected()
    {
        Vector3 worldOffset = transform.TransformPoint(interactCubeOffset);
        Gizmos.DrawCube(worldOffset, interactCubeScale);
        Gizmos.DrawCube(futureGroundCheck.transform.position, futureGroundCheckBox );
        Gizmos.DrawCube(futureGroundCheck2.transform.position, futureGroundCheckBox );
        //Gizmos.DrawSphere(worldOffset, interactingDistance);
        Gizmos.color = Color.blue;
    }
}