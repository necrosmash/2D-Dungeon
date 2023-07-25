using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDisplay : MonoBehaviour
{
    [SerializeField] private Player p;
    private float inputMovementAmount, jumpEndingPos;
    private Vector3 playerPos;
    private bool isGainingHeight;

    private TMPro.TMP_Text display;
    string sInputMovementAmount, sIsGainingHeight;

    void Start()
    {
        display = transform.GetComponent<TMPro.TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        inputMovementAmount = p.InputMoveAmount;
        playerPos = p.transform.position;
        jumpEndingPos = p.JumpEndingY;
        isGainingHeight = p.IsGainingHeight;

        if (inputMovementAmount < 0)
            sInputMovementAmount = "<color=\"red\">" + inputMovementAmount + "</color>";
        else if (inputMovementAmount > 0)
            sInputMovementAmount = "<color=\"green\">" + inputMovementAmount + "</color>";
        else
            sInputMovementAmount = "<color=\"yellow\">" + inputMovementAmount + "</color>";

        if (isGainingHeight)
            sIsGainingHeight = "<color=\"green\">" + isGainingHeight + "</color>";
        else
            sIsGainingHeight = "<color=\"red\">" + isGainingHeight + "</color>";

        display.text = "inputMovement: " + sInputMovementAmount + "\n" +
            "position: " + playerPos + "\n" +
            "jumpEnd: " + jumpEndingPos + "\n" +
            "gainingHeight: " + sIsGainingHeight;
    }
}
