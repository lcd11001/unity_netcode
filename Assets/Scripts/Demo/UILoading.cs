using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class UILoading : UIBase<UILoading>
    {
        [SerializeField] Image spinner;
        [SerializeField] float timer360;

        private Vector3 rotateAngle = new Vector3(0, 0, 0);

        protected override void Start()
        {
            base.Start();
            rotateAngle.z = 360.0f / timer360;
        }

        private void Update()
        {
            spinner.transform.Rotate(rotateAngle * Time.deltaTime);
        }
    }
}
