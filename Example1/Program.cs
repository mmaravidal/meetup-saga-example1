using ColoredConsole;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            WhenSuccess();

            WhenFirstFails();

            WhenLastFails();
        }

        static async Task WhenSuccess()
        {
            ColorConsole.WriteLine();
            ColorConsole.WriteLine("Simulating a successful transaction".DarkGreen());

            using (var transaction = new TransactionScope())
            {
                var repository = new TransactionalService("Repository", false);
                var publisher = new TransactionalService("Publisher", false);

                var item = Guid.NewGuid().ToString();
                repository.Add(item);
                publisher.Add(item);

                transaction.Complete();
            }
        }

        static async Task WhenFirstFails()
        {
            ColorConsole.WriteLine();
            ColorConsole.WriteLine("Simulating a first agent failing transaction".DarkGreen());

            try
            {
                using (var transaction = new TransactionScope())
                {
                    var repository = new TransactionalService("Repository", true);
                    var publisher = new TransactionalService("Publisher", false);

                    var item = Guid.NewGuid().ToString();
                    repository.Add(item);
                    publisher.Add(item);

                    transaction.Complete();
                }
            }
            catch (Exception exception)
            {
                ColorConsole.WriteLine(exception.Message.DarkRed());
            }
        }

        static async Task WhenLastFails()
        {
            ColorConsole.WriteLine();
            ColorConsole.WriteLine("Simulating a last agent failing transaction".DarkGreen());

            try
            {
                using (var transaction = new TransactionScope())
                {
                    var repository = new TransactionalService("Repository", false);
                    var publisher = new TransactionalService("Publisher", true);

                    var item = Guid.NewGuid().ToString();
                    repository.Add(item);
                    publisher.Add(item);

                    transaction.Complete();
                }
            }
            catch (Exception exception)
            {
                ColorConsole.WriteLine(exception.Message.DarkRed());
            }
        }
    }
}
