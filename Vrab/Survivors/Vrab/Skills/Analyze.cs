using System;

namespace Vrab.Skills {
    public class Analyze : SkillBase<Analyze>
    {
        public override string Name => "Analyze";

        public override string Description => "<style=cIsDamage>Convert</style> nearby projectiles into <style=cIsUtility>seeking bolts</style> for <style=cIsDamage>800% damage</style> at each location. <style=cIsUtility>Gain 7% data per analyzed projectile.</style>".AutoFormat();

        public override Type ActivationStateType => typeof(States.Analyze);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 5.5f;

        public override Sprite Icon => Load<Sprite>("Analyze.png");
        public override int StockToConsume => 1;
        public override int MaxStock => 1;
        public override bool FullRestockOnAssign => true;
        public override InterruptPriority InterruptPriority => InterruptPriority.Skill;

        public override string[] Keywords => new string[] { "KEYWORD_DATA" };
    }
}