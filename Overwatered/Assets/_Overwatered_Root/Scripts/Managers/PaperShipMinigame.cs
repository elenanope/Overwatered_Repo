using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PaperShipMinigame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] Image windDirectionIcon;
    [SerializeField] TMP_Text triesText;
    [SerializeField] Image breathBarFill;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject drawPanel;
    [SerializeField] GameObject losePanel;

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
    [SerializeField] float airTakenSpeed = 4f;
    [SerializeField] int wind; //0 no hay, 1 hacia derecha, -1 hacia izquierda
    [SerializeField] float windMult; //1 no hay, 2 hacia derecha, 0.5 hacia izquierda

    //[SerializeField] bool canBreath;
    [SerializeField] int gameDifficulty; //0 easy, 1 medium, 2 hard
    [SerializeField] int breathingPhase; //0 not breathing, 1 breath in, 2 breath out, 3 soplar, 4 coroutina
    [SerializeField] int shipsArrived; //0 ninguno, 1 ya ha frenado el del player, 2 el del npc 1, 3 el del npc 2

    Vector3[] shipsStartPos = new Vector3[3];
    [SerializeField] Transform goalPos;
    float goalDistance;
    int minigameState;//0 fade in, 1 jugar, 2 finalizado, 3 fadeout, 4 fadeout over
    int endResult = -1;

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
            if (((pointsNPC1 == 1 || pointsNPC2 == 1) && pointsPlayer == 1) || triesDone < 2) //si lleva dos intentos o está empatado
            {
                if (breathingPhase != 0)
                {
                    breathBarFill.fillAmount = airTaken / 15f;
                    if (breathingPhase == 1)
                    {
                        breathBarFill.color = Color.blue;
                        if (airTaken >= 17)
                        {
                            breathingPhase = 2;
                        }
                        else
                        {
                            airTaken += Time.deltaTime * airTakenSpeed;
                        }
                    }
                    else if (breathingPhase == 2)
                    {
                        breathBarFill.color = Color.red;
                        airTaken -= Time.deltaTime * airTakenSpeed * 2; //suelta el aire más rápido que cuando lo coge
                        if (airTaken <= 0)
                        {
                            airTaken = 0;
                            breathingPhase = 0;
                        }
                    }
                    else if (breathingPhase == 3)
                    {
                        breathBarFill.color = Color.darkBlue; //o color algo más oscuro del que tiene
                                                              //añadir sonido de soplido
                                                              //añadir fuerza a los barquitos multiplicada por 0.5 si !eastWind (o añadir que sea algo más random)

                        //añadir coroutina para que los barcos lleguen al sitio y se te sumen los puntos
                        breathingPhase = 4;
                        StartCoroutine(ShipsMovement());
                    }
                    else if (breathingPhase == 4)
                    {
                        airTaken -= Time.deltaTime * airTakenSpeed * 2;//más rápido?
                        if (airTaken <= 0)
                        {
                            airTaken = 0;
                        }
                    }
                }
            }
            else if (pointsPlayer >= 2) EndGame(2);
            else if (pointsNPC1 == 1 && pointsPlayer == 1 && pointsPlayer == 1) EndGame(1);
            else EndGame(0);
        }
        //if(GameManager.Instance.fading) //si es posible, mejorar esto (pasarlo al gameManager)
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
            shipPlayer.AddForce(shipPlayer.transform.forward * airTaken * windMult, ForceMode.Impulse);
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
            if (shipsArrived == 1) shipToMove = shipNPC1;
            else if (shipsArrived == 2) shipToMove = shipNPC2;
            if(gameDifficulty == 0) //revisar todo esto
            {
                shipForce = Random.Range(0.5f, 1.9f);
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
        }
        else if(closestShip == 1)
        {
            pointsNPC1++;
        }
        else if(closestShip == 2)
        {
            pointsNPC2++;
        }

    }

    void UpdateWind()
    {
        wind = Random.Range(-1, 2);
        if (wind == -1)
        {
            windDirectionIcon.gameObject.SetActive(true);
            windMult = 0.5f;
        }
        else if (wind == 0)
        {
            windDirectionIcon.gameObject.SetActive(false);
            windMult = 1f;
        }
        else
        {
            windDirectionIcon.gameObject.SetActive(true);
            windMult = 2f;
        }
        windDirectionIcon.rectTransform.eulerAngles = new Vector3(0f, 0f, 90f * wind);
    }

    void ResetShips()
    {
        shipsArrived = 0;
        UpdateWind();
        triesDone++;
        triesText.text = triesDone.ToString();
        breathingPhase = 0;
        if(triesDone < 3)
        {
            shipPlayer.gameObject.transform.position = shipsStartPos[0];
            shipNPC1.gameObject.transform.position = shipsStartPos[1];
            shipNPC2.gameObject.transform.position = shipsStartPos[2];
        }
    }
    
    void EndGame(int winCondition) // 0 lose, 1 empate, 2 win
    {
        minigameState = 2;
        endResult = winCondition;
        if(winCondition == 2)
        {
            winPanel.SetActive(true);
        }
        else if(winCondition == 1)
        {
            drawPanel.SetActive(true);
        }
        else
        {
            losePanel.SetActive(true);
        }
        minigameState = 3;
        StartCoroutine(FinishMinigame());
    }

    IEnumerator FinishMinigame()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(MinigameManager.Instance.ExitMinigame(endResult));
    }
    IEnumerator StartMinigame()
    {
        yield return new WaitForSeconds(0.5f);
        minigameState = 1;
    }
    public void OnBreathing(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            if (breathingPhase == 0) breathingPhase = 1;
        }
        if (ctx.canceled)
        {
            if (breathingPhase == 1) breathingPhase = 3;
        }
    }
}
