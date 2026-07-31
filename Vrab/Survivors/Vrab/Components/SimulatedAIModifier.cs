using System;
using System.Linq;
using RoR2.CharacterAI;

namespace Vrab {
    public class SimulatedAIModifier : MonoBehaviour
    {
        public BaseAI ai;
        public CharacterMaster master;
        public AISkillDriver recallWhenOverloaded;
        public CharacterBody body;
        public AISkillDriver[] driversWithout;
        public AISkillDriver[] driversWith;
        public bool currentlyUsingRecall = false;
        public void Start() {
            ai = GetComponent<BaseAI>();
            master = GetComponent<CharacterMaster>();

            var driversWithout = GetComponents<AISkillDriver>().ToList();
            var driversWith = GetComponents<AISkillDriver>().ToList();

            recallWhenOverloaded = base.gameObject.AddComponent<AISkillDriver>();
            recallWhenOverloaded.maxDistance = 120;
            recallWhenOverloaded.minDistance = 25;
            recallWhenOverloaded.skillSlot = SkillSlot.None;
            recallWhenOverloaded.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
            recallWhenOverloaded.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            recallWhenOverloaded.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            recallWhenOverloaded.requireSkillReady = false;
            
            driversWith.Insert(0, recallWhenOverloaded);

            this.driversWith = driversWith.ToArray();
            this.driversWithout = driversWithout.ToArray();
        }

        public void FixedUpdate() {
            if (ai && ai.currentEnemy.gameObject != null && (!ai.currentEnemy.bestHurtBox || !ai.currentEnemy.bestHurtBox.enabled) || (!ai.currentEnemy.healthComponent || !ai.currentEnemy.healthComponent.alive)) {
                ai.currentEnemy.gameObject = null;
                ai.ForceAcquireNearestEnemyIfNoCurrentEnemy();

                if (!body) {
                    body = master.GetBody();
                }
                else {
                    if (body.HasBuff(Survivor.bdOverload)) {
                        if (!currentlyUsingRecall) {
                            ai.skillDrivers = driversWith;
                            currentlyUsingRecall = true;
                        }
                    }
                    else {
                        if (currentlyUsingRecall) {
                            ai.skillDrivers = driversWithout;
                            currentlyUsingRecall = false;
                        }
                    }
                }
            }
        }
    }
}