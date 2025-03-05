using System;

namespace Vrab.States {
    public class Deconstruct : BaseSkillState {
        public HurtBox target;
        public GameObject beam;
        public Transform start;
        public Transform end;
        public Transform muzzle;
        //
        public const float damageCoeffPerSecond = 3f;
        public const float ticksPerSecond = 6f;
        public Timer timer = new(1f / ticksPerSecond, expires: true, resetOnExpire: true);
        public TargetTracker tracker;
        public const float damageCoeff = damageCoeffPerSecond * (1f / ticksPerSecond);
        public Vector3 targetPosition;
        public DataMeter meter;
        public Transform prevTarget;
        public override void OnEnter()
        {
            base.OnEnter();

            beam = GameObject.Instantiate(Survivor.DeconstructBeam);
            start = beam.GetComponent<ChildLocator>().FindChild("Origin");
            end = beam.GetComponent<ChildLocator>().FindChild("Target");

            meter = GetComponent<DataMeter>();

            muzzle = FindModelChild("MuzzleDeconstruct");

            tracker = GetComponent<TargetTracker>();

            timer.duration = 1f / (ticksPerSecond * base.attackSpeedStat);
            
            end.position = tracker.target.position;
        }
        public override void Update()
        {
            base.Update();

            start.position = muzzle.position;
            end.position = Vector3.MoveTowards(end.position, targetPosition, 200 * Time.deltaTime);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.StartAimMode(0.2f);

            if (prevTarget != null && prevTarget != tracker.target && base.isAuthority) {
                outer.SetNextStateToMain();
                return;
            }

            prevTarget = tracker.target;

            if (tracker.target) {
                target = tracker.target.GetComponent<HurtBox>();
            }

            if (!target || !base.characterMotor.isGrounded && base.isAuthority) {
                outer.SetNextStateToMain();
                return;
            }

            if (target) {
                targetPosition = target.transform.position;
                if (target.healthComponent && !target.healthComponent.alive) {
                    outer.SetNextStateToMain();
                    return;
                }
                
                if (timer.Tick() && target.healthComponent) {
                    DamageInfo info = new();
                    info.damage = base.damageStat * damageCoeff;
                    info.attacker = base.gameObject;
                    info.crit = base.RollCrit();
                    info.position = end.position;
                    info.procCoefficient = 1f;
                    info.damageColorIndex = DamageColorIndex.Default;

                    meter.AddData(7f / ticksPerSecond);

                    if (NetworkServer.active) {
                        target.healthComponent.TakeDamage(info);
                        GlobalEventManager.instance.OnHitEnemy(info, target.healthComponent.body.gameObject);
                        GlobalEventManager.instance.OnHitAll(info, target.healthComponent.body.gameObject);
                    }

                    AkSoundEngine.PostEvent(Events.Play_nullifier_attack1_explode, end.gameObject);
                }
            }

            if (base.fixedAge >= 0.3f && !inputBank.skill1.down && base.isAuthority) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnExit()
        {
            base.OnExit();

            if (beam) {
                Destroy(beam);
            }
        }
    }
}