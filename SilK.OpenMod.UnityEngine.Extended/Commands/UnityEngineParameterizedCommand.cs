using Cysharp.Threading.Tasks;
using OpenMod.Core.Ioc;
using SilK.OpenMod.Extended.Commands;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SilK.OpenMod.UnityEngine.Extended.Commands
{
    /// <summary>
    /// A command base class which uses reflection to automatically resolve and pass parameters to an OnExecuteAsync method.
    /// </summary>
    /// 
    /// <remarks>
    /// Only one OnExecuteAsync method must exist and
    /// the method must have a return type of <c>UniTask</c>,
    /// the method must be <c>protected</c>,
    /// and the method must be named <b>OnExecuteAsync</b> (case-sensitive).
    ///
    /// A basic example with a single parameter is as follows:
    /// <code>
    /// protected async UniTask OnExecuteAsync(int param)
    /// {
    ///     ...
    /// }
    /// </code>
    /// Parameters can be optional as well, assigning the default value if the actor does not specify an input.
    /// <code>
    /// protected async UniTask OnExecuteAsync(int param, string another = "default")
    /// {
    ///     ...
    /// }
    /// </code>
    /// </remarks>
    [DontAutoRegister]
    public abstract class UnityEngineParameterizedCommand : ParameterizedCommand
    {
        /// <inheritdoc />
        protected UnityEngineParameterizedCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <inheritdoc />
        protected override Type CommandMethodType => typeof(UniTask);

        /// <inheritdoc />
        protected override async Task ExecuteMethod(MethodInfo method, object[] parameters)
        {
            var task = (UniTask)method.Invoke(this, parameters);

            await task.AsTask();
        }
    }
}
