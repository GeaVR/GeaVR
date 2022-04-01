using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlWithMouse : MonoBehaviour {
    public float SPEEDROT = 50, MAX_ROTATION_ANGLE = 75;

    // Update is called once per frame
    void Update () {
        float roty = -Input.GetAxis("Mouse Y");
        float futureAngle = ((transform.eulerAngles.x + roty > 0) ? (transform.eulerAngles.x + roty) : (360 + transform.eulerAngles.x + roty)) % 360;
        transform.eulerAngles += (
            (futureAngle > 0 && futureAngle < MAX_ROTATION_ANGLE) ||
            (futureAngle > 360 - MAX_ROTATION_ANGLE && futureAngle < 360)
            ) ? new Vector3(roty, 0, 0) * Time.deltaTime * SPEEDROT : Vector3.zero;
    }
}
