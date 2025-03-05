using System;
using JetBrains.Annotations;

namespace Vrab.Skills {
    public class TargetLockSkillDef : SkillDef {
        public class TargetLockInstanceData : SkillDef.BaseSkillInstanceData {
            public TargetTracker tracker;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new TargetLockInstanceData() {
                tracker = skillSlot.GetComponent<TargetTracker>()
            };
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && skillSlot.skillInstanceData != null && (skillSlot.skillInstanceData as TargetLockInstanceData).tracker.target;
        }
    }

    public class TargetLockSkillDefNoAirborne : SkillDef {
        public class TargetLockInstanceData : SkillDef.BaseSkillInstanceData {
            public TargetTracker tracker;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new TargetLockInstanceData() {
                tracker = skillSlot.GetComponent<TargetTracker>()
            };
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && skillSlot.skillInstanceData != null && (skillSlot.skillInstanceData as TargetLockInstanceData).tracker.target && skillSlot.characterBody.characterMotor.isGrounded;
        }
    }

    public class RequireDataSkillDef : SkillDef {
        public class RequireDataInstanceData : SkillDef.BaseSkillInstanceData {
            public DataMeter meter;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new RequireDataInstanceData() {
                meter = skillSlot.GetComponent<DataMeter>()
            };
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && skillSlot.skillInstanceData != null && (skillSlot.skillInstanceData as RequireDataInstanceData).meter.Data >= 40f;
        }
    }
}