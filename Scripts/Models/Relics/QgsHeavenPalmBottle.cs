using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;

namespace Qgs.Scripts;

/// <summary>同一回合培育三颗种子后，生成一张升级后的以太。</summary>
[Pool(typeof(QgsRelicPool))]
public class QgsHeavenPalmBottle : CustomRelicModel
{
    private const int SeedsPerReward = 3;

    private int _seedsCultivatedThisTurn;

    public override RelicRarity Rarity => RelicRarity.Rare;
    public override bool ShowCounter => CombatManager.Instance.IsInProgress;
    public override int DisplayAmount => _seedsCultivatedThisTurn % SeedsPerReward;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [new CardsVar(SeedsPerReward)];

    public override string PackedIconPath => "res://qgs/images/qgs_character.svg";
    protected override string PackedIconOutlinePath => "res://qgs/images/qgs_character.svg";
    protected override string BigIconPath => "res://qgs/images/qgs_character.svg";

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature))
        {
            _seedsCultivatedThisTurn = 0;
            InvokeDisplayAmountChanged();
        }

        return Task.CompletedTask;
    }

    public async Task OnSeedCultivated(PlayerChoiceContext choiceContext)
    {
        _seedsCultivatedThisTurn++;
        InvokeDisplayAmountChanged();

        if (_seedsCultivatedThisTurn % SeedsPerReward != 0)
        {
            return;
        }

        Flash();
        ICombatState? combatState = Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        QgsAether aether = combatState.CreateCard<QgsAether>(Owner);
        CardCmd.Upgrade(aether, CardPreviewStyle.None);
        await CardPileCmd.AddGeneratedCardToCombat(aether, PileType.Hand, Owner);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _seedsCultivatedThisTurn = 0;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }
}
