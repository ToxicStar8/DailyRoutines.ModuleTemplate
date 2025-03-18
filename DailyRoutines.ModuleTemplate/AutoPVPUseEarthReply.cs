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
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.Scheduler;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using Lumina.Excel.Sheets;
using OmenTools;
using OmenTools.Helpers;
using OmenTools.Infos;

namespace DailyRoutines.Modules;

public unsafe class AutoPVPUseEarthReply : DailyModuleBase
{
    private static Config ModuleConfig = null!;
    //金刚极意
    private const uint _useAction = 29482;
    //金刚转轮
    private const uint _afterAction = 29483;
    //疾跑状态
    private const uint _runStatus = 1342;
    //防御状态
    private const uint _defStatus = 3054;

    public override ModuleInfo Info => new()
    {
        Title = GetLoc("AutoPVPUseEarthReplyTitle"),
        Description = GetLoc("AutoPVPUseEarthReplyDescription"),
        Category = ModuleCategories.Action,
        Author = ["ToxicStar"],
    };

    public override void Init()
    {
        ModuleConfig ??= new Config();
        TaskHelper ??= new TaskHelper { TimeLimitMS = 8_000 };
        UseActionManager.Register(OnUseAction);
    }

    private void OnUseAction(bool result, ActionType actionType, uint actionID, ulong targetID, uint extraParam, ActionManager.UseActionMode queueState, uint comboRouteID, bool* outOptAreaTargeted)
    {
        if (!GameMain.IsInPvPArea() && !GameMain.IsInPvPInstance()) return;
        if (DService.ClientState.LocalPlayer is not { ClassJob.RowId: 20 }) return;

        if (result && actionType is ActionType.Action && actionID is _useAction)
        {
            TaskHelper.Abort();
            TaskHelper.DelayNext(8_000, $"Delay_UseAction{_afterAction}", false, 1);
            TaskHelper.Enqueue(() =>
            {
                if (DService.ClientState.LocalPlayer is not { } localPlayer) return;

                var statusManager = localPlayer.ToBCStruct()->StatusManager;
                if (!ModuleConfig.IsRunningUse   && statusManager.HasStatus(_runStatus)) return;
                if (!ModuleConfig.IsDefendingUse && statusManager.HasStatus(_defStatus)) return;

                UseActionManager.UseAction(ActionType.Action, _afterAction);

            }, $"UseAction_{_afterAction}", 500, true, 1);
        }
    }

    public override void ConfigUI()
    {
        if (ImGui.Checkbox(GetLoc("AutoPVPUseEarthReplyIsRunningUse"), ref ModuleConfig.IsRunningUse))
            SaveConfig(ModuleConfig);

        if (ImGui.Checkbox(GetLoc("AutoPVPUseEarthReplyIsDefendingUse"), ref ModuleConfig.IsDefendingUse))
            SaveConfig(ModuleConfig);
    }

    public override void Uninit()
    {
        base.Uninit();
        UseActionManager.Unregister(OnUseAction);
    }

    public class Config : ModuleConfiguration
    {
        public bool IsRunningUse = false;            //疾跑状态中也使用
        public bool IsDefendingUse = false;          //防御状态中也使用
    }
}
