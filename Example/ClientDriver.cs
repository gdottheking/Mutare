using Microsoft.EntityFrameworkCore;

namespace Sharara.Services.Kumusha
{
    class ClientDriver
    {
        Generated.Proto.Service.ServiceClient client;

        public ClientDriver() {
            var channel = new Grpc.Core.Channel("localhost:5001", Grpc.Core.ChannelCredentials.SecureSsl);
            this.client = new(channel);
        }

        public long GetAddressCount()
        {
            var response =  this.client.GetTeacherCount(new Generated.Proto.GetTeacherCountRequest());
            return response.Payload;
        }
    }
}