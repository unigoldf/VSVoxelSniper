using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using VSVoxelSniper.Brushes;

namespace VSVoxelSniper {
    internal class ServerSideUserWorkspace {

        //for storing server side variable and executing brush calls when packets are received from the player.
        //each player is instanciated their own instance of this on join

        public string PlayerID;
        public ICoreServerAPI sapi;
        public ICoreAPI api;
        IBlockAccessorRevertable bar;

        CloneStamp cs;
        TreeGeneration tg;
        public HeightBrush heightBrush;
        private int LastHeightBrushRotation = 0;

        public List<BlockPos> GunpowderHistory = new List<BlockPos>();
        public List<BlockPos> ArrowHistory = new List<BlockPos>();

        public string Undo(int steps = 1) {
            if (bar == null) {
                bar = sapi.World.GetBlockAccessorRevertable(true, true);
            }

            if (bar.CurrentHistoryState == bar.AvailableHistoryStates) {
                return "Undo Failed. Cannot revert further.";
            }
            int CurState = bar.CurrentHistoryState;

            bar.ChangeHistoryState(steps);
            int quanityChanged = Math.Abs(CurState - bar.CurrentHistoryState);
            if (quanityChanged != steps) {
                return "Successfully reverted " + quanityChanged + " steps, but failed to revert further.";
            }
            return "Successfully reverted " + quanityChanged + " steps.";
        }

        public void HandleBrushStroke(IPlayer player, BrushDataPacket p) {

            //double starttime = ((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds) / 1000;

            if (!player.HasPrivilege("vsvoxelsniperuser")) { return; }

            if (bar == null) {
                bar = sapi.World.GetBlockAccessorRevertable(true, true);
            }

            if (p.Material == null) {
                return;
            }
            if (p.ReplaceMaterial == null) {
                return;
            }

            int brushsize = 0;
            Random rand = new Random();
            if (!p.IsModified){
                brushsize = rand.Next(p.UnmodifiedBrushSize[0], p.UnmodifiedBrushSize[1]);
            }
            else{
                brushsize = rand.Next(p.ModifiedBrushSize[0], p.ModifiedBrushSize[1]);
            }

            BlockPos pos = new BlockPos(player.Entity.Pos.Dimension);
            pos.X = p.BlockPos.X; pos.Y = p.BlockPos.Y; pos.Z = p.BlockPos.Z;

            if (p.brush == SniperData.BrushTypes.snipe) {
                SniperData.SetBlock(bar, p.performer, pos, p.Material, p.ReplaceMaterial, true);
            }
            else if (p.brush == SniperData.BrushTypes.ball) {
                //double onetime = ((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds) / 1000;
                List<BlockPos> ball = Shapes.ball(pos, brushsize);
                // double oneend = ((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds) / 1000;
                // SendChatMessage(player, "calculate delay: " + (oneend - onetime).ToString());

                // double twotime = ((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds) / 1000;
                SniperData.SetBlocks(ball, bar, p.performer, p.Material, p.ReplaceMaterial);
                //double twoend = ((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds) / 1000;
                //SendChatMessage(player, "set delay: " + (twoend - twotime).ToString());
            }
            else if (p.brush == SniperData.BrushTypes.voxel) {
                List<BlockPos> voxel = Shapes.voxel(pos, brushsize);
                SniperData.SetBlocks(voxel, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.disk) {
                List<BlockPos> disk = Shapes.disk(pos, brushsize);
                SniperData.SetBlocks(disk, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.voxeldisk) {
                List<BlockPos> disk = Shapes.voxeldisk(pos, brushsize);
                SniperData.SetBlocks(disk, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.diskface) {
                List<BlockPos> disk = Shapes.disk(pos, brushsize);
                disk = Shapes.face(pos, disk, p.face);
                SniperData.SetBlocks(disk, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.voxeldiskface) {
                List<BlockPos> disk = Shapes.voxeldisk(pos, brushsize);
                disk = Shapes.face(pos, disk, p.face);
                SniperData.SetBlocks(disk, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.cylinder) {
                List<BlockPos> cylinder = Shapes.Cylinder(pos, p.VoxelHeight, brushsize);
                SniperData.SetBlocks(cylinder, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.ray) {
                List<BlockPos> ray = Shapes.line(new BlockPos((int)player.Entity.Pos.X, (int)player.Entity.Pos.Y + 1, (int)player.Entity.Pos.Z), pos);
                SniperData.SetBlocks(ray, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.tree) {
                if (tg == null) {
                    tg = new TreeGeneration();
                }
                tg.PlaceTree(p, pos, sapi, bar, p.TreeSizeRange);
            }
            else if (p.brush == SniperData.BrushTypes.forest) {
                if (tg == null) {
                    tg = new TreeGeneration();
                }
                List<BlockPos> points = Shapes.forest(bar, pos, brushsize, p.TreeDensity, p);
                foreach (BlockPos point in points) {
                    tg.PlaceTree(p, point, sapi, bar, p.TreeSizeRange, false);
                }
                bar.Commit();
            }
            else if (p.brush == SniperData.BrushTypes.filldown) {
                List<BlockPos> positions = Fill.FillDown(bar, p, brushsize);
                SniperData.SetBlocks(positions, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.fillfluid) {
                List<BlockPos> positions = Fill.FillLiquid(bar, p, brushsize);
                Fill.PlaceLiquid(bar, positions, p);
            }
            else if (p.brush == SniperData.BrushTypes.heightbrush){
                if (p.tool == SniperData.ToolType.gunpowder){
                    List<BlockPos> positions = Shapes.ball(pos, brushsize);
                    SniperData.SetBlocks(positions, bar, SniperData.PerformerTypes.Override, p.Material, p.ReplaceMaterial);
                }
                else{
                    List<BlockPos> positions = heightBrush.HeightBrushOperation(pos, brushsize, p.VoxelHeight,
                        ref LastHeightBrushRotation, p, bar, player);
                    if (positions.Count == 0){
                        return;
                    }

                    int[] air = new int[]{ 0 };
                    if (p.HeightBrushMode == SniperData.HeightBrushModes.invert){
                        SniperData.SetBlocks(positions, bar, SniperData.PerformerTypes.Material, air);
                    }
                    else{
                        SniperData.SetBlocks(positions, bar, SniperData.PerformerTypes.MaterialReplace, p.Material,
                            air);
                    }
                }
            }
            else if (p.brush == SniperData.BrushTypes.blendball) {
                Blend.BlendBall(bar, p, brushsize);
            }
            else if (p.brush == SniperData.BrushTypes.blendvoxel) {
                Blend.BlendVoxel(bar, p, brushsize);
            }
            else if (p.brush == SniperData.BrushTypes.blenddisk) {
                Blend.BlendDisk(bar, p, brushsize);
            }
            else if (p.brush == SniperData.BrushTypes.blendvoxeldisk) {
                Blend.BlendVoxelDisk(bar, p, brushsize);
            }
            else if (p.brush == SniperData.BrushTypes.splatterball) {
                List<BlockPos> positions = Splatter.Splatter3d(bar, p, brushsize, pos, player, true);
                SniperData.SetBlocks(positions, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.splattervoxel) {
                List<BlockPos> positions = Splatter.Splatter3d(bar, p, brushsize, pos, player);
                SniperData.SetBlocks(positions, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.splatterdisk) {
                List<BlockPos> positions = Splatter.Splatter2d(bar, p, brushsize, pos, player, true);
                SniperData.SetBlocks(positions, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.splattervoxeldisk) {
                List<BlockPos> positions = Splatter.Splatter2d(bar, p, brushsize, pos, player);
                SniperData.SetBlocks(positions, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.splatterdiskface) {
                List<BlockPos> positions = Splatter.Splatter2d(bar, p, brushsize, pos, player, true);
                positions = Shapes.face(pos, positions, p.face);
                SniperData.SetBlocks(positions, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.splattervoxeldiskface) {
                List<BlockPos> positions = Splatter.Splatter2d(bar, p, brushsize, pos, player);
                positions = Shapes.face(pos, positions, p.face);
                SniperData.SetBlocks(positions, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.spike) {
                List<BlockPos> positions = Shapes.Spike(pos, p.PlayerEyePos.ToVec3f().Add(new Vec3f(p.PlayerPos.X, p.PlayerPos.Y, p.PlayerPos.Z)), brushsize, p.VoxelHeight);
                SniperData.SetBlocks(positions, bar, p.performer, p.Material, p.ReplaceMaterial);
            }
            else if (p.brush == SniperData.BrushTypes.clonestamp) {
                if (cs == null) {
                    cs = new CloneStamp();
                }
                if (p.tool == SniperData.ToolType.gunpowder) {
                    if (p.IsModified) {
                        cs.ClearCloneStampQueue(player, sapi);
                    }
                    else {
                        cs.Clone(bar, sapi, p, brushsize, player);
                    }
                }
                else if (p.tool == SniperData.ToolType.arrow) {
                    if (p.CloneStampForestOption == true) {
                        List<BlockPos> points = Shapes.forest(bar, pos, brushsize, p.CloneStampForestDensity, p);
                        foreach (BlockPos point in points) {
                            cs.Stamp(bar, sapi, p, point, player, false);
                        }
                        bar.Commit();
                        return;
                    }
                    else {
                        cs.Stamp(bar, sapi, p, pos, player, true);
                    }
                    return;
                }
            }

            else if (p.brush == SniperData.BrushTypes.set) {
                if (p.tool == SniperData.ToolType.gunpowder) {
                    GunpowderHistory.Clear();
                    if (p.IsModified) {
                        GunpowderHistory.Add(new BlockPos(p.PlayerPos.X, p.PlayerPos.Y, p.PlayerPos.Z));
                    }
                    else {
                        GunpowderHistory.Add(new BlockPos(p.BlockPos.X, p.BlockPos.Y, p.BlockPos.Z));
                    }
                    SendChatMessage(player, "First Point Set.");
                }
                if (p.tool == SniperData.ToolType.arrow) {
                    ArrowHistory.Clear();
                    if (p.IsModified) {
                        ArrowHistory.Add(new BlockPos(p.PlayerPos.X, p.PlayerPos.Y, p.PlayerPos.Z));
                    }
                    else {
                        ArrowHistory.Add(new BlockPos(p.BlockPos.X, p.BlockPos.Y, p.BlockPos.Z));
                    }
                    List<BlockPos> Positions = Shapes.TwoPointVolume(GunpowderHistory[0].ToVec3i(), ArrowHistory[0].ToVec3i());
                    if (Positions == null) {
                        SendChatMessage(player, "Volume limit exceeded. 10,000,000 Blocks");
                        return;
                    }
                    SendChatMessage(player, "Second Point Set. Placing " + Positions.Count + " Blocks.");
                    SniperData.SetBlocks(Positions, bar, p.performer, p.Material, p.ReplaceMaterial);
                }
            }
            else if (p.brush == SniperData.BrushTypes.line) {
                if (p.tool == SniperData.ToolType.gunpowder) {
                    GunpowderHistory.Clear();
                    if (p.IsModified) {
                        GunpowderHistory.Add(new BlockPos(p.PlayerPos.X, p.PlayerPos.Y, p.PlayerPos.Z));
                    }
                    else {
                        GunpowderHistory.Add(new BlockPos(p.BlockPos.X, p.BlockPos.Y, p.BlockPos.Z));
                    }
                    SendChatMessage(player, "First Point Set.");
                }
                else if (p.tool == SniperData.ToolType.arrow) {
                    if (GunpowderHistory.Count > 0) {
                        ArrowHistory.Clear();
                        if (p.IsModified) {
                            ArrowHistory.Add(new BlockPos(p.PlayerPos.X, p.PlayerPos.Y, p.PlayerPos.Z));
                        }
                        else {
                            ArrowHistory.Add(new BlockPos(p.BlockPos.X, p.BlockPos.Y, p.BlockPos.Z));
                        }
                        List<BlockPos> line = Shapes.line(ArrowHistory[0], GunpowderHistory[0]);
                        SendChatMessage(player, "Second Point set. Placing " + line.Count + " Blocks.");
                        SniperData.SetBlocks(line, bar, p.performer, p.Material, p.ReplaceMaterial);
                    }
                }
            }
            else if (p.brush == SniperData.BrushTypes.overlay) {
                List<BlockPos> blocks = Overlay.DoOverlay(bar, p, brushsize);
                Overlay.OverlaySetBlocks(bar, blocks, p.Material);
            }
            else if (p.brush == SniperData.BrushTypes.splatteroverlay) {
                List<BlockPos> splatterdisk = Splatter.Splatter2d(bar, p, brushsize, pos, player, true);
                List<BlockPos> blocks = Overlay.DoOverlay(bar, p, brushsize, splatterdisk);
                Overlay.OverlaySetBlocks(bar, blocks, p.Material);
            }
            else if (p.brush == SniperData.BrushTypes.erode) {
                Erosion.Erode(player, p, brushsize, bar);
            }
            else if (p.brush == SniperData.BrushTypes.drain) {
                List<BlockPos> ball = Shapes.ball(pos, brushsize);
                SniperData.Drain(ball, bar);
            }
            else if (p.brush == SniperData.BrushTypes.entityremoval){
                Entities en = new Entities();
                int count = en.RemoveEntities(sapi, p, brushsize);
                SendChatMessage(player, "Removed " + count + " entities.");
            }
            else if (p.brush == SniperData.BrushTypes.ruler) {
                if (p.tool == SniperData.ToolType.arrow){
                    if (GunpowderHistory.Count > 0) {
                        ArrowHistory.Clear();
                        double dist = 0;
                        if (p.IsModified) {
                            ArrowHistory.Add(new BlockPos(p.PlayerPos.X, p.PlayerPos.Y, p.PlayerPos.Z));
                            dist = Shapes.Vec3iDistance(GunpowderHistory[0].AsVec3i, p.PlayerPos);
                        }
                        else {
                            ArrowHistory.Add(new BlockPos(p.BlockPos.X, p.BlockPos.Y, p.BlockPos.Z));
                            dist = Shapes.Vec3iDistance(GunpowderHistory[0].AsVec3i, p.BlockPos.AsVec3i);
                        }
                        SendChatMessage(player, "Distance is " + dist + " Blocks.");
                    } 
                }
                if (p.tool == SniperData.ToolType.gunpowder){
                    GunpowderHistory.Clear();
                    if (p.IsModified) {
                        GunpowderHistory.Add(new BlockPos(p.PlayerPos.X, p.PlayerPos.Y, p.PlayerPos.Z));
                    }
                    else {
                        GunpowderHistory.Add(new BlockPos(p.BlockPos.X, p.BlockPos.Y, p.BlockPos.Z));
                    }
                    SendChatMessage(player, "First Point Set.");
                }
            }
            else if (p.brush == SniperData.BrushTypes.teleport) {
                player.Entity.TeleportTo(p.BlockPos.X, p.BlockPos.Y + 1, p.BlockPos.Z);
            }
            else if (p.brush == SniperData.BrushTypes.lightning) {
                WeatherSystemServer wss = sapi.ModLoader.GetModSystem<WeatherSystemServer>(true);
                Vec3d lpos = new Vec3d(p.BlockPos.AsVec3i.X, p.BlockPos.AsVec3i.Y, p.BlockPos.AsVec3i.Z);
                wss.SpawnLightningFlash(lpos);
            }

            //double endtime = ((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds) / 1000;
            //SendChatMessage(player, "Total delay: " + (endtime - starttime).ToString());

        }
        private BlockPos GetGunpowderOffsetPos(BlockPos Pos, SniperData.FaceDirection face) {
            if (face == SniperData.FaceDirection.Up) {
                Pos.Y++;
                return Pos;
            }
            if (face == SniperData.FaceDirection.North) {
                Pos.Z--;
                return Pos;
            }
            if (face == SniperData.FaceDirection.South) {
                Pos.Z++;
                return Pos;
            }
            if (face == SniperData.FaceDirection.East) {
                Pos.X++;
                return Pos;
            }
            if (face == SniperData.FaceDirection.West) {
                Pos.X--;
                return Pos;
            }
            if (face == SniperData.FaceDirection.Down) {
                Pos.Y--;
                return Pos;
            }
            return Pos;
        }
        private void SendChatMessage(IPlayer player, string text) {
            TextMessage message = new TextMessage();
            message.text = text;
            sapi.Network.GetChannel("VintageStoryVoxelSniper").SendPacket(message, (IServerPlayer)player);
        }
    }
}
