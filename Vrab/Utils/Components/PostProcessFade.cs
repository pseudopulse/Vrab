using System;
using UnityEngine.Rendering.PostProcessing;

namespace Vrab.Utils {
    public class PostProcessFade : MonoBehaviour {
        public float time = 0.3f;
        public float stopwatch = 0f;
        public PostProcessVolume volume;
        private bool destroying = false;

        public void Start() {
            
        }

        public void Update() {
            stopwatch += Time.deltaTime;
            if (!destroying) {
                volume.weight = Mathf.Clamp((stopwatch / time), 0.1f, 1f);
            }
            else {
                volume.weight = Mathf.Clamp((1f - (stopwatch / time)), 0.1f, 1f);

                if (stopwatch >= time) {
                    GameObject.Destroy(base.gameObject);
                }
            }
        }

        public void Destroy() {
            if (!destroying) {
                destroying = true;
                stopwatch = 0f;
            }
        }
    }
}