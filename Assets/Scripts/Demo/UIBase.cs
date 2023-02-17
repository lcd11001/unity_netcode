using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo
{
    public abstract class UIBase<T> : MonoBehaviourSingleton<T>
        where T : Component
    {
        public RectTransform root;
        public bool OnStartInstantHide = true;

        private RectTransform rectTransform;
        private Vector2 originPosition;

        private const string validNumer = "0123456789";
        private const string validAscii = "abcdefghijklmnopqrstuvxwyzABCDEFGHIJKLMNOPQRSTUVXWYZ";

        protected virtual void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            originPosition = rectTransform.anchoredPosition;

            if (OnStartInstantHide)
            {
                this.Hide();
            }
            else
            {
                this.Show();
            }
        }

        public virtual void Show()
        {
            rectTransform.anchoredPosition = Vector2.zero;
            root.gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            root.gameObject.SetActive(false);
            rectTransform.anchoredPosition = originPosition;
        }

        protected virtual char ValidateChar(string validCharacters, char addedChar)
        {
            if (validCharacters.IndexOf(addedChar) != -1)
            {
                // Valid
                return addedChar;
            }
            else
            {
                // Invalid
                return '\0';
            }
        }

        protected virtual char OnValidNumber(string text, int charIndex, char addedChar)
        {
            return ValidateChar(validNumer, addedChar);
        }

        protected virtual char OnValidAscii(string text, int charIndex, char addedChar)
        {
            return ValidateChar(validAscii, addedChar);
        }
    }
}
