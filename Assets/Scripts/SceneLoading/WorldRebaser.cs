using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRebaser : MonoBehaviour
{
    public float bounds = 16384.0f;
    public static Vector3 accumulatedOffset = Vector3.zero;

	// Update is called once per frame
	void Update ()
    {
        Vector3 PreMoveTransform = transform.position;
               
        if (PreMoveTransform.magnitude > bounds)
        {
            GameObject[] foundGameObjects = FindObjectsOfType<GameObject>();
            List<Transform> RootTransforms = new List<Transform>();

            // the transform root of the attached gameObject should not be moved
            RootTransforms.Add(transform.root);

            // this is because the attached gameObject is moved itself. 
            transform.position -= PreMoveTransform;

            // then move every other root object
            foreach (var obj in foundGameObjects)
            {
                // only move transforms once
                if ( ! RootTransforms.Contains(obj.transform.root)) 
                {
                    // move everything so this gameobject is at the world origin.
                    obj.transform.root.position -= PreMoveTransform;
                    RootTransforms.Add(obj.transform.root);
                }
            }

            //update accumulated offset
            accumulatedOffset += PreMoveTransform;
        }
    }
}
