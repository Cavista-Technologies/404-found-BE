using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Utilities.Mediator.Implementation
{
    public class Sender(IServiceProvider provider) : IMediator
    {
        private readonly IServiceProvider _serviceProvider = provider;
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var requestType = request.GetType();
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType);

            if (handler == null)
                throw new InvalidOperationException($"Handler not found for {requestType.Name}");

            return await ((dynamic)handler).Handle((dynamic)request, cancellationToken);
        }
    }
}
