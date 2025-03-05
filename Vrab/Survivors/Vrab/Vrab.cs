using System;
using KinematicCharacterController;
using Vrab.Utils.Components;
using ThreeEyedGames;
using Vrab.States;

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

            // SKILLS

            SkillLocator locator = Body.GetComponent<SkillLocator>();

            locator.passiveSkill.skillNameToken = "VRAB_PASSIVE_NAME";
            locator.passiveSkill.skillDescriptionToken = "VRAB_PASSIVE_DESC";
            locator.passiveSkill.keywordToken = "KEYWORD_DATA";
            
            "VRAB_PASSIVE_NAME".Add("Pelagic Drift");
            "VRAB_PASSIVE_DESC".Add("The Vrab has a <style=cIsUtility>slower falling speed</style> and can <style=cIsUtility>ascend</style> by <style=cIsDamage>holding jump</style>, at the cost of <style=cDeath>data</style>. You may not <style=cDeath>Deconstruct</style> while drifting.");

            ReplaceSkills(locator.primary, Skills.Deconstruct.instance.skillDef);
            ReplaceSkills(locator.secondary, Skills.Analyze.instance.skillDef);
            ReplaceSkills(locator.utility, Skills.Refresh.instance.skillDef);
            ReplaceSkills(locator.special, Skills.Simulate.instance.skillDef);

            "KEYWORD_DATA".Add("""
            <style=cKeywordName>Data</style>Data is a resource used by skills.
            """);

            "KEYWORD_SIMULATION".Add("""
            <style=cKeywordName>Simulation</style>Simulated enemies <style=cIsUtility>inherit your items</style> and <style=cDeath>lose health over 60s</style>.
            """);

            "KEYWORD_OVERLOAD".Add("""
            <style=cKeywordName>Overload</style>Overloaded characters attack <style=cIsDamage>50%</style> faster and move <style=cIsDamage>100%</style> faster.
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
            matVrabHologram = Load<Material>("matVrabHologram.mat");
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

                renderer.sharedMaterial = matVrabHologram;
            }
            
        }

        private void HologramDegen(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self.inventory && self.inventory.GetItemCount(SimulMarker) > 0) {
                float timeToKill = 60f;
                float degen = self.maxHealth / timeToKill;
                self.regen = -degen;
            }

            if (self.HasBuff(bdOverload)) {
                self.moveSpeed *= 1.25f;
                self.attackSpeed *= 1.5f;
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