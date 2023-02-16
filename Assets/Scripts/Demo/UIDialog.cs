using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Demo
{
    public class UIDialog : UIBase<UIDialog>
    {
        [SerializeField] Button btnOK;
        [SerializeField] Button btnCancel;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] TMP_InputField input;

        public UnityEvent<string, bool> OnClosed = new UnityEvent<string, bool>();

        private TextMeshProUGUI placeHolder;

        protected override void Start()
        {
            base.Start();
            placeHolder = input.placeholder.GetComponent<TextMeshProUGUI>();
        }

        public void SetTitle(string title)
        {
            this.title.text = title;
        }

        public void SetLabel(string label)
        {
            this.label.text = label;
        }

        public void SetPlaceHolder(string placeHolder)
        {
            this.placeHolder.text = placeHolder;
        }

        private void OnEnable()
        {
            btnOK.onClick.AddListener(OnOkClicked);
            btnCancel.onClick.AddListener(OnCancelClicked);
        }

        private void OnDisable()
        {
            btnOK.onClick.RemoveListener(OnOkClicked);
            btnCancel.onClick.RemoveListener(OnCancelClicked);
        }

        private void OnCancelClicked()
        {
            this.Hide();
            OnClosed?.Invoke(input.text, false);
        }

        private void OnOkClicked()
        {
            this.Hide();
            OnClosed?.Invoke(input.text, true);
        }
    }
}
