using System.Collections.Generic;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Botanist.Scripts;

public class BotanistCharacter : PlaceholderCharacterModel
{
    public override Color NameColor => new(0.36f, 0.62f, 0.78f);
    public override Color EnergyLabelOutlineColor => new(0.12f, 0.28f, 0.42f);
    public override Color MapDrawingColor => new(0.36f, 0.62f, 0.78f);
    public override CharacterGender Gender => CharacterGender.Masculine;

    public override int StartingHp => 80;
    public override int StartingGold => 99;

    public override string CustomVisualPath => "res://botanist/scenes/botanist_character.tscn";
    public override string CustomEnergyCounterPath => "res://botanist/scenes/botanist_energy_counter.tscn";
    public override string CustomIconTexturePath => BotanistArt.Character;
    public override string CustomCharacterSelectBg => "res://botanist/scenes/botanist_character_select_bg.tscn";
    public override string CustomCharacterSelectIconPath => BotanistArt.CharacterSelect;
    public override string CustomCharacterSelectLockedIconPath => BotanistArt.CharacterSelectLocked;

    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    public override CardPoolModel CardPool => ModelDb.CardPool<BotanistCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<BotanistRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<BotanistPotionPool>();

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<BotanistStrike>(),
        ModelDb.Card<BotanistStrike>(),
        ModelDb.Card<BotanistStrike>(),
        ModelDb.Card<BotanistStrike>(),
        ModelDb.Card<BotanistDefend>(),
        ModelDb.Card<BotanistDefend>(),
        ModelDb.Card<BotanistDefend>(),
        ModelDb.Card<BotanistDefend>(),
        ModelDb.Card<BotanistHumus>(),
        ModelDb.Card<BotanistEdelweiss>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<BotanistStarterRelic>()
    ];

    public override List<string> GetArchitectAttackVfx() =>
    [
        "vfx/vfx_attack_slash",
        "vfx/vfx_attack_blunt"
    ];
}
