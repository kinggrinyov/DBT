﻿using Terraria;
using Terraria.ID;

namespace DBT.Items.Accessories.Infusers
{
    public sealed class AmethystInfuser : Infuser
    {
        public AmethystInfuser() : base("Amethyst Ki Infuser", "Hitting enemies with ki attacks inflicts shadowflame.", 131 * Constants.SILVER_VALUE_MULTIPLIER, ItemID.Amethyst)
        {
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            base.OnHitNPC(player, target, damage, knockBack, crit);

            target.AddBuff(BuffID.ShadowFlame, 300);
        }
    }
}