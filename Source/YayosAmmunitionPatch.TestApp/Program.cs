using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Euphoric.YayosAmmunitionPatch
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            ArmorPiercingRatingChart();
            CalculateSampleGuns();
            
            Console.WriteLine("Done.");
        }

        private static void CalculateSampleGuns()
        {
            var parameters = new GunParameter[]
            {
                new GunParameter("Revolver", AmmoType.Industrial, 0.3, 1.6, 0.25, 1, 12, 0.18, 26, 0.80, 0.75, 0.45,
                    0.35),
                new GunParameter("Bolt-action rifle", AmmoType.Industrial, 1.7, 1.5, 0.25, 1, 18, 0.17, 37, 0.65, 0.80,
                    0.90, 0.80),
                new GunParameter("Autopistol", AmmoType.Industrial, 0.3, 1, 0.25, 1, 10, 0.15, 26, 0.80, 0.70, 0.40,
                    0.30),
                new GunParameter("Heavy SMG", AmmoType.Industrial, 0.9, 1.65, 0.1833333, 3, 12, 0.18, 23, 0.85, 0.65,
                    0.35, 0.20),
                new GunParameter("Militia Rifle", AmmoType.Industrial, 1.2, 1.8, 0.2, 3, 13, 0.20, 27, 0.55, 0.65, 0.60,
                    0.50),
                new GunParameter("Assault Rifle", AmmoType.Industrial, 1, 1.7, 1 / 6d, 3, 11, 0.19, 31, 0.60, 0.70,
                    0.65, 0.55),
                new GunParameter("Sniper Rifle", AmmoType.Industrial, 3.5, 2.3, 0.25, 1, 25, 0.38, 45, 0.50, 0.70, 0.86,
                    0.88),
                new GunParameter("Anti-Materiel Rifle", AmmoType.Industrial, 5, 4, 0.25, 1, 45, 0.68, 55, 0.40, 0.55,
                    0.88, 0.95),
                new GunParameter("Minigun", AmmoType.Industrial, 2.5, 2.3, 1 / 12d, 25, 10, 0.15, 31, 0.15, 0.25, 0.25,
                    0.18),
            };

            using (StreamWriter sw = new StreamWriter($"gun_samples.csv"))
            {
                sw.WriteLine($"Gun;Average accuracy;Armor piercing rating;Effective damage;Ammo use per shot;Shots per minute;Ammo use per minute");
                foreach (var parameter in parameters)
                {
                    var calculation = AmmoCalculation.CalculateGunAmmoParameters(parameter, 1);
                    sw.WriteLine(
                        $"{parameter.Name};{calculation.AverageAccuracy};{calculation.ArmorPenetrationRating};{calculation.EffectiveDamage};{calculation.AmmoPerShot};{calculation.ShotsPerMinute};{calculation.AmmoPerMinute}");
                }
            }
        }

        public static double YayoDamageReduction(double armorPenetration, double armorRating, float armorDurability)
        {
            var armorSettings = 0.1f;

            double armorRatingA = Math.Max(armorRating - armorPenetration, 0.0f);
            double armorRatingB = Math.Max(armorRating * 0.9f - armorPenetration, 0.0f);

            var bluntDamageRate = Math.Max(0, Math.Min(1, (armorPenetration * 5 - armorRating * 0.15 * 5)));
            float armorEfficiency = 1f - armorSettings;

            var primaryArmorHitChance = Math.Min(1, armorRatingB * armorDurability / armorEfficiency);
            var fullArmorReflectionChance = Math.Min(armorRatingA, 0.9f);
            var secondaryArmorHitChance = Math.Min(1, armorRatingA * (0.5 + armorDurability * 0.5));

            var a = primaryArmorHitChance * fullArmorReflectionChance * 0;
            var b = primaryArmorHitChance * (1 - fullArmorReflectionChance) * (0.25 + bluntDamageRate * 0.25);
            var c = Math.Max(0, secondaryArmorHitChance - primaryArmorHitChance) * 0.5;
            var d = 1 - Math.Max(secondaryArmorHitChance, primaryArmorHitChance) * 1;

            return a + b + c + d;
        }
        
        public static double VanillaDamageReduction(float armorPenetration, float armorRating)
        {
            var a = Math.Min(Math.Max(armorRating - armorPenetration, 0), 1) * 0.5;
            var b = Math.Min(Math.Max(armorRating - armorPenetration, 0) / 2, 1) * 0.5;
            return 1 - (a + b);
        }

        private static double ArmorRatingDistribution(double armorRating)
        {
            if (armorRating <= 0.00)
            {
                return 0.0;
            }

            if (armorRating <= 0.20)
            {
                return (armorRating - 0.00) / (0.20 - 0.00) * (1.00 - 0.00) + 0.00;
            }

            if (armorRating <= 0.70)
            {
                return (armorRating - 0.20) / (0.70 - 0.20) * (1.00 - 1.00) + 1.00;
            }

            if (armorRating <= 1.00)
            {
                return (armorRating - 0.70) / (1.00 - 0.70) * (0.3 - 1.00) + 1.00;
            }
            
            if (armorRating <= 2.00)
            {
                return (armorRating - 1.00) / (2.00 - 1.00) * (0.03 - 0.30) + 0.30;
            }

            return 0.03;
        }
        
        private static double CalculateArmorPiercingRating(double armorPenetration)
        {
            var armorRange = Enumerable.Range(0, 200).Select(x => x / 100d).ToList();
            
            var armorSamples = armorRange.Select(x=>new {Weight = ArmorRatingDistribution(x), ArmorRating=x}).ToList();
            var totalWeight = armorSamples.Sum(x => x.Weight);
            var samples = armorSamples.Select(x => x.Weight * YayoDamageReduction(armorPenetration, x.ArmorRating, 1)).ToList();
            return samples.Sum() / totalWeight;
        }
        
        private static void ArmorPiercingRatingChart()
        {
            using (StreamWriter sw = new StreamWriter($"armor_piercing_rating.csv"))
            {
                sw.WriteLine("Armor penetration;Armor distribution;Rating;Probabilistic");
                for (int ap = 0; ap <= 200; ap += 1)
                {
                    var armorPenetration = ap / 100d;
                    sw.WriteLine($"{armorPenetration};{ArmorRatingDistribution(armorPenetration)};{AmmoCalculation.AverageArmorPenetrationRating(armorPenetration)};{CalculateArmorPiercingRating(armorPenetration)}");
                }
            }
        }
    }
}