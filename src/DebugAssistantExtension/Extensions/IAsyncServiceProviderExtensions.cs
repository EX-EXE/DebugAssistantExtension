using Microsoft;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.ServiceBroker;

namespace DebugAssistantExtension.Extensions;

public class BrokerProvider<T> : IDisposable
{
    public T Broker { get; private set; }

    public BrokerProvider(T broker)
    {
        this.Broker = broker;
    }

    public void Dispose()
    {
        if (Broker is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

public static class IAsyncServiceProviderExtensions
{
    public static async Task<BrokerProvider<T>> GetProxyServiceAsync<T>(
        this IAsyncServiceProvider asyncServiceProvider,
        ServiceRpcDescriptor serviceRpcDescriptor,
        CancellationToken cancellationToken) where T : class
    {
        var serviceBrokerContainer = await asyncServiceProvider.GetServiceAsync<SVsBrokeredServiceContainer, IBrokeredServiceContainer>();
        var serviceBroker = serviceBrokerContainer.GetFullAccessServiceBroker();
#pragma warning disable ISB001 // Dispose of proxies
        var brokerServiceProxy = await serviceBroker.GetProxyAsync<T>(
            serviceRpcDescriptor,
            cancellationToken: cancellationToken);
#pragma warning restore ISB001 // Dispose of proxies
        Assumes.NotNull(brokerServiceProxy);
        return new BrokerProvider<T>(brokerServiceProxy);
    }
}