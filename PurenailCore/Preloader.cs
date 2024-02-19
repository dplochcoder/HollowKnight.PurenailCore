using UnityEngine;
using PurenailCore.ModUtil;

namespace PurenailCore;

internal class Preloader : ModUtil.Preloader
{
    public static Preloader Instance { get; private set; } = new();

    [Preload("Waterways_08", "Ceiling Dropper")]
    public GameObject Belfly { get; private set; }

    [Preload("Fungus3_23", "Battle Gate")]
    public GameObject BattleGateHorizontal { get; private set; }

    [Preload("Fungus3_23_boss", "Battle Scene/Battle Gate (2)")]
    public GameObject BattleGateVertical { get; private set; }

    [Preload("Crossroads_09", "_Enemies/Mawlek Body")]
    public GameObject BroodingMawlek { get; private set; }

    [Preload("Fungus2_23", "Cave Spikes Invis")]
    public GameObject CaveSpikesInvis { get; private set; }

    [Preload("Fungus2_23", "Cave Spikes tile")]
    public GameObject CaveSpikesTile { get; private set; }

    [Preload("Hive_01", "Hive Breakable Pillar (1)")]
    public GameObject HiveWall { get; private set; }

    [Preload("Fungus2_03", "Mushroom Turret (4)")]
    public GameObject MushroomTurretCeiling { get; private set; }

    [Preload("Fungus2_03", "Mushroom Turret (1)")]
    public GameObject MushroomTurretLeft { get; private set; }

    [Preload("Fungus2_03", "Mushroom Turret")]
    public GameObject MushroomTurretRight { get; private set; }

    [Preload("Fungus3_48", "Plant Trap (8)")]
    public GameObject PlantTrapFloor { get; private set; }

    [Preload("Fungus3_48", "Plant Trap (7)")]
    public GameObject PlantTrapLeft { get; private set; }

    [Preload("Fungus3_48", "Plant Trap (6)")]
    public GameObject PlantTrapRight { get; private set; }

    public TinkEffect TinkEffect => CaveSpikesInvis.GetComponent<TinkEffect>();
}
