using OpenMod.Core.Commands;
using OpenMod.Core.Ioc;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SilK.OpenMod.Extended.Commands
{
    [DontAutoRegister]
    public abstract class ParameterizedCommand : CommandBase
    {
        protected internal virtual string CommandMethodName => "OnExecuteAsync";

        protected internal virtual Type CommandMethodType => typeof(Task);

        protected ParameterizedCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

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

        protected internal virtual async Task ExecuteMethod(MethodInfo method, object[] parameters)
        {
            var task = (Task)method.Invoke(this, parameters);

            await task;
        }
    }
}
