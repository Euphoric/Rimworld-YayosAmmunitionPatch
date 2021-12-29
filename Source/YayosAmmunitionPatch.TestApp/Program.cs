using System;
using System.Globalization;

namespace Euphoric.YayosAmmunitionPatch
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var parameters = new GunParameter[]
            {
                new GunParameter("Revolver", AmmoType.Industrial, 0.3, 1.6, 0.25, 1, 12, 0.18, 26, 0.80, 0.75, 0.45, 0.35),
                new GunParameter("Bolt-action rifle", AmmoType.Industrial, 1.7, 1.5, 0.25, 1, 18, 0.17, 37, 0.65, 0.80, 0.90, 0.80),
                new GunParameter("Autopistol", AmmoType.Industrial, 0.3, 1, 0.25, 1, 10, 0.15, 26, 0.80, 0.70, 0.40, 0.30),
                new GunParameter("Heavy SMG", AmmoType.Industrial, 0.9, 1.65, 0.1833333, 3, 12, 0.18, 23, 0.85, 0.65, 0.35, 0.20),
                new GunParameter("Militia Rifle", AmmoType.Industrial, 1.2, 1.8, 0.2, 3, 13, 0.20, 27, 0.55, 0.65, 0.60, 0.50),
                new GunParameter("Assault Rifle", AmmoType.Industrial, 1, 1.7, 1/6d, 3, 11, 0.19, 31, 0.60, 0.70, 0.65, 0.55),
                new GunParameter("Sniper Rifle", AmmoType.Industrial, 3.5, 2.3, 0.25, 1, 25, 0.38, 45, 0.50, 0.70, 0.86, 0.88),
                new GunParameter("Anti-Materiel Rifle", AmmoType.Industrial, 5, 4, 0.25, 1, 45, 0.68, 55, 0.40, 0.55, 0.88, 0.95),
                new GunParameter("Minigun", AmmoType.Industrial, 2.5, 2.3, 1/12d, 25, 10, 0.15, 31, 0.15, 0.25, 0.25, 0.18),
            };

            Console.WriteLine(
                $"Gun;Average accuracy;Armor piercing rating;Effective damage;Ammo use per shot;Shots per minute;Ammo use per minute");
            foreach (var parameter in parameters)
            {
                var calculation = AmmoCalculation.CalculateGunAmmoParameters(parameter);
                Console.WriteLine($"{parameter.Name};{calculation.AverageAccuracy};{calculation.ArmorPenetrationRating};{calculation.EffectiveDamage};{calculation.AmmoPerShot};{calculation.ShotsPerMinute};{calculation.AmmoPerMinute}");
            }
        }
    }
}