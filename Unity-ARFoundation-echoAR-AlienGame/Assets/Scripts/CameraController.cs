using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private float lookSpeedY = -3f;
    private float lookSpeedX = 3f;

    // Update is called once per frame
    void Update () {
        float y = Input.GetAxis("Mouse X");
        float x = Input.GetAxis("Mouse Y");
        transform.eulerAngles = transform.eulerAngles - new Vector3(x * lookSpeedX, y * lookSpeedY, 0);
    }
}
