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

public unsafe class AutoPVPEarthReply : DailyModuleBase
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
        //Title = GetLoc("AutoPVPEarthReplyTitle"),
        //Description = GetLoc("AutoPVPEarthReplyDescription"),
        Title = "自动金刚转轮",
        Description = "在战场时，自动在技能结束前使用你的金刚转轮",
        Category = ModuleCategories.Combat,
        Author = ["ToxicStar"],
    };

    public override void Init()
    {
        ModuleConfig ??= new Config();
        TaskHelper ??= new TaskHelper { TimeLimitMS = 15_000 };
        UseActionManager.Register(OnUseAction);
    }

    private void OnUseAction(bool result, ActionType actionType, uint actionID, ulong targetID, uint extraParam, ActionManager.UseActionMode queueState, uint comboRouteID, bool* outOptAreaTargeted)
    {
        if (!GameMain.IsInPvPArea() && !GameMain.IsInPvPInstance()) return;
        if (DService.ClientState.LocalPlayer is not { ClassJob.RowId: 20 }) return;

        DService.Log.Debug($"武僧职业在PVP区域使用技能 result={result} actionType={actionType} actionID={actionID}");

        if (result && actionType is ActionType.PvPAction && actionID is _useAction)
        {
            DService.Log.Debug("加入了任务队列");
            TaskHelper.Abort();
            //todo:测试，后续改为14.5秒
            TaskHelper.DelayNext(2_000, $"Delay_UseAction{_afterAction}", false, 1);
            //TaskHelper.DelayNext(14_500, $"Delay_UseAction{_afterAction}", false, 1);
            TaskHelper.Enqueue(() =>
            {
                if (DService.ClientState.LocalPlayer is not { } localPlayer) return;

                var statusManager = localPlayer.ToBCStruct()->StatusManager;
                if (statusManager.HasStatus(_runStatus) && !ModuleConfig.IsRunningUse) return;
                if (statusManager.HasStatus(_defStatus) && !ModuleConfig.IsDefendingUse) return;

                UseActionManager.UseAction(ActionType.PvPAction, _afterAction);

            }, $"UseAction_{_afterAction}", 500, true, 1);
        }
    }

    public override void ConfigUI()
    {
        //if (ImGui.Checkbox(GetLoc("AutoPVPEarthReplyIsRunningUse"), ref ModuleConfig.IsRunningUse))
        if (ImGui.Checkbox("疾跑状态中也使用", ref ModuleConfig.IsRunningUse))
            SaveConfig(ModuleConfig);

        //if (ImGui.Checkbox(GetLoc("AutoPVPEarthReplyIsDefendingUse"), ref ModuleConfig.IsDefendingUse))
        if (ImGui.Checkbox("防御状态中也使用", ref ModuleConfig.IsDefendingUse))
            SaveConfig(ModuleConfig);
    }

    public override void Uninit()
    {
        base.Uninit();
        UseActionManager.Unregister(OnUseAction);
    }

    public class Config : ModuleConfiguration
    {
        public bool IsRunningUse = true;            //疾跑状态中也使用
        public bool IsDefendingUse = true;          //防御状态中也使用
    }
}
