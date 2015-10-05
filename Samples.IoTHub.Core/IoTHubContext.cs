// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IoTHubContext.cs" company="Microsoft"> 
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//   THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR 
//   OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//   ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
//   OTHER DEALINGS IN THE SOFTWARE. 
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;

namespace Samples.IoTHub.Core
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Client;

    public class IoTHubContext
    {
        private RegistryManager _registryManager;
        private ServiceClient _serviceClient;
        private const string IhHostNameKey = "HostName";
        public string HostName { get; set; }

        public void Initialize()
        {
            this.IoTHubConnection = ConfigurationManager.AppSettings["IOTHubConnectionString"];
            this.EventHubConnection = ConfigurationManager.AppSettings["EventHubConnectionString"];
            this.EventHubPath = ConfigurationManager.AppSettings["EventHubPath"];
            var iotHubProperties = this.IoTHubConnection.Split(";".ToCharArray());

            // Get the host name
            var hostNameItem = iotHubProperties.SingleOrDefault(item => item.StartsWith(IhHostNameKey));
            if (hostNameItem != null)
            {
                this.HostName = hostNameItem.Split("=".ToCharArray())[1];
            }
        }

        /// <summary>
        /// Gets the device client.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="deviceKey">The device key.</param>
        /// <returns></returns>
        public DeviceClient CreateDeviceClient(string deviceId, string deviceKey)
        {
            try
            {
                var deviceConnection = GenerateDeviceConnectionString(deviceId, deviceKey);
                return DeviceClient.CreateFromConnectionString(deviceConnection);
                //var authenticationMethod = new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey);
                //return DeviceClient.Create(HostName, authenticationMethod);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Generates the connection string.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="devicekey">The devicekey.</param>
        /// <returns></returns>
        protected string GenerateDeviceConnectionString(string deviceId, string devicekey)
        { 
            var authenticationMethod = new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, devicekey);
            var builder = Microsoft.Azure.Devices.Client.IotHubConnectionStringBuilder.Create(HostName, authenticationMethod);

            return builder.ToString();
        }

        public string IoTHubConnection { get; set; }

        public string EventHubConnection { get; set; }

        public string EventHubPath { get; set; }

        public RegistryManager RegistryManager
        {
            get
            {
                return _registryManager ??
                       (_registryManager = RegistryManager.CreateFromConnectionString(IoTHubConnection));
            }
        }

        public ServiceClient ServiceClient
        {
            get
            {
                return _serviceClient ?? (_serviceClient =
                                                  ServiceClient.CreateFromConnectionString(IoTHubConnection));
            }
        }
    }
}