using RandomizerMod;

namespace PurenailCore.ICUtil
{
    public static class PriorityEvents
    {
        public static void Setup()
        {
            On.SceneManager.Start += OnSceneManagerStart;
        }

        public delegate void UpdateSceneManager(SceneManager sm);

        public static readonly PriorityEvent<UpdateSceneManager> BeforeSceneManagerStart = new(out _beforeSceneManagerStartOwner);
        private static readonly PriorityEvent<UpdateSceneManager>.IPriorityEventOwner _beforeSceneManagerStartOwner;
        public static readonly PriorityEvent<UpdateSceneManager> AfterSceneManagerStart = new(out _afterSceneManagerStartOwner);
        private static readonly PriorityEvent<UpdateSceneManager>.IPriorityEventOwner _afterSceneManagerStartOwner;

        private static void OnSceneManagerStart(On.SceneManager.orig_Start orig, SceneManager self)
        {
            foreach (var updater in _beforeSceneManagerStartOwner.GetSubscribers())
            {
                updater(self);
            }
            orig(self);
            foreach (var updater in _afterSceneManagerStartOwner.GetSubscribers())
            {
                updater(self);
            }
        }
    }
}
