using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class position : MonoBehaviour {

	public Vector3 pos;

	// Use this for initialization
	void Start () {
		pos= transform.position;
		Debug.Log("pos is " + pos);
	}

}