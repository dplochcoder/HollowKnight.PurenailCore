using ItemChanger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurenailCore.ICUtil;

internal record DamageEnemiesRemoverDeployer : IDeployer
{
    public string SceneName { get; set; } = "";

    public void OnSceneChange(Scene to)
    {
        foreach (var fsm in Object.FindObjectsOfType<PlayMakerFSM>())
        {
            if (fsm.FsmName == "damages_enemy" && IsHazardDamager(fsm.gameObject)) Object.Destroy(fsm);
            if (fsm.FsmName == "enemy_message" && fsm.FsmVariables.GetFsmString("Event")?.Value == "ACID") Object.Destroy(fsm);
        }
        foreach (var damageEnemies in Object.FindObjectsOfType<DamageEnemies>()) Object.Destroy(damageEnemies);
    }

    private bool IsHazardDamager(GameObject go)
    {
        if (go.GetComponent<DamageHero>() is DamageHero dh && dh.hazardType > 1) return true;
        if (go.LocateMyFSM("damages_hero") != null) return true;

        return false;
    }
}
