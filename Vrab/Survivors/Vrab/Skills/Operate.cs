using System;

namespace Vrab.Skills {
    public class Operate : SkillBase<Operate>
    {
        public override string Name => "Operate";

        public override string Description => "Leap onto an ally, <style=cDeath>draining data</style> to become <style=cIsUtility>untargetable</style> and <style=cIsUtility>overload</style> them. Take control of simulated allies you attach to.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Operate);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 6f;

        public override Sprite Icon => Load<Sprite>("Operate.png");
        public override int StockToConsume => 0;
        public override int MaxStock => 1;
        public override bool FullRestockOnAssign => true;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;

        public override string[] Keywords => new string[] { "KEYWORD_DATA", "KEYWORD_OVERLOAD" };

        public override void CreateSkillDef()
        {
            base.skillDef = ScriptableObject.CreateInstance<TargetLockSkillDef>();
        }
    }
}