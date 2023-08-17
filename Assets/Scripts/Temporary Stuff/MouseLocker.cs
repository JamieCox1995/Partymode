using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLocker : MonoBehaviour
{
    public bool hideMouse = false;
    public CursorLockMode cursorLock = CursorLockMode.None;

    // Use this for initialization
    void Update ()
    {
        Cursor.lockState = cursorLock;
        Cursor.visible = !hideMouse;
	}
}
