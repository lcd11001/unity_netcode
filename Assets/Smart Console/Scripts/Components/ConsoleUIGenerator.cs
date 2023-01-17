using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SmartConsole.Components
{
    [RequireComponent(typeof(ConsoleSystem))]
    public class ConsoleUIGenerator : MonoBehaviour
    {
        [SerializeField] [Range(0f, 1f)] private float m_alpha = 1.0f;
        [SerializeField] private Image m_mainPanel;
        [SerializeField] private Transform m_GridContent;
        [SerializeField] private GameObject m_LogMessageTextPrefab;
        
        private ConsoleSystem m_ConsoleSystem;
        private List<GameObject> m_AutocompleteInstances = new List<GameObject>();
        private string m_AutocompleteCommandRef;
        private bool m_IsLastMessageEven;
        private float m_PrevAlpha = -1.0f;
        
        private void OnEnable()
        {
            m_ConsoleSystem.OnSubmitLogMessage += GenerateLog;
            m_ConsoleSystem.OnSubmitAutocompleteLogMessage += GenerateAutocompleteLog;
            m_ConsoleSystem.OnClearAutocomplete += ClearAutocompletes;
        }
        
        private void OnDisable()
        {
            m_ConsoleSystem.OnSubmitLogMessage -= GenerateLog;
            m_ConsoleSystem.OnSubmitAutocompleteLogMessage -= GenerateAutocompleteLog;
            m_ConsoleSystem.OnClearAutocomplete -= ClearAutocompletes;
        }

        private void OnValidate()
        {
            SetupAlpha();
        }

        private void Awake()
        {
            m_ConsoleSystem = gameObject.GetComponent<ConsoleSystem>();
        }

        private void Start()
        {
            SetupAlpha();
        }

        private void SetupAlpha()
        { 
            if (m_PrevAlpha == m_alpha)
            {
                return;
            }

            if (m_mainPanel != null)
            {
                Color c = new Color(m_mainPanel.color.r, m_mainPanel.color.g, m_mainPanel.color.b, m_alpha);
                m_mainPanel.color = c;
            }

            if (m_LogMessageTextPrefab != null)
            {
                if (m_LogMessageTextPrefab.TryGetComponent<LogMessageSetup>(out LogMessageSetup logMessageSetup))
                {
                    logMessageSetup.SetBackgroundAlpha(m_alpha);
                }
            }

            if (m_GridContent != null)
            {
                if (m_GridContent.TryGetComponent<Image>(out Image gridBackground))
                {
                    gridBackground.enabled = false;
                }

                LogMessageSetup[] children = m_GridContent.GetComponentsInChildren<LogMessageSetup>();
                foreach(LogMessageSetup logMessage in children)
                {
                    Image background = logMessage.GetComponent<Image>();
                    background.color = new Color(background.color.r, background.color.g, background.color.b, m_alpha);
                }
            }

            m_PrevAlpha = m_alpha;
        }

        private void GenerateLog(LogMessage logMessage)
        {
            GameObject logInstance = Instantiate(m_LogMessageTextPrefab, m_GridContent);
            
            if (logInstance.TryGetComponent(out LogMessageSetup logInstanceSetup))
            {
                logInstanceSetup.SetText(logMessage.Text);
                logInstanceSetup.SetIcon(logMessage.Type);
                logInstanceSetup.SetBackgroundColor(ref m_IsLastMessageEven);
            }
        }

        private void GenerateAutocompleteLog(LogMessage logMessage, string text)
        {
            GameObject logInstance = Instantiate(m_LogMessageTextPrefab, m_GridContent);

            if (string.IsNullOrEmpty(m_AutocompleteCommandRef))
            {
                m_AutocompleteCommandRef = text;
            }
            else if (m_AutocompleteCommandRef != text)
            {
                ClearAutocompletes();
                m_AutocompleteCommandRef = text;
            }
                
            m_AutocompleteInstances.Add(logInstance);
            m_ConsoleSystem.AutocompleteLogMessages.Add(logMessage);

            if (logInstance.TryGetComponent(out LogMessageSetup logInstanceSetup))
            {
                logInstanceSetup.SetText(logMessage.Text, false);
                logInstanceSetup.SetIcon(LogMessageTypes.Autocomplete);
                logInstanceSetup.SetAutocompleteBackgroundColor();
                
                if (logMessage.ParametersNames != null)
                {
                    logInstanceSetup.SetTextParameter(logMessage.ParametersNames);
                }
            }
        }

        private void ClearAutocompletes()
        {
            for (int i = 0; i < m_AutocompleteInstances.Count; i++)
            {
                Destroy(m_AutocompleteInstances[i]);
            }
            
            m_AutocompleteInstances.Clear();
            m_ConsoleSystem.AutocompleteLogMessages.Clear();
        }
        
        public void Clear()
        {
            foreach (Transform child in m_GridContent)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
