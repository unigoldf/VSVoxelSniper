using System;
using static System.Math;
using System.Collections.Generic;
using System.Numerics;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;


namespace VSVoxelSniper.src.Brushes {
    class CloneStamp {


        List<BlockSchematic> Schematics = new List<BlockSchematic>();
        List<Vector4> Offsets = new List<Vector4>();
        int queuelimit = 10;

        int CurrentRotation = 0;
        int SchematicIndex = -1;

        public void Clone(IBlockAccessorRevertable bar, ICoreServerAPI sapi, BrushDataPacket p, IPlayer player) {
            int dim = player.Entity.Pos.Dimension;
            BlockPos pos1 = new BlockPos(new Vec3i(p.BlockPos.X - p.brushsize, p.BlockPos.Y - p.VoxelCentroid, p.BlockPos.Z - p.brushsize), dim);
            BlockPos pos2 = new BlockPos(new Vec3i(p.BlockPos.X + p.brushsize, (p.BlockPos.Y - p.VoxelCentroid) + p.VoxelHeight, p.BlockPos.Z + p.brushsize), dim);

            BlockSchematic schem = new BlockSchematic();
            schem.AddArea(sapi.World, pos1, pos2);
            schem.Pack(sapi.World, new BlockPos(p.BlockPos, dim));
            CurrentRotation = 0;

            int xmax = (schem.PackedOffset.X + schem.SizeX - 1);
            int xmin = schem.PackedOffset.X;
            int zmax = (schem.PackedOffset.Z + schem.SizeZ - 1);
            int zmin = schem.PackedOffset.Z;

            if (p.CloneStampRotationMode == SniperData.CloneStampRotationModes.none) {
                CurrentRotation = 0;
            }

            if (p.CloneStampQueueMode == SniperData.CloneStampQueueModes.single) {
                Offsets.Clear();
                Offsets.Add(new Vector4(xmax, xmin, zmax, zmin));
                Schematics.Clear();
                Schematics.Add(schem);
            }
            else {
                Offsets.Add(new Vector4(xmax, xmin, zmax, zmin));
                if (Offsets.Count > queuelimit) {
                    Offsets.RemoveAt(0);
                }
                Schematics.Add(schem);
                if (Schematics.Count > queuelimit) {
                    Schematics.RemoveAt(0);
                }
            }
            SendChatMessage(player, Environment.NewLine + "Copied", sapi);
        }

        public void Stamp(IBlockAccessorRevertable bar, ICoreServerAPI sapi, BrushDataPacket p, BlockPos pos, IPlayer player, bool commit) {
            if (Schematics.Count < 1) { return; }
            int dim = player.Entity.Pos.Dimension;

            if (p.CloneStampQueueMode == SniperData.CloneStampQueueModes.random) {
                Random random = new Random();
                SchematicIndex = random.Next(0, Schematics.Count);
            }
            else {
                SchematicIndex++;
                if (SchematicIndex > (Schematics.Count - 1)) {
                    SchematicIndex = 0;
                }
            }

            BlockSchematic schem = Schematics[SchematicIndex];

            EnumReplaceMode mode;
            if (p.CloneStampMode == SniperData.CloneStampModes.fill) {
                mode = EnumReplaceMode.ReplaceOnlyAir;
            }
            else if (p.CloneStampMode == SniperData.CloneStampModes.excludeair) {
                mode = EnumReplaceMode.ReplaceAllNoAir;
            }
            else {
                mode = EnumReplaceMode.ReplaceAll;
            }

            BlockSchematic placingschem = schem.ClonePacked();

            Vector2 offset = GetTargetOffset(CurrentRotation);
            BlockPos Target = new BlockPos(new Vec3i(pos.X + (int)offset.X, pos.Y, pos.Z + (int)offset.Y), dim);

            placingschem.TransformWhilePacked(sapi.World, EnumOrigin.StartPos, (int)CurrentRotation);
            placingschem.Place(bar, sapi.World, Target, mode);
            placingschem.PlaceDecors(bar, Target);
            if (commit) {
                bar.Commit();
            }

            //SendChatMessage(player, "asdf", sapi);

            if (p.CloneStampRotationMode == SniperData.CloneStampRotationModes.cycle) {
                //SendChatMessage(player, "cycle", sapi);
                CurrentRotation += 90;
                if (CurrentRotation == 360) {
                    CurrentRotation = 0;
                }
            }
            else if (p.CloneStampRotationMode == SniperData.CloneStampRotationModes.random) {
                //SendChatMessage(player, "random", sapi);
                int[] angles = { 0, 90, 180, 270 };
                Random random = new Random();
                CurrentRotation = angles[random.Next(0,3)];
            }

        }
        public void ClearCloneStampQueue(IPlayer player, ICoreServerAPI sapi) {
            Offsets.Clear();
            Schematics.Clear();
            CurrentRotation = 0;
            SchematicIndex = 0;
            SendChatMessage(player, Environment.NewLine + "Clone Stamp Clipboard Cleared", sapi);
        }

        private Vector2 GetTargetOffset(int rotation) {//this is ass but I'm retarded
            if (rotation == 0) {
                return new Vector2(Offsets[0].Y, Offsets[0].W);
            }
            if (rotation == 90) {
                return new Vector2(-Offsets[0].Z, Offsets[0].Y);
            }
            if (rotation == 180) {
                return new Vector2(-Offsets[0].X, -Offsets[0].Z);
            }
            if (rotation == 270) {
                return new Vector2(Offsets[0].W, -Offsets[0].X);
            }
            return new Vector2(0, 0);
        }
        private void SendChatMessage(IPlayer player, string text, ICoreServerAPI sapi) {
            TextMessage message = new TextMessage();
            message.text = text;
            sapi.Network.GetChannel("VintageStoryVoxelSniper").SendPacket(message, (IServerPlayer)player);
        }
    }
}
