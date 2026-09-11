using System;
using System.Linq;
using RoR2.HudOverlay;
using RoR2.Networking;
using RoR2.Orbs;
using RoR2.UI;

namespace Vrab.States {
    public class Operate : BaseSkillState {
        public float DrainRate = 6f;
        public bool isPossessing = false;
        public CharacterBody target;
        public DataMeter meter;
        public TargetTracker tracker;
        public CharacterMaster originalTargetMaster;
        public GameObject effect;
        public VrabLeapOrb orb;
        public bool hasSetGlitchIcons = false;
        public List<string> BodyBlacklist = new() {
            "UrchinTurret", "ThetaConstructBody", "DeltaConstructBody", "IvyBody", "Drone2Body", "EmergencyDroneBody"
        };
        public override void OnEnter()
        {
            base.OnEnter();

            meter = GetComponent<DataMeter>();
            tracker = GetComponent<TargetTracker>();

            if (base.isAuthority && tracker.targetHB && tracker.targetHB.teamIndex == GetTeam() && !BodyBlacklist.Contains(tracker.targetHB.healthComponent.body.gameObject.name.Replace("(Clone)", ""))) {
                target = tracker.targetHB.healthComponent.body;
            }
            else if (!target) {
                outer.SetNextStateToMain();
                return;
            }

            base.GetModelTransform().gameObject.SetActive(false);

            if (NetworkServer.active) {
                target.SetBuffCount(Survivor.bdOverload.buffIndex, 1);
                base.characterBody.SetBuffCount(Survivor.bdOverload.buffIndex, 1);
                base.characterBody.SetBuffCount(RoR2Content.Buffs.Intangible.buffIndex, 1);
            }

            orb = new();
            orb.arrivalTime = 1f;
            orb.origin = base.characterBody.corePosition;
            orb.target = target.mainHurtBox;
            orb.duration = 1f;
            orb.Begin();

            base.gameObject.layer = LayerIndex.fakeActor.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            effect = GameObject.Instantiate(Survivor.IterateEffect, target.transform);
            if (!base.isAuthority && !target.hasAuthority) {
                effect.GetComponent<PostProcessDuration>().ppVolume.gameObject.SetActive(false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (orb == null) {
                meter.SpendData(DrainRate * Time.fixedDeltaTime);
                base.characterBody.outOfCombat = false;
            }

            if (base.isAuthority) {
                if (meter.Data <= 0f || (target && (target.inputBank.interact.down && !target.inputBank.interact.wasDown))) {
                    outer.SetNextStateToMain();
                    return;
                }

                if (orb == null && target) {
                    characterMotor.Motor.SetPosition(target.corePosition + (Vector3.up * target.radius * 0.35f), true);
                }
            }

            if (orb != null) {
                orb.duration -= Time.fixedDeltaTime;

                if (orb.duration <= 0f) {
                    if (target && !target.isPlayerControlled) {
                        isPossessing = true;

                        CharacterMaster master = target.master;
                        originalTargetMaster = master;
                        master.gameObject.SetActive(false);
                        characterBody.master.bodyInstanceObject = target.gameObject;
                        target._masterObject = characterBody.master.gameObject;
                        target.masterObject = characterBody.master.gameObject;
                        _ = target.masterObject;
                        target._masterObject = characterBody.master.gameObject;
                        target._master = characterBody.master;
                        target.UpdateMasterLink();
                        characterBody.master.playerCharacterMasterController.SetBody(target.gameObject);

                        if (NetworkServer.active) {
                            target.networkIdentity.AssignClientAuthority(characterBody.networkIdentity.clientAuthorityOwner);
                            if (!characterBody.hasAuthority) {
                                target.GetComponent<CharacterNetworkTransform>().hasEffectiveAuthority = false;
                            }
                        }
                    }

                    orb = null;
                }
            }

            if (isPossessing && !hasSetGlitchIcons) {
                SetAllGlitchIcons();
            }

            if (target && effect) {
                effect.transform.position = target.corePosition;
            }
        }

        public void SetAllGlitchIcons() {
            var hud = HUD.instancesList.FirstOrDefault(x => x.targetBodyObject == target.gameObject);

            if (hud != null) {
                hasSetGlitchIcons = true;
                foreach (SkillIcon icon in hud.skillIcons) {
                    icon.AddComponent<OperateSkillSlot>().skillIcon = icon;
                }
            }
        }

        public void UnsetAllGlitchIcons() {
            var hud = HUD.instancesList.FirstOrDefault(x => x.targetBodyObject == target.gameObject || x.targetBodyObject == base.gameObject);

            if (hud != null) {
                foreach (SkillIcon icon in hud.skillIcons) {
                    icon.RemoveComponent<OperateSkillSlot>();
                    icon.iconImage.color = Color.white;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            base.GetModelTransform().gameObject.SetActive(true);

            if (effect) {
                effect.GetComponent<PostProcessFade>().Destroy();
            }

            if (isPossessing) {
                UnsetAllGlitchIcons();
            }

            if (NetworkServer.active && target) {
                target.SetBuffCount(Survivor.bdOverload.buffIndex, 0);
                base.characterBody.SetBuffCount(Survivor.bdOverload.buffIndex, 0);
                base.characterBody.SetBuffCount(RoR2Content.Buffs.Intangible.buffIndex, 0);
                base.characterBody.AddTimedBuff(DLC3Content.Buffs.DrifterFallProtection, 3f);
            }

            if (isAuthority) {
                Vector3? safe = TeleportHelper.FindSafeTeleportDestination(target.footPosition, base.characterBody, Run.instance.spawnRng);
                if (safe.HasValue) {
                    characterMotor.Motor.SetPosition(safe.Value + Vector3.up, true);
                }
                else {
                    characterMotor.Motor.SetPosition(target.transform.position + Vector3.up * (target.radius * 2.5f), true);
                }
            }

            if (target && isPossessing) {
                originalTargetMaster.gameObject.SetActive(true);
                characterBody.master.bodyInstanceObject = base.gameObject;
                characterBody.master.playerCharacterMasterController.SetBody(base.gameObject);
                base.characterBody.masterObject = characterBody.master.gameObject;
                base.characterBody.UpdateMasterLink();
                target.masterObject = originalTargetMaster.gameObject;
                target._master = originalTargetMaster;
                _ = target.masterObject;
                target.UpdateMasterLink();

                if (NetworkServer.active) {
                    target.networkIdentity.RemoveClientAuthority(characterBody.networkIdentity.clientAuthorityOwner);

                    if (!characterBody.hasAuthority) {
                        target.GetComponent<CharacterNetworkTransform>().hasEffectiveAuthority = true;
                    }
                }
            }

            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(target != null);
            if (target) {
                writer.Write(target.gameObject);
            }
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            if (reader.ReadBoolean()) {
                target = reader.ReadGameObject().GetComponent<CharacterBody>();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }

    public class VrabLeapOrb : Orb {
        public LazyAddressable<GameObject> orbEffectPrefab = new(() => Paths.GameObject.MissileVoidOrbEffect);
        public override void Begin()
        {
            base.Begin();

            base.duration = 1f;
            if (target)
            {
                EffectData effectData = new EffectData
                {
                    origin = origin,
                    genericFloat = base.duration,
                };
                
                effectData.SetHurtBoxReference(target);
                EffectManager.SpawnEffect(orbEffectPrefab, effectData, transmit: true);
            }
        }
    }
}