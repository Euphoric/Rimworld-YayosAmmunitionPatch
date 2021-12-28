﻿using System;
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
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Patched weapons:");
            sb.AppendLine($"Def name;Ammo type;Cooldown time;Warmup time;Burst shot count;Seconds between burst;Base damage;Armor penetration;Range;Accuracy touch (3);Accuracy short (12);Accuracy medium (25);Accuracy long (40);;Shots per minute;Average accuracy;Armor penetration rating;EffectiveDamage;Ammo per shot;Ammo per minute");
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(t => t.HasComp(typeof(CompReloadable))))
            {
                var props = thingDef.GetCompProperties<CompProperties_Reloadable>();
                if (props.ammoDef == ThingDef.Named("yy_ammo_industrial"))
                {
                    VerbProperties verb = thingDef.Verbs[0];
                    var secondsBetweenBurstShots = verb.ticksBetweenBurstShots / 60f;
                    var projectile = verb.defaultProjectile.projectile;
                    var cooldownTime = thingDef.statBases.GetStatValueFromList(StatDefOf.RangedWeapon_Cooldown, 0.0f);
                    var baseDamage = projectile.GetDamageAmount(1);
                    var accuracyTouch = thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyTouch, 0.0f);
                    var accuracyShort = thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyShort, 0.0f);
                    var accuracyMedium = thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyMedium, 0.0f);
                    var accuracyLong = thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyLong, 0.0f);
                    var armorPenetration = projectile.GetArmorPenetration(1);

                    GunParameter gunParameter = new GunParameter(
                        thingDef.defName,
                        verb.warmupTime, cooldownTime, secondsBetweenBurstShots,
                        verb.burstShotCount,
                        baseDamage, armorPenetration, verb.range, accuracyTouch, accuracyShort, accuracyMedium,
                        accuracyLong);

                    var gunAmmoSetting = AmmoCalculation.CalculateGunAmmoParameters(gunParameter);

                    sb.AppendLine($"{thingDef.defName};{props.ammoDef.defName};{cooldownTime};{verb.warmupTime};{verb.burstShotCount};{secondsBetweenBurstShots};{baseDamage};{armorPenetration};{verb.range};{accuracyTouch};{accuracyShort};{accuracyMedium};{accuracyLong};;{gunAmmoSetting.ShotsPerMinute};{gunAmmoSetting.AverageAccuracy};{gunAmmoSetting.ArmorPenetrationRating};{gunAmmoSetting.EffectiveDamage};{gunAmmoSetting.AmmoPerShot};{gunAmmoSetting.AmmoPerMinute}");

                    props.ammoCountPerCharge = gunAmmoSetting.AmmoPerShot;
                    props.maxCharges = gunAmmoSetting.AmmoPerMinute; // TODO account for yayo's settings, maybe increase from 60s to 90s
                }
            }

            Logger.Warning(sb.ToString());
        }
    }
}