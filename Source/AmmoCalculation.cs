using System;
using System.Linq;

namespace Euphoric.YayosAmmunitionPatch
{
    public static class AmmoCalculation
    {
        public static GunAmmoSetting CalculateGunAmmoParameters(GunParameter parameter)
        {
            var averageAccuracy = AverageAccuracy(parameter);
            var armorPenetrationRating = AverageArmorPenetrationRating(parameter.ArmorPenetration);

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
                    return 6;
                case AmmoType.IndustrialFire:
                    return 3.5;
                case AmmoType.IndustrialSpecial:
                    return 1.5;
                case AmmoType.Spacer:
                    return 3;
                case AmmoType.SpacerFire:
                    return 2;
                case AmmoType.SpacerSpecial:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ammoType), ammoType, null);
            }
        }

        public static double AverageArmorPenetrationRating(double armorPiercing)
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
        
        private static double RangeAccuracyBias(double range)
        {
            if (range <= 3)
            {
                return 0;
            }
            
            if (range <= 12)
            {
                return (range - 0) / (12 - 3) * (1.0 - 0.75) + 0.75;
            }

            if (range <= 25)
            {
                return (range - 12) / (25 - 12) * (1.0 - 1.0) + 1.0;
            }

            if (range <= 38)
            {
                return (range - 25) / (38 - 25) * (1.0 - 0.2) + 0.2;
            }
            
            if (range <= 60)
            {
                return (range - 38) / (60 - 38) * (0 - 0.2) + 0.2;
            }

            return 0;
        }

        private static readonly Lazy<double> RangeAccuracyBiasTotal = new Lazy<double>(()=>Enumerable.Range(3,60).Select(rg=>RangeAccuracyBias(rg)).Sum());
        
        /// <summary>
        /// Calculates average accuracy of a weapon. With bias towards medium-range and against long-range.
        /// </summary>
        private static double AverageAccuracy(GunParameter parameter)
        {
            var averageAccuracy = Enumerable.Range(3, 60).Select(rg=>parameter.AccuracyAt(rg)*RangeAccuracyBias(rg)).Sum();
            return averageAccuracy / RangeAccuracyBiasTotal.Value;
        }
    }
}