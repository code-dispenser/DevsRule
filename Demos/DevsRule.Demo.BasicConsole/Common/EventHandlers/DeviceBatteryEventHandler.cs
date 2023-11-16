using DevsRule.Core.Areas.Events;
using DevsRule.Demo.BasicConsole.Common.Events;
using DevsRule.Demo.BasicConsole.Common.Models;

namespace DevsRule.Demo.BasicConsole.Common.EventHandlers
{
    public class DeviceBatteryEventHandler : IEventHandler<DeviceConditionEvent>
    {
        private readonly AppSettings _appSettings;
        private readonly HttpClient  _httpClient;
        public DeviceBatteryEventHandler(IHttpClientFactory clientFactory, AppSettings appSettings)

            => (_httpClient, _appSettings) = (clientFactory.CreateClient(), appSettings);

        public async Task Handle(DeviceConditionEvent theEvent, CancellationToken cancellationToken)
        {
            _ = theEvent.TryGetData(out var data);

            if (data is Probe probeData) await RequestDeviceMaintenance(_httpClient, _appSettings.APIUrl, probeData.ProbeID, "low battery reading",theEvent.SenderName);
        }

        private async Task RequestDeviceMaintenance(HttpClient client, string url, Guid probeID, string reason, string sender)
        {
            // use _httpClient to send maintenance request to the deptments API
            await Console.Out.WriteLineAsync($"The dynamic condition event was handled for {sender}: Maintenance request for probe id: {probeID } sent to: {url} due to: {reason}");
        }
    }
}
