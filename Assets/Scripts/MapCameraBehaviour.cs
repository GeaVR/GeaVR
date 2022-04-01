using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapCameraBehaviour : MonoBehaviour {
    public GameObject target;
    public GameObject targetPointer;
    public float HEIGHT = 0;
    float min;
    float max;
    float zoomVal;

    // Update is called once per frame
    void Start()
    {

        if (HEIGHT == 0) HEIGHT = transform.position.x> transform.position.z? transform.position.x: transform.position.z;

        min = HEIGHT * 4;
        max = HEIGHT;
        zoomVal = HEIGHT * 2;

      

    }
	void Update () {


        transform.position = target.transform.position + Vector3.up * zoomVal;
        
        targetPointer.transform.rotation = Quaternion.Euler(0, 0, -1*(target.transform.eulerAngles.y));

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {

            Vector3 rotationVector = new Vector3(0,(target.transform.eulerAngles.y ),-1* (target.transform.eulerAngles.y));

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsInfoOnMap").gameObject.transform.Find("GameObject").gameObject.transform.Find("GameObject").transform.rotation = Quaternion.Euler(rotationVector);
            //GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsInfoOnMap").gameObject.transform.Find("GameObject").gameObject.transform.Find("GameObject").transform.rotation = Quaternion.Euler(rotationVector_after);

        }

    }


    public void ZoomEdit()
    {
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {

            float zoomValue = GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsInfoOnMap").gameObject.transform.Find("Slider").GetComponent<Slider>().value;

      
             zoomVal = min *  zoomValue;


       
        } 
    }

  
}
