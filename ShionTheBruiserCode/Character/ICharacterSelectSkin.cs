using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

/// <summary>
/// One archetype shown as an icon in the character select info panel. Hovering it (or focusing it on gamepad)
/// shows "(Character ID).playstyle.(Key).title" / ".description" from characters.json.
/// </summary>
public sealed record CharacterPlaystyle(string Key, Func<string> IconPath)
{
    /// <summary>Uses the power's big icon (ours or a base game one).</summary>
    public static CharacterPlaystyle Power<T>(string key) where T : PowerModel =>
        new(key, () => ModelDb.Power<T>().ResolvedBigIconPath);

    /// <summary>Uses any image path, for icons that have no power behind them.</summary>
    public static CharacterPlaystyle Image(string key, string path) => new(key, () => path);
}

/// <summary>
/// Characters implementing this get the forum style info panel on the character select screen
/// (Patches/CharacterSelectSkinPatch.cs): class name above the first name ("(Character ID).className") and a row of playstyle icons.
/// </summary>
public interface ICharacterSelectSkin
{
    /// <summary>Empty hides the playstyle row.</summary>
    IReadOnlyList<CharacterPlaystyle> Playstyles { get; }
}
