﻿using DBT.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DBT.Items.Accessories
{
    [AutoloadEquip(EquipType.Waist)]
    public sealed class AncientLegendWaistCape : DBTItem
    {
        public AncientLegendWaistCape() : base("Ancient Legend Waistcape", "A ancient garment made of a ki enhancing material.\n14% reduced ki usage\n6% increased ki damage\n-250 max ki")
        {
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            item.width = 24;
            item.height = 28;
            item.value = 300 * Constants.SILVER_VALUE_MULTIPLIER;
            item.rare = ItemRarityID.Pink;
            item.accessory = true;
            item.defense = 0;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            base.UpdateAccessory(player, hideVisual);

            DBTPlayer dbtPlayer = player.GetModPlayer<DBTPlayer>();

            dbtPlayer.KiDamageMultiplier *= 1.06f;
            dbtPlayer.MaxKiModifier -= 250;
        }

        // TODO Rework recipe.
        public override void AddRecipes()
        {
            /*base.AddRecipes();

            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(mod, nameof(PureKiCrystal), 25);
            recipe.AddIngredient(mod, nameof(SatanicCloth), 8);
            recipe.AddIngredient(mod, nameof(KaiTable));
            
            recipe.SetResult(this);
            recipe.AddRecipe();*/
        }
    }
}