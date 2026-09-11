using System;
using System.Collections;
using RoR2.UI;
using RoR2.ContentManagement;
using UnityEngine.UI;

namespace Vrab {
    public class OperateSkillSlot : MonoBehaviour {
        public Texture2D tex1;
        public Texture2D tex2;
        public Texture2D output;
        public float glitchTimer = 0.55f;
        public float glitchDuration = 0.3f;
        public Sprite mainSprite;
        public Sprite nextSprite;
        public float[] scrollSpeeds = new float[128];
        public int[] scrollDirections = new int[128];
        public float[] offsets = new float[128];
        public float glitchProgress;
        public float glitchStopwatch;
        public SkillIcon skillIcon;
        public Image image;
        public void Start() {
            skillIcon = GetComponent<SkillIcon>();
            image = skillIcon.iconImage;
            
            LoadTextures();
            image.sprite = tex1.MakeSprite();
        }
        public void LoadTextures() {
            if (tex1) { GameObject.Destroy(tex1); }
            if (tex2) { GameObject.Destroy(tex2); }
            if (output) { GameObject.Destroy(output); }

            if (nextSprite) {
                mainSprite = nextSprite;
            }
            else {
                mainSprite = OperateSkillHolder.GetNextIcon(skillIcon.targetSkillSlot);
            }

            nextSprite = OperateSkillHolder.GetNextIcon(skillIcon.targetSkillSlot);

            tex1 = DuplicateTexture(mainSprite);
            tex2 = DuplicateTexture(nextSprite);
            output = DuplicateTexture(mainSprite);

            image.sprite = tex1.MakeSprite();
        }
        public void LateUpdate() {
            glitchStopwatch += Time.deltaTime;

            if (glitchStopwatch >= glitchTimer)
            {
                glitchProgress += Time.deltaTime;

                Texture2D targetTex = tex2;
                Texture2D mainTex = tex1;

                if (glitchProgress >= glitchDuration)
                {
                    glitchProgress = 0f;
                    glitchStopwatch = 0f;

                    output.SetPixels(targetTex.GetPixels());

                    LoadTextures();

                    glitchDuration = Random.Range(0.15f, 0.3f);

                    for (int i = 0; i < 128; i++)
                    {
                        scrollSpeeds[i] = Random.Range(120f / glitchDuration, 160f / glitchDuration);
                        scrollDirections[i] = Util.CheckRoll(50) ? -1 : 1;
                        offsets[i] = 0;
                    }

                    glitchTimer = Random.Range(0.4f, 0.8f);

                    return;
                }

                for (int i = 0; i < 128; i++)
                {
                    offsets[i] += scrollDirections[i] * scrollSpeeds[i] * Time.deltaTime;
                    offsets[i] = Mathf.Clamp(offsets[i], -256, 256);

                    for (int y = 0; y < 128; y++)
                    {
                        int index = y + (int)offsets[i];
                        Color col = default;

                        if (index < 0)
                        {
                            col = targetTex.GetPixel(128 + index, i);
                        }
                        else if (index >= 128)
                        {
                            col = targetTex.GetPixel(index - 128, i);
                        }
                        else
                        {
                            col = mainTex.GetPixel(index, i);
                        }

                        output.SetPixel(y, i, col);
                    }
                }
            }

            output.Apply();
            image.sprite = output.MakeSprite();
            image.color = new Color32(255, 0, 255, 255);
            image.SetAllDirty();
        }

        private Texture2D DuplicateTexture(Sprite sprite)
        {
            Texture2D source = sprite.texture;
            int size = 128;

            Vector2[] uv = sprite.uv;

            Vector2 min = uv[0];
            Vector2 max = uv[0];

            for (int i = 1; i < uv.Length; i++)
            {
                min = Vector2.Min(min, uv[i]);
                max = Vector2.Max(max, uv[i]);
            }

            RenderTexture renderTex = RenderTexture.GetTemporary(
                size,
                size,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear
            );

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            GL.Clear(true, true, Color.clear);

            Mesh mesh = new Mesh();

            mesh.vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0),
                new Vector3(0, 1, 0)
            };

            mesh.uv = new Vector2[]
            {
                new Vector2(min.x, min.y),
                new Vector2(max.x, min.y),
                new Vector2(max.x, max.y),
                new Vector2(min.x, max.y)
            };

            mesh.triangles = new int[]
            {
                0, 1, 2,
                0, 2, 3
            };

            Material material = new Material(image.material.shader);
            material.mainTexture = source;

            material.SetPass(0);

            GL.PushMatrix();
            GL.LoadOrtho();
            Graphics.DrawMeshNow(
                mesh,
                Matrix4x4.TRS(
                    Vector3.zero,
                    Quaternion.identity,
                    Vector3.one
                )
            );
            GL.PopMatrix();

            Texture2D result = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

            result.ReadPixels(
                new Rect(0, 0, size, size),
                0,
                0
            );

            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            Object.Destroy(mesh);
            Object.Destroy(material);

            return result;
        }
    }

    public static class OperateSkillHolder {
        private static List<Sprite> primaries = new();
        private static List<Sprite> secondaries = new();
        private static List<Sprite> utilites = new();
        private static List<Sprite> specials = new();
        public static void Initialize() {
            On.RoR2.ContentManagement.ContentManager.SetContentPacks += AddSkills;
        }
        public static Sprite GetNextIcon(SkillSlot slot) {
            switch (slot) {
                case SkillSlot.Primary:
                    return primaries.GetRandom();
                case SkillSlot.Secondary:
                    return secondaries.GetRandom();
                case SkillSlot.Utility:
                    return utilites.GetRandom();
                case SkillSlot.Special:
                    return specials.GetRandom();
                default:
                    return null;
            }
        }
        private static IEnumerator AddSkills(On.RoR2.ContentManagement.ContentManager.orig_SetContentPacks orig, List<ReadOnlyContentPack> packs) {
            yield return orig(packs);

            foreach (SurvivorDef survivor in ContentManager.survivorDefs) { 
                GameObject prefab = survivor.bodyPrefab;
                SkillLocator locator = prefab.GetComponent<SkillLocator>();
                CollectSkills(locator.primary.skillFamily, ref primaries);
                CollectSkills(locator.secondary.skillFamily, ref secondaries);
                CollectSkills(locator.utility.skillFamily, ref utilites);
                CollectSkills(locator.special.skillFamily, ref specials);
            }
        }
        internal static void CollectSkills(SkillFamily family, ref List<Sprite> list)
        {
            foreach (SkillFamily.Variant variant in family.variants)
            {
                list.Add(variant.skillDef.icon);
            }
        }
    }
}