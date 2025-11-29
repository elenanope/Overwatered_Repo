using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BuoyancyObject : MonoBehaviour
{
    [SerializeField] Transform[] floaters;
    [SerializeField] float floaterRadius;
    [SerializeField] LayerMask waterLayer;

    [SerializeField] float underWaterDrag = 3f;
    [SerializeField] float underWaterAngularDrag = 1f;
    
    [SerializeField] float airDrag = 0f;
    [SerializeField] float airAngularDrag = 0.05f;

    [SerializeField] float floatingPower = 15f;
    [SerializeField] float waterHeight = 0f;

    [SerializeField] float difference;
    [SerializeField] float passedTime;

    Rigidbody rb;

    [SerializeField] bool underwater;
    [SerializeField] int floatersUnderwater;
    [SerializeField] int floatersOverwater;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        floatersUnderwater = 0;
        floatersOverwater = floaters.Length;
    }

    void FixedUpdate()
    {
        passedTime += Time.deltaTime;
        if(passedTime > 0.05f)
        {
            passedTime = 0f;
            floatersUnderwater = 0;
            for (int i = 0; i < floaters.Length; i++)
            {
                //Collider[] hits = Physics.OverlapSphere(floaters[i].position, floaterRadius, waterLayer);
                difference = floaters[i].position.y - waterHeight;// + 0.654f;
                if (difference < 0)
                {
                    rb.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(difference), floaters[i].position, ForceMode.Force);
                    floatersUnderwater++;
                    if (!underwater)
                    {
                        underwater = true;
                        SwitchState();
                    }

                }
                else
                {
                    //floatersOverwater++;
                }

            }
            if (underwater && floatersUnderwater == 0)
            {
                underwater = false;
                SwitchState();
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < floaters.Length; i++)
        {

            Gizmos.DrawSphere(floaters[i].position, floaterRadius);
            Gizmos.color = Color.blue;
        }
    }

    void SwitchState()
    {
        if(underwater)
        {
            rb.linearDamping = underWaterDrag;
            rb.angularDamping = underWaterAngularDrag;
        }
        else
        {
            {
                rb.linearDamping = airDrag;
                rb.angularDamping = airAngularDrag;
            }
        }
    }
}
