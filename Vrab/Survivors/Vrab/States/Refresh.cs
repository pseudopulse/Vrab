using System;
using static RoR2.MasterCatalog;

namespace Vrab.States {
    public class Refresh : BaseSkillState {
        public float radius = 25f;
        public float dataCost = 40f;
        public float buffDuration = 8f;
        public DataMeter meter;

        public override void OnEnter()
        {
            base.OnEnter();

            meter = GetComponent<DataMeter>();

            EffectManager.SpawnEffect(Survivor.RefreshEffect, new EffectData {
                origin = base.transform.position + (Vector3.up * 2f),
                scale = radius * 2f
            }, false);

            AkSoundEngine.PostEvent(Events.Play_voidDevastator_m2_secondary_explo, base.gameObject);

            meter.SpendData(dataCost);

            if (NetworkServer.active) {
                SphereSearch search = new();
                search.mask = LayerIndex.entityPrecise.mask;
                search.origin = base.transform.position;
                search.radius = radius;
                search.RefreshCandidates();
                search.FilterCandidatesByDistinctHurtBoxEntities();
                HurtBox[] boxes = search.GetHurtBoxes();

                for (int i = 0; i < boxes.Length; i++) {
                    if (!boxes[i] || !boxes[i].healthComponent) continue;
                    
                    CharacterBody body = boxes[i].healthComponent.body;

                    for (int j = 0; j < body.timedBuffs.Count; j++) {
                        body.timedBuffs[j].timer = body.timedBuffs[j].totalDuration;
                    }

                    body.AddTimedBuff(Survivor.bdOverload, buffDuration);
                }
            }

            outer.SetNextStateToMain();
        }
    }
}