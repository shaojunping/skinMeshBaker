using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testCol:MonoBehaviour  {
    public Texture2D texture;
	public void OnClick()
    {
        Color[] textureCol = texture.GetPixels();
        for(int i = 0; i < textureCol.Length; i++)
        {
            Debug.LogError(textureCol[i].ToString());
        }

    }
}
