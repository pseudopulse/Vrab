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
        public float OverflowData = 0f;
        public float MaxOverflowData = 75f;
        public float OverflowDrainRate = 20f;
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

            float drain = OverflowDrainRate * Time.fixedDeltaTime;
            if (OverflowData > drain && cb && cb.outOfCombat && cb.outOfDanger) {
                if (Data + drain <= MaxData + (drain * 0.5f)) {
                    Data += drain;
                    if (Data > MaxData) {
                        Data = MaxData;
                    }

                    OverflowData -= drain;
                    if (OverflowData < 0) {
                        OverflowData = 0;
                    }
                }
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
                float overflow = Data - MaxData;
                OverflowData += overflow;
                if (OverflowData > MaxOverflowData) {
                    OverflowData = MaxOverflowData;
                }

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
        public Image overflow;
        public float dataRenderPerct;
        public float overflowRenderPerct;
        public float smoothingTime = 0.2f;
        public CanvasGroup hud;
        public float stopTrying = 0f;
        public void Start() {
            controller = GetComponent<ImageFillController>();
            errorImage = controller.images[2];
            overflow = controller.images[3];
            controller.images = controller.images.Where(x => x != errorImage && x != overflow).ToArray();
            text = GetComponentInChildren<HGTextMeshProUGUI>();
        }
        public void Update() {
            if (!meter) {
                return;
            }

            if (hud) {
                hud.alpha = 1;
            }
            else {
                if (HUD.instancesList.Count >= 1 && stopTrying < 3f) {
                    hud = HUD.instancesList[0].GetComponent<CanvasGroup>();
                    stopTrying += Time.deltaTime;
                }
            }

            if (meter.shouldPreview) {
                errorImage.fillAmount = meter.previewAmount;
                errorImage.enabled = true;
            }
            else {
                errorImage.fillAmount = 0f;
                errorImage.enabled = false;
            }

            float overflowTarget = meter.OverflowData / meter.MaxOverflowData;
            float dataTarget = meter.Data / meter.MaxData;

            overflowRenderPerct = Mathf.MoveTowards(overflowRenderPerct, overflowTarget, (Mathf.Abs(overflowTarget - overflowRenderPerct) / smoothingTime) * Time.fixedDeltaTime);
            dataRenderPerct = Mathf.MoveTowards(dataRenderPerct, dataTarget, (Mathf.Abs(dataTarget - dataRenderPerct) / smoothingTime) * Time.fixedDeltaTime);

            overflow.fillAmount = overflowRenderPerct;
            
            if (dataRenderPerct > 0.995f) {
                dataRenderPerct = 1f;
            }

            if (dataRenderPerct > 0.745f && dataRenderPerct < 0.75f) {
                dataRenderPerct = 0.75f;
            }

            controller.fillScalar = 1f;
            controller.SetTValue(dataRenderPerct);
            text.text = $"{Mathf.Floor((dataRenderPerct) * 100f)}%";
        }
    }
}