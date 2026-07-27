using System;
using JetBrains.Annotations;

namespace Vrab.Skills {
    public class TargetLockSkillDef : SkillDef {
        public class TargetLockInstanceData : SkillDef.BaseSkillInstanceData {
            public TargetTracker tracker;
            public CharacterBody body;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new TargetLockInstanceData() {
                tracker = skillSlot.GetComponent<TargetTracker>(),
                body = skillSlot.GetComponent<CharacterBody>(),
            };
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && skillSlot.skillInstanceData != null && (skillSlot.skillInstanceData as TargetLockInstanceData).tracker.target && !skillSlot.characterBody.HasBuff(Survivor.bdOverload);
        }
    }

    public class SkillDefNoAirborne : SkillDef {
        public class NoAirborneInstanceData : SkillDef.BaseSkillInstanceData {
            public CharacterMotor motor;
            public DataMeter meter;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new NoAirborneInstanceData() {
                motor = skillSlot.GetComponent<CharacterMotor>(),
                meter = skillSlot.GetComponent<DataMeter>(),
            };
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && skillSlot.skillInstanceData != null && (skillSlot.skillInstanceData as NoAirborneInstanceData).motor.isGrounded && (skillSlot.skillInstanceData as NoAirborneInstanceData).meter.Data > 20;
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
            return base.IsReady(skillSlot) && skillSlot.skillInstanceData != null && (skillSlot.skillInstanceData as TargetLockInstanceData).tracker.target && !(skillSlot.skillInstanceData as TargetLockInstanceData).tracker.meter.drifting;
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