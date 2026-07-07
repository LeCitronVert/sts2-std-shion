using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Multiplayer.Game;
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
    
    public static async void Postfix(Task<bool> __result)
    {
        // Capture the ThreadStatic values into local variables BEFORE the await
        var selectedOption = _selectedOption;
        var selectingPlayer = _selectingPlayer;
        
        // Clear the ThreadStatic variables immediately
        _selectedOption = null;
        _selectingPlayer = null;
        
        // Now use the local variables (which won't be affected by thread switches)
        if (selectedOption == null || selectingPlayer == null)
            return;
            
        try
        {
            bool wasSuccessful = await __result;
            
            if (wasSuccessful)
            {
                // Use the captured local variables, not the ThreadStatic ones
                await RestSiteHooks.OnRestSiteActionTaken(selectingPlayer, selectedOption);
            }
        }
        catch (Exception ex)
        {
            // Add error handling to prevent silent failures
            MainFile.Logger.Error($"Error in RestSiteHooks: {ex}");
        }
    }
}