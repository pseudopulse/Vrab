using System;

namespace Vrab.Skills {
    public class Iterate : SkillBase<Iterate>
    {
        public override string Name => "Iterate";

        public override string Description => "Plant into the ground, becoming <style=cIsUtility>untargetable</style> and projecting a field that <style=cIsUtility>overclocks</style> all allies within. All simulated allies are rebuilt at your location. <style=cDeath>The field drains data to maintain, and prevents data generation.</style>".AutoFormat();

        public override Type ActivationStateType => typeof(States.Iterate);

        public override string ActivationMachineName => "Body";
        public override float Cooldown => 12f;
        public override Sprite Icon => Load<Sprite>("Refresh.png");
        public override int StockToConsume => 1;
        public override int MaxStock => 1;
        public override bool FullRestockOnAssign => true;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;
        public override bool BeginCooldownOnSkillEnd => true;

        public override string[] Keywords => new string[] { "KEYWORD_DATA", "KEYWORD_OVERLOAD" };

        public override void CreateSkillDef()
        {
            base.skillDef = ScriptableObject.CreateInstance<SkillDefNoAirborne>();
        }
    }
}