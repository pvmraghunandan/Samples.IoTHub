// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SendTelemetryOperation.cs" company="Microsoft"> 
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//   THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR 
//   OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//   ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
//   OTHER DEALINGS IN THE SOFTWARE. 
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Samples.IoTHub.Core.Interfaces;

namespace Samples.IoTHub.Core.Operations
{
    public class SendTelemetryOperation : IOperation
    {
        public SendTelemetryOperation(IoTHubContext context)
        {
            this.IoTHubContext = context;
        }

        public IoTHubContext IoTHubContext { get; set; }

        public async Task ExecuteAsync(OperationParameters parameters)
        {
            try
            {
                var deviceId = parameters.Arguments["deviceid"].ToString();
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    throw new ArgumentNullException("deviceid");
                }

                var deviceKey = parameters.Arguments["deviceKey"].ToString();

                if (string.IsNullOrWhiteSpace(deviceKey))
                {
                    throw new ArgumentNullException("devicekey");
                }

                object messageObject = parameters.Arguments["message"];

                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageObject)));

                var deviceClient = this.IoTHubContext.CreateDeviceClient(deviceId, deviceKey);

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("sent");
            }
            catch (Exception exception)
            {
                Console.WriteLine("sample");
                // ToDO: Log exception
                // return Task.FromResult(true);
            }
        }
    }
}