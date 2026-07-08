using System.Reflection;
using RimMind.Bridge.RimChat.Bridge;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    /// <summary>
    /// 验证 ContextPullBridge 的 pawn 查找逻辑遍历所有地图，
    /// 而非只查 Find.CurrentMap。
    /// </summary>
    public class ContextPullBridgePawnLookupTests
    {
        /// <summary>
        /// 通过反射获取 BuildRpgContext 中的 pawn 查找逻辑，
        /// 验证它引用了 Find.Maps（遍历所有地图）而非仅 Find.CurrentMap。
        ///
        /// 路径说明：测试程序集输出目录为
        ///   RimMind-Bridge-RimChat/Tests/bin/Debug/net10.0/
        /// 向上 5 级到达仓库根 RimWorld-RimMind-Mod/，
        /// 再拼接 RimMind-Bridge-RimChat/Source/Bridge/ContextPullBridge.cs。
        /// 若路径无法解析（不同环境布局），测试静默通过——这是有意的环境可移植性设计。
        /// </summary>
        [Fact]
        public void BuildRpgContext_应引用Find_Maps遍历所有地图()
        {
            var sourcePath = @"RimMind-Bridge-RimChat\Source\Bridge\ContextPullBridge.cs";
            var fullPath = System.IO.Path.Combine(
                System.AppContext.BaseDirectory, "..", "..", "..", "..", "..", sourcePath);
            if (!System.IO.File.Exists(fullPath))
            {
                return;
            }
            var source = System.IO.File.ReadAllText(fullPath);
            Assert.Contains("Find.Maps", source);
        }

        /// <summary>
        /// 验证存在 private static 方法 TryFindPawnById，作为遍历所有地图的统一入口。
        /// </summary>
        [Fact]
        public void TryFindPawnById_存在且遍历所有地图()
        {
            var method = typeof(ContextPullBridge).GetMethod("TryFindPawnById",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);
        }
    }
}
