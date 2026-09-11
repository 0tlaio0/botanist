// 中文卡名：标本
// 卡面描述：
// 预览：复制生成标本的种子成长效果。
// 生成后：{Growth}。
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Qgs.Scripts;

[Pool(typeof(TokenCardPool))]
public class QgsSpecimen : QgsCardModel
{
    private IQgsSeedCard? _copiedSeed;

    public override bool CanBeGeneratedInCombat => false;
    public override QgsElement Element => QgsElement.Wind;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new StringVar("Growth", "无效果")];

    public QgsSpecimen() : base(1, CardType.Skill, CardRarity.Token, TargetType.Self, true)
    {
    }

    public void SetCopiedSeed(IQgsSeedCard? seed)
    {
        _copiedSeed = seed;
        ((StringVar)DynamicVars["Growth"]).StringValue = GrowthText;
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return _copiedSeed?.OnRipen(choiceContext) ?? Task.CompletedTask;
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        description.Add("HasCopiedSeed", _copiedSeed != null);
    }

    private string GrowthText => _copiedSeed?.RipenSummary ?? "无效果";
}
