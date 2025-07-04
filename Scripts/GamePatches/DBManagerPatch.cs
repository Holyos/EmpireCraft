﻿using db;
using EmpireCraft.Scripts.GameClassExtensions;
using HarmonyLib;
using NeoModLoader.api;
using NeoModLoader.services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireCraft.Scripts.GamePatches;
public class DBManagerPatch:GamePatch
{
    public ModDeclare declare { get; set; }

    public void Initialize()
    {

        //new Harmony(nameof(on_quit)).Patch(
        //    AccessTools.Method(typeof(DBManager), nameof(DBManager.clearAndClose)),
        //    prefix: new HarmonyMethod(GetType(), nameof(on_quit))
        //);

        //new Harmony(nameof(on_application_quit)).Patch(
        //    AccessTools.Method(typeof(DBManager), nameof(DBManager.OnApplicationQuit)),
        //    prefix: new HarmonyMethod(GetType(), nameof(on_application_quit))
        //);
        //LogService.LogInfo("DBManagerPatch加载成功");
    }

    public static void on_application_quit(DBManager __instance)
    {
        ModClass.IS_CLEAR = true;
        AllClear();
    }

    public static void on_quit(DBManager __instance)
    {
        ModClass.IS_CLEAR = true;
    }

    public static void AllClear()
    {
        ActorExtension.Clear();
        CityExtension.Clear();
        ClanExtension.Clear();
        KingdomExtension.Clear();
        WarExtension.Clear();
        LogService.LogInfo("清空所有Mod数据");
    }
}