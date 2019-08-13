﻿using Terraria.ModLoader.IO;

namespace DBT.Players
{
    public sealed partial class DBTPlayer
    {
        private void SaveGuardian(TagCompound tag)
        {
            tag.Add(nameof(PowerWishesLeft), PowerWishesLeft);
            tag.Add(nameof(ImmortalityWishesLeft), ImmortalityWishesLeft);
            tag.Add(nameof(SkillWishesLeft), SkillWishesLeft);
            tag.Add(nameof(AwakeningWishesLeft), AwakeningWishesLeft);
            tag.Add(nameof(ImmortalityRevivesLeft), ImmortalityRevivesLeft);
        }

        private void LoadGuardian(TagCompound tag)
        {
            BaseHealingBonus = tag.GetInt(nameof(BaseHealingBonus));
        }
    }
}
