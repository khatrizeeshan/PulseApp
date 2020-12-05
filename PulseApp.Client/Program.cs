using Grpc.Net.Client;
using PulseApp.Console.Protos;
using System.Threading.Tasks;

namespace PulseApp.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new LeaveManager.LeaveManagerClient(channel);
            var response = await client.GetLeaveTypesAsync(new LeaveTypeRequest { });

            foreach(var type in response.LeaveTypes)
            {
                System.Console.WriteLine($"Leave Type - Id: {type.Id}, Name: {type.Name}");
            }

            System.Console.WriteLine("Press any key to exist...");
            System.Console.ReadKey();
        }
    }
}
