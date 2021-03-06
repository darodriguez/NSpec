namespace NSpec.Domain
{
    public class ActChain : TraversingHookChain
    {
        protected override bool CanRun(nspec instance)
        {
            return context.BeforeAllChain.AnyThrew()
                ? false
                : !context.BeforeChain.AnyThrew();
        }

        public ActChain(Context context, Conventions conventions)
            : base(context, "act", "actAsync", "act_each")
        {
            methodSelector = conventions.GetMethodLevelAct;
            asyncMethodSelector = conventions.GetAsyncMethodLevelAct;
            chainSelector = c => c.ActChain;
        }
    }
}
