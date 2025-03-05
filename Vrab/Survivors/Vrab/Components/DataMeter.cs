using System;
using RoR2.HudOverlay;
using RoR2.UI;
using UnityEngine.UI;

namespace Vrab {
    public class DataMeter : MonoBehaviour {
        public float Data = 0f;
        public float MaxData = 100f;
        public OverlayController controller;
        public float errTime = 0f;
        public float errPerct = 1f;

        public void Start() {
            controller = HudOverlayManager.AddOverlay(base.gameObject, new OverlayCreationParams() {
                prefab = Survivor.OverlayMeter,
                childLocatorEntry = "CrosshairExtras"
            });
            controller.onInstanceAdded += OnAdded;
        }

        private void OnAdded(OverlayController controller, GameObject @object)
        {
            @object.GetComponent<CrosshairDataMeterSync>().meter = this;
        }

        public void OnDestroy() {
            if (controller != null) {
                HudOverlayManager.RemoveOverlay(controller);
                controller.onInstanceAdded -= OnAdded;
            }
        }

        public void AddData(float amount) {
            Data += amount;
            if (Data > MaxData) {
                Data = MaxData;
            }
        }

        public void SpendData(float amount) {
            Data -= amount;
            if (Data < 0f) {
                Data = 0f;
            }
        }
    }

    public class CrosshairDataMeterSync : MonoBehaviour {
        public ImageFillController controller;
        public DataMeter meter;
        public HGTextMeshProUGUI text;
        public Image errorImage;

        public void Start() {
            controller = GetComponent<ImageFillController>();
            errorImage = controller.images[2];
            text = GetComponentInChildren<HGTextMeshProUGUI>();
        }
        public void Update() {
            if (!meter) {
                return;
            }

            if (meter.errTime > 0f) {
                meter.errTime -= Time.fixedDeltaTime;
                text.text = $"!ERR!MISSING\nDATA";
                errorImage.fillAmount = meter.errPerct;
                return;
            }
            
            controller.fillScalar = 1f;
            controller.SetTValue(meter.Data / meter.MaxData);
            errorImage.fillAmount = 0f;
            text.text = $"{Mathf.Floor((meter.Data / meter.MaxData) * 100f)}%";
        }
    }
}