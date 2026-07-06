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
        if (_selectedOption == null || _selectingPlayer == null)
            return;
            
        try
        {
            bool wasSuccessful = await __result;
            
            if (wasSuccessful)
            {
                await RestSiteHooks.OnRestSiteActionTaken(_selectingPlayer, _selectedOption);
            }
        }
        finally
        {
            _selectedOption = null;
            _selectingPlayer = null;
        }
    }
}
