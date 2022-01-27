using OpenMod.Core.Ioc;
using SilK.OpenMod.UnityEngine.Extended.Commands;
using System;

namespace SilK.OpenMod.Unturned.Extended.Commands
{
    [DontAutoRegister]
    public abstract class UnturnedParameterizedCommand : UnityEngineParameterizedCommand
    {
        protected UnturnedParameterizedCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
