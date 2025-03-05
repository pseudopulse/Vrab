using System;
using static RoR2.MasterCatalog;

namespace Vrab.States {
    public class Analyze : BaseSkillState {
        public float radius = 70f;
        public float dataPerProjectile = 7.5f;
        public DataMeter meter;
        public static LazyAddressable<GameObject> Explosion = new(() => Paths.GameObject.VoidMegaCrabDeathBombExplosion);

        public override void OnEnter()
        {
            base.OnEnter();

            meter = GetComponent<DataMeter>();

            EffectManager.SpawnEffect(Survivor.ScanEffect, new EffectData {
                origin = base.transform.position,
                scale = radius
            }, false);

            AkSoundEngine.PostEvent(Events.Play_voidDevastator_m2_primary_explo, base.gameObject);

            if (NetworkServer.active) {
                SphereSearch search = new();
                search.mask = LayerIndex.projectile.mask | LayerIndex.debris.mask | LayerIndex.entityPrecise.mask;
                search.origin = base.transform.position;
                search.radius = radius;
                search.queryTriggerInteraction = QueryTriggerInteraction.Collide;
                search.RefreshCandidates();
                search.FilterCandidatesByProjectileControllers();
                List<ProjectileController> projectiles = new();
                search.GetProjectileControllers(projectiles);

                for (int i = 0; i < projectiles.Count; i++) {
                    AkSoundEngine.PostEvent(Events.Play_voidDevastator_m2_secondary_explo, projectiles[i].gameObject);

                    FireProjectileInfo info = new();
                    info.damage = base.damageStat * 8f;
                    info.crit = base.RollCrit();
                    info.owner = base.gameObject;
                    info.position = projectiles[i].transform.position;
                    info.rotation = Util.QuaternionSafeLookRotation(projectiles[i].transform.forward * -1f);
                    info.projectilePrefab = Survivor.AnalysisBoltProjectile;

                    EffectManager.SpawnEffect(Explosion, new EffectData {
                        origin = info.position,
                        scale = 1.5f
                    }, true);

                    ProjectileManager.instance.FireProjectile(info);
                    meter.AddData(dataPerProjectile);

                    GameObject.Destroy(projectiles[i].gameObject);
                }
            }

            outer.SetNextStateToMain();
        }
    }
}