using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PaperShipMinigame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] Animator windDirectionIcon;
    [SerializeField] TMP_Text triesText;
    [SerializeField] Image breathBarFill;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject drawPanel;
    [SerializeField] GameObject losePanel;
    [SerializeField] GameObject winnerSign;
    [SerializeField] Vector3 winnerSignPos = new Vector3 (0f, 0.47f, 0f);
    [SerializeField] Animator playerAnimator;

    [Header("Ships References")]
    [SerializeField] Rigidbody shipPlayer;
    [SerializeField] Rigidbody shipNPC1;
    [SerializeField] Rigidbody shipNPC2;

    [Header("Game Stats")]
    [SerializeField] int triesDone;
    [SerializeField] int pointsPlayer;
    [SerializeField] int pointsNPC1;
    [SerializeField] int pointsNPC2;
    [SerializeField] float airTaken;
    //[SerializeField] float airTakenSpeed = 1.5f;
    [SerializeField] int wind; //0 no hay, 1 hacia derecha, -1 hacia izquierda
    [SerializeField] float windMult; //1 no hay, 2 hacia derecha, 0.5 hacia izquierda

    //[SerializeField] bool canBreath;
    [SerializeField] int gameDifficulty; //0 easy, 1 medium, 2 hard
    [SerializeField] int breathingPhase; //0 not breathing, 1 breath in, 2 breath out, 3 soplar, 4 coroutina
    [SerializeField] int shipsArrived; //0 ninguno, 1 ya ha frenado el del player, 2 el del npc 1, 3 el del npc 2

    Vector3[] shipsStartPos = new Vector3[3];
    [SerializeField] Transform goalPos;
    [SerializeField] Transform flag;
    float goalDistance;
    int minigameState;//0 fade in, 1 jugar, 2 finalizado, 3 fadeout, 4 fadeout over
    int endResult = -1;
    bool extraRound;
    bool adviceOpened;
    [SerializeField] DialogueManager dialogueManager;

    void Start()
    {
        minigameState = 0;
        GameManager.Instance.StartFade(0);
        triesDone = 0;
        shipsStartPos[0] = shipPlayer.gameObject.transform.position;
        shipsStartPos[1] = shipNPC1.gameObject.transform.position;
        shipsStartPos[2] = shipNPC2.gameObject.transform.position;
        shipsStartPos[0].y += 0.1f;
        shipsStartPos[1].y += 0.1f;
        shipsStartPos[2].y += 0.1f;
        goalDistance = goalPos.position.z;
        UpdateWind();
    }

    void Update()
    {
        if(minigameState == 1)
        {
            if ((pointsNPC2 == 1 && pointsPlayer == 1 && pointsNPC1 != 1) || (pointsNPC1 == 1 && pointsPlayer == 1 && pointsNPC2 != 1) || triesDone < 2) //si lleva dos intentos o está empatado
            {
                if (breathingPhase != 0)
                {
                    breathBarFill.fillAmount = airTaken / 8f;
                    if (breathingPhase == 1)
                    {
                        breathBarFill.color = Color.white;
                        if (airTaken >= 8.1f) breathingPhase = 2;
                        else airTaken += Time.deltaTime; //* airTakenSpeed;
                    }
                    else if (breathingPhase == 2)
                    {
                        breathBarFill.color = Color.red;
                        airTaken -= Time.deltaTime * 2; //* airTakenSpeed  //suelta el aire más rápido que cuando lo coge
                        if (airTaken <= 0)
                        {
                            airTaken = 0;
                            breathingPhase = 0;
                        }
                    }
                    else if (breathingPhase == 3)
                    {
                        breathBarFill.color = Color.lightGray;
                        //añadir sonido de soplido
                        breathingPhase = 4;
                        StartCoroutine(ShipsMovement());
                    }
                    else if (breathingPhase == 4)
                    {
                        airTaken -= Time.deltaTime * 2;
                        if (airTaken <= 0) airTaken = 0;
                    }
                }
            }
            else if (pointsPlayer >= 2) EndGame(2);
            else if (pointsNPC1 == 1 && pointsPlayer == 1 && pointsPlayer == 1) EndGame(1);
            else EndGame(0);
        }

        {
            if (GameManager.Instance.faded)
            {
                if (minigameState == 0)
                {
                    StartCoroutine(StartMinigame());
                }
            } 
        }
    }

    IEnumerator ShipsMovement()
    {
        PhaseUpdate();
        for (int i = 1; i < 5; i++)
        {
            yield return new WaitForSeconds(3f); // o esperar a que frene
            shipsArrived = i;
            PhaseUpdate();
        }
    }

    void PhaseUpdate()
    {
        Rigidbody shipToMove = null;
        float shipForce = 0;

        if(shipsArrived == 0)
        {
            shipPlayer.AddForce(shipPlayer.transform.forward * (airTaken/1) * windMult, ForceMode.Impulse);
            playerAnimator.SetBool("isBreathing", false);
        }
        else if(shipsArrived == 3)
        {
            CalculateDistances();
        }
        else if(shipsArrived == 4)
        {
            ResetShips();
        }
        else
        {
            if (playerAnimator.GetBool("inPos"))
            {
                playerAnimator.SetBool("inPos", false);
            }
            if (shipsArrived == 1) shipToMove = shipNPC1;
            else if (shipsArrived == 2) shipToMove = shipNPC2;
            if(gameDifficulty == 0) //revisar todo esto
            {
                //shipForce = Random.Range(0.5f, 1.9f);
                if (wind == 0)
                {
                    shipForce = Random.Range(3f, 5.5f);
                }
                else if (wind == 1)
                {
                    shipForce = Random.Range(2f, 4.5f);
                }
                else if (wind == -1)
                {
                    shipForce = Random.Range(5f, 8f);
                }
            }
            else if(gameDifficulty == 1)
            {
                if(wind == 0)
                {
                    shipForce = Random.Range(0.5f, 1.5f);
                }
                else if(wind == 1)
                {
                    shipForce = Random.Range(0.1f, 1f);
                }
                else if(wind == -1)
                {
                    shipForce = Random.Range(0.8f, 2f);
                }
            }
            else
            {
                if (wind == 0)
                {
                    shipForce = Random.Range(0.3f, 1.2f);
                }
                else if (wind == 1)
                {
                    shipForce = Random.Range(0.1f, 1f);
                }
                else if (wind == -1)
                {
                    shipForce = Random.Range(1.2f, 2f);
                }
            }
            shipToMove.AddForce(shipToMove.transform.forward * shipForce * windMult, ForceMode.Impulse);// o velocity change
        }
    }//también puedo sacar diálogos random de personajes de fondo mientras se muevan las barcas

    void CalculateDistances()
    {
        int closestShip = -1;
        float closestShipDistance = 100f;
        Debug.Log("Se calculan las distancias y ganador escogido");
        float distancePlayer = shipPlayer.transform.position.z - goalDistance;//repasar esto
        float distanceNPC1 = shipNPC1.transform.position.z - goalDistance;
        float distanceNPC2 = shipNPC2.transform.position.z - goalDistance;
        if (distancePlayer >= 0 && distancePlayer < closestShipDistance)
        {
            closestShip = 0;
            closestShipDistance = distancePlayer;
        }
         if(distanceNPC1 >= 0 && distanceNPC1 < closestShipDistance)
        {
            closestShip = 1;
            closestShipDistance = distanceNPC1;
        }
         if(distanceNPC2 >= 0 && distanceNPC2 < closestShipDistance)
        {
            closestShip = 2;
            closestShipDistance = distanceNPC2;
        }

        if(closestShip == 0)
        {
            pointsPlayer++;
            ActivateWinnerSign(shipPlayer.transform);
        }
        else if(closestShip == 1)
        {
            pointsNPC1++;
            ActivateWinnerSign(shipNPC1.transform);
        }
        else if(closestShip == 2)
        {
            pointsNPC2++;
            ActivateWinnerSign(shipNPC2.transform);
        }
        else
        {
            Debug.Log("Punto para nadie!");
            //O poner que se repita la ronda
        }
    }
    void ActivateWinnerSign(Transform winner)
    {
        winnerSign.transform.parent = winner;
        winnerSign.transform.localPosition = winnerSignPos;
        winnerSign.SetActive(true);
    }
    void UpdateWind()
    {
        wind = Random.Range(-1, 2);
        if (wind == -1)
        {
            windMult = 0.65f;
            flag.rotation = Quaternion.Euler(flag.rotation.eulerAngles.x, -100f, 0f);
            //-100
        }
        else if (wind == 0)
        {
            windMult = 1f;
            flag.rotation = Quaternion.Euler(flag.rotation.eulerAngles.x, -30f, 0f);
            //-30
        }
        else
        {
            windMult = 1.25f;
            flag.rotation = Quaternion.Euler(flag.rotation.eulerAngles.x, 0f, 0f);
            //0
        }
        windDirectionIcon.SetInteger("windDirection", wind);
    }

    void ResetShips()
    {
        float roundNumber;
        shipsArrived = 0;
        UpdateWind();
        triesDone++;
        roundNumber = triesDone + 1;
        triesText.text = roundNumber.ToString();
        breathingPhase = 0;
        if(triesDone < 3)
        {
            shipPlayer.gameObject.transform.position = shipsStartPos[0];
            shipNPC1.gameObject.transform.position = shipsStartPos[1];
            shipNPC2.gameObject.transform.position = shipsStartPos[2];
        }
        winnerSign.SetActive(false);
        playerAnimator.SetBool("inPos", true);
    }
    
    void EndGame(int winCondition) // 0 lose, 1 empate, 2 win
    {
        minigameState = 2;
        endResult = winCondition;
        GameManager.Instance.menuOpened = true;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        if (winCondition == 2)
        {
            winPanel.SetActive(true);
            GameManager.Instance.eventSystem.SetSelectedGameObject(winPanel.transform.GetChild(1).gameObject);
        }
        else if(winCondition == 1)
        {
            drawPanel.SetActive(true);
            GameManager.Instance.eventSystem.SetSelectedGameObject(drawPanel.transform.GetChild(1).gameObject);
        }
        else
        {
            losePanel.SetActive(true);
            GameManager.Instance.eventSystem.SetSelectedGameObject(losePanel.transform.GetChild(1).gameObject);
        }
        minigameState = 3;
        //StartCoroutine(FinishMinigame());
    }
    public void FinishButton()
    {
        StartCoroutine(MinigameManager.Instance.ExitMinigame(endResult));
    }
    IEnumerator StartMinigame()
    {
        yield return new WaitForSeconds(0.5f);
        minigameState = 1;

        playerAnimator.SetBool("inPos", true);
    }
    public void OnBreathing(InputAction.CallbackContext ctx)
    {
        if(dialogueManager.dialogueOver)
        {
            if (ctx.performed)
            {
                if (breathingPhase == 0)
                {
                    breathingPhase = 1;

                    playerAnimator.SetBool("isBreathing", true);
                }
            }
            if (ctx.canceled)
            {
                if (breathingPhase == 1) breathingPhase = 3;
            }
        }
    }
}
