using OpenMod.Core.Commands;
using OpenMod.Core.Ioc;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SilK.OpenMod.Extended.Commands
{
    /// <summary>
    /// A command base class which uses reflection to automatically resolve and pass parameters to an OnExecuteAsync method.
    /// </summary>
    /// 
    /// <remarks>
    /// Only one OnExecuteAsync method must exist and
    /// the method must have a return type of <c>Task</c>,
    /// the method must be <c>protected</c>,
    /// and the method must be named <b>OnExecuteAsync</b> (case-sensitive).
    ///
    /// A basic example with a single parameter is as follows:
    /// <code>
    /// protected async Task OnExecuteAsync(int param)
    /// {
    ///     ...
    /// }
    /// </code>
    /// Parameters can be optional as well, assigning the default value if the actor does not specify an input.
    /// <code>
    /// protected async Task OnExecuteAsync(int param, string another = "default")
    /// {
    ///     ...
    /// }
    /// </code>
    /// </remarks>
    [DontAutoRegister]
    public abstract class ParameterizedCommand : CommandBase
    {
        /// <summary>
        /// The target command method name to be accessed via reflection.
        /// </summary>
        protected virtual string CommandMethodName => "OnExecuteAsync";

        /// <summary>
        /// The target command method return type.This can be overridden, but functionality
        /// for the new return type must be added to an override of <see cref="ExecuteMethod"/>.
        /// </summary>
        protected virtual Type CommandMethodType => typeof(Task);

        /// <summary>
        /// The constructor for the parameterized command.
        /// </summary>
        /// <param name="serviceProvider">The service provider to be used for service resolutions.</param>
        protected ParameterizedCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// The method used to execute this command instance.
        /// </summary>
        public sealed override async Task ExecuteAsync()
        {
            var method = GetType().GetMethod(CommandMethodName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null || method.ReturnType != CommandMethodType || method.IsGenericMethod)
            {
                throw new Exception($"No suitable {CommandMethodName} method could be found.");
            }

            var parameters = method.GetParameters();

            var parameterValues = new object[parameters.Length];

            foreach (var (parameter, index) in parameters.Select((x, y) => (x, y)))
            {
                var type = parameter.ParameterType;

                object parameterValue;

                // If parameter exists or no default value exists
                if (index < Context.Parameters.Length || !parameter.HasDefaultValue)
                {
                    parameterValue = await Context.Parameters.GetAsync(index, type);
                }
                else
                {
                    parameterValue = Type.Missing;
                }

                parameterValues[index] = parameterValue!;
            }

            await ExecuteMethod(method, parameterValues);
        }

        /// <summary>
        /// The method which handles the executing of the method found via reflection.
        /// This method must be overriden with new functionality if <see cref="CommandMethodType"/> is overriden.
        /// </summary>
        /// <param name="method">The target command method found via reflection.</param>
        /// <param name="parameters">The parameters supplied by the user to be passed to the method.</param>
        protected virtual async Task ExecuteMethod(MethodInfo method, object[] parameters)
        {
            var task = (Task)method.Invoke(this, parameters);

            await task;
        }
    }
}
