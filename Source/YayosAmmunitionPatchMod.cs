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
            StringBuilder patchedWeaponsLogMessage = new StringBuilder("Patched weapons:");
            StringBuilder ignoredWeaponsLogMessage = new StringBuilder("Ignored weapons:");
            
            patchedWeaponsLogMessage.AppendLine($"Def name;Ammo type;Damage def;Damage armor category;Explosion radius;Cooldown time;Warmup time;Burst shot count;Seconds between burst;Base damage;Armor penetration;Range;Accuracy touch (3);Accuracy short (12);Accuracy medium (25);Accuracy long (40);;Shots per minute;Average accuracy;Armor penetration rating;EffectiveDamage;Ammo per shot;Ammo per minute");
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(t => t.HasComp(typeof(CompReloadable))))
            {
                var props = thingDef.GetCompProperties<CompProperties_Reloadable>();
                VerbProperties verb = thingDef.Verbs[0];
                var projectile = verb?.defaultProjectile?.projectile;
                var damageDef = projectile?.damageDef;
                
                var ammoType = GetLocalAmmoType(props.ammoDef);
                if (ammoType != AmmoType.Unknown && projectile != null)
                {
                    var secondsBetweenBurstShots = verb.ticksBetweenBurstShots / 60f;
                    var cooldownTime = thingDef.statBases.GetStatValueFromList(StatDefOf.RangedWeapon_Cooldown, 0.0f);
                    var baseDamage = projectile.GetDamageAmount(1);
                    var accuracyTouch = thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyTouch, 0.0f);
                    var accuracyShort = thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyShort, 0.0f);
                    var accuracyMedium = thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyMedium, 0.0f);
                    var accuracyLong = thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyLong, 0.0f);
                    var armorPenetration = projectile.GetArmorPenetration(1);

                    GunParameter gunParameter = new GunParameter(
                        thingDef.defName, ammoType,
                        verb.warmupTime, cooldownTime, secondsBetweenBurstShots,
                        verb.burstShotCount,
                        baseDamage, armorPenetration, verb.range, accuracyTouch, accuracyShort, accuracyMedium,
                        accuracyLong);

                    var gunAmmoSetting = AmmoCalculation.CalculateGunAmmoParameters(gunParameter);

                    patchedWeaponsLogMessage.AppendLine($"{thingDef.defName};{props.ammoDef.defName};{damageDef?.defName};{damageDef?.armorCategory?.defName};{projectile.explosionRadius};{cooldownTime};{verb.warmupTime};{verb.burstShotCount};{secondsBetweenBurstShots};{baseDamage};{armorPenetration};{verb.range};{accuracyTouch};{accuracyShort};{accuracyMedium};{accuracyLong};;{gunAmmoSetting.ShotsPerMinute};{gunAmmoSetting.AverageAccuracy};{gunAmmoSetting.ArmorPenetrationRating};{gunAmmoSetting.EffectiveDamage};{gunAmmoSetting.AmmoPerShot};{gunAmmoSetting.AmmoPerMinute}");

                    props.ammoCountPerCharge = gunAmmoSetting.AmmoPerShot;
                    props.maxCharges = gunAmmoSetting.GunShots; // TODO account for yayo's settings
                }
                else
                {
                    ignoredWeaponsLogMessage.AppendLine($"{thingDef.defName};{props.ammoDef?.defName};{projectile?.ToString() ?? ""}");
                }
            }

            Logger.Warning(patchedWeaponsLogMessage.ToString());
            Logger.Warning(ignoredWeaponsLogMessage.ToString());
        }

        private AmmoType GetLocalAmmoType(ThingDef ammoDef)
        {
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
    }
}