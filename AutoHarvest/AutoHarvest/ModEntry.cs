using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace AutoHarvest
{
    using SVObject = StardewValley.Object;
    using Player = StardewValley.Farmer;

    public class ModEntry : Mod
    {
        private static string version;
        public static string VERSION
        {
            get { return version; }
        }

        private Config config;
        private byte ticksElapsed;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();
            if(!config.Enabled)
            {
                return;
            }

            {
                //Creates version info using manifest.json
                IManifest manifest = helper.ModRegistry.Get(helper.ModRegistry.ModID);
                ISemanticVersion versionInfo = manifest.Version;
                version = string.Format("{0}.{1}.{2}", versionInfo.MajorVersion, versionInfo.MinorVersion, versionInfo.PatchVersion);
            }

            if (config.CheckUpdate)
            {
                UpdateChecker.CheckUpdate(Monitor);
            }

            GameEvents.UpdateTick += OnOneSecondElapsed;
        }

        private void OnOneSecondElapsed(object sender, EventArgs args)
        {
            if(!Context.IsWorldReady || config.TicksInterval == 0)
            {
                return;
            }
            Player player = Game1.player;
            ticksElapsed = (byte)((ticksElapsed + 1) % config.TicksInterval);
            if(ticksElapsed == 0)
            {
                Rectangle bb = ExpandBoundingBox(Game1.player.GetBoundingBox(), Game1.tileSize * 5, Game1.tileSize * 5);
                GameLocation location = Game1.currentLocation;

                //AutoHarvest Grown Crops
                if (config.AutoHarvestCrops)
                {
                    foreach (Vector2 position in location.terrainFeatures.Keys)
                    {
                        TerrainFeature feature = location.terrainFeatures[position];
                        if (feature is HoeDirt dirt)
                        {
                            Crop crop = dirt.crop;
                            if (crop == null)
                            {
                                continue;
                            }
                            if (config.AutoDestroyDeadCrops && crop.dead)
                            {
                                dirt.destroyCrop(position);
                                continue;
                            }
                            if (!crop.fullyGrown || !bb.Intersects(feature.getBoundingBox(position)))
                            {
                                continue;
                            }
                            crop.harvest((int)position.X, (int)position.Y, dirt);
                        }
                    }
                }

                //Collecting Forages
                if (config.AutoCollectForages)
                {
                    ICollection<Vector2> keys = new List<Vector2>(location.Objects.Keys);
                    foreach (Vector2 vec in keys)
                    {
                        SVObject obj = location.Objects[vec];
                        if (!bb.Intersects(obj.boundingBox))
                        {
                            continue;
                        }
                        if (obj.isForage(location))
                        {
                            Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)vec.X + (int)vec.Y * 777);
                            if (player.professions.Contains(16))
                            {
                                obj.quality = 4;
                            }
                            else
                            {
                                if (random.NextDouble() < player.ForagingLevel / 30f)
                                {
                                    obj.quality = 2;
                                }
                                else if (random.NextDouble() < player.ForagingLevel / 15f)
                                {
                                    obj.quality = 1;
                                }
                            }
                            if (player.couldInventoryAcceptThisItem(obj))
                            {
                                if (player.IsMainPlayer)
                                {
                                    Game1.playSound("pickUpItem");
                                    DelayedAction.playSoundAfterDelay("coin", 300);
                                }
                                player.animateOnce(279 + player.FacingDirection);
                                if (!location.isFarmBuildingInterior())
                                {
                                    player.gainExperience(2, 7);
                                }
                                else
                                {
                                    player.gainExperience(0, 5);
                                }
                                player.addItemToInventoryBool(obj.getOne(), false);
                                Stats expr_70E = Game1.stats;
                                uint itemsForaged = expr_70E.ItemsForaged;
                                expr_70E.ItemsForaged = itemsForaged + 1u;
                                if (player.professions.Contains(13) && random.NextDouble() < 0.2 && !obj.questItem && player.couldInventoryAcceptThisItem(obj) && !location.isFarmBuildingInterior())
                                {
                                    player.addItemToInventoryBool(obj.getOne(), false);
                                    player.gainExperience(2, 7);
                                }
                                location.Objects.Remove(vec);
                                return;
                            }
                        }
                    }
                }
            }
        }
        private static Rectangle ExpandBoundingBox(Rectangle parent, int dx, int dy)
        {
            return new Rectangle(parent.X - dx, parent.Y - dy, parent.Width + 2 * dx, parent.Height + 2 * dy);
        }
    }
}
