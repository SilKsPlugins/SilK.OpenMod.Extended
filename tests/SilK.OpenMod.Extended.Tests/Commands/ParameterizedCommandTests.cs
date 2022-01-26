using Microsoft.Extensions.Localization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenMod.API.Commands;
using OpenMod.API.Localization;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using SilK.OpenMod.Extended.Commands;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SilK.OpenMod.Extended.Tests.Commands
{
    [TestClass]
    public class ParameterizedCommandTests
    {
        private static class Commands
        {
            public class Empty : ParameterizedCommand
            {
                public Empty(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }
            }

            public class NoMatch : ParameterizedCommand
            {
                public NoMatch(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected Task RandomMethod() => Task.CompletedTask;
            }

            public class PublicMethod : ParameterizedCommand
            {
                public PublicMethod(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                public Task OnExecuteAsync() => Task.CompletedTask;
            }

            public class CaseSensitiveMethod : ParameterizedCommand
            {
                public CaseSensitiveMethod(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                public Task onexecuteasync() => Task.CompletedTask;
            }

            public class ReturnVoid : ParameterizedCommand
            {
                public ReturnVoid(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected void OnExecuteAsync() { }
            }

            public class ReturnValue : ParameterizedCommand
            {
                public ReturnValue(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected int OnExecuteAsync() => 2;
            }

            public class TwoMatch : ParameterizedCommand
            {
                public TwoMatch(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected Task OnExecuteAsync() => Task.CompletedTask;

                protected Task OnExecuteAsync(int param1) => Task.CompletedTask;
            }

            public class ThreeMatch : ParameterizedCommand
            {
                public ThreeMatch(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected Task OnExecuteAsync() => Task.CompletedTask;

                protected Task OnExecuteAsync(int param1) => Task.CompletedTask;

                protected Task OnExecuteAsync(int param1, string param2) => Task.CompletedTask;
            }

            public class ParametersNone : ParameterizedCommand
            {
                public ParametersNone(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected Task OnExecuteAsync() => Task.CompletedTask;
            }

            public class ParametersInt : ParameterizedCommand
            {
                public ParametersInt(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected Task OnExecuteAsync(int param1)
                {
                    Console.WriteLine(param1);

                    return Task.CompletedTask;
                }
            }

            public class ParametersString : ParameterizedCommand
            {
                public ParametersString(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected Task OnExecuteAsync(string param1)
                {
                    Console.WriteLine(param1);

                    return Task.CompletedTask;
                }
            }

            public class ParametersBool : ParameterizedCommand
            {
                public ParametersBool(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected Task OnExecuteAsync(bool param1)
                {
                    Console.WriteLine(param1);

                    return Task.CompletedTask;
                }
            }

            public class ParametersIntOptional : ParameterizedCommand
            {
                public ParametersIntOptional(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected Task OnExecuteAsync(int param1 = 4)
                {
                    Console.WriteLine(param1);

                    return Task.CompletedTask;
                }
            }

            public class ParametersBoolInt : ParameterizedCommand
            {
                public ParametersBoolInt(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected Task OnExecuteAsync(bool param1, int param2)
                {
                    Console.WriteLine(param1);
                    Console.WriteLine(param2);

                    return Task.CompletedTask;
                }
            }

            public class ParametersBoolIntOptional : ParameterizedCommand
            {
                public ParametersBoolIntOptional(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected Task OnExecuteAsync(bool param1, int param2 = 6)
                {
                    Console.WriteLine(param1);
                    Console.WriteLine(param2);

                    return Task.CompletedTask;
                }
            }

            public class ParametersFiveIntFiveBoolFiveString : ParameterizedCommand
            {
                public ParametersFiveIntFiveBoolFiveString(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                }

                protected Task OnExecuteAsync(int param1, int param2, int param3, int param4, int param5,
                    bool param6, bool param7, bool param8, bool param9, bool param10,
                    string param11, string param12, string param13, string param14, string param15)
                {
                    return Task.CompletedTask;
                }
            }

        }

        private ICommandParameters _commandParameters;
        private ICommandContext _commandContext;
        private IServiceProvider _serviceProvider;

        [TestInitialize]
        public void Initialize()
        {
            var stringLocalizerMock = new Mock<IOpenModStringLocalizer>();

            var commandParameterResolverMock = new Mock<ICommandParameterResolver>();

            var commandRegistration = Mock.Of<ICommandRegistration>();

            var parameters = Mock.Of<ICommandParameters>(MockBehavior.Strict);

            var contextMock = new Mock<ICommandContext>();

            var contextAccessorMock = new Mock<ICurrentCommandContextAccessor>();

            var permissionCheckerMock = new Mock<IPermissionChecker>();

            var commandPermissionBuilder = new Mock<ICommandPermissionBuilder>();

            stringLocalizerMock.Setup(x => x[It.IsAny<string>()])
                .Returns(new LocalizedString("default_path", "Default localization value"));

            stringLocalizerMock.Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns(new LocalizedString("default_path", "Default localization value"));

            commandParameterResolverMock.Setup(x => x.ResolveAsync(typeof(int), It.IsAny<string>()))
                .Returns<Type, string>((_, input) => int.TryParse(input, out var result)
                    ? Task.FromResult<object>(result)
                    : Task.FromResult<object>(null));

            commandParameterResolverMock.Setup(x => x.ResolveAsync(typeof(bool), It.IsAny<string>()))
                .Returns<Type, string>((_, input) => bool.TryParse(input, out var result)
                    ? Task.FromResult<object>(result)
                    : Task.FromResult<object>(null));

            commandParameterResolverMock.Setup(x => x.ResolveAsync(typeof(string), It.IsAny<string>()))
                .Returns<Type, string>((_, input) => Task.FromResult<object>(input));

            contextMock.SetupGet(x => x.CommandRegistration)
                .Returns(commandRegistration);

            contextMock.SetupGet(x => x.Parameters)
                .Returns(() => _commandParameters);

            contextMock.SetupGet(x => x.ServiceProvider)
                .Returns(() => _serviceProvider);

            var serviceProviderMock = new Mock<IServiceProvider>();

            contextAccessorMock.Setup(x => x.Context)
                .Returns(() => contextMock.Object);

            permissionCheckerMock.Setup(x => x.CheckPermissionAsync(It.IsAny<IPermissionActor>(), It.IsAny<string>()))
                .Returns(Task.FromResult(PermissionGrantResult.Grant));

            serviceProviderMock.Setup(x => x.GetService(typeof(IOpenModStringLocalizer)))
                .Returns(stringLocalizerMock.Object);

            serviceProviderMock.Setup(x => x.GetService(typeof(ICommandParameterResolver)))
                .Returns(commandParameterResolverMock.Object);

            serviceProviderMock.Setup(x => x.GetService(typeof(ICurrentCommandContextAccessor)))
                .Returns(contextAccessorMock.Object);

            serviceProviderMock.Setup(x => x.GetService(typeof(IPermissionChecker)))
                .Returns(permissionCheckerMock.Object);

            serviceProviderMock.Setup(x => x.GetService(typeof(ICommandPermissionBuilder)))
                .Returns(commandPermissionBuilder.Object);

            _commandParameters = parameters;
            _commandContext = contextMock.Object;
            _serviceProvider = serviceProviderMock.Object;
        }

        [DataTestMethod]
        [DataRow(typeof(Commands.Empty))]
        [DataRow(typeof(Commands.NoMatch))]
        [DataRow(typeof(Commands.PublicMethod))]
        [DataRow(typeof(Commands.CaseSensitiveMethod))]
        [DataRow(typeof(Commands.ReturnVoid))]
        [DataRow(typeof(Commands.ReturnValue))]
        public async Task ExecuteAsync_TestNoMatchingMethod(Type commandType)
        {
            var command = (ParameterizedCommand)Activator.CreateInstance(commandType, _serviceProvider);

            Assert.IsNotNull(command);

            await Assert.ThrowsExceptionAsync<Exception>(command.ExecuteAsync);
        }

        [DataTestMethod]
        [DataRow(typeof(Commands.TwoMatch))]
        [DataRow(typeof(Commands.ThreeMatch))]
        public async Task ExecuteAsync_AmbiguousMatch(Type commandType)
        {
            var command = (ParameterizedCommand)Activator.CreateInstance(commandType, _serviceProvider);

            Assert.IsNotNull(command);

            await Assert.ThrowsExceptionAsync<AmbiguousMatchException>(command.ExecuteAsync);
        }

        [DataTestMethod]
        [DataRow(typeof(Commands.ParametersNone))]
        [DataRow(typeof(Commands.ParametersInt), "4")]
        [DataRow(typeof(Commands.ParametersString), "")]
        [DataRow(typeof(Commands.ParametersString), "a")]
        [DataRow(typeof(Commands.ParametersBool), "true")]
        [DataRow(typeof(Commands.ParametersBool), "false")]
        [DataRow(typeof(Commands.ParametersIntOptional))]
        [DataRow(typeof(Commands.ParametersIntOptional), "123")]
        [DataRow(typeof(Commands.ParametersBoolInt), "true", "1234")]
        [DataRow(typeof(Commands.ParametersBoolIntOptional), "true")]
        [DataRow(typeof(Commands.ParametersBoolIntOptional), "true", "1234")]
        [DataRow(typeof(Commands.ParametersFiveIntFiveBoolFiveString),
            "1", "2", "3", "4", "5",
            "true", "false", "true", "false", "true",
            "a", "b", "c", "d", "e")]
        public async Task ExecuteAsync_Success(Type commandType, params string[] parameters)
        {
            _commandParameters = new CommandParameters(_commandContext, parameters);

            var command = (ParameterizedCommand)Activator.CreateInstance(commandType, _serviceProvider);

            Assert.IsNotNull(command);

            await command.ExecuteAsync();
        }

        [DataTestMethod]
        [DataRow(typeof(Commands.ParametersInt), "")]
        [DataRow(typeof(Commands.ParametersInt), "a")]
        [DataRow(typeof(Commands.ParametersBool), "")]
        [DataRow(typeof(Commands.ParametersBool), "b")]
        [DataRow(typeof(Commands.ParametersIntOptional), "")]
        [DataRow(typeof(Commands.ParametersIntOptional), "c")]
        [DataRow(typeof(Commands.ParametersBoolInt), "a", "1234")]
        [DataRow(typeof(Commands.ParametersBoolInt), "true", "a")]
        [DataRow(typeof(Commands.ParametersBoolIntOptional), "a", "1234")]
        [DataRow(typeof(Commands.ParametersBoolIntOptional), "true", "b")]
        public async Task ExecuteAsync_ParameterParseException(Type commandType, params string[] parameters)
        {
            _commandParameters = new CommandParameters(_commandContext, parameters);

            var command = (ParameterizedCommand)Activator.CreateInstance(commandType, _serviceProvider);

            Assert.IsNotNull(command);

            await Assert.ThrowsExceptionAsync<CommandParameterParseException>(command.ExecuteAsync);
        }

        [DataTestMethod]
        [DataRow(typeof(Commands.ParametersInt))]
        [DataRow(typeof(Commands.ParametersString))]
        [DataRow(typeof(Commands.ParametersBool))]
        [DataRow(typeof(Commands.ParametersBoolInt), "true")]
        public async Task ExecuteAsync_IndexOutOfRangeException(Type commandType, params string[] parameters)
        {
            _commandParameters = new CommandParameters(_commandContext, parameters);

            var command = (ParameterizedCommand)Activator.CreateInstance(commandType, _serviceProvider);

            Assert.IsNotNull(command);

            await Assert.ThrowsExceptionAsync<CommandIndexOutOfRangeException>(command.ExecuteAsync);
        }
    }
}
