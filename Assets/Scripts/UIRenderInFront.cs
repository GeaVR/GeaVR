using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIRenderInFront : MonoBehaviour
{

    public UnityEngine.Rendering.CompareFunction comparison = UnityEngine.Rendering.CompareFunction.Always;

    public bool apply = true;

    private void Update()
    {
        if (apply)
        {
            apply = false;
            Debug.Log("Updated material val");
            Image image = GetComponent<Image>();
            Material existingGlobalMat = image.materialForRendering;
            Material updatedMaterial = new Material(existingGlobalMat);
            updatedMaterial.SetInt("unity_GUIZTestMode", (int)comparison);
            image.material = updatedMaterial;
        }
    }
}

// found here
// https://answers.unity.com/questions/878667/world-space-canvas-on-top-of-everything.html