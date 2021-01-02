using Klavogonki;
using Klavogonki.Hrustyashki;
using Ninject;
using System;
using System.Threading.Tasks;

namespace Examples
{
    internal class Program
    {
        private static StandardKernel kernel;

        private static void Main(string[] args)
        {
            kernel = new StandardKernel(new NinjectRegistrations());

            Console.WriteLine("Press 1 to update successful players and Hrustyashki top");

            var key = Console.ReadLine();
            if (key == "1")
            {
                _= UpdateHrustyashki();

            }
            Console.ReadLine();
        }

        private static async Task UpdateHrustyashki()
        {
            var successService = kernel.Get<ISuccessService>();

            successService.ProgressChanged += (s, e) => Console.WriteLine($"Обновление списка успешных игроков. {e.Value.Count}/{e.Value.Total} страниц, {e.Value.SecondaryProgress.Count}/{e.Value.SecondaryProgress.Total} игроков");


            var hrustUpdater = kernel.Get<IHrustUpdater>();
            hrustUpdater.ProgressChanged += (s, e) => Console.WriteLine($"Обновление игроков хрустяшек. {e.Value.Count}/{e.Value.Total}");

            try
            {
                await hrustUpdater.UpdateHrustyashki();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
