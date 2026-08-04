using System;
using System.Linq;
using RoR2.CharacterAI;
using static RoR2.MasterCatalog;

namespace Vrab.States {
    public class Simulate : BaseSkillState {
        public static float minHealth = 10f;
        public static float maxHealth = 2400f;
        public static float minData = 35f;
        public static float maxData = 100f;
        public TargetTracker tracker;
        public DataMeter meter;
        public static Dictionary<MasterIndex, GameObject> ReplacementMap = null;
        public static GameObject[] LunarChimera;
        float data;

        public override void OnEnter()
        {
            base.OnEnter();

            if (ReplacementMap == null) {
                ReplacementMap = new() {
                    { MasterCatalog.FindMasterIndex("SolusWingMaster"), Paths.GameObject.VultureHunterMaster },
                    { MasterCatalog.FindMasterIndex("SolusHeartMaster"), Paths.GameObject.VultureHunterMaster },
                    { MasterCatalog.FindMasterIndex("VoidRaidCrabMaster"), Paths.GameObject.VoidMegaCrabMaster },
                    { MasterCatalog.FindMasterIndex("BrotherMaster"), null },
                    { MasterCatalog.FindMasterIndex("BrotherHurtMaster"), null },
                    { MasterCatalog.FindMasterIndex("FalseSonBossMaster"), Paths.GameObject.HalcyoniteMaster },
                    { MasterCatalog.FindMasterIndex("FalseSonBossLunarShardMaster"), Paths.GameObject.HalcyoniteMaster },
                    { MasterCatalog.FindMasterIndex("FalseSonBossLunarShardBrokenMaster"), Paths.GameObject.HalcyoniteMaster },
                };

                LunarChimera = new GameObject[] { Paths.GameObject.LunarWispMaster, Paths.GameObject.LunarGolemMaster };
            }

            meter = GetComponent<DataMeter>();
            tracker = GetComponent<TargetTracker>();

            data = meter.Data;

            if (tracker.target) {
                HurtBox box = tracker.target.GetComponent<HurtBox>();

                if (!box || !box.healthComponent || box.teamIndex == base.GetTeam()) {
                    outer.SetNextStateToMain();
                    return;
                }

                float hp = box.healthComponent.body.baseMaxHealth;
                float data = Math.Clamp(Util.Remap(hp, minHealth, maxHealth, minData, maxData), minData, maxData);

                if (meter.Data < data) {
                    outer.SetNextStateToMain();
                    return;
                }

                MasterIndex index = MasterCatalog.FindAiMasterIndexForBody(box.healthComponent.body.bodyIndex);
                GameObject masterPrefab = MasterCatalog.GetMasterPrefab(index);

                if (ReplacementMap.ContainsKey(index)) {
                    masterPrefab = ReplacementMap[index];
                    if (masterPrefab == null) {
                        masterPrefab = LunarChimera.GetRandom();
                    }
                    index = MasterCatalog.FindMasterIndex(masterPrefab);
                }

                if (box.healthComponent.body.isChampion) {
                    KillAllBosses();
                }
                else if (GetSameIndexSummons(index) >= GetCap(box.healthComponent.body)) {
                    outer.SetNextStateToMain();
                    return;
                }

                if (masterPrefab) {
                    meter.SpendData(data);

                    MasterSummon summon = new();
                    summon.summonerBodyObject = base.gameObject;
                    summon.position = FindModelChild("MuzzleDeconstruct").transform.position + Vector3.up;
                    if (box.healthComponent.body.isFlying) {
                        summon.position += Vector3.up * 8f;
                    }
                    summon.rotation = base.transform.rotation;
                    summon.ignoreTeamMemberLimit = true;
                    summon.inventoryToCopy = base.characterBody.master.inventory;
                    summon.teamIndexOverride = base.characterBody.teamComponent.teamIndex;
                    summon.useAmbientLevel = true;
                    summon.masterPrefab = masterPrefab;
                    summon.inventoryItemCopyFilter = (index) => {
                        ItemDef def = ItemCatalog.GetItemDef(index);

                        if (def && def.ContainsTag(ItemTag.Healing)) {
                            return false;
                        }

                        if (def && def.itemIndex == RoR2Content.Items.ShieldOnly.itemIndex) {
                            return false;
                        }

                        if (def && def.ContainsTag(ItemTag.AIBlacklist)) {
                            return false;
                        }

                        return true;
                    };

                    summon.preSpawnSetupCallback = (master) => {
                        if (!NetworkServer.active) {
                            return;
                        }

                        master.inventory.GiveItem(Survivor.SimulMarker);
                        master.inventory.GiveItem(RoR2Content.Items.BoostDamage, 10);
                        master.inventory.GiveItem(RoR2Content.Items.BoostHp, 25);
                        master.inventory.GiveItem(RoR2Content.Items.MinionLeash);
                        var driver = master.AddComponent<AISkillDriver>();
                        driver.minDistance = 35f;
                        driver.maxDistance = float.PositiveInfinity;
                        driver.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
                        driver.shouldSprint = true;
                        driver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                        driver.aimType = AISkillDriver.AimType.AtCurrentLeader;
                        driver.skillSlot = SkillSlot.None;

                        List<AISkillDriver> drivers = master.GetComponent<BaseAI>().skillDrivers.ToList();
                        foreach (AISkillDriver driver2 in drivers) {
                            if (driver2.maxDistance > 20f) {
                                driver2.maxDistance *= 3f;
                            }
                            driver2.shouldSprint = true;
                        }
                        drivers.Insert(0, driver);
                        master.GetComponent<BaseAI>().skillDrivers = drivers.ToArray();
                        master.AddComponent<SimulatedAIModifier>();
                        master.AddComponent<CopyOwnerInventory>();
                        var ai = master.GetComponent<BaseAI>();
                        ai.aimVectorDampTime = 0;
                        ai.aimVectorMaxSpeed = float.PositiveInfinity;
                        ai.fullVision = true;
                        ai.neverRetaliateFriendlies = true;
                    };

                    EffectManager.SpawnEffect(Survivor.SummonHoloEffect, new EffectData {
                        origin = summon.position,
                        scale = 1f
                    }, false);

                    AkSoundEngine.PostEvent(Events.Play_voidDevastator_m2_secondary_explo, base.gameObject);

                    if (NetworkServer.active) {
                        summon.Perform();
                    }

                    base.skillLocator.special.DeductStock(1);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= 0.7f) {
                outer.SetNextStateToMain();
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(data);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            GetComponent<DataMeter>().Data = reader.ReadSingle();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }

        public void KillAllBosses() {
            foreach (CharacterMaster master in CharacterMaster.instancesList) {
                if (master && master.minionOwnership && master.minionOwnership.ownerMaster == base.characterBody.master && master.GetBody()) {
                    if (master.GetBody().isChampion) {
                        master.TrueKill();
                    }
                }
            }
        }

        public int GetSameIndexSummons(MasterIndex index) {
            int count = 0;

            foreach (CharacterMaster master in CharacterMaster.instancesList) {
                if (master && master.minionOwnership && master.minionOwnership.ownerMaster == base.characterBody.master && master.masterIndex == index) {
                    count++;
                }
            }

            return count;
        }

        public int GetCap(CharacterBody body) {
            if (body.isBoss) {
                return 1;
            }

            return 3;
        }
    }
}