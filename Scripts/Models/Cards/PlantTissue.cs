// 中文卡名：植物组织
// 卡面描述：
// 预览：[gold]成长[/gold]：复制第一颗种子成长效果。
// 生成后：[gold]成长[/gold]：{Growth}。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Botanist.Scripts;

[Pool(typeof(TokenCardPool))]
public class BotanistPlantTissue : BotanistSeedCardModel
{
    private IBotanistSeedCard? _copiedSeed;

    public override bool CanBeGeneratedInCombat => false;
    public override BotanistElement Element => BotanistElement.Water;
    public override string PortraitPath => "res://botanist/images/botanist_character.svg";
    public override string RipenSummary => GrowthText;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new StringVar("Growth", "无效果")];

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Fire, 1),
        new(BotanistElement.Water, 1)
    ];

    public BotanistPlantTissue() : base(1, CardType.Skill, CardRarity.Token, TargetType.Self, true)
    {
    }

    public void SetCopiedSeed(IBotanistSeedCard? seed)
    {
        _copiedSeed = seed;
        ((StringVar)DynamicVars["Growth"]).StringValue = GrowthText;
    }

    public override Task OnRipen(PlayerChoiceContext choiceContext)
    {
        return _copiedSeed?.OnRipen(choiceContext) ?? Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        description.Add("HasCopiedSeed", _copiedSeed != null);
    }

    private string GrowthText => _copiedSeed?.RipenSummary ?? "无效果";
}
