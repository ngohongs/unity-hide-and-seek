using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEscapeInput : MonoBehaviour
{
    public UIController Controller;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEscape()
    {
        Controller.Back();
    }
}
