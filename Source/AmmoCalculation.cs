using System;
using System.Linq;

namespace Euphoric.YayosAmmunitionPatch
{
    public static class AmmoCalculation
    {
        public static GunAmmoSetting CalculateGunAmmoParameters(GunParameter parameter)
        {
            var averageAccuracy = Enumerable.Range(3, 60).Select(parameter.AccuracyAt).Average();
            
            var armorPenetrationRating = EstimatedArmorPenetrationRating(parameter.ArmorPenetration);

            var effectiveDamage = parameter.BaseDamage * averageAccuracy * armorPenetrationRating;
            var ammoPerShot = (int)Math.Max(1, Math.Round(effectiveDamage * GetDamageToAmmoScale(parameter.AmmoType)));

            var shotsPerMinute = 60 / (parameter.Warmup + parameter.Cooldown + parameter.SecondsBetweenBurstShots * parameter.Burst) * parameter.Burst;
            shotsPerMinute = Math.Round(shotsPerMinute, 2);

            var ammoPerMinute = shotsPerMinute * ammoPerShot;
            var gunShots = (int)Math.Round(shotsPerMinute * 1.5);
            return new GunAmmoSetting(averageAccuracy, armorPenetrationRating, effectiveDamage, ammoPerShot, shotsPerMinute, ammoPerMinute, gunShots);
        }

        private static double GetDamageToAmmoScale(AmmoType ammoType)
        {
            switch (ammoType)
            {
                case AmmoType.Industrial:
                    return 10;
                case AmmoType.IndustrialFire:
                    return 5;
                case AmmoType.IndustrialSpecial:
                    return 2;
                case AmmoType.Spacer:
                    return 4;
                case AmmoType.SpacerFire:
                    return 3;
                case AmmoType.SpacerSpecial:
                    return 1.5;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ammoType), ammoType, null);
            }
        }

        private static double EstimatedArmorPenetrationRating(double armorPiercing)
        {
            if (armorPiercing <= 0.10) // 0.10 => 0.025
            {
                return 0.025;
            }

            if (armorPiercing <= 0.65) // 0.65 => 0.8
            {
                return (armorPiercing - 0.10) / (0.65 - 0.10) * (0.8 - 0.025) + 0.025;
            }

            if (armorPiercing <= 1.00) // 1.00 => 0.95
            {
                return (armorPiercing - 0.65) / (1.00 - 0.65) * (0.95 - 0.8) + 0.8;
            }

            if (armorPiercing <= 1.60) // 160 => 1
            {
                return (armorPiercing - 1.00) / (1.60 - 1.00) * (1.0 - 0.95) + 0.95;
            }

            return 1;
        }
    }
}