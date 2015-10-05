// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SendCommandOperation.cs" company="Microsoft"> 
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
using Microsoft.Azure.Devices;
using Samples.IoTHub.Core.Interfaces;

namespace Samples.IoTHub.Core.Operations
{
    public class SendCommandOperation : IOperation
    {
        public SendCommandOperation(IoTHubContext context)
        {
            this.IoTHubContext = context;
        }

        public IoTHubContext IoTHubContext { get; set; }

        public Task ExecuteAsync(OperationParameters parameters)
        {
            var deviceId = parameters.Arguments["deviceid"].ToString();
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentNullException("deviceid");
            }

            object commandToExecuteObject = parameters.Arguments["commandtoexecute"];

            if (commandToExecuteObject == null)
            {
                throw new ArgumentNullException("Command To Execute");
            }
            var commandToExecute = commandToExecuteObject.ToString();

            // get serialized stream
            var bytes = Encoding.Default.GetBytes(commandToExecute);
            var messageId = Guid.NewGuid().ToString();
            var message = new Message(bytes)
            {
                MessageId = messageId // set the correlation id to get command response
            };

            return this.IoTHubContext.ServiceClient.SendAsync(deviceId, message);
        }
    }
}