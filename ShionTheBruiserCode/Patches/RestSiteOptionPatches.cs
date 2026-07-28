using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using System;
using System.Threading.Tasks;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

[HarmonyPatch(typeof(RestSiteSynchronizer), "ChooseOption", MethodType.Normal)]
public class RestSiteOptionPatches
{
    [ThreadStatic] private static RestSiteOption? _selectedOption;
    [ThreadStatic] private static Player? _selectingPlayer;

    public static void Prefix(Player player, int optionIndex, RestSiteSynchronizer __instance)
    {
        var options = __instance.GetOptionsForPlayer(player);

        if (optionIndex >= 0 && optionIndex < options.Count)
        {
            _selectedOption = options[optionIndex];
            _selectingPlayer = player;
        }
    }
    
    public static void Postfix(Task<bool> __result)
    {
        var selectedOption = _selectedOption;
        var selectingPlayer = _selectingPlayer;

        _selectedOption = null;
        _selectingPlayer = null;

        if (selectedOption == null || selectingPlayer == null)
            return;
        
        _ = HandleRestSiteResult(__result, selectingPlayer, selectedOption);
    }

    private static async Task HandleRestSiteResult(Task<bool> resultTask, Player selectingPlayer, RestSiteOption selectedOption)
    {
        try
        {
            bool wasSuccessful = await resultTask;

            if (wasSuccessful)
            {
                await RestSiteHooks.OnRestSiteActionTaken(selectingPlayer, selectedOption);
            }
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Error in RestSiteHooks: {ex}");
        }
    }
}