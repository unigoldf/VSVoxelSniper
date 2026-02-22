using ProtoBuf;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.ServerMods;
using VSVoxelSniper.src;


namespace VSVoxelSniper {
    internal partial class VSVoxelSniperModSystem : ModSystem {

        public ICoreServerAPI sapi;
        public ICoreClientAPI capi;
        ICoreAPI api;

        private List<ServerSideUserWorkspace> Spaces = new();
        GameTickListener tickListener;

        IServerNetworkChannel serverChannel;
        IClientNetworkChannel clientChannel;

        public override void StartServerSide(ICoreServerAPI sapi) {
            base.StartServerSide(sapi);
            this.sapi = sapi;
            RegisterCommands();
            RegisterPermissions();
            serverChannel = sapi.Network.RegisterChannel("VintageStoryVoxelSniper")
                .RegisterMessageType(typeof(BrushDataPacket))
                .SetMessageHandler(new NetworkClientMessageHandler<BrushDataPacket>(this.OnReceivedBrushPacket))
                .RegisterMessageType(typeof(CommandSendPacket))
                .RegisterMessageType(typeof(TextMessage))
            ;
        }
        public override void StartClientSide(ICoreClientAPI capi) {
            base.StartClientSide(capi);
            this.capi = capi;
            clientChannel = capi.Network.RegisterChannel("VintageStoryVoxelSniper")
                .RegisterMessageType(typeof(BrushDataPacket))
                .RegisterMessageType(typeof(CommandSendPacket))
                .SetMessageHandler<CommandSendPacket>(CommandReceivedHandler)
                .RegisterMessageType(typeof(TextMessage))
                .SetMessageHandler<TextMessage>(OnReceivedTextMessage)
            ;
            SetDefaultBrushSettings();
            capi.Event.RegisterGameTickListener(Update, 5);
        }

        private double lastStrokeTime;

        public void Update(float dt) {

            IPlayer LocalPlayer = capi.World.Player;

            ItemSlot slot = LocalPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot == null) { return; }

            Item arrow = capi.World.GetItem(new AssetLocation("arrow-gold"));
            Item powder = capi.World.GetItem(new AssetLocation("blastingpowder"));

            if (arrow == null || powder == null) { return; }

            SniperData.ToolType tool = SniperData.ToolType.arrow;

            if (slot?.Itemstack?.Item == arrow) {
            }
            else if (slot?.Itemstack?.Item == powder) {
                tool = SniperData.ToolType.gunpowder;
            }
            else {
                capi.World.HighlightBlocks(LocalPlayer, 12, new List<BlockPos>(), new List<int>() { ColorUtil.ColorFromRgba(0, 0, 0, 0) }, EnumHighlightBlocksMode.Absolute);
                return;
            }

            Vec3d pos = LocalPlayer.Entity.Pos.XYZ.Add(LocalPlayer.Entity.LocalEyePos);

            BlockSelection bs = null;
            EntitySelection es = null;

            IClientWorldAccessor wa = capi.World;
            bool OriState = wa.ForceLiquidSelectable;
            if (UserSettings.GetSelectLiquid()) {
                wa.ForceLiquidSelectable = true;
            }
            wa.RayTraceForSelection(pos, LocalPlayer.Entity.Pos.Pitch, LocalPlayer.Entity.Pos.Yaw, UserSettings.GetRange(), ref bs, ref es);
            wa.ForceLiquidSelectable = OriState;

            BlockPos TargetBlock = new BlockPos();
            BlockFacing TargetFace = BlockFacing.UP;

            if (bs == null) {
                Vec3d dir = GetVectorFromPitchAndYaw(LocalPlayer.Entity.Pos.Pitch, LocalPlayer.Entity.Pos.Yaw);
                Vec3d tpos = pos - (dir * UserSettings.GetRange());
                TargetBlock = new BlockPos(tpos.AsVec3i, LocalPlayer.Entity.Pos.Dimension);
            }
            else {
                TargetBlock = bs.Position;
                TargetFace = bs.Face;
            }

            if (clientChannel == null) {
                clientChannel = capi.Network.GetChannel("VintageStoryVoxelSniper");
            }
            bool Modified = false;
            if (capi.Input.KeyboardKeyState[(int)GlKeys.ShiftLeft]) {
                Modified = true;
            }

            if (UserSettings.GetHighlightTargetBlock()) {
                List<BlockPos> posi = new List<BlockPos>();
                posi.Add(TargetBlock);
                if (tool == SniperData.ToolType.arrow) {
                    capi.World.HighlightBlocks(LocalPlayer, 12, posi, new List<int>() { ColorUtil.ColorFromRgba(255, 100, 0, 60) }, EnumHighlightBlocksMode.Absolute);
                }
                else {
                    if (SniperData.ShouldGunpowerderOffset(SniperData.GetActiveBrushType())) {
                        BlockPos gpos = GetGunpowderOffsetPos(TargetBlock, TargetFace);
                        posi.Clear();
                        posi.Add(gpos);
                    }
                    capi.World.HighlightBlocks(LocalPlayer, 12, posi, new List<int>() { ColorUtil.ColorFromRgba(255, 100, 0, 60) }, EnumHighlightBlocksMode.Absolute);
                }
            }
            else {
                capi.World.HighlightBlocks(LocalPlayer, 12, new List<BlockPos>(), new List<int>() { ColorUtil.ColorFromRgba(0, 0, 0, 0) }, EnumHighlightBlocksMode.Absolute);
            }

            if (!capi.Input.MouseButton.Right) { return; }

            if (((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds) / 1000 < lastStrokeTime + UserSettings.GetStrokeFrequency()) { return; }
            lastStrokeTime = ((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds) / 1000;

            BrushDataPacket p = SniperData.CreateBrushStrokePacket(
                tool,
                SniperData.GetActiveBlock(),
                SniperData.GetActiveReplaceBlock(),
                TargetBlock.AsVec3i,
                LocalPlayer.Entity.Pos.XYZ.AsVec3i,
                LocalPlayer.Entity.LocalEyePos,
                SniperData.GetBlockFacingEnumFromClass(TargetFace),
                SniperData.GetActiveBrushType(),
                SniperData.GetBrushSize(Modified),
                Modified,
                SniperData.ActivePerformer,
                SniperData.GetActiveErosionType(),
                SniperData.GetOverlayDebth(),
                SniperData.GetActiveOverlayPerformer(),
                SniperData.GetSplatterSeed(),
                SniperData.GetSplatterGrowth(),
                SniperData.GetSplatterRecursions(),
                SniperData.GetVoxelHeight(),
                SniperData.GetVoxelDebth(),
                SniperData.GetVoxelCentroid(),
                SniperData.GetActiveCloneStampMode(),
                SniperData.GetActiveCloneStampQueueMode(),
                SniperData.GetActiveCloneStampRotationMode(),
                SniperData.GetCloneStampForestOption(),
                SniperData.GetCloneStampForestDensity(),
                SniperData.GetActiveTreetypes(),
                SniperData.GetTreeSizeRange(),
                SniperData.GetTreeDensity()
                );
            clientChannel.SendPacket(p);
        }
        private BlockPos GetGunpowderOffsetPos(BlockPos Pos, BlockFacing face) {
            if (face == BlockFacing.UP) {
                Pos.Y++;
                return Pos;
            }
            if (face == BlockFacing.NORTH) {
                Pos.Z--;
                return Pos;
            }
            if (face == BlockFacing.SOUTH) {
                Pos.Z++;
                return Pos;
            }
            if (face == BlockFacing.EAST) {
                Pos.X++;
                return Pos;
            }
            if (face == BlockFacing.WEST) {
                Pos.X--;
                return Pos;
            }
            if (face == BlockFacing.DOWN) {
                Pos.Y--;
                return Pos;
            }
            return Pos;
        }
        private Vec3d GetVectorFromPitchAndYaw(float pitch, float yaw) {
            double x = (float)(Math.Cos(pitch) * Math.Sin(yaw));
            double y = -(float)Math.Sin(pitch);
            double z = (float)(Math.Cos(pitch) * Math.Cos(yaw));
            double Magnitude = Math.Sqrt(x * x + y * y + z * z);
            if (Magnitude > 0) {
                x /= Magnitude;
                y /= Magnitude;
                z /= Magnitude;
            }

            return new Vec3d(x, y, z);
        }


        private void RegisterPermissions() {
            sapi.Permissions.RegisterPrivilege("vsvoxelsniperuser", "Allows the player to utilized VSVoxelSniper commands and brushes", true);
        }


        public override void Start(ICoreAPI api) {
            base.Start(api);
            this.api = api;
        }

        //private void RegisterModifierKey() {
        //    capi.Input.RegisterHotKey("vsmodifier", "Voxel Sniper Modifier", GlKeys.ShiftLeft, HotkeyType.CreativeTool);
        //    if (capi.Input.GetHotKeyByCode("vsmodifier").sta)
        //}

        private ServerSideUserWorkspace GetWorkspace(string playerid) {
            for (int i = 0; i < Spaces.Count; i++) {
                if (Spaces[i].PlayerID == playerid) {
                    return Spaces[i];
                }
            }

            return null;
        }
        private ServerSideUserWorkspace CreateWorkspace(string playerid) {
            ServerSideUserWorkspace ws = new ServerSideUserWorkspace();
            ws.PlayerID = playerid;
            ws.sapi = sapi;
            ws.api = api;
            Spaces.Add(ws);
            return ws;
        }

        public void OnReceivedTextMessage(TextMessage packet) {
            capi.ShowChatMessage(packet.text);

        }

        public void OnReceivedBrushPacket(IServerPlayer player, BrushDataPacket packet) {
            ServerSideUserWorkspace ws = GetWorkspace(player.PlayerUID);
            if (ws == null) {
                ws = CreateWorkspace(player.PlayerUID);
            }

            //Task task = new Task(() => {
            ws.HandleBrushStroke(player, packet);
            //});
            //task.Start();

        }
        private void SetDefaultBrushSettings() {
            SniperData.SetUnmodifiedBrushSize(3, 3);
            SniperData.SetModifiedBrushSize(5, 5);
            List<Block> block = new List<Block>();
            block.Add(api.World.GetBlock(new AssetLocation("rock-claystone")));
            SniperData.SetActiveBlock(block);
            block.Clear();
            block.Add(api.World.GetBlock(new AssetLocation("air")));
            SniperData.SetActiveReplaceBlock(block);
            SniperData.SetActiveBrushType(SniperData.BrushTypes.snipe);

        }
    }
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TextMessage() {
        public string text;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CommandSendPacket() {
        public string CommandType;
        public string[] args;
        public string[] ValidTreeTypes;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class BrushDataPacket {
        public int[] Material;
        public int[] ReplaceMaterial;
        public Vec3i BlockPos;
        public Vec3i PlayerPos;
        public Vec3d PlayerEyePos;
        public SniperData.FaceDirection face;
        public SniperData.BrushTypes brush;
        public int brushsize;
        public bool IsModified;
        public SniperData.PerformerTypes performer;
        public SniperData.ToolType tool;
        public SniperData.ErosionTypes erosionpreset;
        public int OverlayDebth;
        public SniperData.OverlayPerformers OverlayPerformer;
        public int SplatterSeed;
        public int SplatterGrowth;
        public int SplatterRecursions;
        public int VoxelHeight;
        public int VoxelDebth;
        public int VoxelCentroid;
        public SniperData.CloneStampModes CloneStampMode;
        public SniperData.CloneStampQueueModes CloneStampQueueMode;
        public SniperData.CloneStampRotationModes CloneStampRotationMode;
        public bool CloneStampForestOption;
        public float CloneStampForestDensity;
        public List<String> TreeTypes;
        public Vec2f TreeSizeRange;
        public float TreeDensity;
    }
}
