using System;
using static RoR2.MasterCatalog;

namespace Vrab.States
{
    public class Analyze : BaseSkillState
    {
        public float radius = 70f;
        public float dataPerProjectile = 7.5f;
        public DataMeter meter;
        public static LazyAddressable<GameObject> Explosion = new(() => Paths.GameObject.VoidMegaCrabDeathBombExplosion);
        public Timer timer = new Timer(0.06f, false, true, false, true);

        public override void OnEnter()
        {
            base.OnEnter();

            meter = GetComponent<DataMeter>();

            EffectManager.SpawnEffect(Survivor.ScanEffect, new EffectData
            {
                origin = base.transform.position,
                scale = radius
            }, false);

            AkSoundEngine.PostEvent(Events.Play_voidDevastator_m2_primary_explo, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (timer.Tick())
            {
                SphereSearch search = new();
                search.mask = LayerIndex.projectile.mask | LayerIndex.debris.mask;
                search.origin = base.transform.position;
                search.radius = radius;
                search.queryTriggerInteraction = QueryTriggerInteraction.Collide;
                search.RefreshCandidates();
                search.FilterCandidatesByProjectileControllers();
                List<ProjectileController> projectiles = new();
                search.GetProjectileControllers(projectiles);

                for (int i = 0; i < projectiles.Count; i++)
                {
                    if (projectiles[i].gameObject.name.Contains("AnalysisBoltProjectile"))
                    {
                        continue;
                    }

                    AkSoundEngine.PostEvent(Events.Play_voidDevastator_m2_secondary_explo, projectiles[i].gameObject);

                    FireProjectileInfo info = new();
                    info.damage = base.damageStat * 6f;
                    info.crit = base.RollCrit();
                    info.owner = base.gameObject;
                    info.position = projectiles[i].transform.position;
                    info.rotation = Util.QuaternionSafeLookRotation(projectiles[i].transform.forward * -1f);
                    info.projectilePrefab = Survivor.AnalysisBoltProjectile;

                    EffectManager.SpawnEffect(Explosion, new EffectData
                    {
                        origin = info.position,
                        scale = 1.5f
                    }, false);

                    if (NetworkServer.active) {
                        GameObject.Destroy(projectiles[i].gameObject);
                        ProjectileManager.instance.FireProjectile(info);
                    }

                    if (base.isAuthority) {
                        meter.AddData(dataPerProjectile);
                    }
                }
            }

            if (base.fixedAge >= 0.3f)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }
    }
}