using System;

namespace Vrab.Skills {
    public class Refresh : SkillBase<Refresh>
    {
        public override string Name => "Refresh";

        public override string Description => "Release a pulse that <style=cIsVoid>overloads</style> ALL characters, <style=cIsUtility>increasing offensive stats by 25%</style> and <style=cIsDamage>resetting status duration</style>.".AutoFormat();

        public override Type ActivationStateType => typeof(States.Refresh);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 7f;

        public override Sprite Icon => Load<Sprite>("Refresh.png");
        public override int StockToConsume => 1;
        public override int MaxStock => 1;
        public override bool FullRestockOnAssign => true;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;

        public override string[] Keywords => new string[] { "KEYWORD_DATA", "KEYWORD_OVERLOAD" };
    }
}