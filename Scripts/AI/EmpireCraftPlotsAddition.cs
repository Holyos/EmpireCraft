﻿using EmpireCraft.Scripts.Enums;
using EmpireCraft.Scripts.GameClassExtensions;
using EmpireCraft.Scripts.HelperFunc;
using EmpireCraft.Scripts.Layer;
using EmpireCraft.Scripts.TipAndLog;
using NCMS.Extensions;
using NeoModLoader.General;
using NeoModLoader.services;
using System.Collections.Generic;
using System.Linq;

namespace EmpireCraft.Scripts.AI
{
    public static class EmpireCraftPlotsAddition
    {
        public static void init()
        {
            AssetManager.plots_library.add(new PlotAsset
            {
                id = "become_empire",
                path_icon = "ChineseCrown.png",
                group_id = "diplomacy",
                is_basic_plot = true,
                min_level = 5,
                money_cost = 30,
                progress_needed = 60f,
                can_be_done_by_king = true,
                check_is_possible = delegate (Actor pActor)
                {
                    Kingdom kingdom = pActor.kingdom;
                    if (!pActor.isKing()) return false;
                    if (kingdom.isEmpire()) return false;
                    if (kingdom.isInEmpire()) return false;
                    ModClass.EMPIRE_MANAGER.update(-1L);
                    if (ModClass.EMPIRE_MANAGER.Select(e=>e.empire.getMainSubspecies()==kingdom.getMainSubspecies()).Count()>2) return false;
                    if ((kingdom.countCities() < 5)&&(!kingdom.isSupreme()&&World.world.kingdoms.Count()>=2)) return false;
                    return true;
                },
                
                action = BecomeEmpireAndStartEnfeoff
            });
            AssetManager.plots_library.add(new PlotAsset
            {
                id = "empire_enfeoff",
                path_icon = "ChineseCrown.png",
                group_id = "diplomacy",
                is_basic_plot = true,
                min_level = 1,
                money_cost = 100,
                progress_needed = 30f,
                can_be_done_by_king = true,
                check_is_possible = delegate (Actor pActor)
                {
                    Kingdom kingdom = pActor.kingdom;
                    if (!pActor.isKing()) return false;
                    if (!kingdom.isEmpire()) return false;
                    if (!kingdom.isInEmpire()) return false;
                    if (kingdom.hasEnemies()) return false;
                    if (kingdom.countCities() < 3) return false;
                    return true;
                },
                
                action = StartEnfeoff
            });
            AssetManager.plots_library.add(new PlotAsset
            {
                id = "emperor_year_name",
                path_icon = "EmperorQuest.png",
                group_id = "diplomacy",
                is_basic_plot = true,
                min_level = 1,
                progress_needed = 15f,
                can_be_done_by_king = true,
                check_is_possible = delegate (Actor pActor)
                {
                    Kingdom kingdom = pActor.kingdom;
                    if (!pActor.isKing()) return false;
                    if (!kingdom.isEmpire()) return false;
                    if (!kingdom.isInEmpire()) return false;
                    if (!kingdom.GetEmpire().isAllowToMakeYearName()) return false;
                    if (kingdom.GetEmpire().hasYearName()) return false;
                    return true;
                },
                action = delegate(Actor pActor) 
                {
                    Kingdom kingdom = pActor.kingdom;
                    kingdom.GetEmpire().create_year_name();
                    TranslateHelper.LogNewEmperor(pActor, kingdom.capital, kingdom.GetEmpire().data.year_name);
                    return true;
                }
            });
            AssetManager.plots_library.add(new PlotAsset
            {
                id = "king_acquire_title",
                path_icon = "TitleAcquire.png",
                group_id = "diplomacy",
                is_basic_plot = true,
                min_level = 1,
                progress_needed = 15f,
                can_be_done_by_king = true,
                check_is_possible = delegate (Actor pActor)
                {
                    Kingdom kingdom = pActor.kingdom;
                    if (!pActor.isKing()) return false;
                    if (!pActor.canAcquireTitle()) return false;
                    if (kingdom.getWars().Count() > 0) return false;
                    return true;
                },
                action = delegate(Actor pActor) 
                {
                    Kingdom kingdom = pActor.kingdom;
                    List<KingdomTitle> titles = pActor.getAcquireTitle();
                    if (titles.Count() <= 0) return false;
                    foreach (KingdomTitle title in titles)
                    {
                        foreach(City city in title.city_list)
                        {
                            if (!kingdom.cities.Contains(city))
                            {
                                Kingdom targetKingdom = city.kingdom;
                                if (kingdom.countTotalWarriors() > targetKingdom.countTotalWarriors())
                                {
                                    War war = World.world.diplomacy.startWar(kingdom, targetKingdom, WarTypeLibrary.normal);
                                    TranslateHelper.LogKingdomAcquireTitle(kingdom, targetKingdom, title);
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
            });
            AssetManager.plots_library.add(new PlotAsset
            {
                id = "kingdom_destroy_title",
                path_icon = "EmperorQuest.png",
                group_id = "diplomacy",
                is_basic_plot = true,
                min_level = 1,
                progress_needed = 15f,
                can_be_done_by_king = true,
                check_is_possible = delegate (Actor pActor)
                {
                    Kingdom kingdom = pActor.kingdom;
                    if (!pActor.isKing()) return false;
                    if (ModClass.KINGDOM_TITLE_FREEZE) return false;
                    if (pActor.titleCanBeDestroy().Count()<=0) return false;
                    return true;
                },
                action = delegate(Actor pActor) 
                {
                    Kingdom kingdom = pActor.kingdom;
                    List<KingdomTitle> titles = pActor.titleCanBeDestroy();
                    foreach(KingdomTitle title in titles)
                    {
                        ModClass.KINGDOM_TITLE_MANAGER.dissolveTitle(title);
                        pActor.removeTitle(title);
                        TranslateHelper.LogDestroyTitle(kingdom, title);
                    }
                    if (kingdom.capital.hasTitle()&&pActor.GetOwnedTitle().Contains(kingdom.capital.GetTitle().data.id))
                    {
                        KingdomTitle title = kingdom.capital.GetTitle();
                        foreach(City city in kingdom.cities)
                        {
                            if (city.hasTitle()&&!city.isCapitalCity())
                            {
                                title.addCity(city);
                                TranslateHelper.LogCityAddToTitle(city, title);
                            }   
                        }
                    }else
                    {
                        KingdomTitle title = ModClass.KINGDOM_TITLE_MANAGER.newKingdomTitle(kingdom.capital);
                        foreach (City city in kingdom.cities)
                        {
                            if (city.hasTitle() && !city.isCapitalCity())
                            {
                                title.addCity(city);
                                TranslateHelper.LogCityAddToTitle(city, title);
                            }
                        }
                    }
                    return true;
                }
            });
            AssetManager.plots_library.add(new PlotAsset
            {
                id = "kingdom_get_title",
                path_icon = "EmperorQuest.png",
                group_id = "diplomacy",
                is_basic_plot = true,
                min_level = 1,
                progress_needed = 15f,
                can_be_done_by_king = true,
                check_is_possible = delegate (Actor pActor)
                {
                    if (pActor == null) return false;
                    Kingdom kingdom = pActor.kingdom;
                    if (!pActor.isKing()) return false;
                    if (!pActor.canTakeTitle()) return false;
                    return true;
                },
                action = delegate(Actor pActor) 
                {
                    Kingdom kingdom = pActor.kingdom;
                    List<KingdomTitle> titles = pActor.takeTitle();
                    foreach(KingdomTitle title in titles)
                    {
                        TranslateHelper.LogKingTakeTitle(kingdom, title);
                    }
                    return true;
                }
            }); 
            AssetManager.plots_library.add(new PlotAsset
            {
                id = "kingdom_change_capital_title",
                path_icon = "EmperorQuest.png",
                group_id = "diplomacy",
                is_basic_plot = true,
                min_level = 1,
                progress_needed = 15f,
                can_be_done_by_king = true,
                check_is_possible = delegate (Actor pActor)
                {
                    if (pActor == null) return false;
                    Kingdom kingdom = pActor.kingdom;
                    if (!pActor.isKing()) return false;
                    if (!pActor.kingdom.HasMainTitle()) return false;
                    if (pActor.kingdom.GetMainTitle().title_capital==kingdom.capital) return false;
                    if (!kingdom.cities.Contains(pActor.kingdom.GetMainTitle().title_capital)) return false;
                    return true;
                },
                action = delegate(Actor pActor) 
                {
                    Kingdom kingdom = pActor.kingdom;
                    kingdom.setCapital(kingdom.GetMainTitle().title_capital);
                    TranslateHelper.LogKingdomChangeCapitalToTitle(kingdom, kingdom.GetMainTitle());
                    return true;
                }
            });
            AssetManager.plots_library.add(new PlotAsset
            {
                id = "kingdom_join_empire",
                path_icon = "EmperorQuest.png",
                group_id = "diplomacy",
                is_basic_plot = true,
                min_level = 1,
                progress_needed = 15f,
                can_be_done_by_king = true,
                check_is_possible = delegate (Actor pActor)
                {
                    if (pActor == null) return false;
                    Kingdom kingdom = pActor.kingdom;
                    if (!pActor.isKing()) return false;
                    if (kingdom.HasTitle()) return false;
                    if (kingdom.isEmpire()) return false;
                    if (kingdom.isInEmpire()) return false;
                    if (kingdom.GetEmpiresCanbeJoined().Count() <= 0) return false;
                    return true;
                },
                action = delegate(Actor pActor) 
                {
                    Kingdom kingdom = pActor.kingdom;
                    kingdom.GetEmpiresCanbeJoined().FirstOrDefault().join(kingdom);
                    kingdom.SetCountryLevel(countryLevel.countrylevel_4);
                    kingdom.getWars().ForEach(war => war.endForSides(WarWinner.Nobody));
                    TranslateHelper.LogKingdomJoinEmpire(kingdom, kingdom.GetEmpire());
                    return true;
                }
            });
            AssetManager.plots_library.add(new PlotAsset
            {
                id = "kingdom_create_title",
                path_icon = "EmperorQuest.png",
                group_id = "diplomacy",
                is_basic_plot = true,
                min_level = 1,
                progress_needed = 15f,
                can_be_done_by_king = true,
                check_is_possible = delegate (Actor pActor)
                {
                    Kingdom kingdom = pActor.kingdom;
                    if (kingdom.capital==null) return false;
                    if (!pActor.isKing()) return false;
                    if (!pActor.hasKingdom()) return false; 
                    if (kingdom.capital.hasTitle()) return false;
                    return true;
                },
                action = delegate(Actor pActor) 
                {
                    Kingdom kingdom = pActor.kingdom;
                    KingdomTitle title = ModClass.KINGDOM_TITLE_MANAGER.newKingdomTitle(kingdom.capital);
                    TranslateHelper.LogCreateTitle(kingdom, title);
                    title.owner = pActor;
                    pActor.AddOwnedTitle(title);
                    foreach(City c in kingdom.cities)
                    {
                        if (!c.hasTitle())
                        {
                            title.addCity(c);
                            TranslateHelper.LogCityAddToTitle(c, title);
                        }
                    }
                    return true;
                }
            });
            AssetManager.plots_library.add(new PlotAsset
            {
                id = "kingdom_change_name",
                path_icon = "EmperorQuest.png",
                group_id = "diplomacy",
                is_basic_plot = true,
                min_level = 1,
                progress_needed = 15f,
                can_be_done_by_king = true,
                check_is_possible = delegate (Actor pActor)
                {
                    Kingdom kingdom = pActor.kingdom;
                    if (!pActor.isKing()) return false;
                    if (!pActor.HasTitle()) return false;
                    if (!pActor.kingdom.needToBecomeKingdom()) return false;
                    return true;
                },
                action = delegate(Actor pActor) 
                {
                    Kingdom kingdom = pActor.kingdom;
                    string name = kingdom.becomeKingdom(true);
                    TranslateHelper.LogBecomeKingdom(kingdom, name);
                    return true;
                }
            });
            AssetManager.plots_library.add(new PlotAsset
            {
                id = "ministor_acquire_empire",
                path_icon = "MinistorAcquireEmpire.png",
                group_id = "diplomacy",
                is_basic_plot = true,
                min_level = 1,
                money_cost = 30,
                progress_needed = 30f,
                can_be_done_by_king = true,
                requires_diplomacy = true,
                check_is_possible = delegate (Actor pActor)
                {
                    if (pActor==null) return false;
                    Kingdom kingdom = pActor.kingdom;
                    if (!DiplomacyHelpers.isWarNeeded(kingdom)) return false;
                    if (!pActor.isKing()) return false;
                    if (kingdom.isEmpire()) return false;
                    if (!kingdom.isInEmpire()) return false;
                    try
                    {
                        if (!pActor.HasTitle() || (pActor.hasClan() ? pActor.clan.getID() != kingdom.GetEmpire().empire_clan.getID() : true)) return false;
                    } catch
                    {
                        return false;
                    }

                    if (kingdom.countTotalWarriors()<kingdom.GetEmpire().countWarriors()- kingdom.countTotalWarriors()) return false;
                    return true;
                },
                action = ministor_acquire_empire
            });
            AssetManager.plots_library.add(new PlotAsset
            {
                id = "ministor_acquire_title",
                path_icon = "MinistorAcquireTitle.png",
                group_id = "diplomacy",
                is_basic_plot = true,
                min_level = 5,
                progress_needed = 30f,
                can_be_done_by_king = true,
                requires_diplomacy = true,
                check_is_possible = delegate (Actor pActor)
                {
                    Kingdom kingdom = pActor.kingdom;
                    if (!DiplomacyHelpers.isWarNeeded(kingdom)) return false;
                    if (!pActor.isKing()) return false;
                    if (kingdom.isEmpire()) return false;
                    if (!kingdom.isInEmpire()) return false;
                    if (pActor.HasCapitalTitle()) return false;
                    if (!pActor.IsCapitalTitleBelongsToEmperor()) return false;
                    if (kingdom.GetEmpire().emperor == null) return false; 
                    if (kingdom.GetEmpire().emperor.GetOwnedTitle().Count()<=1) return false; 
                    if (pActor.GetPeeragesLevel()==PeeragesLevel.peerages_2) return false;
                    if (kingdom.countTotalWarriors()<kingdom.GetEmpire().countWarriors()- kingdom.countTotalWarriors()) return false;
                    return true;
                },
                action = delegate (Actor pActor) 
                {
                    Empire empire = pActor.kingdom.GetEmpire();
                    Kingdom kingdom = pActor.kingdom;
                    KingdomTitle kingdomTitle;
                    if (kingdom.capital.hasTitle())
                    {
                        kingdomTitle = kingdom.capital.GetTitle();
                        empire.emperor.removeTitle(kingdomTitle);
                        TranslateHelper.LogPowerfulMinisterAcquireTitle(pActor, pActor.kingdom.GetEmpire(), kingdomTitle.data.name + LM.Get("King"));
                    } else
                    {
                        kingdomTitle = ModClass.KINGDOM_TITLE_MANAGER.newKingdomTitle(kingdom.capital);
                        TranslateHelper.LogCreateTitle(kingdom, kingdomTitle);
                        foreach (City city in kingdom.cities)
                        {
                            if (!city.hasTitle())
                            {
                                kingdomTitle.addCity(city);
                                TranslateHelper.LogCityAddToTitle(city, kingdomTitle);
                            }
                        }
                    }
                    pActor.AddOwnedTitle(kingdomTitle);

                    pActor.SetPeeragesLevel(Enums.PeeragesLevel.peerages_2);
                    return true;
                }
            });
            LogService.LogInfo($"目前加载{AssetManager.plots_library.getList().Count().ToString()}个政策");
            AssetManager.plots_library.linkAssets();
        }

        private static bool ministor_acquire_empire(Actor pActor)
        {

            new WorldLogMessage(EmpireCraftWorldLogLibrary.ministor_try_aqcuire_empire_log, pActor.GetTitle(), pActor.data.name, pActor.kingdom.GetEmpire().data.name)
            {
                color_special1 = pActor.kingdom.getColor()._colorText,
                color_special2 = pActor.kingdom.GetEmpire().empire.getColor()._colorText
            }.add();

            if ((float)pActor.kingdom.countCities() / (float)(pActor.kingdom.GetEmpire().countCities()- pActor.kingdom.countCities())>=4)
            {
                pActor.kingdom.GetEmpire().replaceEmpire(pActor.kingdom);
            } 
            else
            {
                War war = World.world.diplomacy.startWar(pActor.kingdom, pActor.kingdom.GetEmpire().empire, WarTypeLibrary.normal);
                if (war != null)
                {
                    war.SetEmpireWarType(EmpireWarType.AquireEmpire);
                }
            }
            return true;
        }

        private static bool BecomeEmpireAndStartEnfeoff(Actor pActor)
        {
            Kingdom kingdom = pActor.kingdom;
            Empire empire = ModClass.EMPIRE_MANAGER.newEmpire(kingdom);
            empire.AutoEnfeoff();
            return true;
        }

        private static bool StartEnfeoff(Actor pActor)
        {
            Kingdom kingdom = pActor.kingdom;
            kingdom.GetEmpire().AutoEnfeoff();
            return true;
        }

    }
}
