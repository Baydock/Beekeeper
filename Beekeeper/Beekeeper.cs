using Assets.Scripts.Models;
using Assets.Scripts.Models.Audio;
using Assets.Scripts.Models.Bloons;
using Assets.Scripts.Models.Bloons.Behaviors;
using Assets.Scripts.Models.Effects;
using Assets.Scripts.Models.GenericBehaviors;
using Assets.Scripts.Models.Map;
using Assets.Scripts.Models.Skins;
using Assets.Scripts.Models.Skins.Behaviors;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Abilities;
using Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Emissions;
using Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors;
using Assets.Scripts.Models.Towers.Filters;
using Assets.Scripts.Models.Towers.Mods;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Models.Towers.Upgrades;
using Assets.Scripts.Models.Towers.Weapons;
using Assets.Scripts.Simulation.Bloons;
using Assets.Scripts.Simulation.Towers.Projectiles;
using Assets.Scripts.Unity.Display;
using Assets.Scripts.Utils;
using Beekeeper.Properties;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using NKVector3 = Assets.Scripts.Simulation.SMath.Vector3;

namespace Beekeeper {
    // TODO: ability mastery mk on lvl 20
    // TODO: resolve bee duplication
    // TODO: bees return to beekeeper
    // TODO: patch bloon damage function, has tower reference in it ;)
    internal static class Beekeeper {
        public const string Name = "Beekeeper";
        private const string TypeLoc = Name + " type";
        private const string DescLoc = Name + " desc";
        private const string IconName = Name + " icon";
        private const string Portrait = Name + " portrait";
        private const string HighlightedDisplay = Name + " highlighted";
        private const string Level3Portrait = Portrait + " 3";
        private const string Level3Display = Name + " 3";
        private const string Level3HighlightedDisplay = Level3Display + " highlighted";
        private const string Level10Portrait = Portrait + " 10";
        private const string Level10Display = Name + " 10";
        private const string Level10HighlightedDisplay = Level10Display + " highlighted";
        private const string BeeDisplay = Name + " bee";
        private const string BeeBloonEffectDisplay = Name + " beeBloonEffect";
        private const string BeeMoabEffectDisplay = Name + " beeMoabEffect";
        private const string BeeBloonMutation = Name + ":bee";
        private const string BeeProjectileId = "Bee";
        private const string BeeProjectileReturningId = BeeProjectileId + " returning";
        private const string Level3AbilityId = "BeeBackup";
        private const string Level3AbilityIcon = Level3AbilityId + " icon";
        private const string Level3AbilityDisplayName = "Bee Backup";
        private const string Level3AbilityDescription = "Triple the bees for a short time.";
        private const string Level10AbilityId = "BeeSwarm";
        private const string Level10AbilityIcon = Level10AbilityId + " icon";
        private const string Level10AbilityDisplayName = "Bee Swarm";
        private const string Level10AbilityDescription = "Launches 100 bees that will seek targets and pop them mercilessly for a short duration.";
        private const string Level2UpgradeName = Name + " Range";
        private const string Level3UpgradeName = Name + " " + Level3AbilityId;
        private const string Level4UpgradeName = Name + " AttackSpeed1";
        private const string Level5UpgradeName = Name + " CamoDetection";
        private const string Level6UpgradeName = Name + " TickSpeed1";
        private const string Level7UpgradeName = Name + " LongerLifeBees";
        private const string Level8UpgradeName = Name + " RegrowDamage";
        private const string Level9UpgradeName = Name + " AttackSpeed2";
        private const string Level10UpgradeName = Name + " " + Level10AbilityId;

        private const float Radius = 13;
        private const float Range = 50;
        private const float Range1 = 55;
        private const float Range2 = 70;
        private const float AttackSpeed = .7f;
        private const float AttackSpeed1 = .6f;
        private const float AttackSpeed2 = .4f;
        private const float AttackSpeed3 = .2f;
        private const float TickSpeed = .75f;
        private const float TickSpeed1 = .5f;
        private const float TickSpeed2 = .25f;
        private const int Damage = 1;
        private const int Damage1 = 2;
        private const int Damage2 = 3;
        private const int RegrowDamage = 0;
        private const int RegrowDamage1 = 1;
        private const int RegrowDamage2 = 2;
        private const int CeramMoabDamage = 0;
        private const int CeramMoabDamage1 = 1;
        private const int CeramMoabDamage2 = 2;

        public static void AddLocalization(Dictionary<string, string> dict) {
            dict.Add(TypeLoc, "Hive Master");
            dict.Add(DescLoc, "Buzz buzz!");
            const string lvl1desc = "Carries a hive of angry bees that zip to their targets and sting bloons until all layers are popped.";
            dict.Add($"{Name} Description", lvl1desc);
            dict.Add($"{Name} Level 1 Description", lvl1desc);
            dict.Add($"{Name} Level 2 Description", "Increased attack range.");
            dict.Add($"{Name} Level 3 Description", $"{Level3AbilityDisplayName}: {Level3AbilityDescription}");
            dict.Add($"{Name} Level 4 Description", "Increased attack speed.");
            dict.Add($"{Name} Level 5 Description", "The bees' heightened senses allow them to now seek out camo bloons.");
            dict.Add($"{Name} Level 6 Description", "The bees tear through layers faster.");
            dict.Add($"{Name} Level 7 Description", "The bees can now seek bloons for longer without needing to return to the hive, and can now pop frozen bloons.");
            dict.Add($"{Name} Level 8 Description", "Bees do +1 damage to regrow bloons.");
            dict.Add($"{Name} Level 9 Description", "Further increased attack speed.");
            dict.Add($"{Name} Level 10 Description", $"{Level10AbilityDisplayName}: {Level10AbilityDescription}");
        }

        public static byte[] LoadSprite(string name) {
            switch (name) {
                case IconName:
                    return Textures.BeekeeperEmoteIcon;
                case Portrait:
                    return Textures.BeekeeperPortrait;
                case Level3Portrait:
                    return Textures.Beekeeper3Portrait;
                case Level3AbilityIcon:
                    return Textures.BeeBackup;
                case Level10Portrait:
                    return Textures.Beekeeper10Portrait;
                case Level10AbilityIcon:
                    return Textures.BeeSwarm;
                default:
                    return null;
            }
        }

        public static byte[][] LoadFrames(string name, out float ppu, out float fps) {
            switch (name) {
                case Name:
                    ppu = 6;
                    fps = 15;
                    return new byte[][] {
                        Textures.BeekeeperF1,
                        Textures.BeekeeperF2,
                        Textures.BeekeeperF3,
                        Textures.BeekeeperF4
                    };
                case HighlightedDisplay:
                    ppu = 6;
                    fps = 15;
                    return new byte[][] {
                        Textures.BeekeeperF1Highlight,
                        Textures.BeekeeperF2Highlight,
                        Textures.BeekeeperF3Highlight,
                        Textures.BeekeeperF4Highlight
                    };
                case Level3Display:
                    ppu = 6;
                    fps = 15;
                    return new byte[][] {
                        Textures.Beekeeper3F1,
                        Textures.Beekeeper3F2,
                        Textures.Beekeeper3F3,
                        Textures.Beekeeper3F4,
                    };
                case Level3HighlightedDisplay:
                    ppu = 6;
                    fps = 15;
                    return new byte[][] {
                        Textures.Beekeeper3F1Highlighted,
                        Textures.Beekeeper3F2Highlighted,
                        Textures.Beekeeper3F3Highlighted,
                        Textures.Beekeeper3F4Highlighted
                    };
                case Level10Display:
                    ppu = 6;
                    fps = 15;
                    return new byte[][] {
                        Textures.Beekeeper10F1,
                        Textures.Beekeeper10F2,
                        Textures.Beekeeper10F3,
                        Textures.Beekeeper10F4,
                    };
                case Level10HighlightedDisplay:
                    ppu = 6;
                    fps = 15;
                    return new byte[][] {
                        Textures.Beekeeper10F1Hilighted,
                        Textures.Beekeeper10F2Hilighted,
                        Textures.Beekeeper10F3Hilighted,
                        Textures.Beekeeper10F4Hilighted
                    };
                default:
                    ppu = 0;
                    fps = 0;
                    return null;
            }
        }

        public static UnityDisplayNode LoadProto(string name) {
            switch (name) {
                case Level10Display:
                case Level3Display:
                case Name: {
                    GameObject beekeeper = new GameObject(name);

                    GameObject tilt = new GameObject("Tilt");
                    tilt.transform.parent = beekeeper.transform;
                    tilt.transform.localPosition = new Vector3(0, 10, 0);
                    tilt.transform.eulerAngles = new Vector3(300, 0, 0);

                    GameObject spriteObject = new GameObject("Sprite");
                    Mod.SetRenderer(spriteObject.AddComponent<SpriteRenderer>(), Textures.BeekeeperF1, 6f);
                    spriteObject.AddComponent<Tower2DAnimator>();
                    spriteObject.transform.parent = tilt.transform;
                    spriteObject.transform.localEulerAngles = new Vector3(0, 0, 0);
                    spriteObject.transform.localPosition = new Vector3(0, 0, 0);

                    UnityDisplayNode udn = beekeeper.AddComponent<UnityDisplayNode>();
                    return udn;
                }
                case BeeDisplay: {
                    GameObject bee = new GameObject(name);
                    SpriteRenderer sprite = bee.AddComponent<SpriteRenderer>();
                    Mod.SetRenderer(sprite, Textures.Bee, 6f);
                    sprite.sortingLayerName = "Projectiles";

                    UnityDisplayNode udn = bee.AddComponent<UnityDisplayNode>();
                    udn.isSprite = true;
                    return udn;
                }
                case BeeBloonEffectDisplay: {
                    GameObject bee = new GameObject(name);
                    SpriteRenderer sprite = bee.AddComponent<SpriteRenderer>();
                    Mod.SetRenderer(sprite, Textures.Bee, 6f);
                    sprite.sortingLayerName = "Bloons";
                    sprite.sortingOrder = 17;

                    UnityDisplayNode udn = bee.AddComponent<UnityDisplayNode>();
                    udn.isSprite = true;
                    return udn;
                }
                case string _ when name.StartsWith(BeeMoabEffectDisplay): {
                    GameObject bee = new GameObject(name);

                    GameObject spriteObject = new GameObject("Sprite");
                    SpriteRenderer sprite = spriteObject.AddComponent<SpriteRenderer>();
                    Mod.SetRenderer(sprite, Textures.Bee, 6f);
                    sprite.sortingLayerName = "Moabs";
                    spriteObject.transform.parent = bee.transform;
                    spriteObject.transform.localEulerAngles = new Vector3(90, 0, 0);
                    Vector3 pos = Vector3.zero;
                    switch (name.Substring(BeeMoabEffectDisplay.Length)) {
                        case "Moab":
                        case "Ddt":
                        case "Bfb":
                            pos = new Vector3(0, 15, -8);
                            break;
                        case "Zomg":
                            pos = new Vector3(0, 21, -15);
                            break;
                        case "Bad":
                            pos = new Vector3(0, 35, 0);
                            break;
                        case "Bloonarius":
                            pos = new Vector3(0, 35, 3);
                            break;
                        case "Lych":
                            pos = new Vector3(0, 39, -15);
                            break;
                        case "MiniLych":
                            pos = new Vector3(0, 25, -9);
                            break;
                    }
                    spriteObject.transform.localPosition = pos;

                    UnityDisplayNode udn = bee.AddComponent<UnityDisplayNode>();
                    udn.isSprite = true;
                    return udn;
                }
                default:
                    return null;
            }
        }

        public static void InitModel(string name, UnityDisplayNode udn) {
            switch (name) {
                case Name: {
                    Tower2DAnimator animator = udn.transform.GetChild(0).GetChild(0).GetComponent<Tower2DAnimator>();
                    animator.framesId = Name;
                    animator.highlightFramesId = HighlightedDisplay;
                    break;
                }
                case Level3Display: {
                    Tower2DAnimator animator = udn.transform.GetChild(0).GetChild(0).GetComponent<Tower2DAnimator>();
                    animator.framesId = Level3Display;
                    animator.highlightFramesId = Level3HighlightedDisplay;
                    break;
                }
                case Level10Display: {
                    Tower2DAnimator animator = udn.transform.GetChild(0).GetChild(0).GetComponent<Tower2DAnimator>();
                    animator.framesId = Level10Display;
                    animator.highlightFramesId = Level10HighlightedDisplay;
                    break;
                }
            }
        }

        public static void ApplyCustomRotation(string display, UnityDisplayNode udn, float rotation) {
            switch (display) {
                case Level10Display:
                case Level3Display:
                case Name: {
                    Transform tilt = udn.transform.GetChild(0);
                    tilt.localEulerAngles = new Vector3(300, -rotation, 0);
                    Transform spriteObject = tilt.GetChild(0);
                    spriteObject.localEulerAngles = new Vector3(0, 0, rotation);
                    break;
                }
            }
        }
        public static bool GetCustomRotation(string display, UnityDisplayNode udn, out float rotation) {
            switch (display) {
                case Level10Display:
                case Level3Display:
                case Name: {
                    Transform spriteObject = udn.transform.GetChild(0).GetChild(0);
                    rotation = spriteObject.localEulerAngles.z;
                    break;
                }
                default:
                    rotation = 0;
                    return false;
            }
            return true;
        }

        public static void ApplyHighlight(string display, UnityDisplayNode udn, bool on) {
            switch (display) {
                case Level10Display:
                case Level3Display:
                case Name: {
                    Tower2DAnimator animator = udn.transform.GetChild(0).GetChild(0).GetComponent<Tower2DAnimator>();
                    animator.Highlighted = on;
                    break;
                }
            }
        }

        public static void OnProjectileCollide(Projectile projectile, Bloon bloon) {
            if (!(projectile?.target?.bloon is null)) {
                if (bloon.Id == projectile.target.bloon.Id) {
                    projectile.Expire();
                }
            }
        }

        public static SkinModel GetSkin() =>
            new HeroSkinModel(name: Name,
                              icon: new SpriteReference(IconName),
                              towerBaseId: Name,
                              locsKeySkinName: TypeLoc,
                              locsKeySkinDescription: DescLoc,
                              mmCost: 0,
                              isDefaultTowerSkin: true,
                              sprites: new SwapTowerSpriteModel[0],
                              graphics: new SwapTowerGraphicModel[0],
                              sounds: new SwapTowerSoundModel[0],
                              materialId: Name,
                              uiPortraits: new SpriteReference[] {
                                  new SpriteReference(Portrait),
                                  new SpriteReference(""),
                                  new SpriteReference(""),
                                  new SpriteReference("")
                              },
                              uiPortraitLvls: new string[] {
                                  "3",
                                  "10",
                                  "20"
                              },
                              heroUnlockedEventSound: SoundModel.blank,
                              heroUnlockedVoiceSound: SoundModel.blank);

        public static UpgradeModel GetUpgrade(int level) {
            switch (level) {
                case 2:
                    return new UpgradeModel(Level2UpgradeName, 0, 180, null, 0, 1, 0, "", "");
                case 3:
                    return new UpgradeModel(Level3UpgradeName, 0, 460, null, 0, 2, 0, "", "");
                case 4:
                    return new UpgradeModel(Level4UpgradeName, 0, 1000, null, 0, 3, 0, "", "");
                case 5:
                    return new UpgradeModel(Level5UpgradeName, 0, 1860, null, 0, 4, 0, "", "");
                case 6:
                    return new UpgradeModel(Level6UpgradeName, 0, 3280, null, 0, 5, 0, "", "");
                case 7:
                    return new UpgradeModel(Level7UpgradeName, 0, 5180, null, 0, 6, 0, "", "");
                case 8:
                    return new UpgradeModel(Level8UpgradeName, 0, 8320, null, 0, 7, 0, "", "");
                case 9:
                    return new UpgradeModel(Level9UpgradeName, 0, 9380, null, 0, 8, 0, "", "");
                case 10:
                    return new UpgradeModel(Level10UpgradeName, 0, 13620, null, 0, 9, 0, "", "");
                default:
                    return null;
            }
        }

        private static AttackModel GetAttack(float range, float attackSpeed, float tickSpeed, bool canPopFrozen, bool canPopLeads, bool canSeeCamo,
                                             int damage, int regrowDamage, int ceramMoabDamage, EmissionModel emission, bool fireWithoutTarget = false) {
            System.Collections.Generic.List<FilterModel> attackFilters = new System.Collections.Generic.List<FilterModel>() {
                new FilterInvisibleModel("", !canSeeCamo, false),
                new FilterMutatedTargetModel("", BeeBloonMutation)
            };
            BloonProperties immuneBloons = BloonProperties.None;
            if (!canPopFrozen) {
                immuneBloons |= BloonProperties.Frozen;
                attackFilters.Add(new FilterFrozenBloonsModel(""));
            }
            if (!canPopLeads) {
                immuneBloons |= BloonProperties.Lead;
                attackFilters.Add(new FilterWithTagModel("", "Lead", true));
                attackFilters.Add(new FilterWithTagModel("", "Ddt", true));
            }
            System.Collections.Generic.List<DamageModifierModel> damageIncreases = null;
            if (regrowDamage > 0 || ceramMoabDamage > 0) {
                damageIncreases = new System.Collections.Generic.List<DamageModifierModel>();
                if (regrowDamage > 0)
                    damageIncreases.Add(new DamageModifierForTagModel("", "Grow", 1, regrowDamage, false, false));
                if (ceramMoabDamage > 0) {
                    damageIncreases.Add(new DamageModifierForTagModel("", "Ceramic", 1, ceramMoabDamage, false, false));
                    damageIncreases.Add(new DamageModifierForTagModel("", "Moabs", 1, ceramMoabDamage, false, false));
                }
            }
            return new AttackModel(name: "",
                weapons: new WeaponModel[] {
                    new WeaponModel(name: "",
                        animation: 1,
                        rate: attackSpeed,
                        projectile:
                            new ProjectileModel(display: BeeDisplay,
                                id: BeeProjectileId,
                                radius: 5,
                                vsBlockerRadius: 0,
                                pierce: 9999999,
                                maxPierce: -1,
                                behaviors: new Model[] {
                                    new TravelStraitModel("", 112, 5),
                                    new TrackTargetModel("", 9999999, true, false, 360, true, 160, false, false),
                                    new DamageModel("", damage, -1, true, false, true, immuneBloons),
                                    new AddBehaviorToBloonModel(name: "", BeeBloonMutation, 180, 9999999, new FilterAllExceptTargetModel(""), null,
                                        behaviors: new BloonBehaviorModel[] {
                                            new DamageOverTimeModel("", damage, tickSpeed, immuneBloons,
                                                null, -1, false, 0, false, 0, false, false, damageIncreases?.ToArray()),
                                            /*new EmitOnPopModel("", new ProjectileModel(display: BeeDisplay,
                                                    id: BeeProjectileReturningId,
                                                    radius: 5,
                                                    vsBlockerRadius: 0,
                                                    pierce: 1,
                                                    maxPierce: 1,
                                                    behaviors: new Model[] {
                                                        //new TravelTowardsEmitTowerModel("", true, 112, 9999999, false),
                                                        new TravelStraitModel("", 112, beeLifespan),
                                                        new ProjectileFilterModel("", new FilterModel[] { /*new FilterAllExceptTargetModel("")*/// }),
                                                        /*new DisplayModel("", BeeDisplay, -1, NKVector3.zero, 1, false, 0)
                                                    },
                                                    filters: new FilterModel[] { /*new FilterAllExceptTargetModel("")*/// }
                                                /*), new SingleEmissionModel("", null), -1, true, 0)*/
                                        }, overlays: new Dictionary<string, AssetPathModel>() {
                                            ["Red"] = new AssetPathModel("", BeeBloonEffectDisplay),
                                            ["RedRegrow"] = new AssetPathModel("", BeeBloonEffectDisplay),
                                            ["Blue"] = new AssetPathModel("", BeeBloonEffectDisplay),
                                            ["BlueRegrow"] = new AssetPathModel("", BeeBloonEffectDisplay),
                                            ["Green"] = new AssetPathModel("", BeeBloonEffectDisplay),
                                            ["GreenRegrow"] = new AssetPathModel("", BeeBloonEffectDisplay),
                                            ["Yellow"] = new AssetPathModel("", BeeBloonEffectDisplay),
                                            ["YellowRegrow"] = new AssetPathModel("", BeeBloonEffectDisplay),
                                            ["Pink"] = new AssetPathModel("", BeeBloonEffectDisplay),
                                            ["PinkRegrow"] = new AssetPathModel("", BeeBloonEffectDisplay),
                                            ["White"] = new AssetPathModel("", BeeBloonEffectDisplay),
                                            ["WhiteRegrow"] = new AssetPathModel("", BeeBloonEffectDisplay),
                                            ["Moab"] = new AssetPathModel("", BeeMoabEffectDisplay + "Moab"),
                                            ["Bfb"] = new AssetPathModel("", BeeMoabEffectDisplay + "Bfb"),
                                            ["Zomg"] = new AssetPathModel("", BeeMoabEffectDisplay + "Zomg"),
                                            ["Ddt"] = new AssetPathModel("", BeeMoabEffectDisplay + "Ddt"),
                                            ["Bad"] = new AssetPathModel("", BeeMoabEffectDisplay + "Bad"),
                                            ["Bloonarius"] = new AssetPathModel("", BeeMoabEffectDisplay + "Bloonarius"),
                                            ["Lych"] = new AssetPathModel("", BeeMoabEffectDisplay + "Lych"),
                                            ["MiniLych"] = new AssetPathModel("", BeeMoabEffectDisplay + "MiniLych")
                                        }, 0, true, false, false, true, 0, false, 0),
                                    new DisplayModel("", BeeDisplay, -1, NKVector3.zero, 1, false, 0)
                                },
                                filters: new FilterModel[0],
                                ignoreBlockers: true,
                                usePointCollisionWithBloons: false,
                                canCollisionBeBlockedByMapLos: false,
                                scale: 1,
                                collisionPasses: new int[] { 0, 1 },
                                dontUseCollisionChecker: false,
                                checkCollisionFrames: 0,
                                ignoreNonTargetable: false,
                                ignorePierceExhaustion: false,
                            null),
                        ejectX: 0,
                        ejectY: 0,
                        ejectZ: 15,
                        animationOffset: 0,
                        fireWithoutTarget: false,
                        fireBetweenRounds: false,
                        emission: emission,
                        behaviors: null,
                        useAttackPosition: false,
                        startInCooldown: false,
                        customStartCooldown: 0,
                        animateOnMainAttack: false)
                },
                range: range,
                behaviors: new Model[] {
                    new RotateToTargetModel("", true, false, false, 0, true, false),
                    new AttackFilterModel("", attackFilters.ToArray()),
                    new TargetFirstModel("", true, false),
                    new TargetLastModel("", true, false),
                    new TargetCloseModel("", true, false),
                    new TargetStrongModel("", true, false)
                },
                targetProvider: null,
                offsetX: 0,
                offsetY: 0,
                offsetZ: 0,
                attackThroughWalls: true,
                fireWithoutTarget: fireWithoutTarget,
                framesBeforeRetarget: 0,
                addsToSharedGrid: true,
                sharedGridRange: 0);
        }

        private static AbilityModel GetLevel3Ability(float range, float attackSpeed, float tickSpeed, bool canPopFrozen, bool canPopLeads, bool canSeeCamo,
                                                     int damage, int regrowDamage, int ceramMoabDamage, float cooldownScale) =>
            new AbilityModel(name: Level3AbilityId,
                             displayName: Level3AbilityDisplayName,
                             description: Level3AbilityDescription,
                             animation: 1,
                             animationOffset: 0,
                             icon: new SpriteReference(Level3AbilityIcon),
                             cooldown: 60 * cooldownScale,
                             behaviors: new Model[] {
                                 new ActivateAttackModel("", 15, true, new AttackModel[] {
                                     GetAttack(range, attackSpeed, tickSpeed, canPopFrozen, canPopLeads, canSeeCamo, damage, regrowDamage, ceramMoabDamage,
                                         new ArcEmissionModel("", 3, 0, 90, null, false))
                                 }, false, true, false, true, false)
                             },
                             activateOnPreLeak: false,
                             activateOnLeak: false,
                             addedViaUpgrade: Level3UpgradeName,
                             cooldownSpeedScale: 0,
                             livesCost: 0,
                             maxActivationsPerRound: -1,
                             resetCooldownOnTierUpgrade: false,
                             activateOnLivesLost: false);

        private static AbilityModel GetLevel10Ability(float range, float attackSpeed, float tickSpeed, bool canPopFrozen, bool canPopLeads, bool canSeeCamo,
                                                     int damage, int regrowDamage, int ceramMoabDamage, float cooldownScale) =>
            new AbilityModel(name: Level10AbilityId,
                             displayName: Level10AbilityDisplayName,
                             description: Level10AbilityDescription,
                             animation: 1,
                             animationOffset: 0,
                             icon: new SpriteReference(Level10AbilityIcon),
                             cooldown: 60 * cooldownScale,
                             behaviors: new Model[] {
                                 new ActivateAttackModel("", attackSpeed, true, new AttackModel[] {
                                     GetAttack(range, attackSpeed, tickSpeed, canPopFrozen, canPopLeads, canSeeCamo, damage, regrowDamage, ceramMoabDamage,
                                         new RandomArcEmissionModel("", 100, 0, 360, 10, 0, null), true)
                                 }, false, false, false, true, true)
                             },
                             activateOnPreLeak: false,
                             activateOnLeak: false,
                             addedViaUpgrade: Level10UpgradeName,
                             cooldownSpeedScale: 0,
                             livesCost: 0,
                             maxActivationsPerRound: -1,
                             resetCooldownOnTierUpgrade: false,
                             activateOnLivesLost: false);

        public static TowerModel Get(int level) {
            switch (level) {
                case 1:
                    return Get(level: level,
                               display: Name,
                               portrait: Portrait,
                               range: Range,
                               attackSpeed: AttackSpeed,
                               tickSpeed: TickSpeed,
                               canPopFrozen: false,
                               canPopLeads: false,
                               canSeeCamo: false,
                               damage: Damage,
                               regrowDamage: RegrowDamage,
                               ceramMoabDamage: CeramMoabDamage,
                               appliedUpgrades: new string[0],
                               upgrade: new UpgradePathModel(Level2UpgradeName, Name + " 2"),
                               abilities: null);
                case 2:
                    return Get(level: level,
                               display: Name,
                               portrait: Portrait,
                               range: Range1,
                               attackSpeed: AttackSpeed,
                               tickSpeed: TickSpeed,
                               canPopFrozen: false,
                               canPopLeads: false,
                               canSeeCamo: false,
                               damage: Damage,
                               regrowDamage: RegrowDamage,
                               ceramMoabDamage: CeramMoabDamage,
                               appliedUpgrades: new string[] { Level2UpgradeName },
                               upgrade: new UpgradePathModel(Level3UpgradeName, Name + " 3"),
                               abilities: null);
                case 3:
                    return Get(level: level,
                               display: Level3Display,
                               portrait: Level3Portrait,
                               range: Range1,
                               attackSpeed: AttackSpeed,
                               tickSpeed: TickSpeed,
                               canPopFrozen: false,
                               canPopLeads: false,
                               canSeeCamo: false,
                               damage: Damage,
                               regrowDamage: RegrowDamage,
                               ceramMoabDamage: CeramMoabDamage,
                               appliedUpgrades: new string[] {
                                   Level2UpgradeName, Level3UpgradeName
                               },
                               upgrade: new UpgradePathModel(Level4UpgradeName, Name + " 4"),
                               abilities: new AbilityModel[] {
                                GetLevel3Ability(Range1, AttackSpeed, TickSpeed, false, false, false, Damage, RegrowDamage, CeramMoabDamage, 1)
                               });
                case 4:
                    return Get(level: level,
                               display: Level3Display,
                               portrait: Level3Portrait,
                               range: Range1,
                               attackSpeed: AttackSpeed1,
                               tickSpeed: TickSpeed,
                               canPopFrozen: false,
                               canPopLeads: false,
                               canSeeCamo: false,
                               damage: Damage,
                               regrowDamage: RegrowDamage,
                               ceramMoabDamage: CeramMoabDamage,
                               appliedUpgrades: new string[] {
                                   Level2UpgradeName, Level3UpgradeName, Level4UpgradeName
                               },
                               upgrade: new UpgradePathModel(Level5UpgradeName, Name + " 5"),
                               abilities: new AbilityModel[] {
                                GetLevel3Ability(Range1, AttackSpeed1, TickSpeed, false, false, false, Damage, RegrowDamage, CeramMoabDamage, 1)
                               });
                case 5:
                    return Get(level: level,
                               display: Level3Display,
                               portrait: Level3Portrait,
                               range: Range1,
                               attackSpeed: AttackSpeed1,
                               tickSpeed: TickSpeed,
                               canPopFrozen: false,
                               canPopLeads: false,
                               canSeeCamo: true,
                               damage: Damage,
                               regrowDamage: RegrowDamage,
                               ceramMoabDamage: CeramMoabDamage,
                               appliedUpgrades: new string[] {
                                   Level2UpgradeName, Level3UpgradeName, Level4UpgradeName, Level5UpgradeName
                               },
                               upgrade: new UpgradePathModel(Level6UpgradeName, Name + " 6"),
                               abilities: new AbilityModel[] {
                                GetLevel3Ability(Range1, AttackSpeed1, TickSpeed, false, false, true, Damage, RegrowDamage, CeramMoabDamage, 1)
                               });
                case 6:
                    return Get(level: level,
                               display: Level3Display,
                               portrait: Level3Portrait,
                               range: Range1,
                               attackSpeed: AttackSpeed1,
                               tickSpeed: TickSpeed1,
                               canPopFrozen: false,
                               canPopLeads: false,
                               canSeeCamo: true,
                               damage: Damage,
                               regrowDamage: RegrowDamage,
                               ceramMoabDamage: CeramMoabDamage,
                               appliedUpgrades: new string[] {
                                   Level2UpgradeName, Level3UpgradeName, Level4UpgradeName, Level5UpgradeName, Level6UpgradeName
                               },
                               upgrade: new UpgradePathModel(Level7UpgradeName, Name + " 7"),
                               abilities: new AbilityModel[] {
                                GetLevel3Ability(Range1, AttackSpeed1, TickSpeed1, false, false, true, Damage, RegrowDamage, CeramMoabDamage, 1)
                               });
                case 7:
                    return Get(level: level,
                               display: Level3Display,
                               portrait: Level3Portrait,
                               range: Range1,
                               attackSpeed: AttackSpeed1,
                               tickSpeed: TickSpeed1,
                               canPopFrozen: true,
                               canPopLeads: false,
                               canSeeCamo: true,
                               damage: Damage,
                               regrowDamage: RegrowDamage,
                               ceramMoabDamage: CeramMoabDamage,
                               appliedUpgrades: new string[] {
                                   Level2UpgradeName, Level3UpgradeName, Level4UpgradeName, Level5UpgradeName, Level6UpgradeName,
                                   Level7UpgradeName
                               },
                               upgrade: new UpgradePathModel(Level8UpgradeName, Name + " 8"),
                               abilities: new AbilityModel[] {
                                GetLevel3Ability(Range1, AttackSpeed1, TickSpeed1, false, false, true, Damage, RegrowDamage, CeramMoabDamage, 1)
                               });
                case 8:
                    return Get(level: level,
                               display: Level3Display,
                               portrait: Level3Portrait,
                               range: Range1,
                               attackSpeed: AttackSpeed1,
                               tickSpeed: TickSpeed1,
                               canPopFrozen: false,
                               canPopLeads: false,
                               canSeeCamo: true,
                               damage: Damage,
                               regrowDamage: RegrowDamage1,
                               ceramMoabDamage: CeramMoabDamage,
                               appliedUpgrades: new string[] {
                                   Level2UpgradeName, Level3UpgradeName, Level4UpgradeName, Level5UpgradeName, Level6UpgradeName,
                                   Level7UpgradeName, Level8UpgradeName
                               },
                               upgrade: new UpgradePathModel(Level9UpgradeName, Name + " 9"),
                               abilities: new AbilityModel[] {
                                GetLevel3Ability(Range1, AttackSpeed1, TickSpeed1, false, false, true, Damage, RegrowDamage1, CeramMoabDamage, 1)
                               });
                case 9:
                    return Get(level: level,
                               display: Level3Display,
                               portrait: Level3Portrait,
                               range: Range1,
                               attackSpeed: AttackSpeed2,
                               tickSpeed: TickSpeed1,
                               canPopFrozen: false,
                               canPopLeads: false,
                               canSeeCamo: true,
                               damage: Damage,
                               regrowDamage: RegrowDamage1,
                               ceramMoabDamage: CeramMoabDamage,
                               appliedUpgrades: new string[] {
                                   Level2UpgradeName, Level3UpgradeName, Level4UpgradeName, Level5UpgradeName, Level6UpgradeName,
                                   Level7UpgradeName, Level8UpgradeName, Level9UpgradeName
                               },
                               upgrade: new UpgradePathModel(Level10UpgradeName, Name + " 10"),
                               abilities: new AbilityModel[] {
                                GetLevel3Ability(Range1, AttackSpeed2, TickSpeed1, false, false, true, Damage, RegrowDamage1, CeramMoabDamage, 1)
                               });
                case 10:
                    return Get(level: level,
                               display: Level10Display,
                               portrait: Level10Portrait,
                               range: Range1,
                               attackSpeed: AttackSpeed2,
                               tickSpeed: TickSpeed1,
                               canPopFrozen: false,
                               canPopLeads: false,
                               canSeeCamo: true,
                               damage: Damage,
                               regrowDamage: RegrowDamage1,
                               ceramMoabDamage: CeramMoabDamage,
                               appliedUpgrades: new string[] {
                                   Level2UpgradeName, Level3UpgradeName, Level4UpgradeName, Level5UpgradeName, Level6UpgradeName,
                                   Level7UpgradeName, Level8UpgradeName, Level9UpgradeName, Level10UpgradeName
                               },
                               upgrade: null,
                               abilities: new AbilityModel[] {
                                GetLevel3Ability(Range1, AttackSpeed2, TickSpeed1, false, false, true, Damage, RegrowDamage1, CeramMoabDamage, 1),
                                GetLevel10Ability(Range1, AttackSpeed2, TickSpeed1, false, false, true, Damage, RegrowDamage1, CeramMoabDamage, 1)
                               });
                default:
                    return null;
            }
        }

        private static TowerModel Get(int level, string display, string portrait, float range, float attackSpeed, float tickSpeed, bool canPopFrozen, bool canPopLeads, bool canSeeCamo,
                                      int damage, int regrowDamage, int ceramMoabDamage, string[] appliedUpgrades, UpgradePathModel upgrade,
                                      AbilityModel[] abilities) {
            System.Collections.Generic.List<Model> behaviors = new System.Collections.Generic.List<Model>() {
                new HeroModel("", 1, 1),
                new CreateEffectOnPlaceModel(Name, new EffectModel("PlacementEffect", "708be580149dc444d89314b6d9f388e9",
                                                1, 6, false, false, false, false, false, false, false)),
                new CreateSoundOnTowerPlaceModel(Name, new SoundModel("PlaceSound1", "0782b04f742eb2542bc8266a38241c5b"),
                                                new SoundModel("PlaceSound2", "80580fcd99708624ab85e37036ca124c"),
                                                new SoundModel("PlaceSound1", "0782b04f742eb2542bc8266a38241c5b"),
                                                new SoundModel("PlaceSound2", "80580fcd99708624ab85e37036ca124c")),
                new CreateSoundOnSellModel(Name, new SoundModel("SellSound", "df4c691ccf8a3c7408ed6f156740c8b7")),
                new CreateSoundOnUpgradeModel(Name, new SoundModel("UpgradeSound", "23a777b9950195f4f8f4bf745ef84031"),
                                                new SoundModel("UpgradeSound", "23a777b9950195f4f8f4bf745ef84031"),
                                                new SoundModel("UpgradeSound", "23a777b9950195f4f8f4bf745ef84031"),
                                                new SoundModel("UpgradeSound", "23a777b9950195f4f8f4bf745ef84031"),
                                                new SoundModel("UpgradeSound", "23a777b9950195f4f8f4bf745ef84031"),
                                                new SoundModel("UpgradeSound", "23a777b9950195f4f8f4bf745ef84031"),
                                                new SoundModel("UpgradeSound", "23a777b9950195f4f8f4bf745ef84031"),
                                                new SoundModel("UpgradeSound", "23a777b9950195f4f8f4bf745ef84031"),
                                                new SoundModel("UpgradeSound", "23a777b9950195f4f8f4bf745ef84031")),
                new CreateEffectOnSellModel(Name, new EffectModel("SellEffect", "b1dec8a8d79593843bc2e2410eae944e",
                                            1, 1, false, false, false, false, false, false, false)),
                new CreateEffectOnUpgradeModel(Name, new EffectModel("UpgradeEffect", "9e800b22f83685141bcb398b78331495",
                                                1, 6, false, false, false, false, false, false, false)),
                GetAttack(range, attackSpeed, tickSpeed, canPopFrozen, canPopLeads, canSeeCamo, damage, regrowDamage, ceramMoabDamage,
                    new SingleEmissionModel("", new EmissionBehaviorModel[] {
                        new EmissionRotationOffBloonDirectionModel("", false, false)
                    })),
                new DisplayModel(Name, display, -1, NKVector3.zero, 1, false, 0)
            };

            if (!(abilities is null))
                behaviors.AddRange(abilities);

            TowerModel beekeeper = new TowerModel(name: level == 1 ? Name : $"{Name} {level}",
                baseId: Name,
                towerSet: "Hero",
                display: display,
                cost: 500,
                radius: Radius,
                range: range,
                ignoreBlockers: true,
                isGlobalRange: false,
                tier: level == 1 ? 0 : level,
                tiers: new int[] { level == 1 ? 0 : level, 0, 0 },
                appliedUpgrades: appliedUpgrades,
                upgrades: upgrade is null ? new UpgradePathModel[0] : new UpgradePathModel[] { upgrade },
                paragonUpgrade: null,
                behaviors: behaviors.ToArray(),
                areaTypes: new AreaType[] { AreaType.land },
                icon: new SpriteReference(portrait),
                portrait: new SpriteReference(portrait),
                instaIcon: null,
                mods: new ApplyModModel[] {
                    new ApplyModModel("BeekeeperKnowledge", "HeroicReach", ""),
                    new ApplyModModel("BeekeeperKnowledge", "HeroicVelocity", ""),
                    new ApplyModModel("BeekeeperKnowledge", "EmpoweredHeroes", ""),
                    new ApplyModModel("BeekeeperKnowledge", "BigBloonBlueprints", BeeProjectileId),
                    new ApplyModModel("BeekeeperKnowledge", "BetterSellDeals", ""),
                    new ApplyModModel("BeekeeperKnowledge", "MonkeyEducation", ""),
                    new ApplyModModel("BeekeeperKnowledge", "VeteranMonkeyTraining", ""),
                    new ApplyModModel("BeekeeperKnowledge", "GlobalAbilityCooldowns", ""),
                    new ApplyModModel("BeekeeperKnowledge", "QuickHands", ""),
                    new ApplyModModel("BeekeeperKnowledge", "Scholarships", ""),
                    new ApplyModModel("BeekeeperKnowledge", "SelfTaughtHeroes", ""),
                    new ApplyModModel("BeekeeperKnowledge", "AbilityDiscipline", Level10AbilityId),
                    new ApplyModModel("BeekeeperKnowledge", "WeakPoint", "")
                },
                ignoreTowerForSelection: false,
                isSubTower: false,
                isBakable: true,
                footprint: new CircleFootprintModel("Circle Footprint", Radius, false, false, false),
                dontDisplayUpgrades: false,
                powerName: null,
                animationSpeed: 1,
                emoteSpriteSmall: new SpriteReference(IconName),
                emoteSpriteLarge: new SpriteReference(IconName),
                doesntRotate: false,
                showPowerTowerBuffs: false,
                towerSelectionMenuThemeId: "Default",
                ignoreCoopAreas: false,
                canAlwaysBeSold: false,
                isParagon: false,
                sellbackModifierAdd: 0);
            return beekeeper;
        }
    }
}
