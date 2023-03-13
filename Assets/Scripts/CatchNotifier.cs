using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchNotifier : MonoBehaviour
{
    public GameManager gameManager;
    private void OnTriggerEnter(Collider other)
    {
        gameManager.EndGame();
    }
}
