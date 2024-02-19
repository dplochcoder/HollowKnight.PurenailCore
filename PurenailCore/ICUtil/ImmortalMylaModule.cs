using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurenailCore.ICUtil;

internal class ImmortalMylaController : MonoBehaviour
{
    public GameObject myla;
    private GameObject knight;
    private GameObject gate;

    private void Awake()
    {
        knight = HeroController.instance.gameObject;

        gate = Instantiate(Preloader.Instance.BattleGateHorizontal);
        gate.transform.position = new(34.5f, 10.75f);
        gate.transform.localScale = new(1.5f, 1, 1);
        gate.SetActive(true);
    }

    private bool locked = false;

    private void Update()
    {
        if (locked) return;

        if (knight.transform.position.y < 9.5f && knight.transform.position.x > 25 && myla.GetComponent<HealthManager>().hp < ImmortalMylaModule.MYLA_HEALTH)
        {
            locked = true;
            gate.LocateMyFSM("BG Control").SendEvent("BG CLOSE");

            myla.GetComponent<HealthManager>().OnDeath += () => StartCoroutine(SlowlyOpenGate());
        }
    }

    private IEnumerator SlowlyOpenGate()
    {
        yield return new WaitForSeconds(5);
        gate.LocateMyFSM("BG Control").SendEvent("BG OPEN");
    }
}

internal class ImmortalMylaModule : ItemChanger.Modules.Module
{
    public override void Initialize() => Events.AddSceneChangeEdit("Crossroads_45", MakeMylaImmortal);

    public override void Unload() => Events.RemoveSceneChangeEdit("Crossroads_45", MakeMylaImmortal);

    private const float RUN_SPEED = 11f;
    private const float SLASH_ANTIC_SPEEDUP = 0.175f;
    internal const int MYLA_HEALTH = 4000;

    private void MakeMylaImmortal(Scene scene)
    {
        var pd = PlayerData.instance;
        if (!pd.GetBool(nameof(pd.hasSuperDash))) return;

        // Don't spawn if Myla is dead.
        var myla = scene.FindGameObject("Zombie Myla");
        var pbi = myla.GetComponent<PersistentBoolItem>();
        pbi.PreSetup();
        if (pbi.persistentBoolData.activated) return;

        // No camping
        Object.Destroy(scene.FindGameObject("_Scenery/plat_float_02"));
        Object.Destroy(scene.FindGameObject("_Scenery/plat_float_03"));

        GameObject controller = new("ImmortalMyla");
        controller.AddComponent<ImmortalMylaController>().myla = myla;

        myla.GetComponent<HealthManager>().hp = MYLA_HEALTH;
        myla.GetComponent<DamageHero>().damageDealt = 2;
        myla.FindChild("Slash").GetComponent<DamageHero>().damageDealt = 4;

        var walker = myla.GetComponent<Walker>();
        walker.walkSpeedL = -RUN_SPEED;
        walker.walkSpeedR = RUN_SPEED;

        var fsm = myla.LocateMyFSM("Zombie Miner");
        fsm.FsmVariables.GetFsmFloat("Evade Speed").Value = 22.5f;
        fsm.FsmVariables.GetFsmFloat("Run Speed").Value = RUN_SPEED;
        fsm.FsmVariables.GetFsmFloat("Slash Speed").Value = 16f;

        // Throw pickaxes unconditionally.
        fsm.GetState("Attack Antic").GetFirstActionOfType<BoolTest>().isTrue = new("");

        // Speed up slash attack.
        var slashAntic = fsm.GetState("Slash Antic");
        slashAntic.AddLastAction(new Lambda(() =>
        {
            var animator = myla.GetComponent<tk2dSpriteAnimator>();
            animator.PlayFrom("Slash Antic", SLASH_ANTIC_SPEEDUP);
        }));

        var pickaxe = fsm.FsmVariables.GetFsmGameObject("Pickaxe");

        BuffPickaxe(pickaxe, fsm.GetState("Spawn Bullet L"));
        BuffPickaxe(pickaxe, fsm.GetState("Spawn Bullet R"));

        var cooldown = fsm.GetState("Cooldown");
        cooldown.AddFirstAction(new Lambda(() =>
        {
            if (spawnBulletChain) fsm.SetState("Attack Antic");
            for (int i = 1; i < cooldown.Actions.Length; i++) cooldown.Actions[i].Enabled = !spawnBulletChain;
        }));
    }

    private const float PICKAXE_SPEED = 25;
    private const float PICKAXE_SPEED_INC = 2.5f;
    private const float PICKAXE_GRAVITY = 36;
    private const float CHAIN_CHANCE = 0.4f;

    private bool spawnBulletChain = false;

    private void BuffPickaxe(FsmGameObject pickaxe, FsmState state)
    {
        var myla = state.Fsm.GameObject;
        state.AddLastAction(new Lambda(() =>
        {
            var obj = pickaxe.Value;
            obj.GetComponent<DamageHero>().damageDealt = 2;

            var kPos = HeroController.instance.transform.position;
            var mPos = myla.transform.position;
            var dist = kPos - mPos;

            // Increase pickaxe throw speed until we can hit the player.
            float v = PICKAXE_SPEED - PICKAXE_SPEED_INC;
            float g = PICKAXE_GRAVITY;
            var g2 = g * g;
            float dx = dist.x + Random.Range(-0.65f, 0.65f);
            float dy = dist.y;
            while (true)
            {
                v += PICKAXE_SPEED_INC;
                var v2 = v * v;
                var v4 = v2 * v2;

                float det1 = v4 - 2 * dy * g * v2 - dx * dx * g2;
                if (det1 < 0) continue;

                float p1 = v2 / g2 - dy / g;
                float p2 = Mathf.Sqrt(det1) / g2;
                float det2a = p1 - p2;
                float det2b = p1 + p2;
                if (det2a < 0 && det2b < 0) continue;

                float det = det2a < 0 ? det2b : det2a;
                float t = Mathf.Sqrt(det * 2);

                float vx = dx / t;
                if (vx > v) continue;

                float vy = Mathf.Sqrt(v2 - vx * vx);

                PurenailCore.Instance.Log($"dx: {dx}, dy: {dy}, t: {t}, v: {v}, vx: {vx}, vy: {vy}");
                obj.GetComponent<Rigidbody2D>().velocity = new(vx, vy);
                break;
            }

            spawnBulletChain = Random.Range(0f, 1f) <= CHAIN_CHANCE;
        }));
    }
}
