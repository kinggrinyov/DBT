﻿using System;
using System.Collections.Generic;
using System.Linq;
using DBT.Dynamicity;
using DBT.Players;
using DBT.Skills;
using DBT.Transformations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace DBT.UserInterfaces.TechniqueMenu
{
    public class TechniqueMenu : DBTMenu
    {
        public const int
            PADDING_X = 10,
            PADDING_Y = PADDING_X,
            TITLE_Y_SPACE = 50,
            SMALL_SPACE = 4;

        private const string
            TECHNIQUE_MENU_PATH = "UserInterfaces/TechniqueMenu",
            LOCKED_TEXTURE = TECHNIQUE_MENU_PATH + "/LockedImage",
            WHITE_PIXEL = TECHNIQUE_MENU_PATH + "/Pixel",
            EQUIP_BUTTON = TECHNIQUE_MENU_PATH + "/EquipButton",
            UNEQUIP_BUTTON = TECHNIQUE_MENU_PATH + "/UnequipButton";

        private int _panelsYOffset = 0;

        private readonly Dictionary<SkillDefinition, UIImagePair> _skillImagePairs = new Dictionary<SkillDefinition, UIImagePair>();

        public TechniqueMenu(Mod authorMod)
        {
            AuthorMod = authorMod;
            BackPanelTexture = authorMod.GetTexture(TECHNIQUE_MENU_PATH + "/BackPanel");
            InfoPanelTexture = authorMod.GetTexture(TECHNIQUE_MENU_PATH + "/InfoPanel");
            EquipButtonTexture = authorMod.GetTexture(EQUIP_BUTTON);

            LockedImageTexture = authorMod.GetTexture(LOCKED_TEXTURE);
            WhitePixel = authorMod.GetTexture(WHITE_PIXEL);
        }

        public override void OnInitialize()
        {
            BackPanel = new UIPanel();

            BackPanel.Width.Set(BackPanelTexture.Width, 0f);
            BackPanel.Height.Set(BackPanelTexture.Height, 0f);

            BackPanel.Left.Set(Main.screenWidth / 2f - BackPanel.Width.Pixels / 2f, 0f);
            BackPanel.Top.Set(Main.screenHeight / 2f - BackPanel.Height.Pixels / 2f, 0f);

            BackPanel.BackgroundColor = new Color(0, 0, 0, 0);

            Append(BackPanel);

            InfoPanel = new UIPanel();

            InfoPanel.Width.Set(InfoPanelTexture.Width, 0f);
            InfoPanel.Height.Set(InfoPanelTexture.Height, 0f);

            InfoPanel.Left.Set(-12, 0f);
            InfoPanel.Top.Set(440, 0f);

            InfoPanel.BackgroundColor = Color.Transparent;
            InfoPanel.BorderColor = Color.Transparent;

            BackPanel.Append(InfoPanel);

            base.OnInitialize();
        }

        // The reason all of this is called in OnPlayerEnterWorld is because the icons themselves don't make it to the UI if the player cannot possibly access them. We need to reset the back panel every time.
        internal void OnPlayerEnterWorld(DBTPlayer dbtPlayer)
        {
            BackPanel.RemoveAllChildren();
            BackPanel.Append(InfoPanel);

            List<Node<SkillDefinition>> rootNodes = SkillDefinitionManager.Instance.Tree.Nodes
                .Where(t => t.Value.CheckPrePlayerConditions())
                .Where(t => t.Parents.Count == 0).ToList();

            List<Node<SkillDefinition>> others = new List<Node<SkillDefinition>>();

            int lastXOffset = 0;

            foreach (Node<SkillDefinition> rootNode in rootNodes)
                if (rootNode.Value.SkillIcon.Height > _panelsYOffset)
                    _panelsYOffset = rootNode.Value.SkillIcon.Height;

            _panelsYOffset += PADDING_Y;

            foreach (Node<SkillDefinition> rootNode in rootNodes)
            {
                lastXOffset += PADDING_X * 2;

                int yOffset = (int)rootNode.Value.MenuPosition.Y;

                RecursiveInitializeSkill(BackPanel, rootNode, ref yOffset);

                lastXOffset += rootNode.Value.SkillIcon.Width;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            DBTPlayer dbtPlayer = Main.LocalPlayer.GetModPlayer<DBTPlayer>();

            foreach (KeyValuePair<SkillDefinition, UIImagePair> kvp in _skillImagePairs)
            {
                bool unlockable = kvp.Key.DoesDisplayInTechniqueMenu(dbtPlayer);
                bool visible = kvp.Key.DoesDisplayInTechniqueMenu(dbtPlayer);

                if (!visible)
                {
                    kvp.Value.button.Width = StyleDimension.Empty;
                    kvp.Value.button.Height = StyleDimension.Empty;
                    kvp.Value.button.SetVisibility(0f, 0f);
                }

                kvp.Value.lockedImage.ImageScale = visible && dbtPlayer.HasAcquiredSkill(kvp.Key) ? 0f : 1f;

                //Main.NewText(kvp.Key.ToString() + "'s unknown image is at" + kvp.Value.unknownImage.ImageScale);
                //Main.NewText(kvp.Key.ToString() + "'s unknown image gray is at" + kvp.Value.unknownImageGray.ImageScale);
                //Main.NewText(kvp.Key.ToString() + "'s locked image is at" + kvp.Value.lockedImage.ImageScale);
            }

            
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(BackPanelTexture, new Vector2(BackPanel.Left.Pixels, BackPanel.Top.Pixels), Color.White);
            base.DrawSelf(spriteBatch);
            List<Node<SkillDefinition>> rootNodes = SkillDefinitionManager.Instance.Tree.Nodes
                .Where(t => t.Value.CheckPrePlayerConditions()).ToList();

            foreach (Node<SkillDefinition> rootNode in rootNodes)
            {
                for (int i = 0; i < rootNode.Parents.Count; i++)
                {
                    Node<SkillDefinition> parent = rootNode.Parents[i];
                    DrawLine(spriteBatch, new Vector2(BackPanel.Left.Pixels + 30, BackPanel.Top.Pixels + 24) + rootNode.Value.MenuPosition, new Vector2(BackPanel.Left.Pixels + 30, BackPanel.Top.Pixels + 24) + parent.Value.MenuPosition, Color.LightGray, 3f);
                }
            }
        }

        private void DrawSkill(UIPanel panel, SkillDefinition skill, Texture2D icon, int left, int top)
        {
            UIImageButton skillButton = null;
            UIImage lockedImage = null;

            skillButton = InitializeButton(icon, new MouseEvent((evt, element) => DrawInfoPanel(skill, evt, element)), left, top, panel);

            lockedImage = InitializeImage(LockedImageTexture, (skillButton.Width.Pixels / 2) - (LockedImageTexture.Width / 2), 0, skillButton);
            lockedImage.ImageScale = 0f;
            lockedImage.Width.Set(1, 0f);
            lockedImage.Height.Set(1, 0f);

            if (!_skillImagePairs.ContainsKey(skill))
                _skillImagePairs.Add(skill, new UIImagePair(new Point(left, top), skillButton, null, null, lockedImage));
        }

        private void RecursiveInitializeSkill(UIPanel panel, Node<SkillDefinition> node, ref int yOffset)
        {
            SkillDefinition skill = node.Value;
            Texture2D texture = skill.SkillIcon;

            if (!CheckIfDraw(skill)) return;
            int xOffset = (int)skill.MenuPosition.X;
            yOffset = (int)skill.MenuPosition.Y;
            DrawSkill(panel, skill, texture, xOffset, yOffset);

            for (int i = 0; i < node.Children.Count; i++)
            {
                Node<SkillDefinition> child = node.Children[i];

                RecursiveInitializeSkill(panel, child, ref yOffset);

                if (node.Children.Count > 1 && CheckIfDraw(child) && node.Children[node.Children.Count - 1] != child)
                {
                    yOffset += texture.Height + SMALL_SPACE * 2;
                }
            }
            
        }

        public void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }

        public void DrawLine(SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(WhitePixel, point, null, color, angle, origin, scale, SpriteEffects.None, 1);
        }

        private static bool CheckIfDraw(Node<SkillDefinition> node) => node.Value.CheckPrePlayerConditions();
        private static bool CheckIfDraw(SkillDefinition skill) => skill.CheckPrePlayerConditions();

        private void DrawInfoPanel(SkillDefinition def, UIMouseEvent evt, UIElement listeningElement)
        {
            InfoPanel.RemoveAllChildren();
            DBTPlayer dbtPlayer = Main.LocalPlayer.GetModPlayer<DBTPlayer>();
            if (dbtPlayer.ActiveSkills.Contains(def))
            {
                EquipButtonTexture = AuthorMod.GetTexture(UNEQUIP_BUTTON);
            }
            else
                EquipButtonTexture = AuthorMod.GetTexture(EQUIP_BUTTON);


            DrawEquipButton(def);
            bool hasAttackSpeed = false;

            if (def.Characteristics.BaseShootSpeed > 0)
                hasAttackSpeed = true;

            skillName = InitializeText(def.DisplayName, 12, 8, 0.8f, Color.White, InfoPanel);
            skillStats = InitializeText("Stats: \nBase Ki Damage: " + def.Characteristics.BaseDamage + (hasAttackSpeed ? "\nAttack Speed: " + def.Characteristics.BaseShootSpeed : "") + " \nKi Drain: " + def.Characteristics.ChargeCharacteristics.BaseCastKiDrain + " \n", 12, 28, 0.6f, Color.White, InfoPanel);
            skillUnlock = InitializeText("Unlock: \n" + def.UnlockDescription, 340, 8, 0.7f, Color.White, InfoPanel);
            skillDescription = InitializeText("Skill Description: \n" + def.Description, 160, 8, 0.7f, Color.White, InfoPanel);
        }

        private void DrawEquipButton(SkillDefinition def)
        {
            if (EquipButton != null)
                EquipButton.Remove();

            EquipButton = InitializeButton(EquipButtonTexture, new MouseEvent((evt, element) => TrySelectingSkill(def, evt, element)), 550, 510, BackPanel);
        }

        private void TrySelectingSkill(SkillDefinition def, UIMouseEvent evt, UIElement listeningElement)
        {

        }

        public Mod AuthorMod { get; }
        public bool Visible { get; set; } = false;
        public Texture2D LockedImageTexture { get; }
        public Texture2D InfoPanelTexture { get; }
        public Texture2D EquipButtonTexture { get; set; }
        private Texture2D WhitePixel { get; }
        public UIPanel InfoPanel { get; set; } = null;

        public UIImageButton EquipButton { get; set; } = null;

        public UIText
            skillName = null,
            skillStats = null,
            skillUnlock = null,
            skillDescription = null;
    }
}
