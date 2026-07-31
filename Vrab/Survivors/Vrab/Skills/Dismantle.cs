using System;

namespace Vrab.Skills {
    public class Dismantle : SkillBase<Dismantle>
    {
        public override string Name => "Dismantle";

        public override string Description => "<style=cDeath>Slowing.</style> Charge up and strike an area for <style=cIsDamage>560% damage</style>. Landing the shot gathers <style=cIsUtility>data</style>.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Dismantle);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 0f;

        public override Sprite Icon => Load<Sprite>("Deconstruct.png");
        public override int StockToConsume => 0;
        public override int MaxStock => 1;
        public override bool FullRestockOnAssign => false;
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        public override string[] Keywords => new string[] { "KEYWORD_DATA" };
    }
}