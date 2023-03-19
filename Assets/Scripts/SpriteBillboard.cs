using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [SerializeField] bool freezeXZAxis = true;
    void Update()
    {
        if (freezeXZAxis)
        {
            this.transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        }
        else
        {
            this.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
