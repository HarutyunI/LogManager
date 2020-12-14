using System.Threading.Tasks;

namespace LogManager
{
	class Program
	{
		static async Task Main(string[] args)
		{
			await new LogMerger().RunAsync();
		}
	}
}
