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

    [SerializeField] bool canBreath;
    [SerializeField] int gameDifficulty; //0 easy, 1 medium, 2 hard
    [SerializeField] int breathingPhase; //0 not breathing, 1 breath in, 2 breath out, 3 soplar, 4 coroutina
    [SerializeField] int shipsArrived; //0 ninguno, 1 ya ha frenado el del player, 2 el del npc 1, 3 el del npc 2


    void Start()
    {
        triesDone = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (((pointsNPC1 == 1 || pointsNPC2 == 1) && pointsPlayer == 1) || triesDone <= 2) //si lleva dos intentos o está empatado
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
                    triesDone++;
                    triesText.text = triesDone.ToString();
                    //añadir sonido de soplido
                    //añadir fuerza a los barquitos multiplicada por 0.5 si !eastWind (o añadir que sea algo más random)

                    //añadir coroutina para que los barcos lleguen al sitio y se te sumen los puntos
                    breathingPhase = 4;
                    StartCoroutine(ShipsMovement());
                }
                else if(breathingPhase == 4)
                {
                    airTaken -= Time.deltaTime * airTakenSpeed * 2;//más rápido?
                }
            }
        }
        else if (pointsPlayer >= 2) WinGame();
        else if(pointsNPC1 == 1 && pointsPlayer == 1 && pointsPlayer == 1) DrawGame();
        else LoseGame();
    }
    IEnumerator ShipsMovement()
    {
        wind = Random.Range(-1, 2);
        if (wind == -1) windMult = 0.5f;
        else if (wind == 0) windMult = 1f;
        else windMult = 2f;
        PhaseUpdate();
        Debug.Log("Barco Player");
        yield return new WaitForSeconds(5f); // o esperar a que frene
        shipsArrived = 1;
        PhaseUpdate();
        Debug.Log("Barco 1");
        yield return new WaitForSeconds(5f); // o esperar a que frene
        shipsArrived = 2;
        PhaseUpdate();
        Debug.Log("Barco 2");
        yield return new WaitForSeconds(5f); // o esperar a que frene
        shipsArrived = 3;

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
            //calcular distancias de cada bote y sumar puntos
            Debug.Log("Juego finalizado");
        }
        else
        {
            if (shipsArrived == 1) shipToMove = shipNPC1;
            else if (shipsArrived == 2) shipToMove = shipNPC2;
            if(gameDifficulty == 0) //revisar todo esto
            {
                shipForce = Random.Range(0.1f, 1.9f);
            }
            else if(gameDifficulty == 1)
            {
                if(wind == 0)
                {
                    shipForce = Random.Range(0.2f, 1.5f);
                }
                else if(wind == 1)
                {
                    shipForce = Random.Range(0.1f, 1f);
                }
                else if(wind == -1)
                {
                    shipForce = Random.Range(0.4f, 2f);
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
                    shipForce = Random.Range(0.6f, 1.6f);
                }
            }
            shipToMove.AddForce(shipToMove.transform.forward * shipForce * windMult, ForceMode.Impulse);// o velocity change
        }
    }//también puedo sacar diálogos random de personajes de fondo mientras se muevan las barcas
    void ResetShips()
    {
        //para nueva ronda, todo debe estar como al principio
    }
    #region EndResults
    void WinGame()
    {
        Debug.Log("You won! Here is a gift");
    }
    void DrawGame()
    {
        Debug.Log("It is a draw! Here is a gift for your dedication");
    }
    void LoseGame()
    {
        Debug.Log("It is a draw! Here is a gift for your dedication");
    }
    #endregion
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
