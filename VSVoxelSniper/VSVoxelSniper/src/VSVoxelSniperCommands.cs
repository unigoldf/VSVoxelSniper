using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using VSVoxelSniper.src;

namespace VSVoxelSniper {
    internal partial class VSVoxelSniperModSystem {

        private void RegisterCommands() {

            var parsers = sapi.ChatCommands.Parsers;
            sapi.ChatCommands
                .GetOrCreate("brush")
                .WithAlias("b")
                .WithArgs(parsers.OptionalAll("Specify a brush, or brush size"))
                .WithDescription("For setting brush")
                .RequiresPrivilege("vsvoxelsniperuser")
                .HandleWith(B)
                .RequiresPlayer()
                .Validate()
            ;
            sapi.ChatCommands
                .GetOrCreate("v")
                .IgnoreAdditionalArgs()
                .WithDescription("For setting material")
                .RequiresPrivilege("vsvoxelsniperuser")
                .WithArgs(parsers.OptionalAll("Block"))
                .HandleWith(V)
                .RequiresPlayer()
                .Validate()
            ;
            sapi.ChatCommands
                .GetOrCreate("vr")
                .IgnoreAdditionalArgs()
                .WithDescription("For setting replace material")
                .RequiresPrivilege("vsvoxelsniperuser")
                .WithArgs(parsers.OptionalAll("Block"))
                .HandleWith(VR)
                .RequiresPlayer()
                .Validate()
                ;
            sapi.ChatCommands
                .GetOrCreate("u")
                .IgnoreAdditionalArgs()
                .WithDescription("For undoing a brush stroke")
                .RequiresPrivilege("vsvoxelsniperuser")
                .WithArgs(parsers.OptionalAll("Steps"))
                .HandleWith(UndoCommand)
                .RequiresPlayer()
                .Validate()
                ;
            sapi.ChatCommands
                .GetOrCreate("vh")
                .IgnoreAdditionalArgs()
                .WithDescription("For setting voxel height")
                .RequiresPrivilege("vsvoxelsniperuser")
                .WithArgs(parsers.Int("Height"))
                .HandleWith(VH)
                .RequiresPlayer()
                .Validate()
                ;
            sapi.ChatCommands
                .GetOrCreate("vd")
                .IgnoreAdditionalArgs()
                .WithDescription("For setting voxel debth")
                .RequiresPrivilege("vsvoxelsniperuser")
                .WithArgs(parsers.Int("Debth"))
                .HandleWith(VD)
                .RequiresPlayer()
                .Validate()
                ;
            sapi.ChatCommands
                .GetOrCreate("vc")
                .IgnoreAdditionalArgs()
                .WithDescription("For setting voxel centroid")
                .RequiresPrivilege("vsvoxelsniperuser")
                .WithArgs(parsers.Int("value"))
                .HandleWith(VC)
                .RequiresPlayer()
                .Validate()
                ;
            sapi.ChatCommands
                .GetOrCreate("vs")
                .IgnoreAdditionalArgs()
                .WithDescription("For altering user settings")
                .RequiresPrivilege("vsvoxelsniperuser")
                .WithArgs(parsers.OptionalAll("Steps"))
                .HandleWith(VS)
                .RequiresPlayer()
                .Validate()
                ;

        }


        #region UndoCommand
        private TextCommandResult UndoCommand(TextCommandCallingArgs args) {
            //Undo(args);
            //return TextCommandResult.Success();
            string str = (string)args[0];
            int count = 1;
            if (str != null) {

                string[] SuppliedArgs = str.Split(' ');
                if (SniperData.IsNumeric(SuppliedArgs[0])) {
                    count = Math.Abs(Int32.Parse(SuppliedArgs[0]));
                }
                else {
                    //maybe handle undo other players here some day
                }
            }
            ServerSideUserWorkspace ws = GetWorkspace(args.Caller.Player.PlayerUID);
            string result = "";
            if (ws != null) {
                result = ws.Undo(count);
            }
            return TextCommandResult.Success(result);
        }

        //private async void Undo(TextCommandCallingArgs args) {
        //    string str = (string)args[0];
        //    int count = 1;
        //    if (str != null) {

        //        string[] SuppliedArgs = str.Split(' ');
        //        if (SniperData.IsNumeric(SuppliedArgs[0])) {
        //            count = Math.Abs(Int32.Parse(SuppliedArgs[0]));
        //        }
        //        else {
        //            //maybe handle undo other players here some day
        //        }
        //    }
        //    ServerSideUserWorkspace ws = GetWorkspace(args.Caller.Player.PlayerUID);
        //    string result = "";
        //    if (ws != null) {
        //        result = await ws.Undo(count);
        //    }
        //}

        #endregion

        #region MaterialSetting
        private void SetVoxelReplaceMaterial(string[] args) {
            ProcessMaterialCommand(args, capi.World.Player, true);
            List<Block> block = SniperData.GetActiveReplaceBlock();
            string str = "";
            if (block.Count > 0) {
                for (int i = 0; i < block.Count; i++) {
                    str += block[i].ToString() + ", ";
                }
                str = FormatMaterialSetString(str);
            }
            capi.ShowChatMessage(Environment.NewLine + SniperData.ReplaceMaterialSetToText + str);
        }

        private void SetVoxelMaterial(string[] args) {
            ProcessMaterialCommand(args, capi.World.Player);
            List<Block> block = SniperData.GetActiveBlock();
            string str = "";
            if (block.Count > 0) {
                for (int i = 0; i < block.Count; i++) {
                    str += block[i].ToString() + ", ";
                }
                str = FormatMaterialSetString(str);
            }
            capi.ShowChatMessage(Environment.NewLine + SniperData.MaterialSetToText + str);
        }
        private string FormatMaterialSetString(string str) {
            str = str.Replace("game:block ", "");
            str = str.Replace("/", " ");
            str = str.Remove(str.Length - 2);
            return str;
        }
        private void ProcessMaterialCommand(string[] args, IPlayer player, bool RepalceMat = false) {

            List<Block> blocks = new List<Block>();

            if (args == null) {
                blocks = GetTheBlockThePlayerIsLookingAtAsArray(player);
                if (RepalceMat) {
                    SniperData.SetActiveReplaceBlock(blocks);
                    return;
                }
                else {
                    SniperData.SetActiveBlock(blocks);
                    return;
                }
            }
            bool add = false;
            bool remove = false;

            if (args[0].ToLower() == "add") { add = true; }
            else if (args[0].ToLower() == "remove") { remove = true; }

            if (add || remove) {
                if (args.Length == 1) {
                    blocks = GetTheBlockThePlayerIsLookingAtAsArray(player);
                }
                else {
                    blocks = SniperData.ProcessBlockListString(args[1], capi);
                }
            }
            else {
                blocks = SniperData.ProcessBlockListString(args[0], capi);
            }

            for (int i = 0; i < blocks.Count; i++) {
                if (blocks[i] == null) {
                    capi.SendChatMessage("invalid block");
                    return;
                }
            }

            if (add) {
                if (RepalceMat) {
                    SniperData.AddActiveRepalceBlock(blocks);
                    return;
                }
                else {
                    SniperData.AddActiveBlock(blocks);
                    return;
                }
            }
            else if (remove) {
                if (RepalceMat) {
                    SniperData.RemoveActiveRepalceBlock(blocks);
                    return;
                }
                else {
                    SniperData.RemoveActiveBlock(blocks);
                    return;
                }
            }
            if (RepalceMat) {
                SniperData.SetActiveReplaceBlock(blocks);
            }
            else {
                SniperData.SetActiveBlock(blocks);
            }
        }
        private List<Block> GetTheBlockThePlayerIsLookingAtAsArray(IPlayer player) {
            Block aimBlock;
            IClientWorldAccessor ws = capi.World;
            Vec3d pos = player.Entity.Pos.XYZ.Add(player.Entity.LocalEyePos);
            BlockSelection bs = null;
            EntitySelection es = null;

            bool CurState = ws.ForceLiquidSelectable;
            ws.ForceLiquidSelectable = true;
            ws.RayTraceForSelection(pos, player.Entity.Pos.Pitch, player.Entity.Pos.Yaw, UserSettings.GetRange(), ref bs, ref es);
            ws.ForceLiquidSelectable = CurState;

            if (bs != null) {
                aimBlock = bs.Block;
            }
            else {
                aimBlock = capi.World.GetBlock(0);
            }
            List<Block> final = new List<Block>();
            final.Add(aimBlock);
            return final;
        }
        #endregion

        #region UserSettings

        public void HandleSettingsChange(string[] args) {
            if (args == null || args.Length == 0) {
                capi.ShowChatMessage(Environment.NewLine + UserSettings.RangeSetToText + UserSettings.GetRange().ToString());
                capi.ShowChatMessage(UserSettings.LiquidSelectableText + UserSettings.GetSelectLiquid().ToString());
                capi.ShowChatMessage(UserSettings.StrokeFrequencyText + UserSettings.GetStrokeFrequency().ToString());
                return;
            }
            if (args[0].ToLower() == "range") {
                if (args.Length > 1) {
                    if (SniperData.IsNumeric(args[1])) {
                        int range = Int32.Parse(args[1]);
                        UserSettings.SetRange(range);
                    }
                    else {
                        capi.ShowChatMessage("Invalid Input");
                        return;
                    }
                }
                capi.ShowChatMessage(Environment.NewLine + UserSettings.RangeSetToText + UserSettings.GetRange().ToString());
                return;
            }
            if (args[0].ToLower() == "ls" || args[0].ToLower() == "liquidselectable") {
                if (args.Length > 1) {
                    if (args[1].ToLower() == "true") {
                        UserSettings.SetSelectLiquid(true);
                    }
                    else if (args[1].ToLower() == "false") {
                        UserSettings.SetSelectLiquid(false);
                    }
                    else {
                        capi.ShowChatMessage("Invalid Input");
                        return;
                    }
                }
                capi.ShowChatMessage(Environment.NewLine + UserSettings.LiquidSelectableText + UserSettings.GetSelectLiquid().ToString());
                return;
            }
            if (args[0].ToLower() == "sf" || args[0].ToLower() == "strokefrequency") {
                if (args.Length > 1) {
                    float freq;
                    if (!float.TryParse(args[1], CultureInfo.InvariantCulture, out freq)) {
                        capi.ShowChatMessage("Invalid Input");
                        return;
                    }
                    UserSettings.SetStrokeFrequency(freq);
                }
            }
            if (args[0].ToLower() == "hl" || args[0].ToLower() == "highlight") {
                if (args.Length > 1) {
                    if (args[1].ToLower() == "true") {
                        UserSettings.SetHighlightTaretBlock(true);
                    }
                    else if (args[1].ToLower() == "false") {
                        UserSettings.SetHighlightTaretBlock(false);
                    }
                    else {
                        capi.ShowChatMessage("Invalid Input");
                        return;
                    }
                }
                capi.ShowChatMessage(Environment.NewLine + UserSettings.ShowHighlightText + UserSettings.GetHighlightTargetBlock().ToString());
                return;
            }
            capi.ShowChatMessage(Environment.NewLine + UserSettings.StrokeFrequencyText + UserSettings.GetStrokeFrequency().ToString());
            return;

        }

        #endregion

        #region Height, debth, and centroid

        public void SetVoxelHeight(string[] args) {
            if (args[0] != null && SniperData.IsNumeric(args[0])) {
                int value = Int32.Parse(args[0]);
                SniperData.SetVoxelHeight(value);
                capi.ShowChatMessage(Environment.NewLine + SniperData.VoxelHeightSetToText + SniperData.GetVoxelHeight().ToString());
            }
            else {

            }
        }
        public void SetVoxelDebth(string[] args) {
            if (args[0] != null && SniperData.IsNumeric(args[0])) {
                int value = Int32.Parse(args[0]);
                SniperData.SetVoxelDebth(value);
                capi.ShowChatMessage(Environment.NewLine + SniperData.VoxelDebthSetToText + SniperData.GetVoxelDebth().ToString());
            }
            else {

            }
        }
        public void SetVoxelCentroid(string[] args) {

            if (args[0] != null && SniperData.IsNumeric(args[0])) {
                int value = Int32.Parse(args[0]);
                SniperData.SetVoxelCentroid(value);
                capi.ShowChatMessage(Environment.NewLine + SniperData.VoxelCentroidSetToText + SniperData.GetVoxelCentroid().ToString());
            }
            else {

            }
        }

        #endregion

        #region Brush
        public void CommandReceivedHandler(CommandSendPacket packet) {
            if (packet.CommandType == "b") {
                ClientBrushCommandHandler(packet);
            }
            else if (packet.CommandType == "v") {
                SetVoxelMaterial(packet.args);
            }
            else if (packet.CommandType == "vr") {
                SetVoxelReplaceMaterial(packet.args);
            }
            else if (packet.CommandType == "vh") {
                SetVoxelHeight(packet.args);
            }
            else if (packet.CommandType == "vd") {
                SetVoxelDebth(packet.args);
            }
            else if (packet.CommandType == "vc") {
                SetVoxelCentroid(packet.args);
            }
            else if (packet.CommandType == "vs") {
                HandleSettingsChange(packet.args);
            }
        }

        public void ClientBrushCommandHandler(CommandSendPacket packet) {
            if (packet.args == null) {
                capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType() + Environment.NewLine +
                    SniperData.GetConfiguredBrushSizeDisplayString()
                    );
                return;
            }

            string arg1 = packet.args[0];

            if (arg1.ToLower() == "s" || arg1.ToLower() == "snipe") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.snipe)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "b" || arg1.ToLower() == "ball") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.ball)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "v" || arg1.ToLower() == "voxel") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.voxel)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "vd" || arg1.ToLower() == "voxeldisk") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.voxeldisk)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "d" || arg1.ToLower() == "disk") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.disk)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "df" || arg1.ToLower() == "diskface") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.diskface)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "vdf" || arg1.ToLower() == "voxeldiskface") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.voxeldiskface)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "c" || arg1.ToLower() == "cylinder") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.cylinder)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "line") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.line)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "fd" || arg1.ToLower() == "filldown") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.filldown)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "ff" || arg1.ToLower() == "fillfluid") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.fillfluid)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "ray") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.ray)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "spike") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.spike)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "t" || arg1.ToLower() == "tree") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.tree)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                    if (packet.args.Length > 1) {
                        string error = ParseTreeString(packet.args[1], packet.ValidTreeTypes);
                        if (error != "") {
                            capi.ShowChatMessage(error);
                            capi.ShowChatMessage(GetValidTreeTypeString(packet));
                        }
                        else {
                            String ValidString = "Selected Tree Type(s): ";
                            string[] trees = packet.args[1].Split(',');
                            SniperData.SetTreeTypes(trees.ToList());
                            for (int i = 0; i < trees.Length; i++) {
                                ValidString += trees[i] + ", ";
                            }
                            capi.ShowChatMessage(ValidString);
                        }
                    }
                    else {
                        capi.ShowChatMessage(GetValidTreeTypeString(packet));
                    }
                    SniperData.SetTreeSizeRange(ParseTreeSize(packet.args));
                    Vec2f size = SniperData.GetTreeSizeRange();
                    capi.ShowChatMessage("Tree Size: " + size.X + " - " + size.Y);
                    return;
                }
            }
            else if (arg1.ToLower() == "forest") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.forest)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                    if (packet.args.Length > 1) {
                        string error = ParseTreeString(packet.args[1], packet.ValidTreeTypes);
                        if (error != "") {
                            capi.ShowChatMessage(error);
                            capi.ShowChatMessage(GetValidTreeTypeString(packet));
                        }
                        else {
                            String ValidString = "Selected Tree Type(s): ";
                            string[] trees = packet.args[1].Split(',');
                            SniperData.SetTreeTypes(trees.ToList());
                            for (int i = 0; i < trees.Length; i++) {
                                ValidString += trees[i] + ", ";
                            }
                            capi.ShowChatMessage(ValidString);
                        }
                    }
                    else {
                        capi.ShowChatMessage(GetValidTreeTypeString(packet));
                    }
                    SniperData.SetTreeSizeRange(ParseTreeSize(packet.args));
                    SniperData.SetTreeDensity(ParseForestDensity(packet.args));
                    Vec2f size = SniperData.GetTreeSizeRange();
                    capi.ShowChatMessage("Forest Density: " + SniperData.GetTreeDensity() + ", Tree Size: " + size.X + " - " + size.Y);
                    return;
                }
            }
            else if (arg1.ToLower() == "sb" || arg1.ToLower() == "splatterball") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.splatterball)) {
                    SetSplatterSettings(packet);
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString() + " " + SniperData.SplatterSeedText + SniperData.GetSplatterSeed().ToString()
                        + " " + SniperData.SplatterGrowthText + SniperData.GetSplatterGrowth().ToString() + " " + SniperData.SplatterRecursionText + SniperData.GetSplatterRecursions().ToString());
                }
            }
            else if (arg1.ToLower() == "sv" || arg1.ToLower() == "splattervoxel") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.splattervoxel)) {
                    SetSplatterSettings(packet);
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString() + " " + SniperData.SplatterSeedText + SniperData.GetSplatterSeed().ToString()
                        + " " + SniperData.SplatterGrowthText + SniperData.GetSplatterGrowth().ToString() + " " + SniperData.SplatterRecursionText + SniperData.GetSplatterRecursions().ToString());
                }
            }
            else if (arg1.ToLower() == "sd" || arg1.ToLower() == "splatterdisk") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.splatterdisk)) {
                    SetSplatterSettings(packet);
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString() + " " + SniperData.SplatterSeedText + SniperData.GetSplatterSeed().ToString()
                        + " " + SniperData.SplatterGrowthText + SniperData.GetSplatterGrowth().ToString() + " " + SniperData.SplatterRecursionText + SniperData.GetSplatterRecursions().ToString());
                }
            }
            else if (arg1.ToLower() == "svd" || arg1.ToLower() == "splattervoxeldisk") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.splattervoxeldisk)) {
                    SetSplatterSettings(packet);
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString() + " " + SniperData.SplatterSeedText + SniperData.GetSplatterSeed().ToString()
                        + " " + SniperData.SplatterGrowthText + SniperData.GetSplatterGrowth().ToString() + " " + SniperData.SplatterRecursionText + SniperData.GetSplatterRecursions().ToString());
                }
            }
            else if (arg1.ToLower() == "sdf" || arg1.ToLower() == "splatterdiskface") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.splatterdiskface)) {
                    SetSplatterSettings(packet);
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString() + " " + SniperData.SplatterSeedText + SniperData.GetSplatterSeed().ToString()
                        + " " + SniperData.SplatterGrowthText + SniperData.GetSplatterGrowth().ToString() + " " + SniperData.SplatterRecursionText + SniperData.GetSplatterRecursions().ToString());
                }
            }
            else if (arg1.ToLower() == "svdf" || arg1.ToLower() == "splattervoxeldiskface") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.splattervoxeldiskface)) {
                    SetSplatterSettings(packet);
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString() + " " + SniperData.SplatterSeedText + SniperData.GetSplatterSeed().ToString()
                        + " " + SniperData.SplatterGrowthText + SniperData.GetSplatterGrowth().ToString() + " " + SniperData.SplatterRecursionText + SniperData.GetSplatterRecursions().ToString());
                }
            }
            else if (arg1.ToLower() == "e" || arg1.ToLower() == "erode") {
                SniperData.SetActiveBrushType(SniperData.BrushTypes.erode);
                if (packet.args.Length > 1) {
                    SetErosionType(packet.args[1]);
                }
                capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString() + ". Erosion type set to " + SniperData.GetActiveErosionType());
                return;
            }
            else if (arg1.ToLower() == "over" || arg1.ToLower() == "overlay") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.overlay)) {
                    if (packet.args.Length > 1) {
                        SetOverlayDebth(packet.args[1]);
                        SetOverlayPerformer(packet.args[1]);
                    }
                    if (packet.args.Length > 2) {
                        SetOverlayDebth(packet.args[2]);
                        SetOverlayPerformer(packet.args[2]);
                    }
                    capi.ShowChatMessage(SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString() + ". "
                        + SniperData.OverlayDebthText + SniperData.GetOverlayDebth().ToString() + ". "
                        + SniperData.OverlayPerformerText + SniperData.GetActiveOverlayPerformer());
                }
                return;
            }
            else if (arg1.ToLower() == "sover" || arg1.ToLower() == "splatteroverlay") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.splatteroverlay)) {
                    if (packet.args.Length > 1) {
                        SetOverlayDebth(packet.args[1]);
                        SetOverlayPerformer(packet.args[1]);
                    }
                    if (packet.args.Length > 2) {
                        SetOverlayDebth(packet.args[2]);
                        SetOverlayPerformer(packet.args[2]);
                    }
                    SetSplatterSettings(packet);
                    capi.ShowChatMessage(SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString() + ". "
                        + SniperData.OverlayDebthText + SniperData.GetOverlayDebth().ToString() + ". "
                        + SniperData.OverlayPerformerText + SniperData.GetActiveOverlayPerformer() + " "
                        + SniperData.SplatterSeedText + SniperData.GetSplatterSeed().ToString() + " "
                        + SniperData.SplatterGrowthText + SniperData.GetSplatterGrowth().ToString() + " "
                        + SniperData.SplatterRecursionText + SniperData.GetSplatterRecursions().ToString());
                }
                return;
            }
            else if (arg1.ToLower() == "drain") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.drain)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
                return;
            }
            else if (arg1.ToLower() == "set") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.set)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
            }
            else if (arg1.ToLower() == "cs" || arg1.ToLower() == "clonestamp") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.clonestamp)) {
                    SetCloneStampMode(packet);
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString()
                        + Environment.NewLine + SniperData.CloneStampModeSetToText + SniperData.GetActiveCloneStampMode() + ", "
                        + SniperData.CloneStampRotationModeSetToText + SniperData.GetActiveCloneStampRotationMode() + ", "
                        + SniperData.CloneStampQueueModeSetToText + SniperData.GetActiveCloneStampQueueMode() + ", "
                        + SniperData.CloneStampForestModeSetToText + SniperData.GetCloneStampForestOption() + ", "
                        + SniperData.CloneStampForestDensitySetToText + SniperData.GetCloneStampForestDensity());
                }
            }
            else if (arg1.ToLower() == "tp" || arg1.ToLower() == "teleport") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.teleport)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
                return;
            }
            //else if (arg1.ToLower() == "en" || arg1.ToLower() == "entity") {
            //    if (SniperData.SetActiveBrushType(SniperData.BrushTypes.entity)) {
            //        capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
            //    }
            //    return;
            //}
            else if (arg1.ToLower() == "bb" || arg1.ToLower() == "blendball") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.blendball)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
                return;
            }
            else if (arg1.ToLower() == "bv" || arg1.ToLower() == "blendvoxel") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.blendvoxel)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
                return;
            }
            else if (arg1.ToLower() == "bd" || arg1.ToLower() == "blenddisk") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.blenddisk)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
                return;
            }
            else if (arg1.ToLower() == "bvd" || arg1.ToLower() == "blendvoxeldisk") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.blendvoxeldisk)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
                return;
            }
            else if (arg1.ToLower() == "light" || arg1.ToLower() == "lightning") {
                if (SniperData.SetActiveBrushType(SniperData.BrushTypes.lightning)) {
                    capi.ShowChatMessage(Environment.NewLine + SniperData.BrushSetToText + SniperData.GetActiveBrushType().ToString());
                }
                return;
            }
            else {
                capi.ShowChatMessage("Invalid Brush");
            }

            if (packet.args.Length > 1) {
                for (int i = 1; i < packet.args.Length; i++) {
                    if (packet.args[i] == "m") {
                        SniperData.SetPerformer(SniperData.PerformerTypes.Material);
                        capi.ShowChatMessage("performer type set to material");
                    }
                    if (packet.args[i] == "mm") {
                        SniperData.SetPerformer(SniperData.PerformerTypes.MaterialReplace);
                        capi.ShowChatMessage("performer type set to material replace");
                    }
                }
            }


            SniperData.HandleBrushSizeText(arg1);
            capi.ShowChatMessage(SniperData.GetConfiguredBrushSizeDisplayString());
        }
        private void SetErosionType(string args) {
            if (args == null && args == "") {
                return;
            }
            if (args.ToLower() == "melt") {
                SniperData.SetActiveErosionType(SniperData.ErosionTypes.melt);
            }
            else if (args.ToLower() == "fill") {
                SniperData.SetActiveErosionType(SniperData.ErosionTypes.fill);
            }
            else if (args.ToLower() == "smooth") {
                SniperData.SetActiveErosionType(SniperData.ErosionTypes.smooth);
            }
            else if (args.ToLower() == "lift") {
                SniperData.SetActiveErosionType(SniperData.ErosionTypes.lift);
            }
            else if (args.ToLower() == "floatclean") {
                SniperData.SetActiveErosionType(SniperData.ErosionTypes.floatclean);
            }
        }
        private void SetOverlayDebth(string args) {
            args = args.ToLower();
            if (!args.Contains("d")) {
                return;
            }
            args = args.Replace("d", "");
            int debth = 0;
            if (SniperData.IsNumeric(args)) {
                debth = Int32.Parse(args);
                SniperData.SetOverlayDebth(debth);
            }
        }
        private void SetOverlayPerformer(string args) {
            args = args.ToLower();
            if (args == "all") {
                SniperData.SetOverlayPerformer(SniperData.OverlayPerformers.all);
            }
            else if (args == "some") {
                SniperData.SetOverlayPerformer(SniperData.OverlayPerformers.some);
            }
            else if (args == "replace") {
                SniperData.SetOverlayPerformer(SniperData.OverlayPerformers.replace);
            }
            else {
                return;
            }
        }
        private string ParseTreeString(string args, string[] ValidTrees) {
            args = args.ToLower().Trim();
            String[] ArgsList = args.Split(',');

            string ErrorPrefix = "Invalid tree type(s): ";
            string CurError = ErrorPrefix;

            for (int i = 0; i < ArgsList.Length; i++) {
                bool isvalid = false;
                for (int a = 0; a < ValidTrees.Length; a++) {
                    if (ArgsList[i] == ValidTrees[a]) {
                        isvalid = true;
                        break;
                    }
                }
                if (isvalid) {
                    continue;
                }
                else {
                    CurError += ArgsList[i] + " ";
                }
            }
            if (CurError == ErrorPrefix) {
                CurError = "";
            }
            return CurError;
        }
        private string GetValidTreeTypeString(CommandSendPacket packet) {
            string ValidTreeTypes = "Valid Tree Types: ";
            for (int i = 0; i < packet.ValidTreeTypes.Length; i++) {
                ValidTreeTypes += packet.ValidTreeTypes[i] + ", ";
            }
            return ValidTreeTypes;
        }
        private Vec2f ParseTreeSize(string[] args) {
            for (int i = 0; i < args.Length; i++) {
                if (args[i].ToLower().Contains("s:") || args[i].ToLower().Contains("size:")) {
                    string str = args[i].Replace("s:", "");
                    str.Replace("size:", "");
                    if (str.Contains(",")) {
                        string[] strs = str.Split(",");
                        if (strs.Length != 2) { break; }
                        Vec2f result = new Vec2f();
                        float first = 1;
                        float second = 1;
                        float.TryParse(strs[0], out first);
                        float.TryParse(strs[1], out second);
                        return new Vec2f(first, second);
                    }
                    else {
                        float number = 1f;
                        float.TryParse(str, out number);
                        return new Vec2f(number, number);
                    }
                }
            }
            return new Vec2f(1, 1);
        }
        private float ParseForestDensity(string[] args) {
            for (int i = 0; i < args.Length; i++) {
                if (args[i].ToLower().Contains("d:") || args[i].ToLower().Contains("density:")) {
                    string str = args[i].Replace("d:", "");
                    str.Replace("density:", "");
                    float density = 1;
                    float.TryParse(str, out density);
                    density = Math.Clamp(density, .01f, 1f);
                    return density;
                }
            }
            return 1f;
        }
        private void SetSplatterSettings(CommandSendPacket p) {
            for (int i = 1; i < p.args.Length; i++) {
                if (p.args[i].ToLower().Contains("s")) {
                    string str = p.args[i].ToLower().Replace("s", "");
                    if (SniperData.IsNumeric(str)) {
                        int seed = Int32.Parse(str);
                        SniperData.SetSplatterSeed(seed);
                        continue;
                    }
                }
                if (p.args[i].ToLower().Contains("g")) {
                    string str = p.args[i].ToLower().Replace("g", "");
                    if (SniperData.IsNumeric(str)) {
                        int seed = Int32.Parse(str);
                        SniperData.SetSplatterGrowth(seed);
                        continue;
                    }
                }
                if (p.args[i].ToLower().Contains("r")) {
                    string str = p.args[i].ToLower().Replace("r", "");
                    if (SniperData.IsNumeric(str)) {
                        int seed = Int32.Parse(str);
                        SniperData.SetSplatterRecursions(seed);
                        continue;
                    }
                }
            }
        }

        private void SetCloneStampMode(CommandSendPacket p) {
            if (p.args.Length < 2 || p.args[1] == null) { return; }
            for (int i = 1; i < p.args.Length; i++) {
                if (p.args[i] == "a") {
                    SniperData.SetActiveCloneStampMode(SniperData.CloneStampModes.excludeair);
                    continue;
                }
                else if (p.args[i] == "d") {
                    SniperData.SetActiveCloneStampMode(SniperData.CloneStampModes.force);
                    continue;
                }
                else if (p.args[i] == "f") {
                    SniperData.SetActiveCloneStampMode(SniperData.CloneStampModes.fill);
                    continue;
                }
                else if (p.args[i].StartsWith("queue:") || p.args[i].StartsWith("q:")) {
                    string str = p.args[i].Replace("q:", "").ToLower();
                    str = str.Replace("queue:", "").ToLower();
                    if (str == "s" || str == "single") {
                        SniperData.SetActiveCloneStampQueueMode(SniperData.CloneStampQueueModes.single);
                        continue;
                    }
                    if (str == "c" || str == "cycle") {
                        SniperData.SetActiveCloneStampQueueMode(SniperData.CloneStampQueueModes.cycle);
                        continue;
                    }
                    if (str == "r" || str == "random") {
                        SniperData.SetActiveCloneStampQueueMode(SniperData.CloneStampQueueModes.random);
                        continue;
                    }
                }
                else if (p.args[i].StartsWith("rotation:") || p.args[i].StartsWith("r:")) {
                    string str = p.args[i].Replace("r:", "").ToLower();
                    str = str.Replace("rotation:", "").ToLower();
                    if (str == "n" || str == "none") {
                        SniperData.SetActiveCloneStampRotationMode(SniperData.CloneStampRotationModes.none);
                        continue;
                    }
                    if (str == "c" || str == "cycle") {
                        SniperData.SetActiveCloneStampRotationMode(SniperData.CloneStampRotationModes.cycle);
                        continue;
                    }
                    if (str == "r" || str == "random") {
                        SniperData.SetActiveCloneStampRotationMode(SniperData.CloneStampRotationModes.random);
                        continue;
                    }
                }
                else if (p.args[i].StartsWith("forest:") || p.args[i].StartsWith("f:")) {
                    string str = p.args[i].Replace("f:", "").ToLower();
                    str = str.Replace("forest:", "").ToLower();
                    if (str == "y" || str == "yes" || str == "t" || str == "true") {
                        SniperData.SetCloneStampForestOption(true);
                        continue;
                    }
                    else if (str == "n" || str == "no" || str == "f" || str == "false") {
                        SniperData.SetCloneStampForestOption(false);
                        continue;
                    }
                }
                else if (p.args[i].StartsWith("density:") || p.args[i].StartsWith("d:")) {
                    SniperData.SetCloneStampForestDensity(ParseForestDensity(p.args));
                    continue;
                }
            }
        }

        #endregion

        #region CommandPacketSending

        private TextCommandResult B(TextCommandCallingArgs args) {
            CommandSendPacket p = new CommandSendPacket();
            p.CommandType = "b";
            if (args[0] != null) {
                string str = (string)args[0];
                string[] Arguments = Regex.Split(str, " ");
                p.args = Arguments;
                if (Arguments[0] == "t" || Arguments[0] == "tree" || Arguments[0] == "forest") {
                    p.ValidTreeTypes = sapi.World.TreeGenerators.Keys.Select(a => a.Path).ToArray();
                }
            }
            serverChannel.SendPacket(p, (IServerPlayer)args.Caller.Player);
            return TextCommandResult.Success();
        }
        private TextCommandResult V(TextCommandCallingArgs args) {
            CommandSendPacket p = new CommandSendPacket();
            p.CommandType = "v";
            if (args[0] != null) {
                string str = (string)args[0];
                string[] Arguments = Regex.Split(str, " ");
                p.args = Arguments;
            }
            serverChannel.SendPacket(p, (IServerPlayer)args.Caller.Player);
            return TextCommandResult.Success();
        }
        private TextCommandResult VR(TextCommandCallingArgs args) {
            CommandSendPacket p = new CommandSendPacket();
            p.CommandType = "vr";
            if (args[0] != null) {
                string str = (string)args[0];
                string[] Arguments = Regex.Split(str, " ");
                p.args = Arguments;
            }
            serverChannel.SendPacket(p, (IServerPlayer)args.Caller.Player);
            return TextCommandResult.Success();
        }
        private TextCommandResult VH(TextCommandCallingArgs args) {
            CommandSendPacket p = new CommandSendPacket();
            p.CommandType = "vh";
            if (args[0] != null) {
                string str = args[0].ToString();
                string[] Arguments = Regex.Split(str, " ");
                p.args = Arguments;
            }
            serverChannel.SendPacket(p, (IServerPlayer)args.Caller.Player);
            return TextCommandResult.Success();
        }
        private TextCommandResult VD(TextCommandCallingArgs args) {
            CommandSendPacket p = new CommandSendPacket();
            p.CommandType = "vd";
            if (args[0] != null) {
                string str = args[0].ToString();
                string[] Arguments = Regex.Split(str, " ");
                p.args = Arguments;
            }
            serverChannel.SendPacket(p, (IServerPlayer)args.Caller.Player);
            return TextCommandResult.Success();
        }
        private TextCommandResult VS(TextCommandCallingArgs args) {
            CommandSendPacket p = new CommandSendPacket();
            p.CommandType = "vs";
            if (args[0] != null) {
                string str = args[0].ToString();
                string[] Arguments = Regex.Split(str, " ");
                p.args = Arguments;
            }
            serverChannel.SendPacket(p, (IServerPlayer)args.Caller.Player);
            return TextCommandResult.Success();
        }
        private TextCommandResult VC(TextCommandCallingArgs args) {
            CommandSendPacket p = new CommandSendPacket();
            p.CommandType = "vc";
            if (args[0] != null) {
                string str = args[0].ToString();
                string[] Arguments = Regex.Split(str, " ");
                p.args = Arguments;
            }
            serverChannel.SendPacket(p, (IServerPlayer)args.Caller.Player);
            return TextCommandResult.Success();
        }
        #endregion
    }
}
