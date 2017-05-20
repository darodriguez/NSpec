using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSpec.Domain.Extensions;

namespace NSpec.Domain
{
    public class BeforeChain
    {
        public void BuildMethodLevel(Conventions conventions, List<Type> classHierarchy)
        {
            var methods = ChainUtils.GetMethodsFromHierarchy(
                classHierarchy, conventions.GetMethodLevelBefore);

            if (methods.Count > 0)
            {
                ClassHook = instance => methods.Do(m => m.Invoke(instance, null));
            }

            var asyncMethods = ChainUtils.GetMethodsFromHierarchy(
                classHierarchy, conventions.GetAsyncMethodLevelBefore);

            if (asyncMethods.Count > 0)
            {
                AsyncClassHook = instance => asyncMethods.Do(m => new AsyncMethodLevelBefore(m).Run(instance));
            }
        }

        public void Run(nspec instance)
        {
            if (CanRun(instance))
            {
                ChainUtils.RunAndHandleException(RunHooks, instance, ref Exception);
            }
        }

        void RunHooks(nspec instance)
        {
            // parent chain

            context.RecurseAncestors(c => c.BeforeChain.RunHooks(instance));

            // class (method-level)

            if (ClassHook != null && AsyncClassHook != null)
            {
                throw new AsyncMismatchException(
                    "A spec class with all its ancestors cannot set both sync and async " +
                    "class-level 'before_each' hooks, they should either be all sync or all async");
            }

            ClassHook.SafeInvoke(instance);

            AsyncClassHook.SafeInvoke(instance);

            // context-level

            if (Hook != null && AsyncHook != null)
            {
                throw new AsyncMismatchException(
                    "A single context cannot set both a 'before' and an 'beforeAsync', please pick one of the two");
            }

            if (Hook != null && Hook.IsAsync())
            {
                throw new AsyncMismatchException(
                    "'before' cannot be set to an async delegate, please use 'beforeAsync' instead");
            }

            Hook.SafeInvoke();

            AsyncHook.SafeInvoke();
        }

        bool CanRun(nspec instance)
        {
            return !context.BeforeAllChain.AnyBeforeAllsThrew();
        }

        public BeforeChain(Context context)
        {
            this.context = context;
        }

        public Action Hook;
        public Func<Task> AsyncHook;
        
        public Action<nspec> ClassHook { get; private set; }
        public Action<nspec> AsyncClassHook { get; private set; }

        public Exception Exception;

        readonly Context context;
    }
}