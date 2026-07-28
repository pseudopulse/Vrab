using System;
using KinematicCharacterController;
using Vrab.Utils.Components;
using ThreeEyedGames;
using Vrab.States;
using UnityEngine.Rendering.PostProcessing;

namespace Vrab {
    public class Survivor : SurvivorBase<Survivor>
    {
        public override string Name => "Vrab";

        public override string Description => 
        """
        thats so awesome we call that a vrab void crab what the fuck is that

        < ! > Deconstruct cannot miss, but may only be used on the ground.

        < ! > Analyze can collect large amounts of data while clearing out dangerous projectiles.

        < ! > Refresh can boost your holograms at the expense of boosting enemies caught in the blast.
        
        < ! > To offset Deconstruct's middling damage, utilize your data to construct holograms of powerful enemies with Simulate.
        
        """;

        public override string Subtitle => "Luminescent Carrier";

        public override string Outro => "...and so it left, its curiosity satiated.";

        public override string Failure => "...and so it vanished, its data forever lost.";
        public static LazyIndex VrabIndex = new("VrabBody");
        public static GameObject TargetPainter;
        public static GameObject Display;
        public static GameObject DeconstructBeam;
        public static GameObject OverlayMeter;
        public static ItemDef SimulMarker;
        public static Material matVrabHologram;
        public static GameObject SummonHoloEffect;
        public static GameObject ScanEffect;
        public static GameObject AnalysisBoltProjectile;
        public static BuffDef bdOverload;
        public static GameObject RefreshEffect;
        public static GameObject IterateWard;
        public static GameObject IterateEffect;
        public static GameObject DismantleIndicator;
        public static List<FogDamageController> fogDamageControllers = new();
        public static Material[] VrabHologramMat;
        
        public override void LoadAssets()
        {
            // SETUP AND LOADING
            Body = Load<GameObject>("VrabBody.prefab");
            Display = Load<GameObject>("VrabDisplay.prefab");
            Master = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.TreebotMonsterMaster, "VrabMaster");

            CharacterMaster master = Master.GetComponent<CharacterMaster>();
            master.bodyPrefab = Body;

            SurvivorDef = Load<SurvivorDef>("sdVrab.asset");

            // BODY CONFIG

            Body.GetComponent<KinematicCharacterMotor>().playerCharacter = true;

            ContentAddition.AddNetworkedObject(Body);
            PrefabAPI.RegisterNetworkPrefab(Body);

            Body.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpTreebot;

            Body.AddComponent<TargetTracker>();

            Body.GetComponent<CharacterDeathBehavior>().deathState = new(typeof(EntityStates.NullifierMonster.DeathState));

            EntityStateMachine.FindByCustomName(Body, "Body").mainStateType = new(typeof(PelagicDrift));

            // MAT SWAP

            SwapMaterial("mdlVoidWardCrab", 0, Paths.Material.matVoidwardCrabPurple);
            SwapMaterial("mdlVoidWardCrabArmRings", 1, Paths.Material.matNullifierArmor);
            SwapMaterial("mdlVoidWardCrabEyes", 2, Paths.Material.matVoidwardCrabEyes);
            SwapMaterial("mdlVoidWardCrabMouth", 3, Paths.Material.matVoidwardCrabMouth);
            SwapMaterial("mdlVoidWardCrabWardMetal", 4, Paths.Material.matArenaTrim);
            SwapMaterial("mdlVoidWardCrabWardSphere", 5, Paths.Material.matArenaTerrainGem);
            Paths.Material.matArenaTerrainGem.SetFloat("_RampInfo", 0);

            // SKILLS

            SkillLocator locator = Body.GetComponent<SkillLocator>();

            locator.passiveSkill.skillNameToken = "VRAB_PASSIVE_NAME";
            locator.passiveSkill.skillDescriptionToken = "VRAB_PASSIVE_DESC";
            locator.passiveSkill.keywordToken = "KEYWORD_DATA";
            
            "VRAB_PASSIVE_NAME".Add("Pelagic Drift");
            "VRAB_PASSIVE_DESC".Add("The Vrab has a <style=cIsUtility>slower falling speed</style> and can <style=cIsUtility>ascend</style> by <style=cIsDamage>holding jump</style>, at the cost of <style=cDeath>data</style>. You may not <style=cDeath>Deconstruct</style> while drifting.");

            ReplaceSkills(locator.primary, Skills.Deconstruct.instance.skillDef, Skills.Dismantle.instance.skillDef);
            ReplaceSkills(locator.secondary, Skills.Analyze.instance.skillDef);
            if (Main.config.Bind<bool>("Configuration", "Enable Old Utility", false, "Enables the old Refresh utility from before 1.2.0").Value) {
                ReplaceSkills(locator.utility, Skills.Iterate.instance.skillDef, Skills.RefreshOld.instance.skillDef);
            }
            else {
                ReplaceSkills(locator.utility, Skills.Iterate.instance.skillDef);
            }
            ReplaceSkills(locator.special, Skills.Simulate.instance.skillDef);

            "KEYWORD_DATA".Add("""
            <style=cKeywordName>Data</style>Data is a resource used by skills.
            """);

            "KEYWORD_SIMULATION".Add("""
            <style=cKeywordName>Simulation</style>Simulated enemies <style=cIsUtility>inherit your items</style> and <style=cDeath>lose health over 60s</style>.
            """);

            "KEYWORD_OVERLOAD".Add("""
            <style=cKeywordName>Overload</style>Overloaded characters attack <style=cIsDamage>35%</style> faster while gaining <style=cIsDamage>35%</style> increased damage, simulations receive double this effect. Overloaded characters regenerate <style=cIsHealing>5% max health</style> each second and simulations do not decay.
            """);

            // TARGET VFX

            TargetPainter = PrefabAPI.InstantiateClone(Paths.GameObject.EngiMissileTrackingIndicator, "VrabTargetPainter");
            var spr = TargetPainter.FindComponent<SpriteRenderer>("Base Core");
            spr.color = Color.magenta;
            spr.sprite = Paths.Texture2D.texCaptainCrosshairInner.MakeSprite();
            spr.transform.localScale = new(0.015f, 0.015f, 0.015f);

            // LOAD VFX

            DeconstructBeam = Load<GameObject>("DeconstructBeam.prefab");
            var fade = DeconstructBeam.AddComponent<DetachLineRendererAndFade>();
            fade.line = DeconstructBeam.GetComponentInChildren<LineRenderer>();
            fade.decayTime = 0.4f;

            ScanEffect = Load<GameObject>("ScanEffect.prefab");
            ScanEffect.AddComponent<EffectComponent>().applyScale = true;
            ContentAddition.AddEffect(ScanEffect);

            RefreshEffect = Load<GameObject>("RefreshEffect.prefab");
            RefreshEffect.AddComponent<EffectComponent>().applyScale = true;
            ContentAddition.AddEffect(RefreshEffect);

            // METER
            OverlayMeter = Load<GameObject>("VrabMeter.prefab");
            OverlayMeter.AddComponent<CrosshairDataMeterSync>();
            Body.AddComponent<DataMeter>();

            // SIMULATION ITEM
            SimulMarker = Load<ItemDef>("idSimulMarker.asset");
            ContentAddition.AddItemDef(SimulMarker);

            // SIMULATION VFX
            // matVrabHologram = Load<Material>("matVrabHologram.mat");
            VrabHologramMat = new Material[] {
                //Paths.Material.matVoidBarnacleExplosion,
                Paths.Material.matNullifierSphereFresnelStars,
                Paths.Material.matNullifierExplosionAreaIndicatorSoft,
                Paths.Material.matNullifierDistortionLight,
            };
            On.RoR2.CharacterBody.RecalculateStats += HologramDegen;
            On.RoR2.CharacterModel.UpdateRendererMaterials += HologramEffect;
            On.RoR2.CharacterBody.GetDisplayName += HologramName;

            SummonHoloEffect = PrefabAPI.InstantiateClone(Paths.GameObject.InfiniteTowerSafeWardAwaitingInteraction.transform.Find("ModelBase/mdlVoidWardCrab/BuiltInEffects/Active").gameObject, "SummonHologramPulse");
            SummonHoloEffect.SetActive(true);
            SummonHoloEffect.AddComponent<EffectComponent>().applyScale = true;
            SummonHoloEffect.AddComponent<DestroyOnTimer>().duration = 5f;
            
            ContentAddition.AddEffect(SummonHoloEffect);

            // OVERLOAD
            bdOverload = Load<BuffDef>("bdOverload.asset");
            ContentAddition.AddBuffDef(bdOverload);
        
            // ANALYSIS BOLT
            AnalysisBoltProjectile = PrefabAPI.InstantiateClone(Paths.GameObject.VoidBarnacleBullet, "AnalysisBoltProjectile");
            var AnalysisBoltGhost = PrefabAPI.InstantiateClone(Paths.GameObject.VoidBarnacleBulletGhost, "AnalysisBoltGhost");
            AnalysisBoltGhost.transform.localScale *= 3f;
            AnalysisBoltProjectile.GetComponent<ProjectileController>().ghostPrefab = AnalysisBoltGhost;
            AnalysisBoltProjectile.GetComponent<ProjectileImpactExplosion>().explosionEffect = Paths.GameObject.VoidMegaCrabDeathBombletsExplosion;
            ContentAddition.AddProjectile(AnalysisBoltProjectile);
            PrefabAPI.RegisterNetworkPrefab(AnalysisBoltProjectile);

            // ITERATE
            IterateWard = PrefabAPI.InstantiateClone(Paths.GameObject.WarbannerWard, "IterateBuffWard");
            IterateWard.GetComponent<BuffWard>().buffDef = bdOverload;
            IterateWard.GetComponent<BuffWard>().animateRadius = true;
            IterateWard.GetComponent<BuffWard>().radius = 30f;
            IterateWard.GetComponent<BuffWard>().radiusCoefficientCurve = new(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
            IterateWard.GetComponent<BuffWard>().expireDuration = 0.5f;
            IterateWard.transform.localPosition = Vector3.zero;
            IterateWard.transform.Find("mdlWarbanner").gameObject.SetActive(false);
            IterateWard.GetComponentInChildren<MeshRenderer>().sharedMaterials = new Material[] {
                Paths.Material.matITSafeWardAreaIndicator1, Paths.Material.matNullifierExplosionAreaIndicatorSoft
            };
            IterateWard.AddComponent<IterateSafeZone>();

            IterateEffect = PrefabAPI.InstantiateClone(Paths.GameObject.VoidFogMildEffect, "IterateFog");
            IterateEffect.RemoveComponent<DestroyOnTimer>();
            IterateEffect.RemoveComponent<TemporaryVisualEffect>();
            IterateEffect.RemoveComponent<LocalCameraEffect>();
            IterateEffect.transform.Find("VisualEffect").transform.localScale = new Vector3(3f, 3f, 3f);
            IterateEffect.transform.Find("VisualEffect").Find("Point Light").GetComponent<Light>().range = 15;
            IterateEffect.transform.Find("VisualEffect").Find("Point Light").RemoveComponent<FlickerLight>();
            IterateEffect.AddComponent<PostProcessFade>().volume = IterateEffect.GetComponent<PostProcessDuration>().ppVolume;
            ContentAddition.AddNetworkedObject(IterateWard);

            On.RoR2.FogDamageController.Start += (orig, self) => {
                orig(self);
                fogDamageControllers.Add(self);
                fogDamageControllers.RemoveAll(x => x == null);
            };

            // DISMANTLE
            DismantleIndicator = PrefabAPI.InstantiateClone(Paths.GameObject.NullifierPreBombGhost, "   DismantleIndicator");
            DismantleIndicator.RemoveComponent<ProjectileGhostController>();
            DismantleIndicator.RemoveComponent<VFXAttributes>();
            DismantleIndicator.RemoveComponent<Rigidbody>();
            DismantleIndicator.transform.Find("Sphere").GetComponent<ObjectScaleCurve>().useOverallCurveOnly = true;
            DismantleIndicator.transform.Find("Sphere").GetComponent<ObjectScaleCurve>().overallCurve = new(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
            DismantleIndicator.transform.Find("Sphere").GetComponent<MeshRenderer>().sharedMaterials = new Material[] { Paths.Material.matNullifierBlackholeZoneAreaIndicator };
            DismantleIndicator.transform.Find("Sphere").AddComponent<SphereCollider>().isTrigger = true;
            DismantleIndicator.transform.Find("Sphere").AddComponent<DismantleIndicatorBehavior>();
            var indicator = GameObject.Instantiate(DismantleIndicator.transform.Find("Sphere").gameObject, DismantleIndicator.transform.Find("Sphere"));
            indicator.GetComponent<MeshRenderer>().sharedMaterial = Paths.Material.matNullifierExplosionAreaIndicatorHard;
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = Vector3.one;
            var curve = indicator.GetComponent<ObjectScaleCurve>();
            curve.overallCurve = new(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
        }

        public class DismantleIndicatorBehavior : MonoBehaviour {
            private Dictionary<Collider, List<Renderer>> pairs = new Dictionary<Collider, List<Renderer>>();

            private void OnTriggerEnter(Collider other)
            {
                AddIndictator(other);
            }
            private void OnTriggerExit(Collider other)
            {
                DelIndictator(other);
            }
            private void AddIndictator(Collider target)
            {
                if (pairs.ContainsKey(target))
                {
                    return;
                }
                CharacterModel characterModel = target.GetComponent<ModelLocator>()?.modelTransform?.GetComponent<CharacterModel>();
                if (characterModel == null || characterModel.body.teamComponent.teamIndex == TeamIndex.Player)
                {
                    return;
                }
                List<Renderer> list = new List<Renderer>();
                CharacterModel.RendererInfo[] baseRendererInfos = characterModel.baseRendererInfos;
                for (int i = 0; i < baseRendererInfos.Length; i++)
                {
                    CharacterModel.RendererInfo rendererInfo = baseRendererInfos[i];
                    if (!rendererInfo.ignoreOverlays)
                    {
                        list.Add(rendererInfo.renderer);
                    }
                }
                if (list.Count > 0)
                {
                    pairs.Add(target, list);
                }
            }
            private void DelIndictator(Collider target)
            {
                if (pairs.ContainsKey(target))
                {
                    pairs.Remove(target);
                }
            }
            private void OnEnable()
            {
                OutlineHighlight.onPreRenderOutlineHighlight = (Action<OutlineHighlight>)Delegate.Combine(OutlineHighlight.onPreRenderOutlineHighlight, new Action<OutlineHighlight>(OnPreRenderOutlineHighlight));
            }
            private void OnDisable()
            {
                OutlineHighlight.onPreRenderOutlineHighlight = (Action<OutlineHighlight>)Delegate.Remove(OutlineHighlight.onPreRenderOutlineHighlight, new Action<OutlineHighlight>(OnPreRenderOutlineHighlight));
            }
            private void OnPreRenderOutlineHighlight(OutlineHighlight outlineHighlight)
            {
                foreach (List<Renderer> value in pairs.Values)
                {
                    foreach (Renderer item in value)
                    {
                        outlineHighlight.AddHighlight(item, Color.magenta);
                    }
                }
            }
        }

        public class IterateSafeZone : MonoBehaviour, IZone
        {
            public BuffWard ward;
            public void Start() {
                ward = GetComponent<BuffWard>();

                foreach (FogDamageController controller in fogDamageControllers) {
                    controller.AddSafeZone(this);
                }
            }
            public void OnDestroy() {
                foreach (FogDamageController controller in fogDamageControllers) {
                    controller.RemoveSafeZone(this);
                }
            }
            public bool IsInBounds(Vector3 position)
            {
                return Vector3.Distance(position, base.transform.position) <= ward.calculatedRadius;
            }
        }

        private string HologramName(On.RoR2.CharacterBody.orig_GetDisplayName orig, CharacterBody self)
        {
            string name = orig(self);
            if (self.inventory && self.inventory.GetItemCount(SimulMarker) > 0) {
                name = "Simulated " + name;
            }

            return name;
        }

        private void HologramEffect(On.RoR2.CharacterModel.orig_UpdateRendererMaterials orig, CharacterModel self, Renderer renderer, Material defaultMaterial, bool ignoreOverlays)
        {
            orig(self, renderer, defaultMaterial, ignoreOverlays);

            if (self.body && self.body.inventory && self.body.inventory.GetItemCount(SimulMarker) > 0) {
                if (renderer is ParticleSystemRenderer) return;

                // renderer.sharedMaterials = matVrabHologram;
                renderer.sharedMaterials = VrabHologramMat;
            }
            
        }

        private void HologramDegen(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self.inventory && self.inventory.GetItemCount(SimulMarker) > 0) {
                if (self.HasBuff(bdOverload)) {
                    self.regen = self.maxHealth * 0.05f;
                    self.attackSpeed *= 1.60f;
                    self.damage *= 1.60f;
                }
                else {
                    float timeToKill = 60f;
                    float degen = self.maxHealth / timeToKill;
                    self.regen = -degen;
                }
            }
            else {
                if (self.HasBuff(bdOverload)) {
                    self.regen += self.maxHealth * 0.05f;
                    self.attackSpeed *= 1.30f;
                    self.damage *= 1.30f;
                }
            }
        }

        private void SwapMaterial(string mesh, int cm, Material mat) {
            SwapMaterialOnObject(mesh, cm, mat, Body);
            SwapMaterialOnObject(mesh, cm, mat, Display);
        }

        private void SwapMaterialOnObject(string mesh, int cm, Material mat, GameObject obj) {
            CharacterModel model = obj.GetComponentInChildren<CharacterModel>();
            Transform root = model.transform;

            root.Find(mesh).GetComponent<Renderer>().sharedMaterial = mat;
            model.baseRendererInfos[cm].defaultMaterial = mat;
        }
    }
}