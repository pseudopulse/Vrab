using System;

namespace Vrab.Skills {
    public class Simulate : SkillBase<Simulate>
    {
        public override string Name => "Simulate";

        public override string Description => "<style=cDeath>Spend data</style> to <style=cIsUtility>simulate</style> a <style=cIsDamage>holographic clone</style> of the target. <style=cDeath>Data cost</style> scales with enemy strength.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Simulate);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 8f;

        public override Sprite Icon => Load<Sprite>("Simulate.png");
        public override int StockToConsume => 1;
        public override int MaxStock => 1;
        public override bool FullRestockOnAssign => true;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;

        public override string[] Keywords => new string[] { "KEYWORD_DATA", "KEYWORD_SIMULATION" };

        public override void CreateSkillDef()
        {
            base.skillDef = ScriptableObject.CreateInstance<TargetLockSkillDef>();
        }
    }
}