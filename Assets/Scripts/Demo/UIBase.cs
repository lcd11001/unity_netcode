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

        protected virtual void Start()
        {
            if (OnStartInstantHide)
            {
                this.Hide();
            }
        }

        public virtual void Show()
        {
            root.gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            root.gameObject.SetActive(false);
        }
    }
}
