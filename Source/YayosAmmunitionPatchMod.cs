using System;
using System.Linq;
using System.Text;
using HugsLib;
using RimWorld;
using Verse;

namespace Euphoric.YayosAmmunitionPatch
{
    public class YayosAmmunitionPatchMod : ModBase
    {
        public override string ModIdentifier => "Euphoric.YayosAmmunitionPatch";

        public override void DefsLoaded()
        {
            StringBuilder patchedWeaponsLogMessage = new StringBuilder();
            patchedWeaponsLogMessage.AppendLine("Patched weapons:");
            StringBuilder ignoredWeaponsLogMessage = new StringBuilder();
            ignoredWeaponsLogMessage.AppendLine("Ignored weapons:");

            patchedWeaponsLogMessage.AppendLine(
                $"Def name;Label;Mod;Ammo type;Damage def;Damage armor category;Projectile class;Explosion radius;Explosion spawn;Cooldown time;Warmup time;Burst shot count;Seconds between burst;Base damage;Armor penetration;Range;Accuracy touch (3);Accuracy short (12);Accuracy medium (25);Accuracy long (40);Forced miss radius;;Shots per minute;Average accuracy;Armor penetration rating;Explosion rating;Effective damage;Ammo per shot;Ammo per minute;Cost per shot;Cost per full;Ingredients per shot");
            var reloadableThings = 
                DefDatabase<ThingDef>.AllDefs.Where(t => t.HasComp(typeof(CompReloadable)));
            foreach (ThingDef thingDef in reloadableThings)
            {
                var props = thingDef.GetCompProperties<CompProperties_Reloadable>();
                VerbProperties verb = thingDef.Verbs[0];
                var defaultProjectile = verb?.defaultProjectile;
                var projectileParams = defaultProjectile?.projectile;
                var damageDef = projectileParams?.damageDef;

                var ammoType = GetLocalAmmoType(props.ammoDef);
                if (ammoType != AmmoType.Unknown && projectileParams != null)
                {
                    var secondsBetweenBurstShots = verb.ticksBetweenBurstShots / 60f;
                    var cooldownTime = thingDef.statBases.GetStatValueFromList(StatDefOf.RangedWeapon_Cooldown, 0.0f);
                    var baseDamage = projectileParams.GetDamageAmount(1);
                    var accuracyTouch = thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyTouch, 0.0f);
                    var accuracyShort = thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyShort, 0.0f);
                    var accuracyMedium = thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyMedium, 0.0f);
                    var accuracyLong = thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyLong, 0.0f);
                    var armorPenetration = projectileParams.GetArmorPenetration(1);
                    var leavesBehind = GetLeavesBehind(projectileParams.preExplosionSpawnThingDef, projectileParams.postExplosionSpawnThingDef);
                    
                    GunParameter gunParameter = new GunParameter(
                        thingDef.defName, ammoType,
                        verb.warmupTime, cooldownTime, secondsBetweenBurstShots,
                        verb.burstShotCount,
                        baseDamage, armorPenetration, 
                        verb.range, accuracyTouch, accuracyShort, accuracyMedium, accuracyLong,
                        verb.ForcedMissRadius, projectileParams.explosionRadius, leavesBehind);

                    var gunAmmoSetting =
                        AmmoCalculation.CalculateGunAmmoParameters(gunParameter, yayoCombat.yayoCombat.maxAmmo);

                    var leavesBehindStr = $"{projectileParams.preExplosionSpawnThingDef}|{projectileParams.postExplosionSpawnThingDef}";

                    props.ammoCountPerCharge = gunAmmoSetting.AmmoPerShot;
                    props.maxCharges = gunAmmoSetting.GunShots;

                    var statsExtension = new CompProperties_ReloadableDisplayStatsExtension();
                    statsExtension.CompReloadableProperties = props;
                    statsExtension.AmmoRecipe = GetAmmoRecipe(props.ammoDef);
                    thingDef.comps.Add(statsExtension);

                    var usageCost = statsExtension.CalculateUsageCost();
                    
                    patchedWeaponsLogMessage.AppendLine(
                        $"{thingDef.defName};{thingDef.label};{thingDef.modContentPack?.Name};{props.ammoDef?.defName};{damageDef?.defName};{damageDef?.armorCategory?.defName};{defaultProjectile.thingClass?.Name};{projectileParams.explosionRadius};{leavesBehindStr};{cooldownTime};{verb.warmupTime};{verb.burstShotCount};{secondsBetweenBurstShots};{baseDamage};{armorPenetration};{verb.range};{accuracyTouch};{accuracyShort};{accuracyMedium};{accuracyLong};{verb.ForcedMissRadius};;{gunAmmoSetting.ShotsPerMinute};{gunAmmoSetting.AverageAccuracy};{gunAmmoSetting.ArmorPenetrationRating};{gunAmmoSetting.ExplosionRating};{gunAmmoSetting.EffectiveDamage};{gunAmmoSetting.AmmoPerShot};{gunAmmoSetting.AmmoPerMinute};{usageCost.CostPerShot};{usageCost.CostPerFull};{string.Join("|", usageCost.IngredientsPerShot)}");
                }
                else
                {
                    ignoredWeaponsLogMessage.AppendLine(
                        $"{thingDef.defName};{props.ammoDef?.defName};{projectileParams?.ToString() ?? ""}");
                }
            }

            Logger.Message(patchedWeaponsLogMessage.ToString());
            //Logger.Message(ignoredWeaponsLogMessage.ToString());
        }

        private RecipeDef GetAmmoRecipe(ThingDef ammoDef)
        {
            var ammoRecipe = DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(rec => rec.products.FirstOrDefault()?.thingDef == ammoDef);
            if (ammoRecipe == null)
            {
                throw new Exception("Didn't find recipe for " + ammoDef.defName);
            }

            return ammoRecipe;
        }

        private AmmoType GetLocalAmmoType(ThingDef ammoDef)
        {
            if (ammoDef == ThingDef.Named("yy_ammo_primitive"))
            {
                return AmmoType.Primitive;
            }

            if (ammoDef == ThingDef.Named("yy_ammo_primitive_fire"))
            {
                return AmmoType.PrimitiveFire;
            }

            if (ammoDef == ThingDef.Named("yy_ammo_primitive_emp"))
            {
                return AmmoType.PrimitiveSpecial;
            }
            
            if (ammoDef == ThingDef.Named("yy_ammo_industrial"))
            {
                return AmmoType.Industrial;
            }

            if (ammoDef == ThingDef.Named("yy_ammo_industrial_fire"))
            {
                return AmmoType.IndustrialFire;
            }

            if (ammoDef == ThingDef.Named("yy_ammo_industrial_emp"))
            {
                return AmmoType.IndustrialSpecial;
            }

            if (ammoDef == ThingDef.Named("yy_ammo_spacer"))
            {
                return AmmoType.Spacer;
            }

            if (ammoDef == ThingDef.Named("yy_ammo_spacer_fire"))
            {
                return AmmoType.SpacerFire;
            }

            if (ammoDef == ThingDef.Named("yy_ammo_spacer_emp"))
            {
                return AmmoType.SpacerSpecial;
            }

            return AmmoType.Unknown;
        }
        
        private LeavesBehind GetLeavesBehind(ThingDef preSpawnThingDef, ThingDef postSpawnThingDef)
        {
            if ((preSpawnThingDef?.defName?.Contains("Fuel") | postSpawnThingDef?.defName?.Contains("Fuel"))==true)
            {
                return LeavesBehind.Fuel;
            }
            if ((preSpawnThingDef?.defName?.Contains("Smoke") | postSpawnThingDef?.defName?.Contains("Smoke"))==true)
            {
                return LeavesBehind.Smoke;
            }
            if ((preSpawnThingDef?.defName?.Contains("Gas") | postSpawnThingDef?.defName?.Contains("Gas"))==true)
            {
                return LeavesBehind.Gas;
            }

            return LeavesBehind.Nothing;
        }
    }
}