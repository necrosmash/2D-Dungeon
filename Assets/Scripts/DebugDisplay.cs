using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDisplay : MonoBehaviour
{
    [SerializeField] private Player p;
    private float inputRunAmount, jumpEndingPos;
    private Vector3 playerPos;
    private bool isGainingHeight;

    private TMPro.TMP_Text display;
    string sInputRunAmount, sIsGainingHeight;

    void Start()
    {
        display = transform.GetComponent<TMPro.TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        inputRunAmount = p.InputRunAmount;
        playerPos = p.transform.position;
        jumpEndingPos = p.JumpEndingY;
        isGainingHeight = p.IsGainingHeight;

        if (inputRunAmount < 0)
            sInputRunAmount = "<color=\"red\">" + inputRunAmount + "</color>";
        else if (inputRunAmount > 0)
            sInputRunAmount = "<color=\"green\">" + inputRunAmount + "</color>";
        else
            sInputRunAmount = "<color=\"yellow\">" + inputRunAmount + "</color>";

        if (isGainingHeight)
            sIsGainingHeight = "<color=\"green\">" + isGainingHeight + "</color>";
        else
            sIsGainingHeight = "<color=\"red\">" + isGainingHeight + "</color>";

        display.text = "inputMovement: " + sInputRunAmount + "\n" +
            "position: " + playerPos + "\n" +
            "jumpEnd: " + jumpEndingPos + "\n" +
            "gainingHeight: " + sIsGainingHeight;
    }
}
