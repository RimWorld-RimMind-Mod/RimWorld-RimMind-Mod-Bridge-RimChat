using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    /// <summary>
    /// 验证 ShouldSkipFloatMenuOption 与 ShouldSkipDialogue("PlayerInput") 始终返回相同结果。
    /// 消除逻辑二路：FloatMenu 应委托给 PlayerInput 路径。
    /// </summary>
    public class DialogueGateFloatMenuConsistencyTests
    {
        public DialogueGateFloatMenuConsistencyTests()
        {
            RimChatDetector.IsRimChatActive = false;
            BridgeRimChatSettings.Reset();
        }

        [Theory]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        [InlineData(false, false, true)]
        public void FloatMenu_与PlayerInput_始终一致(bool enableGate, bool skipPlayer, bool forceRimMind)
        {
            RimChatDetector.IsRimChatActive = true;
            var s = BridgeRimChatSettings.Get();
            s.enablePlayerInputGate = enableGate;
            s.skipPlayerDialogue = skipPlayer;
            s.forceRimMindPlayerDialogue = forceRimMind;

            var floatMenuResult = DialogueGate.ShouldSkipFloatMenuOption();
            var playerInputResult = DialogueGate.ShouldSkipDialogue(null!, "PlayerInput");

            Assert.Equal(playerInputResult, floatMenuResult);
        }
    }
}
