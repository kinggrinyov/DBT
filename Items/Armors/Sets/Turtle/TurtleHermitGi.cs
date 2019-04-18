﻿using DBT.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DBT.Items.Armors.Sets.Turtle
{
    [AutoloadEquip(EquipType.Body)]
    public class TurtleHermitGi : DBTItem
    {
        public TurtleHermitGi() : base("Turtle Hermit Gi", "5% Increased Ki Damage\n3% Increased Ki Crit Chance\nIncreased ki regen",
            28, 18, value: Item.buyPrice(silver: 36), defense: 8, rarity: ItemRarityID.Orange)
        {
        }

		public override void DrawHands(ref bool drawHands, ref bool drawArms)
        {
            drawArms = true;
            drawHands = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return legs.type == mod.ItemType(nameof(TurtleHermitPants));
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "15% reduced ki usage and +200 Max Ki.";

            DBTPlayer dbtPlayer = player.GetModPlayer<DBTPlayer>();

            //dbtPlayer.KiUsageMultiplier -= 0.15f;
            dbtPlayer.MaxKiModifier += 200;
        }
        public override void UpdateEquip(Player player)
        {
            DBTPlayer dbtPlayer = player.GetModPlayer<DBTPlayer>();

            dbtPlayer.KiDamageMultiplier += 0.05f;
            dbtPlayer.KiCritAddition += 3;
            dbtPlayer.NaturalKiRegeneration += 1;

        }
        /*public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 20);
            recipe.AddIngredient(mod, nameof(EarthenShard), 12);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }*/
    }
}