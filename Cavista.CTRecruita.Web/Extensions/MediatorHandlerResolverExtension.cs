using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using System.Reflection;

namespace Cavista.CTRecruita.Web.Extensions
{
    public static class MediatorHandlerResolverExtension
    {
        public static IServiceCollection AddMediatRHandlersFromAssemblies(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

            var handlerInterfaces = new[] { typeof(IRequestHandler<,>) };

            var allTypes = assemblies
                .SelectMany(a => a.DefinedTypes)
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .ToList();

            foreach (var type in allTypes)
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                                handlerInterfaces.Contains(i.GetGenericTypeDefinition()))
                    .ToList();

                foreach (var iface in interfaces)
                {
                    if (!type.IsGenericTypeDefinition)
                    {
                        services.AddScoped(iface, type);
                    }
                }
            }

            return services;
        }

        public static IServiceCollection AddMediatR(this IServiceCollection services)
        {
            //services.AddScoped<IMediator, Sender>();
            //services.AddMediatRHandlersFromAssemblies(typeof(LoginCommand).Assembly);
            //services.AddMediatRHandlersFromAssemblies(typeof(GetActivityLogsQuery).Assembly);

            return services;
        }
    }
}
