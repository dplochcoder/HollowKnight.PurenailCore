using HutongGames.PlayMaker.Actions;
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
            if (fsm.FsmName == "damages_enemy") Object.Destroy(fsm);
        }
        foreach (var damageEnemies in Object.FindObjectsOfType<DamageEnemies>()) Object.Destroy(damageEnemies);
    }
}
