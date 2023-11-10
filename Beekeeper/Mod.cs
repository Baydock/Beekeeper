using Assets.Scripts.Models;
using Assets.Scripts.Models.Profile;
using Assets.Scripts.Models.Skins;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Mods;
using Assets.Scripts.Models.Towers.Upgrades;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Simulation.Bloons;
using Assets.Scripts.Simulation.Objects;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Simulation.Towers.Projectiles;
using Assets.Scripts.Unity.Display;
using Assets.Scripts.Unity.Skins;
using Assets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Assets.Scripts.Unity.UI_New.Main.HeroSelect;
using Assets.Scripts.Utils;
using HarmonyLib;
using MelonLoader;
using NinjaKiwi.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(Beekeeper.Mod), "Beekeeper", "1.0.0", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace Beekeeper {
    [HarmonyPatch]
    public partial class Mod : MelonMod {
        internal static MelonLogger.Instance Logger;

        public override void OnApplicationStart() {
            Logger = LoggerInstance;

            ClassInjector.RegisterTypeInIl2Cpp<Tower2DAnimator>();
        }

        internal static Texture2D LoadTexture(byte[] data) {
            Texture2D tex = new Texture2D(0, 0) { wrapMode = TextureWrapMode.Clamp };
            ImageConversion.LoadImage(tex, data);
            return tex;
        }

        internal static Sprite CreateSprite(Texture2D tex) => Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);

        internal static Sprite CreateSprite(Texture2D tex, float ppu) => Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one / 2, ppu);

        internal static void SetImage(Image image, byte[] data) => SetImage(image, LoadTexture(data));

        internal static void SetImage(Image image, Texture2D tex) => image.sprite = CreateSprite(tex);

        internal static void SetRenderer(SpriteRenderer sprite, byte[] data, float ppu) => SetRenderer(sprite, LoadTexture(data), ppu);

        internal static void SetRenderer(SpriteRenderer sprite, Texture2D tex, float ppu) => sprite.sprite = CreateSprite(tex, ppu);

        [HarmonyPatch(typeof(GameModelLoader), nameof(GameModelLoader.Load))]
        [HarmonyPostfix]
        public static void AddTower(ref GameModel __result) {
            HeroDetailsModel beekeeperDetails = new HeroDetailsModel(Beekeeper.Name, __result.heroSet.Length, 20, 1, 0, 0, 0, null, false);
            __result.heroSet = __result.heroSet.AddItem(beekeeperDetails).ToArray();
            __result.childDependants.Add(beekeeperDetails);

            List<TowerModel> beekeeperTowerModels = new List<TowerModel>();
            for (int i = 1; i < 11; i++) {
                TowerModel beekeeper = Beekeeper.Get(i);
                beekeeperTowerModels.Add(beekeeper);
                __result.childDependants.Add(beekeeper);
            }
            __result.towers = __result.towers.Concat(beekeeperTowerModels).ToArray();

            List<UpgradeModel> beekeeperUpgrades = new List<UpgradeModel>();
            for (int i = 2; i < 11; i++) {
                UpgradeModel beekeeperUpgrade = Beekeeper.GetUpgrade(i);
                beekeeperUpgrades.Add(beekeeperUpgrade);
                __result.childDependants.Add(beekeeperUpgrade);
            }
            __result.upgrades = __result.upgrades.Concat(beekeeperUpgrades).ToArray();

            SkinModel beekeeperSkin = Beekeeper.GetSkin();
            __result.skins = __result.skins.AddItem(beekeeperSkin).ToArray();
            __result.childDependants.Add(beekeeperSkin);

            __result.knowledgeSets[4].tiers[1].levels[1].items[0].mod.mutatorMods[0].Cast<SimTowerDiscountModModel>().tower += ",Beekeeper";

            Beekeeper.AddLocalization(LocalizationManager.Instance.defaultTable);
        }

        [HarmonyPatch(typeof(ProfileModel), nameof(ProfileModel.Validate))]
        [HarmonyPostfix]
        public static void PatchProfile(ref ProfileModel __instance) {
            __instance.unlockedHeroes.AddIfNotPresent(Beekeeper.Name);
        }

        [HarmonyPatch(typeof(SkinManager), nameof(SkinManager.GetMMCost))]
        [HarmonyPrefix]
        public static bool HandleSkins(string skinId, ref int __result) {
            if (!string.IsNullOrEmpty(skinId) && skinId.Equals(Beekeeper.Name)) {
                __result = 0;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(HeroUpgradeDetails), nameof(HeroUpgradeDetails.Awake))]
        [HarmonyPostfix]
        public static void HandleHeroData(ref HeroUpgradeDetails __instance) {
            HeroFontMaterial beekeeperFont = new HeroFontMaterial {
                name = Beekeeper.Name,
                heroNameMaterial = __instance.heroSprites[0].heroNameMaterial
            };
            __instance.heroSprites = __instance.heroSprites.AddItem(beekeeperFont).ToArray();
        }

        [HarmonyPatch(typeof(ResourceLoader), nameof(ResourceLoader.LoadSpriteFromSpriteReferenceAsync))]
        [HarmonyPrefix]
        public static bool LoadSprites(SpriteReference reference, Image image) {
            if (!(reference is null)) {
                byte[] bytes = Beekeeper.LoadSprite(reference.GUID);
                if (!(bytes is null)) {
                    SetImage(image, bytes);
                    return false;
                }
            }
            return true;
        }

        private static readonly Dictionary<string, UnityDisplayNode> protos = new Dictionary<string, UnityDisplayNode>();
        [HarmonyPatch(typeof(Factory), nameof(Factory.FindAndSetupPrototypeAsync))]
        [HarmonyPrefix]
        public static bool LoadProtos(ref Factory __instance, string objectId, Il2CppSystem.Action<UnityDisplayNode> onComplete) {
            if (!protos.ContainsKey(objectId) || protos[objectId].isDestroyed) {
                UnityDisplayNode udn = Beekeeper.LoadProto(objectId);
                if (!(udn is null)) {
                    udn.gameObject.SetActive(false);
                    udn.transform.parent = __instance.PrototypeRoot;
                    udn.RecalculateGenericRenderers();
                    protos.Add(objectId, udn);
                }
            }
            if (protos.ContainsKey(objectId)) {
                onComplete?.Invoke(protos[objectId]);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Factory), nameof(Factory.ProtoFlush))]
        [HarmonyPostfix]
        public static void ClearProtos() {
            foreach (UnityDisplayNode proto in protos.Values) {
                if (!(proto is null))
                    Object.Destroy(proto.gameObject);
            }
            protos.Clear();
        }

        [HarmonyPatch(typeof(Factory), nameof(Factory.CreateAsync))]
        [HarmonyPrefix]
        public static bool InitModel(string objectId, ref Il2CppSystem.Action<UnityDisplayNode> onComplete) {
            onComplete += new System.Action<UnityDisplayNode>(udn => {
                Beekeeper.InitModel(objectId, udn);
                Beekeeper.ApplyCustomRotation(objectId, udn, 0);
            });
            return true;
        }

        [HarmonyPatch(typeof(CommonBehaviorProxy), nameof(CommonBehaviorProxy.Rotation), MethodType.Setter)]
        [HarmonyPostfix]
        public static async void CustomSetRotation(CommonBehaviorProxy __instance, float value) {
            if (Il2CppType.Of<Tower>().IsAssignableFrom(__instance.GetIl2CppType())) {
                Tower tower = __instance.Cast<Tower>();
                while (tower?.Node?.Graphic is null)
                    await Task.Delay(1);
                Beekeeper.ApplyCustomRotation(tower.towerModel.display, tower.Node.Graphic, -value);
            }
        }

        [HarmonyPatch(typeof(CommonBehaviorProxy), nameof(CommonBehaviorProxy.Rotation), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool CustomGetRotation(CommonBehaviorProxy __instance, ref float __result) {
            if (Il2CppType.Of<Tower>().IsAssignableFrom(__instance.GetIl2CppType())) {
                Tower tower = __instance.Cast<Tower>();
                bool success = tower.towerModel.baseId.Equals(Beekeeper.Name);
                if (!(tower?.Node?.Graphic is null)) {
                    success = Beekeeper.GetCustomRotation(tower.towerModel.display, tower.Node.Graphic, out float result);
                    __result = -result;
                } else
                    __result = 180;
                return !success;
            }
            return true;
        }

        [HarmonyPatch(typeof(Tower), nameof(Tower.Hilight))]
        [HarmonyPostfix]
        public static void ApplyCustomHighlight(ref Tower __instance) => Beekeeper.ApplyHighlight(__instance.towerModel.display, __instance.Node.graphic, true);

        [HarmonyPatch(typeof(Tower), nameof(Tower.UnHighlight))]
        [HarmonyPostfix]
        public static void RemoveCustomHighlight(ref Tower __instance) => Beekeeper.ApplyHighlight(__instance.towerModel.display, __instance.Node.graphic, false);

        // I tried every function I could find, had to resort to this lol
        [HarmonyPatch(typeof(Tower), nameof(Tower.UpdatedModel))]
        [HarmonyPrefix]
        public static void PassStateToUpgrade(Tower __instance, Model modelToUse) {
            if (__instance.towerModel.baseId.Equals(Beekeeper.Name)) {
                if (Il2CppType.Of<TowerModel>().IsAssignableFrom(modelToUse.GetIl2CppType())) {
                    TowerModel model = modelToUse.Cast<TowerModel>();
                    if (!__instance.towerModel.display.Equals(model.display))
                        WaitForAndAttemptPass(__instance, __instance.Node.graphic.GetInstanceID(), __instance.Rotation);
                }
            }
        }
        private static async void WaitForAndAttemptPass(Tower __instance, int oldId, float oldRotation) {
            while (oldId == __instance.Node.graphic.GetInstanceID())
                await Task.Delay(1);
            Tower selectedTower = TowerSelectionMenu.instance?.GetSelectedTower()?.tower;
            if (!(selectedTower is null) && selectedTower.Id == __instance.Id)
                Beekeeper.ApplyHighlight(__instance.towerModel.display, __instance.Node.graphic, true);
            __instance.Rotation = oldRotation;
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.CollideBloon))]
        [HarmonyPostfix]
        public static void AdditionalCollisionBehavior(Projectile __instance, Bloon bloon) {
            if (__instance.EmittedBy.towerModel.baseId.Equals(Beekeeper.Name))
                Beekeeper.OnProjectileCollide(__instance, bloon);
        }

        [HarmonyPatch(typeof(Bloon), nameof(Bloon.Degrade))]
        [HarmonyPrefix]
        public static bool AdditionalPopBehavior(Tower tower) {
            //Logger.Msg(tower.towerModel.name);
            return true;
        }
    }
}
