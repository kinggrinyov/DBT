using Microsoft.Xna.Framework;
using System;
using DBT.Extensions;
using DBT.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using DBT.Helpers;
using DBT.NPCs.Bosses.FriezaShip.Projectiles;
using DBT.NPCs.Bosses.FriezaShip.Minions;
using DBT.NPCs.Bosses.FriezaShip.Items;
using DBT.NPCs.Saibas;
using DBT.Projectiles;

namespace DBT.NPCs.Bosses.FriezaShip
{
    [AutoloadBossHead]
    //Thanks a bit to examplemod's flutterslime for helping with organization
    public class FriezaShip : ModNPC
    {
        public const int
            STAGE_HOVER = 0,
            STAGE_SLAM = 1,
            STAGE_SLAMBARRAGE = 2,
            STAGE_MINION = 3,
            STAGE_HYPER = 4,

            AI_STAGE_SLOT = 0,
            AI_TIMER_SLOT = 1;

        public FriezaShip()
        {
            HoverDistance = new Vector2(0, 150);
            HoverCooldown = 500;
            SlamDelay = 10;
            MinionAmount = 2;
            HasDoneExplodeEffect = false;
            IsChargingSlam = false;

        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("A Frieza Force Ship");
            Main.npcFrameCount[npc.type] = 8;
            NPCID.Sets.TrailingMode[npc.type] = 2;
            NPCID.Sets.TrailCacheLength[npc.type] = 6;
        }

        public override void SetDefaults()
        {
            npc.width = 220;
            npc.height = 120;
            npc.damage = 36;
            npc.defense = 28;
            npc.lifeMax = 3600;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath2;
            npc.value = Item.buyPrice(0, 3, 25, 80);
            npc.knockBackResist = 0f;
            npc.aiStyle = -1;
            npc.boss = true;
            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.noTileCollide = true;
            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/TheUnexpectedArrival");
            bossBag = mod.ItemType<FFShipBag>();
        }

        //To-Do: Add the rest of the stages to the AI. Make green saibaman code.
        //Make the speed of the ship's movements increase with less health, increase the speed of the projectiles, increase how fast the ship goes down on the dash, finish dash afterimage, make the homing projectiles move faster if the player is flying.
        //Boss loot: Drops Undecided material that's used to create a guardian class armor set (frieza cyborg set). Alternates drops between a weapon and accessory, accessory is arm cannon mk2, weapon is a frieza force beam rifle. Expert item is the mechanical amplifier.
        //Spawn condition: Near the ocean you can find a frieza henchmen, if he runs away then you'll get an indicator saying the ship will be coming the next morning.


        public override void AI()
        {
            Player player = Main.player[npc.target];
            npc.TargetClosest(true);

            //Runaway if no players are alive
            if (!player.active || player.dead)
            {
                npc.TargetClosest(false);
                player = Main.player[npc.target];
                if (!player.active || player.dead)
                {
                    npc.velocity = new Vector2(0f, -10f);

                    if (npc.timeLeft > 10)
                        npc.timeLeft = 10;

                    return;
                }
            }

            //Make sure the stages loop back around
            if (AIStage > 4)
                AIStage = STAGE_HOVER;

            //Speed between stages and general movement speed drastically increased with health lost
            if (npc.life < npc.lifeMax * 0.80f)
            {
                HoverCooldown = 400;
                SpeedAdd = 1f;
                SlamDelay = 8;
                if (npc.life < npc.lifeMax * 0.50f)
                {
                    HoverCooldown = 250;
                    SpeedAdd = 2f;
                    SlamDelay = 6;
                }
                if (npc.life < npc.lifeMax * 0.2f)
                {
                    HoverCooldown = 100;
                    SpeedAdd = 4f;
                    SlamDelay = 4;
                    MinionAmount = 4;
                }
            }

            //General movement (stage 0)
            if (AIStage == STAGE_HOVER)
            {
                //Y Hovering

                if (Main.player[npc.target].position.Y != npc.position.Y + HoverDistance.Y)
                {
                    YHoverTimer++;

                    if (YHoverTimer > 10)
                    {
                        //Thanks UncleDanny for this <3
                        if (Main.player[npc.target].position.Y < npc.position.Y + HoverDistance.Y)
                            npc.velocity.Y -= npc.velocity.Y > 0f ? 1f : 0.15f;

                        if (Main.player[npc.target].position.Y > npc.position.Y + HoverDistance.Y)
                            npc.velocity.Y += npc.velocity.Y < 0f ? 1f : 0.15f;
                    }
                }
                else
                {
                    npc.velocity.Y = 0;
                    YHoverTimer = 0;
                }

                //X Hovering, To-Do: Make the ship not just center itself on the player, have some left and right alternating movement?
                if (Vector2.Distance(new Vector2(player.position.X, 0), new Vector2(npc.position.X, 0)) != HoverDistance.X)
                {
                    //float hoverSpeedY = (-2f + Main.rand.NextFloat(-3, -8));
                    XHoverTimer++;
                    if (XHoverTimer > 30)
                    {
                        npc.velocity.X = 2.5f * npc.direction + SpeedAdd * npc.direction;
                        if (AITimer > HoverCooldown / 1.2)
                        {
                            npc.velocity.X = 7f * npc.direction + SpeedAdd * 2 * npc.direction;
                        }

                    }
                }
                else
                {
                    npc.velocity.X = 0;
                    XHoverTimer = 0;
                }

                //Next Stage
                AITimer++;
                if (AITimer > HoverCooldown)
                {
                    StageAdvance();
                    AITimer = 0;
                }

            }

            //Slam attack (stage 1) - Quickly moves to directly above the player, then waits a second before slamming straight down.
            //To-Do: Fix afterimage on slam not working.

            if (AIStage == STAGE_SLAM)
            {
                npc.velocity.X = 0;
                IsChargingSlam = true;
                AITimer++;

                if(AITimer > 180 && IsChargingSlam)
                {
                    IsChargingSlam = false;

                    TeleportAbove();

                    if (AITimer > SlamDelay)
                    {
                        if (AITimer == 180 + SlamDelay)
                        {
                            if (npc.life <= npc.lifeMax * 0.50)//Double the contact damage if below 50% health
                                npc.damage = npc.damage * 2;

                            npc.noTileCollide = false;
                            npc.velocity.Y = 25f;
                        }

                        if (CoordinateExtensions.IsPositionInTile(npc.getRect().Bottom()) || AITimer > 30)//If the bottom of the ship touches a tile, nullify speed and do dust particles
                        {
                            npc.velocity.Y = -8f;
                            if (!HasDoneExplodeEffect)
                            {
                                ExplodeEffect();
                                SoundHelper.PlayCustomSound("Sounds/Kiplosion", npc.position, 1.0f);
                            }

                        }
                    }
                }

                

                if (npc.velocity.Y == -8f)
                    SlamCoolDownTimer++;

                if (SlamCoolDownTimer > 30)
                {
                    StageAdvance();
                    AITimer = 0;
                    SlamCoolDownTimer = 0;
                    npc.noTileCollide = true;
                    HasDoneExplodeEffect = false;

                    if (npc.life <= npc.lifeMax * 0.50)//Reset the damage back to its normal amount
                        npc.damage = npc.damage / 2;
                }

            }

            //To-Do: Summon saibamen (stage 4) - Summons a green saiba from the ship, green dust when this happens to make it look smoother (Perhaps make this something after 40% HP)
            if (Main.netMode != 1 && AIStage == STAGE_MINION)
            {
                if (AITimer == 0)
                {
                    SummonSaiba();
                    SummonFfMinions();
                }

                AITimer++;

                if (AITimer > 60)
                {
                    StageAdvance();
                    AITimer = 0;
                }
            }

            //Main.NewText(AIStage);
        }

        public void TeleportAbove()
        {
            npc.alpha = 255;
            npc.position = Main.player[npc.target].position + new Vector2(-20, -80);
            Projectile.NewProjectile(npc.Center, Vector2.Zero, mod.ProjectileType<TransmissionLinesProj>(), 0, 0);
            SoundHelper.PlayCustomSound("Sounds/ShipTeleport");
            npc.alpha = 0;
        }

        public int SummonSaiba()
        {
            for (int amount = 0; amount < MinionAmount; amount++)
            {
                npc.netUpdate = true;

                switch (Main.rand.Next(0, 3))
                {
                    case 0:
                        return NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, mod.NPCType<Saibaman1>());
                    case 1:
                        return NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, mod.NPCType<Saibaman2>());
                    case 2:
                        return NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, mod.NPCType<Saibaman3>());
                    case 3:
                        return NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, mod.NPCType<Saibaman4>());
                    default:
                        return 0;
                }
            }
            return NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, mod.NPCType<Saibaman1>());
        }

        public int SummonFfMinions()
        {
            for (int amount = 0; amount < MinionAmount; amount++)
            {
                npc.netUpdate = true;
                switch (Main.rand.Next(0, 2))
                {
                    case 0:
                        return NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, mod.NPCType<FriezaForceMinion1>());
                    case 1:
                        return NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, mod.NPCType<FriezaForceMinion2>());
                    case 2:
                        return NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, mod.NPCType<FriezaForceMinion3>());
                    default:
                        return 0;
                }
            }
            return NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, mod.NPCType<FriezaForceMinion1>());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (AIStage == STAGE_SLAM)
            {
                float extraDrawY = Main.NPCAddHeight(npc.whoAmI);
                Vector2 drawOrigin = new Vector2(Main.npcTexture[npc.type].Width / 2, Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type] / 2);
                for (int k = 0; k < npc.oldPos.Length; k++)
                {
                    Vector2 drawPos = new Vector2(npc.position.X - Main.screenPosition.X + npc.width / 2 - (float)Main.npcTexture[npc.type].Width * npc.scale / 2f + drawOrigin.X * npc.scale,
                        npc.position.Y - Main.screenPosition.Y + npc.height - Main.npcTexture[npc.type].Height * npc.scale / Main.npcFrameCount[npc.type] + 4f + extraDrawY + drawOrigin.Y * npc.scale + npc.gfxOffY);

                    Color color = npc.GetAlpha(lightColor) * ((float)(npc.oldPos.Length - k) / (float)npc.oldPos.Length);
                    spriteBatch.Draw(Main.npcTexture[npc.type], drawPos, npc.frame, color, npc.rotation, drawOrigin, npc.scale, SpriteEffects.None, 0f);
                }
            }
            return true;
        }

        private void StageAdvance()
        {
            AIStage++;
        }

        //Animations
        int _frame = 0;
        int _frameTimer = 0;
        int _frameRate = 1;
        public override void FindFrame(int frameHeight)
        {
            if(IsChargingSlam)
            {
                npc.frameCounter += _frameRate;
                _frameTimer++;
                if(_frameTimer > 30)
                {
                    _frameRate = 2;
                    if(_frameTimer > 50)
                    {
                        _frameRate = 3;
                        if(_frameTimer > 80)
                        {
                            _frameRate = 4;
                        }
                    }
                }
            }
            else
            {
                npc.frameCounter++;
                _frameRate = 1;
                _frameTimer = 0;
            }
                

            if (npc.frameCounter > 4)
            {
                _frame++;
                npc.frameCounter = 0;
            }

            if (_frame > 7) //Make it 7 because 0 is counted as a frame, making it 8 frames
                _frame = 0;

            npc.frame.Y = frameHeight * _frame;
        }

        public override void NPCLoot()
        {

            if (Main.expertMode)
                npc.DropBossBags();
            else
            {
                int choice = Main.rand.Next(0, 1);

                if (choice == 0)
                    Item.NewItem(npc.getRect(), mod.ItemType<BeamRifle>());

                /*if (choice == 1)
                    Item.NewItem(npc.getRect(), mod.ItemType<HenchBlast>());*/

                Item.NewItem(npc.getRect(), mod.ItemType<CyberneticParts>(), Main.rand.Next(7, 18));
                Item.NewItem(npc.getRect(), mod.ItemType<ArmCannonMK2>());
            }

            if (!DBTWorld.downedFriezaShip)
            {
                DBTWorld.downedFriezaShip = true;

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.WorldData);
            }
        }

        public void ExplodeEffect()
        {
            for (int num619 = 0; num619 < 3; num619++)
            {
                float scaleFactor9 = 3f;

                if (num619 == 1)
                    scaleFactor9 = 3f;

                int num620 = Gore.NewGore(new Vector2(npc.position.X, npc.position.Y), default(Vector2), Main.rand.Next(61, 64), 1f);
                Main.gore[num620].velocity *= scaleFactor9;

                Gore gore97 = Main.gore[num620];
                gore97.velocity.X = gore97.velocity.X + 1f;

                Gore gore98 = Main.gore[num620];
                gore98.velocity.Y = gore98.velocity.Y + 1f;

                num620 = Gore.NewGore(new Vector2(npc.position.X, npc.position.Y), default(Vector2), Main.rand.Next(61, 64), 1f);
                Main.gore[num620].velocity *= scaleFactor9;

                Gore gore99 = Main.gore[num620];
                gore99.velocity.X = gore99.velocity.X - 1f;

                Gore gore100 = Main.gore[num620];
                gore100.velocity.Y = gore100.velocity.Y + 1f;

                num620 = Gore.NewGore(new Vector2(npc.position.X, npc.position.Y), default(Vector2), Main.rand.Next(61, 64), 1f);
                Main.gore[num620].velocity *= scaleFactor9;

                Gore gore101 = Main.gore[num620];
                gore101.velocity.X = gore101.velocity.X + 1f;

                Gore gore102 = Main.gore[num620];
                gore102.velocity.Y = gore102.velocity.Y - 1f;

                num620 = Gore.NewGore(new Vector2(npc.position.X, npc.position.Y), default(Vector2), Main.rand.Next(61, 64), 1f);
                Main.gore[num620].velocity *= scaleFactor9;

                Gore gore103 = Main.gore[num620];
                gore103.velocity.X = gore103.velocity.X - 1f;

                Gore gore104 = Main.gore[num620];
                gore104.velocity.Y = gore104.velocity.Y - 1f;
            }

            HasDoneExplodeEffect = true;
        }

        public Vector2 HoverDistance { get; set; }

        public float HoverCooldown { get; set; }
        public int SlamDelay { get; set; }
        public int SlamCoolDownTimer { get; set; }
        public int MinionAmount { get; set; }
        public bool HasDoneExplodeEffect { get; set; }
        public bool IsChargingSlam { get; set; }
        public float SpeedAdd { get; set; }

        public int YHoverTimer { get; set; }
        public int XHoverTimer { get; set; }

        public float AIStage
        {
            get { return npc.ai[AI_STAGE_SLOT]; }
            set { npc.ai[AI_STAGE_SLOT] = value; }
        }

        public float AITimer
        {
            get { return npc.ai[AI_TIMER_SLOT]; }
            set { npc.ai[AI_TIMER_SLOT] = value; }
        }
    }
}
