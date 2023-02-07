using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CopyToClipboard : MonoBehaviour
{
    [SerializeField] TMP_InputField input;

    public void DoCopyToClipboard()
    {
        input.text.CopyToClipboard();
    }
}
