using System;

namespace Vrab.Skills {
    public class Empty : SkillBase<Empty>
    {
        public override string Name => "placeholder";

        public override string Description => "oh my gyatt".AutoFormat();

        public override Type ActivationStateType => typeof(EntityStates.Idle);

        public override string ActivationMachineName => "Weapon";

        public override float Cooldown => 10f;

        public override Sprite Icon => null;
        public override int StockToConsume => 1;
        public override int MaxStock => 1;
        public override bool FullRestockOnAssign => false;
        public override InterruptPriority InterruptPriority => InterruptPriority.PrioritySkill;

        public override string[] Keywords => new string[] { };
    }
}