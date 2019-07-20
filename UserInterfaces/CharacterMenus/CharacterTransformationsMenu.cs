﻿using System;
using System.Collections.Generic;
using System.Linq;
using DBT.Dynamicity;
using DBT.Extensions;
using DBT.Players;
using DBT.Transformations;
using DBT.UserInterfaces.Buttons;
using DBT.UserInterfaces.Tabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace DBT.UserInterfaces.CharacterMenus
{
    public sealed class CharacterTransformationsMenu : DBTMenu
    {
        public const int
            PADDING_X = 10,
            PADDING_Y = PADDING_X,
            TITLE_Y_SPACE = 50,
            SMALL_SPACE = 4;

        private const string
            CHARACTER_MENU_PATH = "UserInterfaces/CharacterMenus",
            UNKNOWN_TEXTURE = CHARACTER_MENU_PATH + "/UnknownImage", UNKNOWN_GRAY_TEXTURE = CHARACTER_MENU_PATH + "/UnknownImageGray", LOCKED_TEXTURE = CHARACTER_MENU_PATH + "/LockedImage";

        private int _panelsYOffset = 0;

        private readonly List<Tab> _tabs = new List<Tab>();
        private readonly Dictionary<UIHoverImageButton, Tab> _tabButtons = new Dictionary<UIHoverImageButton, Tab>();
        private readonly Dictionary<Tab, TransformationDefinition> _tabsForTransformations = new Dictionary<Tab, TransformationDefinition>();

        public CharacterTransformationsMenu(Mod authorMod)
        {
            AuthorMod = authorMod;
            BackPanelTexture = authorMod.GetTexture(CHARACTER_MENU_PATH + "/BackPanel");

            UnknownImageTexture = authorMod.GetTexture(UNKNOWN_TEXTURE);
            UnknownGrayImageTexture = authorMod.GetTexture(UNKNOWN_GRAY_TEXTURE);
            LockedImageTexture = authorMod.GetTexture(LOCKED_TEXTURE);
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

            BackPanelImage = new UIImage(BackPanelTexture);
            BackPanelImage.Width.Set(BackPanelTexture.Width, 0f);
            BackPanelImage.Height.Set(BackPanelTexture.Height, 0f);

            BackPanelImage.Left.Set(-12, 0f);
            BackPanelImage.Top.Set(-12, 0f);

            BackPanel.Append(BackPanelImage);

            base.OnInitialize();
        }

        // The reason all of this is called in OnPlayerEnterWorld is because the icons themselves don't make it to the UI if the player cannot possibly access them. We need to reset the back panel every time.
        internal void OnPlayerEnterWorld(DBTPlayer dbtPlayer)
        {
            _tabs.Clear();
            _tabButtons.Clear();
            _tabsForTransformations.Clear();

            BackPanel.RemoveAllChildren();
            BackPanel.Append(BackPanelImage);

            List<Node<TransformationDefinition>> rootNodes = TransformationDefinitionManager.Instance.Tree.Nodes
                .Where(t => t.Value.CheckPrePlayerConditions() && t.Value.BaseConditions(dbtPlayer) && t.Value.DoesDisplayInCharacterMenu(dbtPlayer))
                .Where(t => t.Parents.Count == 0).ToList();

            List<Node<TransformationDefinition>> others = new List<Node<TransformationDefinition>>();

            int lastXOffset = 0;

            foreach (Node<TransformationDefinition> rootNode in rootNodes)
                if (rootNode.Value.TransformationIcon.Height > _panelsYOffset)
                    _panelsYOffset = rootNode.Value.TransformationIcon.Height;

            _panelsYOffset += PADDING_Y;

            foreach (Node<TransformationDefinition> rootNode in rootNodes)
            {
                if (rootNode.Children.Count == 0)
                {
                    others.Add(rootNode);
                    continue;
                }

                lastXOffset += PADDING_X * (_tabs.Count + 1);

                UIHoverImageButton tabButton = InitializeHoverTextButton(rootNode.Value.TransformationIcon, rootNode.Value.TabHoverText, OnUITabClick, lastXOffset, PADDING_Y, BackPanelImage);

                UIPanel tabPanel = new UIPanel()
                {
                    Width = new StyleDimension(BackPanelTexture.Width - 20, 0),
                    Height = new StyleDimension(BackPanelTexture.Height - (_panelsYOffset + 10 + PADDING_Y), 0),

                    Left = new StyleDimension(0, 0),
                    Top = new StyleDimension(0, 0),

                    BackgroundColor = Color.Transparent,
                    BorderColor = rootNode.Value.Appearance.GeneralColor.HasValue ? rootNode.Value.Appearance.GeneralColor.Value : Color.White
                };

                BackPanel.Append(tabPanel);

                Tab tab = new Tab(tabButton, tabPanel);
                tab.Panel.Deactivate();

                _tabs.Add(tab);
                _tabButtons.Add(tabButton, tab);
                _tabsForTransformations.Add(tab, rootNode.Value);

                lastXOffset += rootNode.Value.TransformationIcon.Width;
            }
        }

        public override void Update(GameTime gameTime)
        {
            DBTPlayer dbtPlayer = Main.LocalPlayer.GetModPlayer<DBTPlayer>();

            foreach (Tab tab in _tabButtons.Values)
            {
                if (_tabsForTransformations[tab] != LastActiveTransformationTab)
                {
                    tab.Panel.Left.Set(-Main.screenWidth, 0);
                    tab.Panel.Top.Set(-Main.screenHeight, 0);
                }
                else
                {
                    tab.Panel.Left.Set(0, 0);
                    tab.Panel.Top.Set(_panelsYOffset, 0);
                }
            }

            foreach (KeyValuePair<UIHoverImageButton, Tab> kvp in _tabButtons)
                kvp.Value.TabButton.SetImage(_tabsForTransformations[kvp.Value].TransformationIcon);
        }

        private void OnUITabClick(UIMouseEvent evt, UIElement listeningelement)
        {
            UIHoverImageButton button = listeningelement as UIHoverImageButton;

            LastActiveTransformationTab = _tabsForTransformations[_tabButtons[button]];
        }


        private static void TrySelectingTransformation(TransformationDefinition def, UIMouseEvent evt, UIElement listeningElement)
        {
            DBTPlayer dbtPlayer = Main.LocalPlayer.GetModPlayer<DBTPlayer>();

            if (def.CheckPrePlayerConditions() && dbtPlayer.HasAcquiredTransformation(def) && def.BaseConditions(dbtPlayer))
            {
                // TODO Add sounds.
                //SoundHelper.PlayVanillaSound(SoundID.MenuTick);

                if (!dbtPlayer.SelectedTransformations.Contains(def))
                {
                    dbtPlayer.SelectTransformation(def);
                    Main.NewText($"Selected {def.DisplayName}, Mastery: {Math.Round(def.GetMaxMastery(dbtPlayer) * def.GetCurrentMastery(dbtPlayer), 2)}%");
                }
                else
                    Main.NewText($"{def.DisplayName} Mastery: {Math.Round(100f * def.GetCurrentMastery(dbtPlayer), 2)}%");
            }
        }

        public Mod AuthorMod { get; }
        public bool Visible { get; set; }

        public Texture2D UnknownImageTexture { get; }
        public Texture2D UnknownGrayImageTexture { get; }
        public Texture2D LockedImageTexture { get; }

        public static TransformationDefinition LastActiveTransformationTab { get; internal set; }
    }
}
