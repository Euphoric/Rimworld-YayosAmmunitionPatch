using System;
using System.Linq;
using JetBrains.Annotations;

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

            var ammoPerMinute = (int)Math.Round(shotsPerMinute * ammoPerShot);
            return new GunAmmoSetting(averageAccuracy, armorPenetrationRating, effectiveDamage, ammoPerShot, shotsPerMinute, ammoPerMinute);
        }

        private static double GetDamageToAmmoScale(AmmoType ammoType)
        {
            switch (ammoType)
            {
                case AmmoType.Industrial:
                    return 10;
                case AmmoType.Spacer:
                    return 3;
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