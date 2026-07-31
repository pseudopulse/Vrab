using System;

namespace Vrab.States {
    public class Dismantle : BaseSkillState {
        public GameObject indicator;
        public float duration = 1.4f;
        public Vector3 target;
        public float damageCoeff = 5.6f;
        public float radius = 6.5f;
        public GameObject tracerEffect => Paths.GameObject.VoidSurvivorBeamTracer;
        public GameObject blastEffect => Paths.GameObject.VoidRaidCrabTripleBeamExplosion;
        public Vector3 direction;
        public Vector3 origin;
        public float speed = 35f;
        public bool collided = false;

        public override void OnEnter()
        {
            base.OnEnter();

            duration /= base.attackSpeedStat;

            if (base.isAuthority) {
                target = base.inputBank.aimOrigin;
                origin = target;
                direction = base.inputBank.aimDirection;

                indicator = GameObject.Instantiate(Survivor.DismantleIndicator, target, Quaternion.identity);
                indicator.transform.localScale = Vector3.one;
                indicator.transform.Find("Sphere").localScale = new Vector3(radius * 2, radius * 2, radius * 2);
                indicator.transform.Find("Sphere").GetComponent<ObjectScaleCurve>().baseScale = new Vector3(radius * 2, radius * 2, radius * 2);
                indicator.transform.Find("Sphere").GetComponent<ObjectScaleCurve>().timeMax = duration * 0.5f;
                indicator.transform.Find("Sphere").GetChild(0).GetComponent<ObjectScaleCurve>().timeMax = duration * 1.45f;
                indicator.transform.Find("Sphere").GetChild(0).GetComponent<ObjectScaleCurve>().baseScale = Vector3.one;
            }

            AkSoundEngine.PostEvent(Events.Play_nullifier_attack1_summon, indicator);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority) {
                if (base.inputBank.skill1.down && !collided) {
                    Vector3 lastTarget = target;
                    target += direction * speed * Time.fixedDeltaTime;

                    if (Physics.Raycast(origin, direction, Vector3.Distance(origin, target), LayerIndex.world.mask, QueryTriggerInteraction.Ignore)) {
                        collided = true;
                        target = lastTarget;
                    }
                }

                if (base.fixedAge >= duration) {
                    outer.SetNextState(new DismantleEndlag());
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (indicator) {
                indicator.transform.position = Vector3.MoveTowards(indicator.transform.position, target, 150f * Time.deltaTime);
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (base.isAuthority) {
                GameObject.Destroy(indicator);

                EffectManager.SpawnEffect(tracerEffect, new() {
                    origin = FindModelChild("MuzzleDeconstruct").position,
                    start = FindModelChild("MuzzleDeconstruct").position + new Vector3(0, 500f, 0f),
                    scale = 12f,
                }, true);

                EffectManager.SpawnEffect(tracerEffect, new() {
                    origin = target,
                    start = target + new Vector3(0, 500f, 0f),
                    scale = 12f,
                }, true);

                EffectManager.SpawnEffect(blastEffect, new() {
                    origin = target,
                    scale = radius * 2 * 0.7f
                }, true);

                BlastAttack attack = new();
                attack.radius = radius;
                attack.attacker = base.gameObject;
                attack.damageType = DamageTypeCombo.GenericPrimary | DamageType.SlowOnHit;
                attack.baseDamage = base.damageStat * damageCoeff;
                attack.falloffModel = BlastAttack.FalloffModel.None;
                attack.crit = base.RollCrit();
                attack.position = target;
                attack.teamIndex = base.GetTeam();
                attack.procCoefficient = 1;
                var res = attack.Fire();

                if (res.hitCount > 0 && !base.characterBody.HasBuff(Survivor.bdOverload)) {
                    GetComponent<DataMeter>().AddData(18f);

                    if (res.hitCount > 1) {
                        GetComponent<DataMeter>().AddData(Math.Min(5f * (res.hitCount - 1), 36f));
                    }
                }
            }

            AkSoundEngine.PostEvent(Events.Play_voidman_m1_shoot, base.gameObject);
            AkSoundEngine.PostEvent(Events.Play_nullifier_attack1_summon, base.gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

    public class DismantleEndlag : BaseSkillState {
        public float duration = 0.5f;
        public override void OnEnter()
        {
            base.OnEnter();

            duration /= base.attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}