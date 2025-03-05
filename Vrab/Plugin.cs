global using static Vrab.Main;

using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using RoR2.UI;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets.ResourceLocators;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using R2API.Networking;

namespace Vrab {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pseudopulse";
        public const string PluginName = "Vrab";
        public const string PluginVersion = "1.0.0";

        public static BepInEx.Logging.ManualLogSource ModLogger;
        public static AssetBundle Assets;

        public void Awake() {
            // set logger
            ModLogger = Logger;

            Assets = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("Vrab.dll", "vrabassets"));

            SwapAllShaders(Assets);
            ScanTypes<SkillBase>((x) => x.Init());
            ScanTypes<SurvivorBase>((x) => x.Init());
            ScanTypesNoInstance<EntityState>((x) =>
            {
                ContentAddition.AddEntityState(x, out _);
            });

            NetworkingAPI.RegisterMessageType<SyncVrabTarget>();
        }

        internal static void ScanTypesNoInstance<T>(Action<Type> action)
        {
            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(T)));

            foreach (Type type in types)
            {
                action(type);
            }
        }

        public static T Load<T>(string path) where T : UnityEngine.Object {
            return Assets.LoadAsset<T>(path);
        }

        internal static void ScanTypes<T>(Action<T> action)
        {
            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(T)));

            foreach (Type type in types)
            {
                T instance = (T)Activator.CreateInstance(type);
                action(instance);
            }
        }

        public void SwapAllShaders(AssetBundle bundle)
        {
            Material[] array = bundle.LoadAllAssets<Material>();
            foreach (Material val in array)
            {
                switch (val.shader.name)
                {
                    case "Hopoo Games/FX/Cloud Remap":
                        val.shader = Utils.Assets.Shader.HGCloudRemap;
                        break;

                    case "Stubbed Hopoo Games/Deferred/Standard":
                        val.shader = Resources.Load<Shader>("shaders/deferred/hgstandard");
                        break;

                    case "StubbedShader/deferred/hgstandard":
                        val.shader = Utils.Assets.Shader.HGStandard;
                        break;

                    case "StubbedShader/fx/hgintersectioncloudremap":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgintersectioncloudremap");
                        break;

                    case "Stubbed Hopoo Games/Deferred/Snow Topped":
                        val.shader = Resources.Load<Shader>("shaders/deferred/hgsnowtopped");
                        break;

                    case "Stubbed Hopoo Games/FX/Cloud Remap":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgcloudremap");
                        break;

                    case "Stubbed Hopoo Games/FX/Cloud Intersection Remap":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgintersectioncloudremap");
                        break;

                    case "Stubbed Hopoo Games/FX/Opaque Cloud Remap":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgopaquecloudremap");
                        break;

                    case "Stubbed Hopoo Games/FX/Distortion":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgdistortion");
                        break;

                    case "Stubbed Hopoo Games/FX/Solid Parallax":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgsolidparallax");
                        break;

                    case "Stubbed Hopoo Games/Environment/Distant Water":
                        val.shader = Resources.Load<Shader>("shaders/environment/hgdistantwater");
                        break;

                    case "StubbedRoR2/Base/Shaders/HGStandard":
                        val.shader = LegacyShaderAPI.Find("Hopoo Games/Deferred/Standard");
                        break;

                    case "StubbedRoR2/Base/Shaders/HGCloudRemap":
                        val.shader = Utils.Assets.Shader.HGCloudRemap;
                        break;

                    case "StubbedRoR2/Base/Shaders/HGIntersectionCloudRemap":
                        val.shader = LegacyShaderAPI.Find("Hopoo Games/FX/Cloud Intersection Remap");
                        break;
                }
            }
        }
    }
}