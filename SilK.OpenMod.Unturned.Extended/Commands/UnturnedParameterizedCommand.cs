using OpenMod.Core.Ioc;
using SilK.OpenMod.UnityEngine.Extended.Commands;
using System;

namespace SilK.OpenMod.Unturned.Extended.Commands
{
    /// <inheritdoc cref="UnityEngineParameterizedCommand"/>
    [DontAutoRegister]
    public abstract class UnturnedParameterizedCommand : UnityEngineParameterizedCommand
    {
        /// <inheritdoc cref="UnityEngineParameterizedCommand"/>
        protected UnturnedParameterizedCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
