namespace Euphoric.YayosAmmunitionPatch
{
    public class GunAmmoSetting
    {
        public GunAmmoSetting(float averageAccuracy, double armorPenetrationRating, double effectiveDamage, int ammoPerShot,
            double shotsPerMinute, int ammoPerMinute)
        {
            AverageAccuracy = averageAccuracy;
            ArmorPenetrationRating = armorPenetrationRating;
            EffectiveDamage = effectiveDamage;
            AmmoPerShot = ammoPerShot;
            ShotsPerMinute = shotsPerMinute;
            AmmoPerMinute = ammoPerMinute;
        }

        public float AverageAccuracy { get; }
        public double ArmorPenetrationRating { get; }

        public double EffectiveDamage { get; }
        public int AmmoPerShot { get; }
        public double ShotsPerMinute { get; }
        public int AmmoPerMinute { get; }
    }
}