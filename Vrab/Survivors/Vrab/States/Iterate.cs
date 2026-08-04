using System;
using System.Linq;
using RoR2.CharacterAI;
using UnityEngine.Rendering.PostProcessing;

namespace Vrab.States {
    public class Iterate : GenericCharacterMain {
        public GameObject wardInstance;
        public bool canCancel = false;
        public Animator anim;
        public DataMeter meter;
        public GameObject effect;
        public override void OnEnter()
        {
            base.OnEnter();

            if (NetworkServer.active) {
                wardInstance = GameObject.Instantiate(Survivor.IterateWard, base.transform);
                wardInstance.GetComponent<TeamFilter>().teamIndex = base.GetTeam();
                NetworkServer.Spawn(wardInstance);

                TeleportAllIntoRadius(wardInstance.GetComponent<BuffWard>().radius);

                base.characterBody.SetBuffCount(RoR2Content.Buffs.Intangible.buffIndex, 1);
            }

            anim = GetModelAnimator();
            anim.SetBool("shouldBurrow", true);

            meter = GetComponent<DataMeter>();

            effect = GameObject.Instantiate(Survivor.IterateEffect, base.transform);
            if (!base.isAuthority) {
                effect.GetComponent<PostProcessDuration>().ppVolume.gameObject.SetActive(false);
            }
        }

        public void TeleportAllIntoRadius(float radius) {
            TargetTracker tracker = GetComponent<TargetTracker>();
            foreach (CharacterMaster master in CharacterMaster.instancesList) {
                if (master && master.minionOwnership && master.minionOwnership.ownerMaster == base.characterBody.master && master.inventory && master.inventory.GetItemCount(Survivor.SimulMarker) > 0) {
                    CharacterBody body = master.GetBody();

                    if (body) {
                        if (Vector3.Distance(body.transform.position, base.transform.position) > radius) {
                            Vector3 point = GetRandomPositionIgnoreNodegraph(base.transform.position, radius * 0.2f, radius * 0.8f);
                            TeleportHelper.TeleportBody(body, point);
                            EffectManager.SpawnEffect(Paths.GameObject.VoidInfestEffect, new EffectData() {
                                origin = point,
                                scale = 5f
                            }, true);
                        }

                        BaseAI ai = master.GetComponent<BaseAI>();
                        ai.currentEnemy.Reset();
                        
                        if (tracker.targetHB && tracker.targetHB.teamIndex != GetTeam()) {
                            ai.currentEnemy.gameObject = tracker.targetHB.healthComponent.gameObject;
                        }
                        else {
                            ai.ForceAcquireNearestEnemyIfNoCurrentEnemy();
                        }
                    }
                }
            }
        }

        public Vector3 GetRandomPositionIgnoreNodegraph(Vector3 origin, float min, float max, int attempts = 0) {
            Vector3 dirVec = Vector3.zero;

            // generate a random forward vector
            while (dirVec == Vector3.zero) {
                dirVec = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            }

            // decide how far along the direction point should be
            float dist = Random.Range(min, max);

            Vector3 pos = ValidatePosition(origin + (dirVec * dist));

            if (pos == base.transform.position && attempts < 20) {
                // invalid spot, reroll (up to a limit)
                return GetRandomPositionIgnoreNodegraph(origin, min, max, attempts + 1);
            }
            
            return pos;
        }

        public Vector3 ValidatePosition(Vector3 position) {
            Vector3 pos = position;

            if (Physics.Raycast(pos + (Vector3.up * 10f), Vector3.down, out RaycastHit hit, 200f, LayerIndex.world.mask)) {
                return hit.point;
            }

            return base.transform.position;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            meter.SpendData(7f * Time.fixedDeltaTime);
            base.characterBody.outOfCombat = false;

            if (base.isAuthority) {
                if (meter.Data <= 0f) {
                    outer.SetNextStateToMain();
                    return;
                }
                
                base.characterMotor.velocity = Vector3.zero;

                if (!this.inputBank.skill3.down && base.fixedAge >= 1f) {
                    canCancel = true;
                }

                if (this.inputBank.skill3.down) {
                    if (canCancel) {
                        outer.SetNextStateToMain();
                    }
                }
            }
        }

        public override void HandleMovements() {}
        public override void ProcessJump() {}

        public override void OnExit()
        {
            base.OnExit();

            anim.SetBool("shouldBurrow", false);

            if (wardInstance) {
                GameObject.Destroy(wardInstance);
            }

            if (effect) {
                effect.GetComponent<PostProcessFade>().Destroy();
            }

            if (NetworkServer.active) {
                base.characterBody.SetBuffCount(RoR2Content.Buffs.Intangible.buffIndex, 0);
            }
        }
    }
}