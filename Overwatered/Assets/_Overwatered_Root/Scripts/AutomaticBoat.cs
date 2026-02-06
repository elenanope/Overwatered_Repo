using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class AutomaticBoat : MonoBehaviour
{

    #region General Variables
    [SerializeField] bool canRow = true;
    [Header("Player References")]
    [SerializeField] Animator anim;
    [SerializeField] Animator animatorL;
    [SerializeField] Animator animatorR;
    [SerializeField] AudioSource playerSpeaker;
    #endregion
    private void Start()
    {
        anim.SetInteger("rowDirection", 0);
        anim.SetBool("inBoat", true);
    }
    private void FixedUpdate()
    {
        if (canRow) BoatMovement();
    }

    void BoatMovement()
    {
        canRow = false;
        StartCoroutine(RowingCoroutine());
    }
    IEnumerator RowingCoroutine()
    {
        animatorL.SetBool("moveForward", true);
        animatorR.SetBool("moveForward", true);
        animatorL.SetTrigger("row");
        animatorR.SetTrigger("row");
        anim.SetTrigger("row");
        yield return new WaitForSeconds(0.3f);
        anim.ResetTrigger("row");
        animatorL.ResetTrigger("row");
        animatorR.ResetTrigger("row");
        yield return new WaitForSeconds(1.7f);
        StartCoroutine(ResetRow());
        yield break;
    }

    IEnumerator ResetRow()
    {
        yield return new WaitForSeconds(0.1f); //se reproduce idle de row (transición entre barridos)
        canRow = true;
        yield break;
    }
}

