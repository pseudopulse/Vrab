using System;
using System.Linq;
using RoR2.CharacterAI;
using static RoR2.MasterCatalog;

namespace Vrab.States {
    public class Simulate : BaseSkillState {
        public float minHealth = 10f;
        public float maxHealth = 2400f;
        public float minData = 35f;
        public float maxData = 100f;
        public TargetTracker tracker;
        public DataMeter meter;
        float data;

        public override void OnEnter()
        {
            base.OnEnter();

            meter = GetComponent<DataMeter>();
            tracker = GetComponent<TargetTracker>();

            data = meter.Data;

            if (tracker.target) {
                HurtBox box = tracker.target.GetComponent<HurtBox>();

                if (!box || !box.healthComponent) {
                    outer.SetNextStateToMain();
                    return;
                }

                float hp = box.healthComponent.body.baseMaxHealth;
                float data = Util.Remap(hp, minHealth, maxHealth, minData, maxData);

                if (meter.Data < data) {
                    meter.errPerct = data / meter.MaxData;
                    meter.errTime = 2f;
                    outer.SetNextStateToMain();
                    return;
                }

                MasterIndex index = MasterCatalog.FindAiMasterIndexForBody(box.healthComponent.body.bodyIndex);
                GameObject masterPrefab = MasterCatalog.GetMasterPrefab(index);

                if (masterPrefab) {
                    meter.SpendData(data);

                    MasterSummon summon = new();
                    summon.summonerBodyObject = base.gameObject;
                    summon.position = FindModelChild("MuzzleDeconstruct").transform.position + Vector3.up;
                    summon.rotation = base.transform.rotation;
                    summon.ignoreTeamMemberLimit = true;
                    summon.inventoryToCopy = base.characterBody.master.inventory;
                    summon.teamIndexOverride = base.characterBody.teamComponent.teamIndex;
                    summon.useAmbientLevel = true;
                    summon.masterPrefab = masterPrefab;
                    summon.preSpawnSetupCallback = (master) => {
                        master.inventory.GiveItem(Survivor.SimulMarker);
                        master.inventory.GiveItem(RoR2Content.Items.BoostDamage, 5);
                        master.inventory.GiveItem(RoR2Content.Items.BoostHp, 10);
                        var driver = master.AddComponent<AISkillDriver>();
                        driver.minDistance = 50f;
                        driver.maxDistance = float.PositiveInfinity;
                        driver.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
                        driver.shouldSprint = true;
                        driver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                        driver.aimType = AISkillDriver.AimType.AtCurrentLeader;
                        
                        List<AISkillDriver> drivers = master.GetComponent<BaseAI>().skillDrivers.ToList();
                        foreach (AISkillDriver driver2 in drivers) {
                            if (driver2.maxDistance > 20f) {
                                driver2.maxDistance *= 3f;
                            }
                            driver2.shouldSprint = true;
                        }
                        drivers.Insert(0, driver);
                        master.GetComponent<BaseAI>().skillDrivers = drivers.ToArray();
                    };

                    EffectManager.SpawnEffect(Survivor.SummonHoloEffect, new EffectData {
                        origin = summon.position,
                        scale = 1f
                    }, false);

                    AkSoundEngine.PostEvent(Events.Play_voidDevastator_m2_secondary_explo, base.gameObject);

                    if (NetworkServer.active) {
                        summon.Perform();
                    }
                }
            }

            outer.SetNextStateToMain();
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(data);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            GetComponent<DataMeter>().Data = reader.ReadSingle();
        }
    }
}