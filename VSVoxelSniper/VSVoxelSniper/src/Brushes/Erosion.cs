using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace VSVoxelSniper.assets.Brushes {
    static class Erosion {
        private static Vec3i[] BlockOffsets = {
            new Vec3i(0, 0, 1),
            new Vec3i(0, 0, -1),
            new Vec3i(0, 1, 0),
            new Vec3i(0, -1, 0),
            new Vec3i(1, 0, 0),
            new Vec3i(-1, 0, 0)
        };
        public static void Erode(IPlayer player, BrushDataPacket packet, IBlockAccessorRevertable bar) {
            ErosionPreset type = GetPresetValues(packet.erosionpreset);
            if (packet.tool == SniperData.ToolType.gunpowder) {
                type = getInverted(GetPresetValues(packet.erosionpreset));
            }
            BlockChangeTracker bct = new BlockChangeTracker();

            Vec3i PlayerPosition = new Vec3i((int)player.Entity.Pos.X, (int)player.Entity.Pos.Y + 1, (int)player.Entity.Pos.Z);
            Vec3i BlockPosition = new Vec3i(packet.BlockPos.X, packet.BlockPos.Y, packet.BlockPos.Z);

            Vec3i vec = Shapes.Vec3iSubtract(PlayerPosition, BlockPosition);

            float AdjustedRadius = (float)(packet.brushsize + 0.5);

            for (int i = 0; i < type.ErosionRecursion; i++) {
                ErosionIteration(player, packet, type, AdjustedRadius, bar, bct);
            }

            for (int i = 0; i < type.FillRecursion; i++) {
                FillIteration(player, packet, type, AdjustedRadius, bar, bct);
            }

            foreach (BlockWrapper b in bct.GetAll().Values) {
                Block block = b.getBlockData();
                if (block != null) {
                    bar.SetBlock(block.BlockId, b.getBlock());
                }
            }

            bar.Commit();
        }


        private static void ErosionIteration(IPlayer player, BrushDataPacket packet, ErosionPreset erosionPreset, float MaxRadius, IBlockAccessorRevertable bar, BlockChangeTracker bct) {

            int currentIteration = bct.NextIteration();

            for (int x = packet.BlockPos.X - packet.brushsize; x <= packet.BlockPos.X + packet.brushsize; ++x) {
                for (int z = packet.BlockPos.Z - packet.brushsize; z <= packet.BlockPos.Z + packet.brushsize; ++z) {
                    for (int y = packet.BlockPos.Y - packet.brushsize; y <= packet.BlockPos.Y + packet.brushsize; ++y) {

                        Vec3i CurBlockPos = new Vec3i(x, y, z);
                        double distance = Shapes.Vec3iDistance(CurBlockPos, packet.BlockPos);

                        if (distance >= MaxRadius) { continue; }

                        BlockPos pos = new BlockPos(CurBlockPos, player.Entity.Pos.Dimension);
                        Block block = bar.GetBlock(pos);
                        if (block.ForFluidsLayer || block.BlockId == 0) { continue; }

                        int NonSolidNeighbors = 0;
                        BlockPos NeighborPos = new BlockPos();

                        for (int i = 0; i < BlockOffsets.Length; i++) {
                            Vec3i nvec = new Vec3i(pos.X + BlockOffsets[i].X, pos.Y + BlockOffsets[i].Y, pos.Z + BlockOffsets[i].Z);
                            NeighborPos = new BlockPos(nvec, player.Entity.Pos.Dimension);
                            BlockWrapper NeighboringBlock = bct.get(NeighborPos, currentIteration, bar);
                            if (NeighboringBlock == null) { continue; }
                            if (NeighboringBlock.getBlockData().BlockId == 0 || NeighboringBlock.getBlockData().ForFluidsLayer) {
                                NonSolidNeighbors++;
                            }
                        }
                        if (NonSolidNeighbors >= erosionPreset.ErosionFaces) {
                            bct.put(pos, new BlockWrapper(pos, bar.GetBlock(0)), currentIteration);
                        }
                    }
                }
            }
        }
        private static void FillIteration(IPlayer player, BrushDataPacket packet, ErosionPreset erosionPreset, float MaxRadius, IBlockAccessorRevertable bar, BlockChangeTracker bct) {

            int currentIteration = bct.NextIteration();

            for (int x = packet.BlockPos.X - packet.brushsize; x <= packet.BlockPos.X + packet.brushsize; ++x) {
                for (int z = packet.BlockPos.Z - packet.brushsize; z <= packet.BlockPos.Z + packet.brushsize; ++z) {
                    for (int y = packet.BlockPos.Y - packet.brushsize; y <= packet.BlockPos.Y + packet.brushsize; ++y) {
                        Vec3i CurBlockPos = new Vec3i(x, y, z);
                        double distance = Shapes.Vec3iDistance(CurBlockPos, packet.BlockPos);

                        if (distance >= MaxRadius) { continue; }

                        BlockPos pos = new BlockPos(CurBlockPos, player.Entity.Pos.Dimension);
                        Block block = bar.GetBlock(pos);

                        BlockWrapper CurrentBlock = bct.get(pos, currentIteration, bar);

                        if (!(CurrentBlock.isEmpty() || CurrentBlock.getBlockData().ForFluidsLayer)) { continue; }
                        int SolidNeighbors = 0;
                        Dictionary<BlockWrapper, int> blockcount = new Dictionary<BlockWrapper, int>();

                        for (int i = 0; i < BlockOffsets.Length; i++) {
                            BlockPos NeighborPos = new BlockPos(pos.X + BlockOffsets[i].X, pos.Y + BlockOffsets[i].Y, pos.Z + BlockOffsets[i].Z, player.Entity.Pos.Dimension);
                            BlockWrapper NeighboringBlock = bct.get(NeighborPos, currentIteration, bar);

                            if (!(NeighboringBlock.isEmpty() || NeighboringBlock.getBlockData().ForFluidsLayer)) {
                                SolidNeighbors++;
                                BlockWrapper typeblock = new BlockWrapper(null, bar.GetBlock(NeighborPos));
                                if (blockcount.ContainsKey(typeblock)) {
                                    blockcount.TryAdd(typeblock, blockcount.Get(typeblock) + 1);
                                }
                                else {
                                    blockcount.TryAdd(typeblock, 1);
                                }
                            }
                        }
                        BlockWrapper currentMaterial = new BlockWrapper(null, bar.GetBlock(0));
                        int amount = 0;

                        foreach (BlockWrapper wrapper in blockcount.Keys) {
                            int CurCount = blockcount.Get(wrapper);
                            if (amount <= CurCount) {
                                currentMaterial = wrapper;
                                amount = CurCount;
                            }
                        }
                        if (SolidNeighbors >= erosionPreset.FillFaces) {
                            bct.put(pos, new BlockWrapper(pos, currentMaterial.getBlockData()), currentIteration);
                        }
                    }
                }
            }
        }




        private class BlockChangeTracker {

            private Dictionary<int, Dictionary<BlockPos, BlockWrapper>> blockchanges = new Dictionary<int, Dictionary<BlockPos, BlockWrapper>>();
            private Dictionary<BlockPos, BlockWrapper> flatchanges = new Dictionary<BlockPos, BlockWrapper>();
            private int NextIterationID = 0;


            public BlockWrapper get(BlockPos position, int Iteration, IBlockAccessorRevertable bar) {
                BlockWrapper blockChanges = null;
                for (int i = Iteration - 1; i >= 0; --i) {
                    if (blockchanges.ContainsKey(i) && blockchanges.Get(i).ContainsKey(position)) {
                        blockChanges = blockchanges.Get(i).Get(position);
                        return blockChanges;
                    }
                }
                blockChanges = new BlockWrapper(position, bar);
                return blockChanges;
            }

            public void put(BlockPos position, BlockWrapper changedblock, int iteration) {
                if (!blockchanges.ContainsKey(iteration)) {
                    blockchanges.Add(iteration, new Dictionary<BlockPos, BlockWrapper>());
                }
                blockchanges.Get(iteration).TryAdd(position, changedblock);
                flatchanges.TryAdd(position, changedblock);
            }

            public Dictionary<BlockPos, BlockWrapper> GetAll() {
                return flatchanges;
            }

            public int NextIteration() {
                return NextIterationID++;
            }
        }



        private class BlockWrapper {

            private BlockPos blockPos;
            private Block blockData;

            public BlockWrapper(BlockPos blockpos, IBlockAccessorRevertable bar) {
                blockPos = blockpos;
                blockData = bar.GetBlock(blockpos);
            }

            public BlockWrapper(BlockPos blockpos, Block block) {
                blockPos = blockpos;
                blockData = block;
            }

            public BlockPos getBlock() {
                return blockPos;
            }

            public Block getBlockData() {
                return blockData;
            }

            //public VoxelMaterial getMaterial() {
            //    return this.blockData.getMaterial();
            //}

            public bool isEmpty() {
                if (blockData.BlockId == 0) {
                    return true;
                }
                return false;
            }
        }




        private static ErosionPreset GetPresetValues(SniperData.ErosionTypes preset) {
            if (preset == SniperData.ErosionTypes.melt) {
                return new ErosionPreset(2, 1, 5, 1);
            }
            if (preset == SniperData.ErosionTypes.fill) {
                return new ErosionPreset(5, 1, 2, 1);
            }
            if (preset == SniperData.ErosionTypes.smooth) {
                return new ErosionPreset(3, 1, 3, 1);
            }
            if (preset == SniperData.ErosionTypes.lift) {
                return new ErosionPreset(6, 0, 1, 1);
            }
            if (preset == SniperData.ErosionTypes.floatclean) {
                return new ErosionPreset(6, 1, 6, 1);
            }
            return null;
        }
        public static ErosionPreset getInverted(ErosionPreset preset) {
            return new ErosionPreset(preset.FillFaces, preset.FillRecursion, preset.ErosionFaces, preset.ErosionRecursion);
        }
    }
    public class ErosionPreset {
        public int ErosionFaces;
        public int ErosionRecursion;
        public int FillFaces;
        public int FillRecursion;
        public ErosionPreset(int erosionFaces, int erosionRecursion, int fillFaces, int fillRecursion) {
            ErosionFaces = erosionFaces;
            ErosionRecursion = erosionRecursion;
            FillFaces = fillFaces;
            FillRecursion = fillRecursion;
        }
    }
}
