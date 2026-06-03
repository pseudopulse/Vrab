using System;
using RoR2.CharacterAI;

namespace Vrab {
    public class SimulatedAIModifier : MonoBehaviour
    {
        public BaseAI ai;

        public void Start() {
            ai = GetComponent<BaseAI>();
        }

        public void FixedUpdate() {
            if (ai && ai.currentEnemy.gameObject != null && (!ai.currentEnemy.bestHurtBox || !ai.currentEnemy.bestHurtBox.enabled) || (!ai.currentEnemy.healthComponent || !ai.currentEnemy.healthComponent.alive)) {
                ai.currentEnemy.gameObject = null;
                ai.ForceAcquireNearestEnemyIfNoCurrentEnemy();
            }
        }
    }
}