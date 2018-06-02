using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class testColNew : MonoBehaviour {

    public Text text;
    public void OnClick()
    {
        text.color = new Color(0f, 0f, 0f);
    }
}
