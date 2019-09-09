﻿using System;
using System.Collections.Generic;
using DBT.Commons;
using DBT.Players;
using DBT.Transformations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace DBT.HairStyles
{
    public class HairStyle : IHasUnlocalizedName
    {
        public const string HAIR_STYLE_SUFFIX = "HairStyle";

        protected readonly Dictionary<string, Texture2D> texturesPerTransformationUnlocalized = new Dictionary<string, Texture2D>();

        public HairStyle(int xOffsetRight = 0, int yOffsetRight = 0, int xOffsetLeft = 0, int yOffsetLeft = 0, bool autoBobbing = true, string hairStyleSuffix = HAIR_STYLE_SUFFIX)
        {
            //XOffsetRight = xOffsetRight;

            Offset = new Vector4(xOffsetRight, -yOffsetRight, xOffsetLeft, -yOffsetLeft);
            AutoBobbing = autoBobbing;

            Initialize(this.GetType(), hairStyleSuffix);
        }


        private void Initialize(Type type, string hairStyleSuffix = HAIR_STYLE_SUFFIX)
        {
            string[] segments = type.Namespace.Split('.');
            string root = string.Join("/", segments, 1, segments.Length - 1) + '/';

            UnlocalizedName = type.Name.Replace(hairStyleSuffix, "");
            RootPath = root;

            Mod mod = ModLoader.GetMod(segments[0]);

            string basePath = RootPath + "Base";

            if (mod.TextureExists(basePath))
                Base = mod.GetTexture(basePath);

            foreach (string key in TransformationDefinitionManager.Instance.Keys)
            {
                if (mod.TextureExists(RootPath + key))
                {
                    texturesPerTransformationUnlocalized.Add(key, mod.GetTexture(RootPath + key));
                }
            }      
        }

        public virtual bool CanAccess() => true;
        
        public Texture2D this[TransformationDefinition transformation] => transformation.IsManualLookup ? 
            this[transformation.ManualHairLookup] : this[transformation.UnlocalizedName];

        public Texture2D this[string key]
        {
            get
            {
                if (!texturesPerTransformationUnlocalized.ContainsKey(key))
                    return Base;

                return texturesPerTransformationUnlocalized[key];
            }
        }

        public virtual Vector4 GetHairOffset(DBTPlayer dbtPlayer) => Offset;

        public string UnlocalizedName { get; private set; }

        public string RootPath { get; protected set; }


        public Vector4 Offset { get; protected set; }

        /*#region Offset Hooks

        public virtual int GetXOffsetRight(DBTPlayer dbtPlayer) => XOffsetRight;
        /*public virtual int GetYOffsetRight(DBTPlayer dbtPlayer) => YOffsetRight;
        public virtual int GetXOffsetLeft(DBTPlayer dbtPlayer) => XOffsetLeft;
        public virtual int GetYOffsetLeft(DBTPlayer dbtPlayer) => YOffsetLeft;
        public virtual int XOffsetRight { get; protected set; }
        /*public virtual int YOffsetRight { get; protected set; } = 0;
        public virtual int XOffsetLeft { get; protected set; } = 0;
        public virtual int YOffsetLeft { get; protected set; } = 0;

        #endregion*/

        public bool AutoBobbing { get; }


        public Texture2D Base { get; private set; }
    }
}
