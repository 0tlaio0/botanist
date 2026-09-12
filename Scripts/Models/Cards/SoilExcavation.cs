// 中文卡名：土壤开掘
// 卡面描述：获得{Capacity:diff()}个培育区栏位。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistSoilExcavation : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Earth;
    public override string PortraitPath => "res://botanist/images/botanist_character.svg";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new IntVar("Capacity", 2m)];

    public BotanistSoilExcavation() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        BotanistCultivation.AddCapacity(Owner, DynamicVars["Capacity"].IntValue);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Capacity"].UpgradeValueBy(1);
    }
}
