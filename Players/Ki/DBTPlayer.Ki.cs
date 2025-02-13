﻿using System;
using System.Collections.Generic;
using DBT.Commons;
using DBT.Network;
using Terraria;
using WebmilioCommons.Extensions;

namespace DBT.Players
{
    public sealed partial class DBTPlayer
    {
        private bool _charging;


        public float ModifyKi(float kiAmount)
        {
            // TODO Add mastery for being in a form, if need be.

            float projectedKi = Ki + kiAmount;

            if (projectedKi < 0)
                projectedKi = 0;
            else if (projectedKi > MaxKi)
                projectedKi = MaxKi;

            Ki = projectedKi;
            return Ki;
        }

        public float ModifyKi(float kiAmount, int kiRegenHaltedFor)
        {
            float result = ModifyKi(kiAmount);

            KiRegenerationHaltedFor = kiRegenHaltedFor;
            return result;
        }

        internal void ResetKiEffects()
        {
            KiDamageMultiplier = 1;

            MaxKiMultiplier = 1;
            MaxKiModifier = 0;

            KiSpeedAddition = 0;
            KiKnockbackAddition = 0;
            KiCritAddition = 0;

            BaseNaturalKiRegenerationPercentage = 0.01f / Constants.TICKS_PER_SECOND;
            BaseNaturalKiRegenerationModifier = 0f;
            NaturalKiRegenerationMultiplier = 1;

            ExternalKiRegenerationPercentage = 0f;
            ExternalKiRegenerationModifier = 0;
            NaturalKiRegenerationMultiplier = 1f;

            KiOrbRestoreAmount = 100;
            KiOrbGrabRange = 2;

            KiOrbDropChance = 3;

            KiChargeRatePercentage = 0.05f / Constants.TICKS_PER_SECOND;
            KiChargeRateModifier = 1;
            KiChargeRateMultiplierLimit = 0;

            KiDrainMultiplier = 1;
            KiDrainModifier = 0;
        }

        internal void PreUpdateKi()
        {
            //Main.NewText("Powerlevel is: " + Powerlevel);
        }

        internal void PostUpdateKi()
        {
            if (Charging)
            {
                ModifyKi(KiChargeRate);

                List<IUpdatesOnChargeTick> items = player.GetItemsByType<IUpdatesOnChargeTick>(accessories: true, armor: true);
                float defenseMultiplier = 1;

                for (int i = 0; i < items.Count; i++)
                    items[i].OnPlayerPostUpdateChargingTick(this, ref defenseMultiplier);

                player.statDefense = (int)(defenseMultiplier * player.statDefense);
            }

            if (KiRegenerationHaltedFor >= 0)
            {
                KiRegenerationHaltedFor--;
                return;
            }

            if (OverloadKiMultiplier != 0)
                MaxKiMultiplier += OverloadKiMultiplier - 1f;

            if (Ki < MaxKi)
                ModifyKi(NaturalKiRegeneration / 2 + ExternalKiRegenerationModifier);
        }


        public float GetChargeLevelLimit(float skillChargeLevelLimit) => skillChargeLevelLimit * SkillChargeLevelLimitModifier * SkillChargeLevelLimitMultiplier;

        public float GetKiDrain(float kiDrain) => (kiDrain + KiDrainMultiplier) + KiDrainModifier;

        public bool IsKiDepleted(float projectedKiDrain = 0f) => Ki - projectedKiDrain <= 0;

        public float KiDamageMultiplier { get; set; } = 1f;

        public float Ki { get; private set; }

        public float KiChargeRatePercentage { get; set; }
        public float KiChargeRateModifier { get; set; }
        public float KiChargeRateMultiplierLimit { get; set; } // TODO Implement this

        public float KiChargeRate => (KiChargeRatePercentage * MaxKi + KiChargeRateModifier);

        public float KiRegenerationHaltedFor { get; set; }

        /// <summary>Percentage of maximum Ki restored per second.</summary>
        public float BaseNaturalKiRegenerationPercentage { get; set; }
        public float BaseNaturalKiRegenerationModifier { get; set; }

        public float NaturalKiRegenerationMultiplier { get; set; }
        public float NaturalKiRegeneration => (BaseNaturalKiRegenerationPercentage * MaxKi + BaseNaturalKiRegenerationModifier) * NaturalKiRegenerationMultiplier;

        public float ExternalKiRegenerationPercentage { get; set; }
        public float ExternalKiRegenerationModifier { get; set; }

        public float ExternalKiRegenerationMultiplier { get; set; }
        public float ExternalKiRegeneration => (ExternalKiRegenerationPercentage * MaxKi + ExternalKiRegenerationModifier) * ExternalKiRegenerationMultiplier;

        public float KiOrbRestoreAmount { get; set; }
        public float KiOrbGrabRange { get; set; }

        public int KiOrbDropChance { get; set; }

        public bool Charging
        {
            get => _charging;
            set
            {
                if (_charging == value)
                    return;

                _charging = value;

                if (player.whoAmI == Main.myPlayer)
                    new PlayerChargingPacket().Send();
            }
        }


        public float BaseMaxKi { get; set; }

        public float MaxKiMultiplier { get; set; } = 1;
        public float MaxKiModifier { get; set; }

        public float MaxKiModifierPerm { get; set; }

        public int MaxKi => (int)(Math.Round(BaseMaxKi * MaxKiMultiplier) + MaxKiModifier + MaxKiModifierPerm);

        public float KiSpeedAddition { get; set; }
        public float KiKnockbackAddition { get; set; }
        public int KiCritAddition { get; set; }

        public float KiDrainMultiplier { get; set; }
        public float KiDrainModifier { get; set; }
    }
}
