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

            WhenFirstRollsback();
            WhenLastRollsback();

            WhenFirstFails();
            WhenLastFails();
        }

        static async Task WhenSuccess()
        {
            ColorConsole.WriteLine();
            ColorConsole.WriteLine("Simulating a first agent rollsback transaction".DarkGreen());

            try
            {
                using (var transaction = new TransactionScope())
                {
                    var repository = new TransactionalService("Repository", rollsback: false, commits: true);
                    var publisher = new TransactionalService("Publisher", rollsback: false, commits: true);

                    var item = Guid.NewGuid().ToString();
                    repository.Add(item);
                    publisher.Add(item);

                    transaction.Complete();
                }

                ColorConsole.WriteLine("The transaction has completed".DarkGreen());
            }
            catch (Exception exception)
            {
                ColorConsole.WriteLine(exception.Message.DarkRed());
            }
        }

        static async Task WhenFirstRollsback()
        {
            ColorConsole.WriteLine();
            ColorConsole.WriteLine("Simulating a first agent rollsback transaction".DarkGreen());

            try
            {
                using (var transaction = new TransactionScope())
                {
                    var repository = new TransactionalService("Repository", rollsback: true, commits: true);
                    var publisher = new TransactionalService("Publisher", rollsback: false, commits: true);

                    var item = Guid.NewGuid().ToString();
                    repository.Add(item);
                    publisher.Add(item);

                    transaction.Complete();
                }

                ColorConsole.WriteLine("The transaction has completed".DarkGreen());
            }
            catch (Exception exception)
            {
                ColorConsole.WriteLine(exception.Message.DarkRed());
            }
        }

        static async Task WhenLastRollsback()
        {
            ColorConsole.WriteLine();
            ColorConsole.WriteLine("Simulating a first agent rollsback transaction".DarkGreen());

            try
            {
                using (var transaction = new TransactionScope())
                {
                    var repository = new TransactionalService("Repository", rollsback: false, commits: true);
                    var publisher = new TransactionalService("Publisher", rollsback: true, commits: true);

                    var item = Guid.NewGuid().ToString();
                    repository.Add(item);
                    publisher.Add(item);

                    transaction.Complete();
                }

                ColorConsole.WriteLine("The transaction has completed".DarkGreen());
            }
            catch (Exception exception)
            {
                ColorConsole.WriteLine(exception.Message.DarkRed());
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
                    var repository = new TransactionalService("Repository", rollsback: false, commits: false);
                    var publisher = new TransactionalService("Publisher", rollsback: false, commits: true);

                    var item = Guid.NewGuid().ToString();
                    repository.Add(item);
                    publisher.Add(item);

                    transaction.Complete();
                }

                ColorConsole.WriteLine("The transaction has completed".DarkGreen());
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
                    var repository = new TransactionalService("Repository", rollsback: false, commits: true);
                    var publisher = new TransactionalService("Publisher", rollsback: false, commits: false);

                    var item = Guid.NewGuid().ToString();
                    repository.Add(item);
                    publisher.Add(item);

                    transaction.Complete();
                }

                ColorConsole.WriteLine("The transaction has completed".DarkGreen());
            }
            catch (Exception exception)
            {
                ColorConsole.WriteLine(exception.Message.DarkRed());
            }
        }
    }
}
