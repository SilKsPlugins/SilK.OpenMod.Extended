using Cysharp.Threading.Tasks;
using OpenMod.Core.Ioc;
using SilK.OpenMod.Extended.Commands;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SilK.OpenMod.UnityEngine.Extended.Commands
{
    [DontAutoRegister]
    public abstract class UnityEngineParameterizedCommand : ParameterizedCommand
    {
        protected UnityEngineParameterizedCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override Type CommandMethodType => typeof(UniTask);

        protected override async Task ExecuteMethod(MethodInfo method, object[] parameters)
        {
            var task = (UniTask)method.Invoke(this, parameters);

            await task.AsTask();
        }
    }
}
