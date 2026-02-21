using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSVoxelSniper.src {
    internal static class UserSettings {

        public const string RangeSetToText = "Voxel Sniper Range set to: ";
        public const string LiquidSelectableText = "Voxel Sniper Liquid Selection Option set to: ";
        public const string StrokeFrequencyText = "Voxel Sniper Stroke Frquency set to: ";
        public const string ShowHighlightText = "Highlight Voxel Sniper Target Block: ";

        private static int Range = 350;
        private static bool SelectLiquid = true;
        private static float StrokeFrequency = .1f;
        private static bool HighlightTargetBlock = true;

        public static void SetRange(int range) {
            Range = range;
        }
        public static int GetRange() {
            return Range;
        }
        public static void SetSelectLiquid(bool ShouldSelectLiquid) {
            SelectLiquid = ShouldSelectLiquid;
        }
        public static bool GetSelectLiquid() {
            return SelectLiquid;
        }
        public static void SetStrokeFrequency(float strokeFrequency) {
            strokeFrequency = Math.Clamp(strokeFrequency, .05f, 1f);
            StrokeFrequency = strokeFrequency;
        }
        public static float GetStrokeFrequency() {
            return StrokeFrequency;
        }
        public static void SetHighlightTaretBlock(bool shouldhighlight) {
            HighlightTargetBlock = shouldhighlight;
        }
        public static bool GetHighlightTargetBlock() {
            return HighlightTargetBlock;
        }
    }
}
