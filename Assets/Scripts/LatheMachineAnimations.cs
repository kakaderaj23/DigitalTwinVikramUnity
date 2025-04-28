using Unity.VisualScripting;
using UnityEngine;

public class LatheMachineAnimations : MonoBehaviour
{
    // Rotating drill
    public GameObject Drill;

    // Rotation speed of the drill
    public float drillSpeed = 1f;

    // Moving tailstock object
    public GameObject tailStock;

    // Speed at which the tailstock moves
    public float tailStockSpeed = 1f;

    // Movement limits on the Y-axis
    private float tailStockMinY; // forward limit
    private float tailStockMinY1; // forward limit
    private float tailStockMaxY; // backward limit
    public GameObject tailStockLimitReferenceMin;
    public GameObject tailStockLimitReferenceMin1;
    public GameObject tailStockLimitReferenceMax;
    float moveAmount;
    // Direction control
    private bool movingAhead = false;
    void Start()
    {
        moveAmount = 0.001f * tailStockSpeed;
        tailStockMaxY = tailStockLimitReferenceMax.gameObject.transform.position.z;
        tailStockMinY = tailStockLimitReferenceMin.gameObject.transform.position.z;
        tailStockMinY1 = tailStockLimitReferenceMin1.gameObject.transform.position.z;

    }

    void Update()
    {
        // Rotate the drill
        if (Drill != null)
        {
            Drill.transform.Rotate(0, drillSpeed, 0);
        }

        if (tailStock != null)
        {

            // Move forward (downward in Y)
            if (movingAhead)
            {
                Debug.Log("if running");
                Debug.Log(tailStock.transform.position.z + "Z Position");
                Debug.Log(tailStock.transform.position.y + "Y Position");
                
                tailStock.transform.Translate(0, moveAmount, 0);

                if (tailStock.transform.position.z <= tailStockMaxY)
                {
                    movingAhead = false;
                    Debug.Log("moving ahead value changed to false");
                }
            }
            // Move backward (upward in Y)
            else
            {
                Debug.Log("else running");
                if (tailStock.transform.position.z >= tailStockMinY1)
                {
                    tailStock.transform.Translate(0, -moveAmount / 2, 0);
                }
                else
                {
                    tailStock.transform.Translate(0, -moveAmount, 0);
                }
                
                Debug.Log(tailStock.transform.position.z + "Z Position");
                Debug.Log(tailStock.transform.position.y + "Y Position");
                if (tailStock.transform.position.z >= tailStockMinY)
                {   
                    Debug.Log("moving ahead value changed to true");
                    movingAhead = true; 
                }

            }
        }
    }
}
