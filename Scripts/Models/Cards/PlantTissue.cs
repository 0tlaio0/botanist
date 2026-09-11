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

namespace Qgs.Scripts;

[Pool(typeof(QgsCardPool))]
public class QgsPlantTissue : QgsSeedCardModel
{
    private IQgsSeedCard? _copiedSeed;

    public override bool CanBeGeneratedInCombat => false;
    public override QgsElement Element => QgsElement.Water;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";
    public override string RipenSummary => GrowthText;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new StringVar("Growth", "无效果")];

    public override IReadOnlyList<KeyValuePair<QgsElement, int>> Requirements =>
    [
        new(QgsElement.Fire, 1),
        new(QgsElement.Water, 1)
    ];

    public QgsPlantTissue() : base(1, CardType.Skill, CardRarity.Token, TargetType.Self, true)
    {
    }

    public void SetCopiedSeed(IQgsSeedCard? seed)
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
