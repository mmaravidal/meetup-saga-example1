using ColoredConsole;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Transactions;

namespace Cli
{
    public class TransactionalService : IEnlistmentNotification
    {
        private readonly List<string> _items = new List<string>();
        private readonly string _name;
        private readonly bool _rollsback;
        private readonly bool _commits;

        public TransactionalService(string name, bool rollsback, bool commits)
        {
            _name = name;
            _rollsback = rollsback;
            _commits = commits;

            // Enlist the service within the transaction
            Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
        }

        public void Commit(Enlistment enlistment)
        {
            // This method is used to commit the data. Should never throw.

            ColorConsole.WriteLine($"{_name}: Executing Commit()".White());

            if (!_commits) throw new InvalidOperationException(_name + " throws");

            foreach (var item in _items)
                ColorConsole.WriteLine($"  {_name}: Commited {item}".DarkGray());
            
            ColorConsole.WriteLine($"  {_name}: Calls Done()".DarkGray());
            enlistment.Done();
        }

        public void Add(string item)
        {
            _items.Add(item);
        }

        public void InDoubt(Enlistment enlistment)
        {
            // This method is used when the transaction is in doubt, i.e: a failure occurred in any agent while commiting

            ColorConsole.WriteLine($"{_name}: Executing InDount()".White());
            
            ColorConsole.WriteLine($"  {_name} Calls Done()".DarkGray());
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            // This method is used to prepare the data for the commit phase.

            ColorConsole.WriteLine($"{_name}: Executing Prepare()".White());

            if (_rollsback)
            {
                ColorConsole.WriteLine($"  {_name}: Calls ForceRollback()".DarkGray());
                preparingEnlistment.ForceRollback();
            }
            else
            {
                ColorConsole.WriteLine($"  {_name}: Calls Prepared()".DarkGray());
                preparingEnlistment.Prepared();
            }
        }

        public void Rollback(Enlistment enlistment)
        {
            // Rollback the transaction

            ColorConsole.WriteLine($"{_name}: Executing Rollback()".White());

            _items.Clear();

            ColorConsole.WriteLine($"  {_name}: Calls Done()".DarkGray());
            enlistment.Done();
        }
    }
}
