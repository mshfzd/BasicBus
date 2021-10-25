using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LiteBus.Messaging.Abstractions.Extensions
{
    public static class MessageRegistryExtensions
    {
        private static readonly HashSet<Assembly> ScannedAssemblies = new();

        /// <summary>
        ///     Registers messages and hooks from the specified assemblies
        /// </summary>
        /// <param name="registry">The message registry</param>
        /// <param name="assembly">the specified assembly</param>
        public static void Register(this IMessageRegistry registry, Assembly assembly)
        {
            if (ScannedAssemblies.Contains(assembly))
            {
                return;
            }

            foreach (var typeInfo in assembly.DefinedTypes)
            {
                Register(registry, typeInfo);
            }

            ScannedAssemblies.Add(assembly);
        }

        /// <summary>
        ///     Register the specified type if it's a message handler or a hook
        /// </summary>
        /// <param name="type">the type to register</param>
        /// <param name="registry">The message registry</param>
        public static void Register(this IMessageRegistry registry, TypeInfo type)
        {
            var interfaces = type.ImplementedInterfaces
                                 .Where(i => i.IsGenericType)
                                 .Select(i => i.GetGenericTypeDefinition());

            foreach (var @interface in interfaces)
            {
                if (@interface.IsAssignableTo(typeof(IMessageHandler<,>)))
                {
                    registry.RegisterHandler(type);
                }
                else if (@interface.IsAssignableTo(typeof(IMessagePreHandler)))
                {
                    registry.RegisterPreHandler(type);
                }
                else if (@interface.IsAssignableTo(typeof(IMessagePostHandler)))
                {
                    registry.RegisterPostHandler(type);
                }
                else if (@interface.IsAssignableTo(typeof(IMessageErrorHandler)))
                {
                    registry.RegisterErrorHandler(type);
                }
            }
        }

        public static void RegisterHandler<THandler, TMessage, TMessageResult>(this IMessageRegistry registry)
            where THandler : IMessageHandler<TMessage, TMessageResult>
        {
            registry.RegisterHandler(typeof(THandler));
        }
    }
}