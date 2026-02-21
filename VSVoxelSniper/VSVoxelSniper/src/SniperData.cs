using System;
using System.Collections.Generic;
using System.Numerics;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VSVoxelSniper {
    public static class SniperData {

        //for client side data and access
        #region constants
        public const string BrushSetToText = "Brush Type set to: ";
        public const string BrushSizeSetToText = "Brush Size set to: ";
        public const string ModifiedBrushSizeSetToText = "Modified Brush Size set to: ";
        public const string MaterialSetToText = "Material Set To: ";
        public const string ReplaceMaterialSetToText = "Brush Will Replace: ";
        public const string OverlayDebthText = "Debth set to: ";
        public const string OverlayPerformerText = "Overlay Performer set to: ";
        public const string SplatterSeedText = "Seed Percentage: ";
        public const string SplatterGrowthText = "Growth Percentage: ";
        public const string SplatterRecursionText = "Recurstions: ";
        public const string VoxelHeightSetToText = "Voxel Height set to: ";
        public const string VoxelDebthSetToText = "Voxel Debth set to: ";
        public const string VoxelCentroidSetToText = "Voxel Centroid set to: ";
        public const string CloneStampModeSetToText = "Mode: ";
        public const string CloneStampRotationModeSetToText = "Rotation: ";
        public const string CloneStampQueueModeSetToText = "Queue: ";
        public const string CloneStampForestModeSetToText = "Forest Mode: ";
        public const string CloneStampForestDensitySetToText = "Forest Density: ";
        #endregion

        #region BrushAndToolTypes
        public enum BrushTypes {
            snipe,
            ball,
            splatterball,
            disk,
            splatterdisk,
            diskface,
            splatterdiskface,
            voxel,
            splattervoxel,
            voxeldisk,
            splattervoxeldisk,
            voxeldiskface,
            splattervoxeldiskface,
            line,
            ray,
            erode,
            overlay,
            splatteroverlay,
            drain,
            teleport,
            lightning,
            set,
            filldown,
            fillfluid,
            blendball,
            blendvoxel,
            blenddisk,
            blendvoxeldisk,
            cylinder,
            entity,
            clonestamp,
            tree,
            forest
        }


        public static List<BrushTypes> GunpowerderOffsetExcluded = new List<BrushTypes> {
            BrushTypes.line,
            BrushTypes.ray,
            BrushTypes.set,
            BrushTypes.clonestamp
        };

        public enum ErosionTypes {
            melt,
            fill,
            lift,
            smooth,
            floatclean
        }

        public enum CloneStampModes {
            force,
            excludeair,
            fill
        }
        public enum CloneStampQueueModes {
            single,
            cycle,
            random
        }
        public enum CloneStampRotationModes {
            none,
            cycle,
            random
        }

        public enum ToolType {
            arrow,
            gunpowder
        }

        #endregion

        #region Trees
        private static List<String> TreeTypes;
        private static Vec2f TreeSizeRange;
        private static float TreeDensity;
        public static void SetTreeTypes(List<String> Types) {
            TreeTypes = Types;
        }
        public static List<String> GetActiveTreetypes() {
            return TreeTypes;
        }
        public static void SetTreeSizeRange(Vec2f range) {
            TreeSizeRange = range;
        }
        public static Vec2f GetTreeSizeRange() {
            return TreeSizeRange;
        }
        public static void SetTreeDensity(float desnity) {
            TreeDensity = desnity;
        }
        public static float GetTreeDensity() {
            return TreeDensity;
        }
        #endregion

        #region CloneStamp

        public static CloneStampModes ActiveCloneStampMode = CloneStampModes.excludeair;
        public static CloneStampQueueModes ActiveCloneStampQueueMode = CloneStampQueueModes.single;
        public static CloneStampRotationModes ActiveCloneStampRotationMode = CloneStampRotationModes.none;
        public static bool CloneStampForestOption = false;
        public static float CloneStampForestDensity = .7f;

        public static void SetActiveCloneStampMode(CloneStampModes mode) {
            ActiveCloneStampMode = mode;
        }
        public static CloneStampModes GetActiveCloneStampMode() {
            return ActiveCloneStampMode;
        }
        public static void SetActiveCloneStampQueueMode(CloneStampQueueModes mode) {
            ActiveCloneStampQueueMode = mode;
        }
        public static CloneStampQueueModes GetActiveCloneStampQueueMode() {
            return ActiveCloneStampQueueMode;
        }
        public static void SetActiveCloneStampRotationMode(CloneStampRotationModes mode) {
            ActiveCloneStampRotationMode = mode;
        }
        public static CloneStampRotationModes GetActiveCloneStampRotationMode() {
            return ActiveCloneStampRotationMode;
        }
        public static void SetCloneStampForestOption(bool enabled) {
            CloneStampForestOption = enabled;
        }
        public static bool GetCloneStampForestOption() {
            return CloneStampForestOption;
        }
        public static void SetCloneStampForestDensity(float density) {
            CloneStampForestDensity = density;
        }
        public static float GetCloneStampForestDensity() {
            return CloneStampForestDensity;
        }

        #endregion

        #region Entity



        #endregion

        #region Height, debth, and centroid

        private static int VoxelHeight = 0;
        private static int VoxelDebth = 0;
        private static int VoxelCentroid = 0;

        public static void SetVoxelHeight(int height) {
            VoxelHeight = height;
        }
        public static int GetVoxelHeight() {
            return VoxelHeight;
        }
        public static void SetVoxelDebth(int debth) {
            VoxelDebth = debth;
        }
        public static int GetVoxelDebth() {
            return VoxelDebth;
        }
        public static void SetVoxelCentroid(int centroid) {
            VoxelCentroid = centroid;
        }
        public static int GetVoxelCentroid() {
            return VoxelCentroid;
        }

        #endregion

        #region Overlay

        private static int OverlayDebth = 3;
        private static OverlayPerformers ActiveOverlayPerformer = OverlayPerformers.some;
        public enum OverlayPerformers {
            all,
            some,
            replace
        };
        public static void SetOverlayDebth(int debth) {
            OverlayDebth = debth;
        }
        public static int GetOverlayDebth() {
            return OverlayDebth;
        }
        public static void SetOverlayPerformer(OverlayPerformers performer) {
            ActiveOverlayPerformer = performer;
        }
        public static OverlayPerformers GetActiveOverlayPerformer() {
            return ActiveOverlayPerformer;
        }

        #endregion

        #region Splatter

        public static int SplatterSeed = 50;
        public static int SplatterGrowth = 2500;
        public static int SplatterRecursions = 4;

        public static void SetSplatterSeed(int value) {
            SplatterSeed = value;
        }
        public static int GetSplatterSeed() {
            return SplatterSeed;
        }
        public static void SetSplatterGrowth(int value) {
            SplatterGrowth = value;
        }
        public static int GetSplatterGrowth() {
            return SplatterGrowth;
        }
        public static void SetSplatterRecursions(int value) {
            SplatterRecursions = value;
        }
        public static int GetSplatterRecursions() {
            return SplatterRecursions;
        }

        #endregion

        #region MaterialHandling

        public enum MaterialCommandProcessType {
            JustDisplay,
            SetBlockPlayerIsLookingAt,
            AddBlockPlayerIsLookingAt,
            RemoveBlockPlayerIsLookingAt,
            SetFromList,
            AddFromList,
            RemoveFromList
        }

        private static List<Block> ActiveBlock;
        private static List<Block> ReplaceBlock;
        public static void SetActiveReplaceBlock(List<Block> block) {
            ReplaceBlock = block;
        }
        public static List<Block> GetActiveReplaceBlock() {
            return ReplaceBlock;
        }
        public static Block GetAnActiveRepalceBlock() {
            Random r = new Random();
            Block block = ReplaceBlock[r.Next(0, ReplaceBlock.Count)];
            return block;
        }
        public static void AddActiveRepalceBlock(List<Block> block) {
            ReplaceBlock.AddRange(block);
        }
        public static void RemoveActiveRepalceBlock(List<Block> block) {

            for (int i = 0; i < block.Count; i++) {
                ReplaceBlock.RemoveAll(a => a == block[i]);
            }
        }

        public static void SetActiveBlock(List<Block> block) {
            ActiveBlock = block;
        }
        public static Block GetAnActiveBlock() {
            Random r = new Random();
            Block block = ActiveBlock[r.Next(0, ActiveBlock.Count)];
            return block;
        }
        public static List<Block> GetActiveBlock() {
            return ActiveBlock;
        }
        public static void AddActiveBlock(List<Block> block) {
            ActiveBlock.AddRange(block);
        }
        public static void RemoveActiveBlock(List<Block> block) {
            for (int i = 0; i < block.Count; i++) {
                ActiveBlock.RemoveAll(a => a == block[i]);
            }
        }

        public static List<Block> ProcessBlockListString(string args, ICoreAPI api) {

            string ArgText = args;
            if (string.IsNullOrEmpty(ArgText)) {
                return null;
            }

            string[] text = ArgText.Split(",");
            List<Block> result = new List<Block>();

            for (int i = 0; i < text.Length; i++) {
                Block block = IsValidBlock(text[i], api);
                if (block != null) {
                    result.Add(block);
                }
            }
            return result;
        }
        public static Block IsValidBlock(string text, ICoreAPI api) {
            Block block = api.World.GetBlock(new AssetLocation(text));
            if (block != null) {
                return block;
            }
            if (IsNumeric(text)) {
                block = api.World.GetBlock(int.Parse(text));
                if (block != null) {
                    return block;
                }
            }
            return null;
        }

        public static int GetSingleBlockIdFromProvidedList(List<Block> list) {
            Random random = new Random();
            int r = random.Next(list.Count);
            return list[r].BlockId;
        }
        public static int GetSingleBlockIdFromArrayOfInts(int[] arr) {
            Random random = new Random();
            int r = random.Next(arr.Length);
            return arr[r];
        }


        #endregion

        #region BrushTypeHandling
        private static BrushTypes ActiveBrush = BrushTypes.snipe;

        public static bool SetActiveBrushType(BrushTypes brushtype) {
            ActiveBrush = brushtype;
            return true;
        }
        public static BrushTypes GetActiveBrushType() {
            return ActiveBrush;
        }
        public static bool ShouldGunpowerderOffset(BrushTypes brushtype) {
            if (GunpowerderOffsetExcluded.Contains(brushtype)) {
                return false;
            }
            return true;
        }

        #endregion

        #region BrushSizeHandling
        private static int[] UnmodifiedBrushSize = { 3, 3 };
        private static int[] ModifiedBrushSize = { 3, 3 };

        public static bool HandleBrushSizeText(string args) {

            string[] StringParts = new string[4];
            Array.Fill(StringParts, "0");

            string[] ArgParts = new string[0];

            if (args.Split(',').Length > 2) {
                return false;
            }

            if (args.Contains(",")) {
                ArgParts = new string[2];
                ArgParts = args.Split(",");
            }
            else {
                ArgParts = new string[1];
                ArgParts[0] = args;
            }

            int index = 0;
            for (int i = 0; i < ArgParts.Length; i++) {
                string[] strings = ArgParts[i].Split('-');
                StringParts[index++] = strings[0];
                if (strings.Length == 2) {
                    StringParts[index] = strings[1];
                }
                index++;
            }

            for (int i = 0; i < StringParts.Length; i++) {
                if (!IsNumeric(StringParts[i])) {
                    return false;
                }
            }

            int[] sizes = new int[4];
            Array.Fill(sizes, 0);

            for (int i = 0; i < StringParts.Length; i++) {
                if (StringParts[i] != "") {
                    sizes[i] = int.Parse(StringParts[i]);
                }
            }
            SetUnmodifiedBrushSize(sizes[0], sizes[1]);
            SetModifiedBrushSize(sizes[2], sizes[3]);

            return true;
        }
        public static bool IsNumeric(string arg) {

            int i = 0;
            return int.TryParse(arg, out i);
        }
        private static string[] Split(string args, string charcter) {
            var Split = args.Split(charcter);
            return Split;
        }

        public static void SetUnmodifiedBrushSize(int Min, int Max) {

            Min = Math.Clamp(Min, 0, 150);
            Max = Math.Clamp(Max, 0, 150);

            UnmodifiedBrushSize[0] = Min;
            if (Max == 0) {
                UnmodifiedBrushSize[1] = Min;
            }
            else {
                UnmodifiedBrushSize[1] = Max;
            }
        }

        public static void SetModifiedBrushSize(int Min, int Max) {

            Min = Math.Clamp(Min, 0, 150);
            Max = Math.Clamp(Max, 0, 150);

            ModifiedBrushSize[0] = Min;
            if (Max == 0) {
                ModifiedBrushSize[1] = Min;
            }
            else {
                ModifiedBrushSize[1] = Max;
            }
        }
        public static string GetConfiguredBrushSizeDisplayString() {
            string[] sizes = GetConfiguredBrushSize();

            string BrushSizeDisplay = BrushSizeSetToText + sizes[0];
            if (sizes[1] != "") {
                BrushSizeDisplay += Environment.NewLine + ModifiedBrushSizeSetToText + sizes[1];
            }
            return BrushSizeDisplay;
        }
        public static string[] GetConfiguredBrushSize() {
            string[] text = new string[2];
            Array.Fill(text, "");
            if (UnmodifiedBrushSize[0] == UnmodifiedBrushSize[1]) {
                text[0] = UnmodifiedBrushSize[0].ToString();
            }
            else {
                text[0] = "range of " + UnmodifiedBrushSize[0].ToString() + " to " + UnmodifiedBrushSize[1].ToString();
            }
            if (ModifiedBrushSize[0] != 0 && ModifiedBrushSize[1] != 0) {
                if (ModifiedBrushSize[0] == ModifiedBrushSize[1]) {
                    text[1] = ModifiedBrushSize[0].ToString();
                }
                else {
                    text[1] = "range of " + ModifiedBrushSize[0].ToString() + " to " + ModifiedBrushSize[1].ToString();
                }
            }
            return text;

        }
        public static int GetBrushSize(bool IsModified) {
            int[] MinMax;
            if (!IsModified) {
                MinMax = UnmodifiedBrushSize;
            }
            else {
                MinMax = ModifiedBrushSize;
            }
            Random rand = new Random();
            int Result = rand.Next(MinMax[0], MinMax[1]);
            return Result;
        }

        #endregion

        #region MaterialPerformers

        public enum PerformerTypes {
            Material,
            MaterialReplace
        }
        public static PerformerTypes ActivePerformer = PerformerTypes.Material;
        public static bool SetPerformer(PerformerTypes performer) {
            ActivePerformer = performer;
            return true;
        }

        #endregion

        #region ErosionTypeHandling

        private static ErosionTypes ActiveErosionType = ErosionTypes.melt;

        public static void SetActiveErosionType(ErosionTypes type) {
            ActiveErosionType = type;
        }
        public static ErosionTypes GetActiveErosionType() {
            return ActiveErosionType;
        }

        #endregion

        #region FaceEnum

        public enum FaceDirection {
            Up, Down, North, South, East, West
        }
        public static FaceDirection GetBlockFacingEnumFromClass(BlockFacing block) {
            if (block == BlockFacing.UP) {
                return FaceDirection.Up;
            }
            else if (block == BlockFacing.NORTH) {
                return FaceDirection.North;
            }
            else if (block == BlockFacing.SOUTH) {
                return FaceDirection.South;
            }
            else if (block == BlockFacing.EAST) {
                return FaceDirection.East;
            }
            else if (block == BlockFacing.WEST) {
                return FaceDirection.West;
            }
            else {
                return FaceDirection.Down;
            }
        }
        public static BlockFacing GetBlockFacingClassFromEnum(FaceDirection enu) {
            if (enu == FaceDirection.Up) {
                BlockFacing b = BlockFacing.UP;
                return b;
            }
            else if (enu == FaceDirection.North) {
                BlockFacing b = BlockFacing.NORTH;
                return b;
            }
            else if (enu == FaceDirection.South) {
                BlockFacing b = BlockFacing.SOUTH;
                return b;
            }
            else if (enu == FaceDirection.East) {
                BlockFacing b = BlockFacing.EAST;
                return b;
            }
            else if (enu == FaceDirection.West) {
                BlockFacing b = BlockFacing.WEST;
                return b;
            }
            else {
                BlockFacing b = BlockFacing.DOWN;
                return b;
            }
        }


        #endregion

        #region CraftBrushStrokePacket

        public static BrushDataPacket CreateBrushStrokePacket(ToolType tool, List<Block> Material, List<Block> ReplaceMaterial, Vec3i BlockPos, Vec3i PlayerPos, 
            FaceDirection face, BrushTypes brush, int brushsize, bool IsModified, PerformerTypes performer, ErosionTypes erosionpreset, int OverlayDebth, SniperData.OverlayPerformers OverlayPerformer,
            int SplatterSeed, int SplatterGrowth, int SplatterRecursions, int VoxelHeight, int VoxelDebth, int VoxelCentroid, SniperData.CloneStampModes CloneStampMode, CloneStampQueueModes CloneStampQueueMode,
            CloneStampRotationModes cloneStampRotationMode, bool CloneStampForestOption, float CloneStampForestDensity, List<string> TreeTypes, Vec2f TreeSizeRange, float TreeDensity) {

            int[] mats = ConvertBlockListToArray(Material);
            int[] rmats = ConvertBlockListToArray(ReplaceMaterial);

            BrushDataPacket packet = new BrushDataPacket();
            packet.tool = tool;
            packet.Material = mats;
            packet.ReplaceMaterial = rmats;
            packet.BlockPos = BlockPos;
            packet.PlayerPos = PlayerPos;
            packet.face = face;
            packet.brush = brush;
            packet.brushsize = brushsize;
            packet.IsModified = IsModified;
            packet.performer = performer;
            packet.erosionpreset = erosionpreset;
            packet.OverlayDebth = OverlayDebth;
            packet.OverlayPerformer = OverlayPerformer;
            packet.SplatterSeed = SplatterSeed;
            packet.SplatterGrowth = SplatterGrowth;
            packet.SplatterRecursions = SplatterRecursions;
            packet.VoxelHeight = VoxelHeight;
            packet.VoxelDebth = VoxelDebth;
            packet.VoxelCentroid = VoxelCentroid;
            packet.CloneStampMode = CloneStampMode;
            packet.CloneStampQueueMode = CloneStampQueueMode;
            packet.CloneStampRotationMode = cloneStampRotationMode;
            packet.CloneStampForestOption = CloneStampForestOption;
            packet.CloneStampForestDensity = CloneStampForestDensity;
            packet.TreeTypes = TreeTypes;
            packet.TreeSizeRange = TreeSizeRange;
            packet.TreeDensity = TreeDensity;
            return packet;
        }

        private static int[] ConvertBlockListToArray(List<Block> list) {
            if (list == null) { return null; }
            if (list.Count == 0) { return null; }
            int[] ints = new int[list.Count];
            for (int i = 0; i < list.Count; i++) {
                if (list[i] == null) { continue; }
                ints[i] = list[i].BlockId;
            }
            return ints;
        }
        private static List<Block> ConvertBlockArrayToList(int[] arr, ICoreAPI api) {
            if (arr == null) { return null; }
            if (arr.Length == 0) { return null; }
            List<Block> blocks = new List<Block>();
            for (int i = 0; i < arr.Length; i++) {
                Block b = api.World.GetBlock(arr[i]);
                blocks.Add(b);
            }
            return blocks;
        }

        #endregion

        #region SettingBlocks

        public static void SetBlocks(List<BlockPos> blocks, IBlockAccessorRevertable bar, PerformerTypes performer, int[] blockid, int[] replacingblockid = null) {
            for (int i = 0; i < blocks.Count; i++) {
                SetBlock(bar, performer, blocks[i], blockid, replacingblockid);
            }
            bar.Commit();
        }

        public static void SetBlock(IBlockAccessorRevertable bar, PerformerTypes performer, BlockPos pos, int[] blockid, int[] replacingblockid = null, bool commit = false) {


            if (performer == PerformerTypes.Material) {
                Block block = bar.GetBlock(blockid[RandomInt(blockid)]);
                if (block.ForFluidsLayer) {
                    bar.SetBlock(blockid[RandomInt(blockid)], pos, BlockLayersAccess.Fluid);
                    bar.SetBlock(0, pos, BlockLayersAccess.Solid);
                    
                    //bar.ExchangeBlock(blockid[RandomInt(blockid)], pos);
                }
                else {
                    bar.SetBlock(0, pos, BlockLayersAccess.Fluid);
                    bar.SetBlock(blockid[RandomInt(blockid)], pos, BlockLayersAccess.Solid);
                    //bar.ExchangeBlock(blockid[RandomInt(blockid)], pos);
                }
            }
            else if (performer == PerformerTypes.MaterialReplace) {

                if (replacingblockid == null) { return; }
                Block block = bar.GetBlock(blockid[RandomInt(blockid)]);

                if (IsReplaceable(bar.GetBlock(pos).BlockId, replacingblockid)) {

                    if (block.ForFluidsLayer) {
                        bar.SetBlock(blockid[RandomInt(blockid)], pos, BlockLayersAccess.Fluid);
                        bar.SetBlock(0, pos, BlockLayersAccess.Solid);
                        //bar.ExchangeBlock(blockid[RandomInt(blockid)], pos);
                    }
                    else {
                        bar.SetBlock(0, pos, BlockLayersAccess.Fluid);
                        bar.SetBlock(blockid[RandomInt(blockid)], pos, BlockLayersAccess.Solid);
                        //bar.ExchangeBlock(blockid[RandomInt(blockid)], pos);
                    }
                }
            }
            if (commit) { bar.Commit(); }
        }
        internal static int RandomInt(int[] arr) {
            Random rand = new Random();
            int r = rand.Next(0, arr.Length);
            return r;
        }
        private static bool IsReplaceable(int current, int[] arr) {
            for (int i = 0; i < arr.Length; i++) {
                if (arr[i] == current) {
                    return true;
                }
            }
            return false;
        }
        public static void Drain(List<BlockPos> blocks, IBlockAccessorRevertable bar) {
            for (int i = 0; i < blocks.Count; i++) {
                bar.SetBlock(0, blocks[i], BlockLayersAccess.Fluid);
            }
            bar.Commit();
        }

        #endregion
    }
}

