using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TextureScaling : MonoBehaviour
{
    private Vector3 objectScale = Vector3.zero;

	// Update is called once per frame
	void Update ()
    {
		if (objectScale != transform.localScale)
        {
            objectScale = transform.localScale;

            Material mat = GetComponent<Renderer>().material;

            mat.SetTextureScale("_MainTex", new Vector2(objectScale.x, objectScale.z));
        }
	}
}
