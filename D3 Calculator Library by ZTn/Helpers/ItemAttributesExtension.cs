﻿using System;
using System.Reflection;
using ZTn.BNet.D3.Items;

namespace ZTn.BNet.D3.Calculator.Helpers
{
    /// <summary>
    /// Extension class to be used with <see cref="ItemAttributes"/> objects
    /// </summary>
    public static class ItemAttributesExtension
    {
        private static readonly TypeInfo ItemAttributesTypeInfo = typeof(ItemAttributes).GetTypeInfo();

        #region >> Reflection Helpers

        /// <summary>
        /// Returns the value of an attribute of an item given the attribute's name
        /// </summary>
        /// <param name="itemAttributes">Source attributes</param>
        /// <param name="fieldName">Name of the attribute to retrieve</param>
        /// <returns></returns>
        public static ItemValueRange GetAttributeByName(this ItemAttributes itemAttributes, String fieldName)
        {
            return (ItemValueRange)ItemAttributesTypeInfo.GetDeclaredField(fieldName).GetValue(itemAttributes);
        }

        /// <summary>
        /// Sets the value of an attribute of an ItemAttributes given the attribute's name
        /// </summary>
        /// <param name="itemAttributes">Source attributes</param>
        /// <param name="fieldName">Name of the attribute to retrieve</param>
        /// <param name="value">Value to set</param>
        public static ItemAttributes SetAttributeByName(this ItemAttributes itemAttributes, string fieldName, ItemValueRange value)
        {
            ItemAttributesTypeInfo.GetDeclaredField(fieldName).SetValue(itemAttributes, value);

            return itemAttributes;
        }

        #endregion

        /// <summary>
        /// Computes armor brought by an item.
        /// </summary>
        /// <param name="itemAttr">Attributes of an item.</param>
        /// <returns></returns>
        public static ItemValueRange GetArmor(this ItemAttributes itemAttr)
        {
            return itemAttr.armorItem + itemAttr.armorBonusItem;
        }

        /// <summary>
        /// Computes damages other than weapon damages (on rings, amulets, ...).
        /// </summary>
        /// <param name="itemAttr">Attributes of an item.</param>
        /// <returns></returns>
        public static ItemValueRange GetRawAverageBonusDamage(this ItemAttributes itemAttr)
        {
            // formula: ( min + max ) / 2
            return (itemAttr.GetRawBonusDamageMin() + itemAttr.GetRawBonusDamageMax()) / 2;
        }

        #region >> getRawBonusDamageMin *

        /// <summary>
        /// Computes min damage of the item.
        /// </summary>
        /// <param name="itemAttr">Attributes of an item.</param>
        /// <returns></returns>
        public static ItemValueRange GetRawBonusDamageMin(this ItemAttributes itemAttr)
        {
            return itemAttr.GetRawBonusDamageMin("Arcane") + itemAttr.GetRawBonusDamageMin("Cold")
                   + itemAttr.GetRawBonusDamageMin("Fire") + itemAttr.GetRawBonusDamageMin("Holy")
                   + itemAttr.GetRawBonusDamageMin("Lightning") + itemAttr.GetRawBonusDamageMin("Physical")
                   + itemAttr.GetRawBonusDamageMin("Poison");
        }

        /// <summary>
        /// Computes min damage of the item (based on a specific resist).
        /// </summary>
        /// <param name="itemAttr">Attributes of an item.</param>
        /// <param name="resist">Name of the resist.</param>
        /// <param name="useDamageTypePercentBonus"></param>
        /// <returns></returns>
        public static ItemValueRange GetRawBonusDamageMin(this ItemAttributes itemAttr, String resist, bool useDamageTypePercentBonus = true)
        {
            var result = itemAttr.GetAttributeByName("damageMin_" + resist) + itemAttr.GetAttributeByName("damageBonusMin_" + resist);

            if (useDamageTypePercentBonus && resist != "Physical")
            {
                result += itemAttr.GetRawBonusDamageMin("Physical") * itemAttr.GetAttributeByName("damageTypePercentBonus_" + resist);
            }

            return result;
        }

        #endregion

        #region >> getRawBonusDamageMax *

        /// <summary>
        /// Computes max damage of the item.
        /// </summary>
        /// <param name="itemAttr">Attributes of an item.</param>
        /// <returns></returns>
        public static ItemValueRange GetRawBonusDamageMax(this ItemAttributes itemAttr)
        {
            return itemAttr.GetRawBonusDamageMax("Arcane") + itemAttr.GetRawBonusDamageMax("Cold")
                   + itemAttr.GetRawBonusDamageMax("Fire") + itemAttr.GetRawBonusDamageMax("Holy")
                   + itemAttr.GetRawBonusDamageMax("Lightning") + itemAttr.GetRawBonusDamageMax("Physical")
                   + itemAttr.GetRawBonusDamageMax("Poison");
        }

        /// <summary>
        /// Computes max damage of the item (based on a specific resist).
        /// </summary>
        /// <param name="itemAttr">Attributes of an item.</param>
        /// <param name="resist">Name of the resist.</param>
        /// <param name="useDamageTypePercentBonus"></param>
        /// <returns></returns>
        public static ItemValueRange GetRawBonusDamageMax(this ItemAttributes itemAttr, String resist, bool useDamageTypePercentBonus = true)
        {
            var result = itemAttr.GetAttributeByName("damageMin_" + resist) + itemAttr.GetAttributeByName("damageDelta_" + resist);

            if (useDamageTypePercentBonus && resist != "Physical")
            {
                result += itemAttr.GetRawBonusDamageMax("Physical") * itemAttr.GetAttributeByName("damageTypePercentBonus_" + resist);
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Returns the resistance value given by the gems for the given resist
        /// </summary>
        /// <param name="itemAttr"></param>
        /// <param name="resist"></param>
        /// <returns></returns>
        public static ItemValueRange GetResistance(this ItemAttributes itemAttr, String resist)
        {
            return itemAttr.GetAttributeByName("resistance_" + resist);
        }

        /// <summary>
        /// Computes weapon attack speed (attack per second).
        /// </summary>
        /// <param name="itemAttr"></param>
        /// <returns></returns>
        public static ItemValueRange GetRawWeaponAttackPerSecond(this ItemAttributes itemAttr)
        {
            return itemAttr.GetWeaponAttackPerSecond(ItemValueRange.Zero);
        }

        /// <summary>
        /// Computes raw weapon dps ie before all multipliers ( = average damage * attack per second )
        /// </summary>
        /// <param name="itemAttr"></param>
        /// <returns></returns>
        public static ItemValueRange GetRawWeaponDps(this ItemAttributes itemAttr)
        {
            return itemAttr.GetRawAverageWeaponDamage() * itemAttr.GetRawWeaponAttackPerSecond();
        }

        /// <summary>
        /// Computes weapon only damages
        /// </summary>
        /// <param name="itemAttr"></param>
        /// <returns></returns>
        public static ItemValueRange GetRawAverageWeaponDamage(this ItemAttributes itemAttr)
        {
            // formula: ( min + max ) / 2
            return (itemAttr.GetRawWeaponDamageMin() + itemAttr.GetRawWeaponDamageMax()) / 2;
        }

        /// <summary>
        /// Computes min damage of the weapon without taking account of "+% weapon damage".
        /// </summary>
        /// <param name="itemAttr">Attributes of a weapon.</param>
        /// <param name="resist"></param>
        /// <returns></returns>
        public static ItemValueRange GetBaseWeaponDamageMin(this ItemAttributes itemAttr, String resist)
        {
            return itemAttr.GetAttributeByName("damageWeaponMin_" + resist) +
                   itemAttr.GetAttributeByName("damageWeaponBonusMin_" + resist) +
                   itemAttr.GetAttributeByName("damageWeaponBonusMinX1_" + resist) +
                   itemAttr.GetAttributeByName("damageWeaponBonusFlat_" + resist);
        }

        #region >> getRawWeaponDamageMin *

        /// <summary>
        /// Computes min damage of the weapon.
        /// </summary>
        /// <param name="weaponAttr">Attributes of a weapon.</param>
        /// <returns></returns>
        public static ItemValueRange GetRawWeaponDamageMin(this ItemAttributes weaponAttr)
        {
            return weaponAttr.GetRawWeaponDamageMin("Arcane") + weaponAttr.GetRawWeaponDamageMin("Cold")
                   + weaponAttr.GetRawWeaponDamageMin("Fire") + weaponAttr.GetRawWeaponDamageMin("Holy")
                   + weaponAttr.GetRawWeaponDamageMin("Lightning") + weaponAttr.GetRawWeaponDamageMin("Physical")
                   + weaponAttr.GetRawWeaponDamageMin("Poison");
        }

        /// <summary>
        /// Computes min damage of the weapon (based on a specific resist).
        /// </summary>
        /// <param name="weaponAttr">Attributes of a weapon.</param>
        /// <param name="resist">Name of the resist.</param>
        /// <param name="useDamageTypePercentBonus"></param>
        /// <returns></returns>
        public static ItemValueRange GetRawWeaponDamageMin(this ItemAttributes weaponAttr, String resist, bool useDamageTypePercentBonus = true)
        {
            var damage = weaponAttr.GetBaseWeaponDamageMin(resist);

            damage *= 1 + weaponAttr.GetAttributeByName("damageWeaponPercentBonus_" + resist);

            if (useDamageTypePercentBonus && resist != "Physical")
            {
                damage += weaponAttr.GetRawWeaponDamageMin("Physical") * weaponAttr.GetAttributeByName("damageTypePercentBonus_" + resist);
            }

            return damage;
        }

        #endregion

        /// <summary>
        /// Computes max damage of the weapon without taking account of "+% weapon damage".
        /// </summary>
        /// <param name="weaponAttr">Attributes of a weapon.</param>
        /// <param name="resist"></param>
        /// <returns></returns>
        public static ItemValueRange GetBaseWeaponDamageMax(this ItemAttributes weaponAttr, String resist)
        {
            return weaponAttr.GetAttributeByName("damageWeaponMin_" + resist) +
                   weaponAttr.GetAttributeByName("damageWeaponBonusMinX1_" + resist) +
                   weaponAttr.GetAttributeByName("damageWeaponDelta_" + resist) +
                   weaponAttr.GetAttributeByName("damageWeaponBonusDelta_" + resist) +
                   weaponAttr.GetAttributeByName("damageWeaponBonusDeltaX1_" + resist) +
                   weaponAttr.GetAttributeByName("damageWeaponBonusFlat_" + resist);
        }

        #region >> getRawWeaponDamageMax *

        /// <summary>
        /// Computes max damage of the weapon.
        /// </summary>
        /// <param name="weaponAttr">Attributes of a weapon.</param>
        /// <returns></returns>
        public static ItemValueRange GetRawWeaponDamageMax(this ItemAttributes weaponAttr)
        {
            return weaponAttr.GetRawWeaponDamageMax("Arcane") + weaponAttr.GetRawWeaponDamageMax("Cold")
                   + weaponAttr.GetRawWeaponDamageMax("Fire") + weaponAttr.GetRawWeaponDamageMax("Holy")
                   + weaponAttr.GetRawWeaponDamageMax("Lightning") + weaponAttr.GetRawWeaponDamageMax("Physical")
                   + weaponAttr.GetRawWeaponDamageMax("Poison");
        }

        /// <summary>
        /// Computes max damage of the weapon (based on a specific resist).
        /// </summary>
        /// <param name="weaponAttr">Attributes of a weapon.</param>
        /// <param name="resist">Name of the resist.</param>
        /// <param name="useDamageTypePercentBonus"></param>
        /// <returns></returns>
        public static ItemValueRange GetRawWeaponDamageMax(this ItemAttributes weaponAttr, String resist, bool useDamageTypePercentBonus = true)
        {
            var damage = weaponAttr.GetBaseWeaponDamageMax(resist);

            damage *= ItemValueRange.One + weaponAttr.GetAttributeByName("damageWeaponPercentBonus_" + resist);

            if (useDamageTypePercentBonus && resist != "Physical")
            {
                damage += weaponAttr.GetRawWeaponDamageMax("Physical") * weaponAttr.GetAttributeByName("damageTypePercentBonus_" + resist);
            }

            return damage;
        }

        #endregion

        /// <summary>
        /// Computes weapon attack speed.
        /// </summary>
        /// <param name="weaponAttr">Attributes of used weapon.</param>
        /// <param name="increaseFromOtherItems">Increase Attack Speed from items other than the weapon.</param>
        /// <returns></returns>
        public static ItemValueRange GetWeaponAttackPerSecond(this ItemAttributes weaponAttr, ItemValueRange increaseFromOtherItems)
        {
            var weaponAttackSpeed = weaponAttr.attacksPerSecondItem;

            weaponAttackSpeed *= 1 + weaponAttr.attacksPerSecondItemPercent + increaseFromOtherItems;

            return weaponAttackSpeed;
        }

        #region >> checkAndUpdateWeaponDelta *

        /// <summary>
        /// Check a specific case of "invalid" weapon damage values:
        /// If bonus min > delta, then delta should be replaced by bonus min + 1
        /// </summary>
        /// <param name="itemAttr"></param>
        public static ItemAttributes CheckAndUpdateWeaponDelta(this ItemAttributes itemAttr)
        {
            return itemAttr
                .CheckAndUpdateWeaponDelta("Arcane")
                .CheckAndUpdateWeaponDelta("Cold")
                .CheckAndUpdateWeaponDelta("Fire")
                .CheckAndUpdateWeaponDelta("Holy")
                .CheckAndUpdateWeaponDelta("Lightning")
                .CheckAndUpdateWeaponDelta("Physical")
                .CheckAndUpdateWeaponDelta("Poison");
        }

        /// <summary>
        /// Check a specific case of "invalid" Weapon damage values (based on a specific resist):
        /// If bonus min > delta, then delta should be replaced by bonus min + 1
        /// </summary>
        /// <param name="itemAttr"></param>
        /// <param name="resist"></param>
        public static ItemAttributes CheckAndUpdateWeaponDelta(this ItemAttributes itemAttr, String resist)
        {
            var damageWeaponBonusMin = itemAttr.GetAttributeByName("damageWeaponBonusMin_" + resist);
            var damageWeaponDelta = itemAttr.GetAttributeByName("damageWeaponDelta_" + resist);

            // Check "black weapon bug"
            if ((damageWeaponDelta != null) && (damageWeaponBonusMin != null) && (damageWeaponDelta.Min < damageWeaponBonusMin.Min))
            {
                damageWeaponDelta = damageWeaponBonusMin + 1;
            }

            // Store new values
            itemAttr.SetAttributeByName("damageWeaponDelta_" + resist, damageWeaponDelta);

            return itemAttr;
        }

        #endregion

        /// <summary>
        /// Informs if the item is ancient based on its attributes.
        /// </summary>
        /// <param name="itemAttr">Attributes of the item.</param>
        /// <returns></returns>
        public static bool IsAncient(this ItemAttributes itemAttr)
        {
            return itemAttr?.AncientRank != null;
        }

        /// <summary>
        /// Informs if the item is a legendary jewel based on its attributes.
        /// </summary>
        /// <param name="itemAttr">Attributes of the item.</param>
        /// <returns></returns>
        public static bool IsJewel(this ItemAttributes itemAttr)
        {
            return itemAttr?.JewelRank != null;
        }

        /// <summary>
        /// Informs if the item is a weapon based on its attributes
        /// </summary>
        /// <param name="itemAttr">Attributes of the item.</param>
        /// <returns></returns>
        public static bool IsWeapon(this ItemAttributes itemAttr)
        {
            return itemAttr?.attacksPerSecondItem != null;
        }

        /// <summary>
        /// Returns a new <see cref="ItemAttributes"/> by aggregating some raw attributes of <paramref name="itemAttr"/> (for easier editing for example).
        /// </summary>
        /// <param name="itemAttr"></param>
        /// <returns>The <paramref name="itemAttr"/> instance.</returns>
        public static ItemAttributes GetSimplified(this ItemAttributes itemAttr)
        {
            var attr = new ItemAttributes(itemAttr);

            // Characteristics
            attr.armorItem = itemAttr.GetArmor().NullIfZero();
            attr.armorBonusItem = null;

            // Weapon characterics
            attr.attacksPerSecondItem = itemAttr.GetRawWeaponAttackPerSecond().NullIfZero();
            attr.attacksPerSecondItemPercent = null;

            itemAttr.CheckAndUpdateWeaponDelta();

            foreach (var resist in Constants.DamageResists)
            {
                var baseWeaponDamageMin = itemAttr.GetBaseWeaponDamageMin(resist);
                var baseWeaponDamageDelta = itemAttr.GetBaseWeaponDamageMax(resist) - baseWeaponDamageMin;

                attr.SetAttributeByName("damageWeaponMin_" + resist, baseWeaponDamageMin.NullIfZero())
                    .SetAttributeByName("damageWeaponDelta_" + resist, baseWeaponDamageDelta.NullIfZero())
                    .SetAttributeByName("damageWeaponBonusMin_" + resist, null)
                    .SetAttributeByName("damageWeaponBonusDelta_" + resist, null)
                    .SetAttributeByName("damageWeaponBonusMinX1_" + resist, null)
                    .SetAttributeByName("damageWeaponBonusDeltaX1_" + resist, null)
                    .SetAttributeByName("damageWeaponBonusFlat_" + resist, null);
            }

            // Item damage bonuses
            foreach (var resist in Constants.DamageResists)
            {
                var rawDamageMin = itemAttr.GetRawBonusDamageMin(resist, false);
                var rawDamageDelta = itemAttr.GetRawBonusDamageMax(resist, false) - rawDamageMin;

                attr.SetAttributeByName("damageMin_" + resist, rawDamageMin.NullIfZero())
                    .SetAttributeByName("damageDelta_" + resist, rawDamageDelta.NullIfZero())
                    .SetAttributeByName("damageBonusMin_" + resist, null);
            }

            return attr;
        }
    }
}