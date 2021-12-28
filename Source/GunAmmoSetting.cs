namespace Euphoric.YayosAmmunitionPatch
{
    public class GunAmmoSetting
    {
        public GunAmmoSetting(float averageAccuracy, double armorPenetrationRating, double effectiveDamage, int ammoPerShot,
            double shotsPerMinute, double ammoPerMinute, int gunShots)
        {
            AverageAccuracy = averageAccuracy;
            ArmorPenetrationRating = armorPenetrationRating;
            EffectiveDamage = effectiveDamage;
            AmmoPerShot = ammoPerShot;
            ShotsPerMinute = shotsPerMinute;
            AmmoPerMinute = ammoPerMinute;
            GunShots = gunShots;
        }

        public float AverageAccuracy { get; }
        public double ArmorPenetrationRating { get; }

        public double EffectiveDamage { get; }
        public int AmmoPerShot { get; }
        public double ShotsPerMinute { get; }
        public double AmmoPerMinute { get; }
        public int GunShots { get; }
    }
}