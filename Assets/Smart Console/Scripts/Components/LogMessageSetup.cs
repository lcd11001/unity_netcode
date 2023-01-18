using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SmartConsole.Components
{
    public class LogMessageSetup : MonoBehaviour
    {
        [SerializeField] private Image m_IconImage;
        [SerializeField] private TextMeshProUGUI m_Text;
        [SerializeField] private Image m_BackgroundImage;

        [Space(10)]
        
        [SerializeField] private Sprite m_LogSprite;
        [SerializeField] private Sprite m_ErrorSprite;
        [SerializeField] private Sprite m_WarningSprite;
        [SerializeField] private Sprite m_CommandSprite;

        [Space(10)]

        [SerializeField] private Color m_LogColor = Color.white;
        [SerializeField] private Color m_WarningColor = Color.yellow;
        [SerializeField] private Color m_ErrorColor = Color.red;
        [SerializeField] private Color m_CommandColor = Color.green;

        [Space(10)]

        [SerializeField] private Color m_ParameterColor;
        [SerializeField] private Color m_BackgroundColor1;
        [SerializeField] private Color m_BackgroundColor2;
        [SerializeField] private Color m_AutocompleteBackgroundColor;
        
        private const string k_DateFormat = "hh:mm:ss";
        
        public void SetText(string message, LogMessageTypes type, bool addDate = true)
        {
            if (addDate)
            {
                string time = DateTime.Now.ToString(k_DateFormat);
                m_Text.text = "[" + time + "] ";
            }
            
            m_Text.text += message;

            m_Text.color = type switch
            {
                LogMessageTypes.Log => m_LogColor,
                LogMessageTypes.Error => m_ErrorColor,
                LogMessageTypes.Warning => m_WarningColor,
                LogMessageTypes.Command => m_CommandColor,
                _ => m_LogColor
            };
        }
        
        public void SetTextParameter(string[] parameters)
        {
            string colorHex = ColorUtility.ToHtmlStringRGBA(m_ParameterColor);

            for (int i = 0; i < parameters.Length; i++)
            {
                m_Text.text += $"<color=#{colorHex}> {parameters[i]}</color>";
            }
        }

        public void SetIcon(LogMessageTypes type)
        {
            m_IconImage.sprite = type switch
            {
                LogMessageTypes.Log => m_LogSprite,
                LogMessageTypes.Error => m_ErrorSprite,
                LogMessageTypes.Warning => m_WarningSprite,
                LogMessageTypes.Command => m_CommandSprite,
                LogMessageTypes.Autocomplete => null,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public void SetBackgroundColor(ref bool even)
        {
            Color color = even ? m_BackgroundColor2 : m_BackgroundColor1;
            m_BackgroundImage.color = color;
            even = !even;
        }
        
        public void SetAutocompleteBackgroundColor()
        {
            m_BackgroundImage.color = m_AutocompleteBackgroundColor;
        }

        public void SetBackgroundAlpha(float alpha)
        {
            Color c1 = new Color(m_BackgroundColor1.r, m_BackgroundColor1.g, m_BackgroundColor1.b, alpha);
            m_BackgroundColor1 = c1;
            
            Color c2 = new Color(m_BackgroundColor2.r, m_BackgroundColor2.g, m_BackgroundColor2.b, alpha);
            m_BackgroundColor2 = c2;
        }
    }
}
