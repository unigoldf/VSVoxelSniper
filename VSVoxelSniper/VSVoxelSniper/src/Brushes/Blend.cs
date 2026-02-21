using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using VSCreativeMod;

namespace VSVoxelSniper.src.Brushes {
    internal static class Blend {

        private static Vec3i[] BlockOffsets = {
            new Vec3i(0, 0, 1),
            new Vec3i(0, 0, -1),
            new Vec3i(0, 1, 0),
            new Vec3i(0, -1, 0),
            new Vec3i(1, 0, 0),
            new Vec3i(-1, 0, 0)
        };
        private static Vec3i[] BlockOffsets2d = {
            new Vec3i(0, 0, 1),
            new Vec3i(0, 0, -1),
            new Vec3i(1, 0, 0),
            new Vec3i(-1, 0, 0)
        };

        public static void BlendVoxel(IBlockAccessorRevertable bar, BrushDataPacket p) {
            bool Exclude = true;
            if (p.tool == SniperData.ToolType.gunpowder) {
                Exclude = false;
            }

            Block air = bar.GetBlock(0);

            Dictionary<BlockPos, Block> NewBlocks = new Dictionary<BlockPos, Block>();

            for (int x = p.BlockPos.X - p.brushsize; x <= p.BlockPos.X + p.brushsize; x++) {
                for (int y = p.BlockPos.Y - p.brushsize; y <= p.BlockPos.Y + p.brushsize; y++) {
                    for (int z = p.BlockPos.Z - p.brushsize; z <= p.BlockPos.Z + p.brushsize; z++) {
                        BlockPos pos = new BlockPos(x, y, z);
                        Block block = bar.GetBlock(pos);
                        if (!Exclude && (block.BlockId != 0 && !block.IsLiquid())) { continue; }

                        List<Block> NeightboringMaterials = new List<Block>();

                        for (int i = 0; i < BlockOffsets.Length; i++) {
                            BlockPos NeightborPos = new BlockPos(x + BlockOffsets[i].X, y + BlockOffsets[i].Y, z + BlockOffsets[i].Z);
                            Block NeightborBlock = bar.GetBlock(NeightborPos);
                            if (!Exclude && (NeightborBlock.BlockId == 0 || NeightborBlock.IsLiquid())) { continue; }
                            NeightboringMaterials.Add(NeightborBlock);
                        }

                        if (NeightboringMaterials.Count == 0) { continue; }

                        int solid = 0;
                        int nonsolid = 0;
                        SolidCount(NeightboringMaterials, ref solid, ref nonsolid);

                        Block MostCommon = MostCommon<Block>(NeightboringMaterials);

                        if (solid == nonsolid) {
                            MostCommon = air;
                        }
                        NeightboringMaterials.Clear();
                        NewBlocks.Add(pos, MostCommon);
                    }
                }
            }
            SetBlocks(NewBlocks, bar);
        }
        public static void BlendBall(IBlockAccessorRevertable bar, BrushDataPacket p) {
            bool Exclude = true;
            if (p.tool == SniperData.ToolType.gunpowder) {
                Exclude = false;
            }

            Block air = bar.GetBlock(0);

            Dictionary<BlockPos, Block> NewBlocks = new Dictionary<BlockPos, Block>();
            double AdjustedRadius = p.brushsize + 0.5;
            double radiusSquared = Math.Pow(AdjustedRadius, 2);

            for (int x = p.BlockPos.X - p.brushsize; x <= p.BlockPos.X + p.brushsize; x++) {
                for (int y = p.BlockPos.Y - p.brushsize; y <= p.BlockPos.Y + p.brushsize; y++) {
                    for (int z = p.BlockPos.Z - p.brushsize; z <= p.BlockPos.Z + p.brushsize; z++) {
                        double distance = (p.BlockPos.X - x) * (p.BlockPos.X - x) + (p.BlockPos.Y - y) * (p.BlockPos.Y - y) + (p.BlockPos.Z - z) * (p.BlockPos.Z - z);
                        if (distance > radiusSquared) { continue; }
                        BlockPos pos = new BlockPos(x, y, z);
                        Block block = bar.GetBlock(pos);
                        if (!Exclude && (block.BlockId != 0 && !block.IsLiquid())) { continue; }

                        List<Block> NeightboringMaterials = new List<Block>();

                        for (int i = 0; i < BlockOffsets.Length; i++) {
                            BlockPos NeightborPos = new BlockPos(x + BlockOffsets[i].X, y + BlockOffsets[i].Y, z + BlockOffsets[i].Z);
                            Block NeightborBlock = bar.GetBlock(NeightborPos);
                            if (!Exclude && (NeightborBlock.BlockId == 0 || NeightborBlock.IsLiquid())) { continue; }
                            NeightboringMaterials.Add(NeightborBlock);
                        }

                        if (NeightboringMaterials.Count == 0) { continue; }

                        int solid = 0;
                        int nonsolid = 0;
                        SolidCount(NeightboringMaterials, ref solid, ref nonsolid);

                        Block MostCommon = MostCommon<Block>(NeightboringMaterials);

                        if (solid == nonsolid) {
                            MostCommon = air;
                        }
                        NeightboringMaterials.Clear();
                        NewBlocks.Add(pos, MostCommon);
                    }
                }
            }
            SetBlocks(NewBlocks, bar);
        }
        public static void BlendDisk(IBlockAccessorRevertable bar, BrushDataPacket p) {
            bool Exclude = true;
            if (p.tool == SniperData.ToolType.gunpowder) {
                Exclude = false;
            }

            Block air = bar.GetBlock(0);

            int y = p.BlockPos.Y;

            Dictionary<BlockPos, Block> NewBlocks = new Dictionary<BlockPos, Block>();
            double AdjustedRadius = p.brushsize + 0.5;
            double radiusSquared = Math.Pow(AdjustedRadius, 2);

            for (int x = p.BlockPos.X - p.brushsize; x <= p.BlockPos.X + p.brushsize; x++) {
                for (int z = p.BlockPos.Z - p.brushsize; z <= p.BlockPos.Z + p.brushsize; z++) {
                    double distance = (p.BlockPos.X - x) * (p.BlockPos.X - x) + (p.BlockPos.Z - z) * (p.BlockPos.Z - z);
                    if (distance > radiusSquared) { continue; }
                    BlockPos pos = new BlockPos(x, y, z);
                    Block block = bar.GetBlock(pos);
                    if (!Exclude && (block.BlockId != 0 && !block.IsLiquid())) { continue; }

                    List<Block> NeightboringMaterials = new List<Block>();

                    for (int i = 0; i < BlockOffsets2d.Length; i++) {
                        BlockPos NeightborPos = new BlockPos(x + BlockOffsets2d[i].X, y, z + BlockOffsets2d[i].Z);
                        Block NeightborBlock = bar.GetBlock(NeightborPos);
                        if (!Exclude && (NeightborBlock.BlockId == 0 || NeightborBlock.IsLiquid())) { continue; }
                        NeightboringMaterials.Add(NeightborBlock);
                    }

                    if (NeightboringMaterials.Count == 0) { continue; }

                    int solid = 0;
                    int nonsolid = 0;
                    SolidCount(NeightboringMaterials, ref solid, ref nonsolid);

                    Block MostCommon = MostCommon<Block>(NeightboringMaterials);

                    if (solid == nonsolid) {
                        MostCommon = air;
                    }
                    NeightboringMaterials.Clear();
                    NewBlocks.Add(pos, MostCommon);
                }
            }
            SetBlocks(NewBlocks, bar);
        }
        public static void BlendVoxelDisk(IBlockAccessorRevertable bar, BrushDataPacket p) {
            bool Exclude = true;
            if (p.tool == SniperData.ToolType.gunpowder) {
                Exclude = false;
            }

            Block air = bar.GetBlock(0);

            int y = p.BlockPos.Y;

            Dictionary<BlockPos, Block> NewBlocks = new Dictionary<BlockPos, Block>();

            for (int x = p.BlockPos.X - p.brushsize; x <= p.BlockPos.X + p.brushsize; x++) {
                for (int z = p.BlockPos.Z - p.brushsize; z <= p.BlockPos.Z + p.brushsize; z++) {
                    BlockPos pos = new BlockPos(x, y, z);
                    Block block = bar.GetBlock(pos);
                    if (!Exclude && (block.BlockId != 0 && !block.IsLiquid())) { continue; }

                    List<Block> NeightboringMaterials = new List<Block>();

                    for (int i = 0; i < BlockOffsets2d.Length; i++) {
                        BlockPos NeightborPos = new BlockPos(x + BlockOffsets2d[i].X, y, z + BlockOffsets2d[i].Z);
                        Block NeightborBlock = bar.GetBlock(NeightborPos);
                        if (!Exclude && (NeightborBlock.BlockId == 0 || NeightborBlock.IsLiquid())) { continue; }
                        NeightboringMaterials.Add(NeightborBlock);
                    }

                    if (NeightboringMaterials.Count == 0) { continue; }

                    int solid = 0;
                    int nonsolid = 0;
                    SolidCount(NeightboringMaterials, ref solid, ref nonsolid);

                    Block MostCommon = MostCommon<Block>(NeightboringMaterials);

                    if (solid == nonsolid) {
                        MostCommon = air;
                    }
                    NeightboringMaterials.Clear();
                    NewBlocks.Add(pos, MostCommon);
                }
            }
            SetBlocks(NewBlocks, bar);
        }

        private static void SetBlocks(Dictionary<BlockPos, Block> NewBlocks, IBlockAccessorRevertable bar) {
            foreach (var block in NewBlocks) {
                bar.SetBlock(block.Value.BlockId, block.Key);
            }
            bar.Commit();
        }

        public static bool NextBoolean(this Random random) {
            return random.Next() > (Int32.MaxValue / 2);
        }

        private static void SolidCount(List<Block> positions, ref int SolidCount, ref int NonsolidCount) {

            foreach (var index in positions) {
                if (index.BlockId == 0 || index.IsLiquid()) {
                    NonsolidCount++;
                }
                else {
                    SolidCount++;
                }
            }
        }

        public static T MostCommon<T>(List<T> list) {
            var counts = new Dictionary<T, int>();
            foreach (var item in list) {
                if (counts.ContainsKey(item))
                    counts[item]++;
                else
                    counts[item] = 1;
            }
            return counts.OrderByDescending(x => x.Value).First().Key;
        }
    }
}
