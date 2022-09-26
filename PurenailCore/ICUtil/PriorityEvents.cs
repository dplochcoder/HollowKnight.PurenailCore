using RandomizerMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurenailCore.ICUtil
{
    public static class PriorityEvents
    {
        public static void Setup()
        {
            On.SceneManager.Start += HandleBeforeSceneManagerStart;
        }

        public delegate void UpdateSceneManager(SceneManager sm);
        public static readonly PriorityEvent<UpdateSceneManager> BeforeSceneManagerStart = new(out _beforeSceneManagerStartOwner);
        private static readonly PriorityEvent<UpdateSceneManager>.IPriorityEventOwner _beforeSceneManagerStartOwner;

        private static void HandleBeforeSceneManagerStart(On.SceneManager.orig_Start orig, SceneManager self)
        {
            foreach (var updater in _beforeSceneManagerStartOwner.GetSubscribers())
            {
                updater(self);
            }
            orig(self);
        }
    }
}
