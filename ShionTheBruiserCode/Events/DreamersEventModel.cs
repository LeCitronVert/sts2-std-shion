using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Events;

/// <summary>
/// Base of the mod's events. Default layout: one portrait, images/events/(id without prefix).png.
/// The portrait fills the whole event screen (2560x1200, keep aspect): the art goes in the left half,
/// the right half stays dark under the text and options.
/// Its text lives in localization/*/events.json under its Id (title, pages.(PAGE).description, options).
///
/// Acts stays empty (shared pool) and the act is checked in IsAllowed instead: BaseLib reads Acts while ModelDb
/// is still being built, when ModelDb.Act isn't safe to call.
/// </summary>
public abstract class DreamersEventModel : CustomEventModel
{
    public override string CustomInitialPortraitPath => $"events/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".ImagePath();

    /// <summary>
    /// Whether the run is in one of these acts. Multiplayer-safe (all clients share the act).
    /// </summary>
    protected static bool IsInAct<T>(MegaCrit.Sts2.Core.Runs.IRunState runState) where T : ActModel =>
        runState.Act.Id == ModelDb.Act<T>().Id;
}
