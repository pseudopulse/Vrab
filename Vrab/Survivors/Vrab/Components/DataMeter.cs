using System;
using System.Linq;
using RoR2.HudOverlay;
using RoR2.UI;
using UnityEngine.UI;
using static Vrab.States.Simulate;

namespace Vrab {
    public class DataMeter : MonoBehaviour {
        public float Data = 0f;
        public float MaxData = 100f;
        public OverlayController controller;
        public TargetTracker tracker;
        public bool shouldPreview = false;
        public float previewAmount = 0f;
        public bool drifting = false;
        public CharacterBody cb;

        public void Start() {
            controller = HudOverlayManager.AddOverlay(base.gameObject, new OverlayCreationParams() {
                prefab = Survivor.OverlayMeter,
                childLocatorEntry = "CrosshairExtras"
            });
            controller.onInstanceAdded += OnAdded;
            tracker = GetComponent<TargetTracker>();
            cb = GetComponent<CharacterBody>();
        }

        private void OnAdded(OverlayController controller, GameObject @object)
        {
            @object.GetComponent<CrosshairDataMeterSync>().meter = this;
        }

        public void FixedUpdate() {
            if (tracker && tracker.targetHB) {
                HealthComponent hc = tracker.targetHB.healthComponent;

                if (hc.body.teamComponent.teamIndex == cb.teamComponent.teamIndex) {
                    shouldPreview = false;
                    previewAmount = 0f;
                    return;
                }
                else {
                    shouldPreview = true;

                    float hp = hc.body.baseMaxHealth;
                    float data = Math.Clamp(Util.Remap(hp, minHealth, maxHealth, minData, maxData), minData, maxData);

                    previewAmount = Mathf.Clamp01(data / MaxData);
                }
            }
            else {
                shouldPreview = false;
                previewAmount = 0f;
            }
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
            controller.images = controller.images.Where(x => x != errorImage).ToArray();
            text = GetComponentInChildren<HGTextMeshProUGUI>();
        }
        public void Update() {
            if (!meter) {
                return;
            }

            if (meter.shouldPreview) {
                errorImage.fillAmount = meter.previewAmount;
                errorImage.enabled = true;
            }
            else {
                errorImage.fillAmount = 0f;
                errorImage.enabled = false;
            }
            
            controller.fillScalar = 1f;
            controller.SetTValue(meter.Data / meter.MaxData);
            text.text = $"{Mathf.Floor((meter.Data / meter.MaxData) * 100f)}%";
        }
    }
}