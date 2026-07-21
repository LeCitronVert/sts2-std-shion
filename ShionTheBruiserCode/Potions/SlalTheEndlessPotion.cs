using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Potions;

[Pool(typeof(SlalTheEndlessPotionPool))]
public abstract class SlalTheEndlessPotion : CustomPotionModel
{
    public override string CustomPackedImagePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
    public override string CustomPackedOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".PotionImagePath();

}

