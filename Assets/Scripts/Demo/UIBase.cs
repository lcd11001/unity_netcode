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
    }
}
