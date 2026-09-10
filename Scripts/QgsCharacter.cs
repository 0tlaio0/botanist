using System.Collections.Generic;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Qgs.Scripts;

public class QgsCharacter : PlaceholderCharacterModel
{
    public override Color NameColor => new(0.36f, 0.62f, 0.78f);
    public override Color EnergyLabelOutlineColor => new(0.12f, 0.28f, 0.42f);
    public override Color MapDrawingColor => new(0.36f, 0.62f, 0.78f);
    public override CharacterGender Gender => CharacterGender.Masculine;

    public override int StartingHp => 80;
    public override int StartingGold => 99;

    public override string CustomVisualPath => "res://qgs/scenes/qgs_character.tscn";
    public override string CustomEnergyCounterPath => "res://qgs/scenes/qgs_energy_counter.tscn";
    public override string CustomIconTexturePath => "res://qgs/images/qgs_character.svg";
    public override string CustomCharacterSelectBg => "res://qgs/scenes/qgs_character_select_bg.tscn";
    public override string CustomCharacterSelectIconPath => "res://qgs/images/character_select.svg";
    public override string CustomCharacterSelectLockedIconPath => "res://qgs/images/character_select_locked.svg";

    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    public override CardPoolModel CardPool => ModelDb.CardPool<QgsCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<QgsRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<QgsPotionPool>();

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<QgsStrike>(),
        ModelDb.Card<QgsStrike>(),
        ModelDb.Card<QgsStrike>(),
        ModelDb.Card<QgsStrike>(),
        ModelDb.Card<QgsDefend>(),
        ModelDb.Card<QgsDefend>(),
        ModelDb.Card<QgsDefend>(),
        ModelDb.Card<QgsDefend>(),
        ModelDb.Card<QgsSunflower>(),
        ModelDb.Card<QgsAether>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<QgsStarterRelic>()
    ];

    public override List<string> GetArchitectAttackVfx() =>
    [
        "vfx/vfx_attack_slash",
        "vfx/vfx_attack_blunt"
    ];
}
