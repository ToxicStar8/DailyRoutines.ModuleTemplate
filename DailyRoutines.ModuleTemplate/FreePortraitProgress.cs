using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DailyRoutines.Abstracts;
using DailyRoutines.Helpers;
using DailyRoutines.Infos;
using DailyRoutines.Managers;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using OmenTools;
using OmenTools.Helpers;
using OmenTools.Infos;

namespace DailyRoutines.Modules;

public unsafe class FreePortraitProgress : DailyModuleBase
{
    private static readonly CompSig FreePortraitProgressSig = new("48 89 5C 24 ?? 4C 8B 91 ?? ?? ?? ?? 33 C0");
    private delegate void InfoProxyBlackListUpdateDelegate(InfoProxyBlacklist.BlockResult* outBlockResult, ulong accountId, ulong contentId);
    private static Hook<InfoProxyBlackListUpdateDelegate>? FreePortraitProgressHook;

    public override ModuleInfo Info => new()
    {
        //Title = GetLoc("FreePortraitProgressTitle"),
        //Description = GetLoc("FreePortraitProgressDescription"),
        Title = "测试标题",
        Description = "测试说明",
        Category = ModuleCategories.System,
        Author = ["ToxicStar"],
    };

    public override void Init()
    {
        //FreePortraitProgressHook ??= FreePortraitProgressSig.GetHook<InfoProxyBlackListUpdateDelegate>(InfoProxyBlackListUpdateDetour);
        //FreePortraitProgressHook.Enable();

        FrameworkManager.Register(true, OnUpdate);
    }

    //private void InfoProxyBlackListUpdateDetour(InfoProxyBlacklist.BlockResult* outBlockResult, ulong accountId, ulong contentId)
    //{
    //    FreePortraitProgressHook.Original(outBlockResult, accountId, contentId);
    //}

    private static void OnUpdate(IFramework _)
    {
        var editor = AgentBannerEditor.Instance()->EditorState;
        if (editor is null) return;

        DService.Log.Debug((editor->CharaView is null).ToString());
    }

    public override void ConfigUI()
    {

    }

    public override void Uninit()
    {
        FrameworkManager.Unregister(OnUpdate);

        base.Uninit();
    }

    public class Config : ModuleConfiguration
    {
        public int CheckRange = 2;
    }
}
