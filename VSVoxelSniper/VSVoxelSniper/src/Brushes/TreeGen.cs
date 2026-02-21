using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;

namespace VSVoxelSniper.src.Brushes {
    class TreeGeneration {
        private TreeGeneratorsUtil treeGenerators;

        public void PlaceTree(BrushDataPacket p, BlockPos pos, ICoreServerAPI sapi, IBlockAccessorRevertable bar, Vec2f size, bool commit = true) {
            if (treeGenerators == null) {
                treeGenerators = new TreeGeneratorsUtil(sapi);
                treeGenerators.ReloadTreeGenerators();
            }

            Random random = new Random();
            double min = Math.Min((double)size.X, (double)size.Y);
            double max = Math.Max((double)size.X, (double)size.Y);
            double EndSize = random.NextDouble() * (max - min) + min;

            if (p.TreeTypes == null) {
                return;
            }

            List<AssetLocation> Locations = new List<AssetLocation>();

            for (int i = 0; i < p.TreeTypes.Count; i++) {
                AssetLocation Loc = new AssetLocation(p.TreeTypes[i]);
                if (Loc != null) {
                    Locations.Add(Loc);
                }
            }

            if (Locations.Count == 0) {
                return;
            }
            treeGenerators.RunGenerator(Locations[random.Next(0, Locations.Count)], bar, pos, new TreeGenParams() { size = (float)EndSize, skipForestFloor = true });
            if (commit) {
                bar.Commit();
            }
        }
    }
}
