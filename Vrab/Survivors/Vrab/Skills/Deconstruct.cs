using System;

namespace Vrab.Skills {
    public class Deconstruct : SkillBase<Deconstruct>
    {
        public override string Name => "Deconstruct";

        public override string Description => "Liquify a target for <style=cIsDamage>300% damage per second</style>. Damaging targets collects <style=cIsUtility>data</style>.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Deconstruct);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Load<Sprite>("Deconstruct.png");
        public override int StockToConsume => 0;
        public override int MaxStock => 1;
        public override bool FullRestockOnAssign => false;
        public override InterruptPriority InterruptPriority => InterruptPriority.Any;

        public override string[] Keywords => new string[] { "KEYWORD_DATA" };

        public override void CreateSkillDef()
        {
            base.skillDef = ScriptableObject.CreateInstance<TargetLockSkillDefNoAirborne>();
        }
    }
}