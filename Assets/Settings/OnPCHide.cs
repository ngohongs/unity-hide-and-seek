using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnPCHide : MonoBehaviour
{
    private void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            var canvas = GetComponent<Canvas>();
            canvas.enabled = false;
        }
    }
}
