using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPS_Counter : MonoBehaviour
{    
        float deltaTime = 0.0f;
        Text text;
        private void Start()
        {
            text = GetComponent<Text>();
        }
        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            float msec = deltaTime * 1000.0f;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            this.text.text = text;
        }    
}
