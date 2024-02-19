using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurenailCore.ICUtil;

internal class ImmortalMylaController : MonoBehaviour
{
    private GameObject knight;
    private GameObject gate;
    private GameObject myla;

    private void Awake()
    {
        knight = HeroController.instance.gameObject;
        myla = GameObject.Find("Zombie Myla");

        gate = Instantiate(Preloader.Instance.BattleGateHorizontal);
        gate.transform.position = new(34.5f, 10.75f);
        gate.transform.localScale = new(1.5f, 1, 1);
        gate.SetActive(true);
    }

    private bool locked = false;

    private void Update()
    {
        if (locked) return;

        if (knight.transform.position.y < 9.5f && knight.transform.position.x > 25 && myla.GetComponent<HealthManager>().hp < 999999999)
        {
            locked = true;
            gate.LocateMyFSM("BG Control").SendEvent("BG CLOSE");
        }
    }
}

internal class ImmortalMylaModule : ItemChanger.Modules.Module
{
    public override void Initialize() => Events.AddSceneChangeEdit("Crossroads_45", MakeMylaImmortal);

    public override void Unload() => Events.RemoveSceneChangeEdit("Crossroads_45", MakeMylaImmortal);

    private const float RUN_SPEED = 12.5f;
    private const float SLASH_ANTIC_SPEEDUP = 0.34f;

    private void MakeMylaImmortal(Scene scene)
    {
        var pd = PlayerData.instance;
        if (!pd.GetBool(nameof(pd.hasSuperDash))) return;

        GameObject controller = new("ImmortalMyla");
        controller.AddComponent<ImmortalMylaController>();

        var myla = GameObject.Find("Zombie Myla");
        myla.GetComponent<HealthManager>().hp = 999999999;
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
    }

    private const float PICKAXE_BUFF = 2.25f;

    private void BuffPickaxe(FsmGameObject pickaxe, FsmState state)
    {
        state.AddLastAction(new Lambda(() =>
        {
            var obj = pickaxe.Value;
            obj.GetComponent<DamageHero>().damageDealt = 2;

            var r2d = obj.GetComponent<Rigidbody2D>();
            var oldVel = r2d.velocity;
            r2d.velocity = new(oldVel.x * PICKAXE_BUFF, oldVel.y / PICKAXE_BUFF);
        }));
    }
}
