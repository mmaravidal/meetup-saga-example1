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
        private readonly bool _throws;

        public TransactionalService(string name, bool throws)
        {
            _name = name;
            _throws = throws;

            // Enlist the service within the transaction
            Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
        }

        public void Commit(Enlistment enlistment)
        {
            ColorConsole.WriteLine(_name, ": ", "Executing Commit()".Black().OnDarkGray());

            if (_throws) throw new InvalidOperationException(_name + " throws");

            foreach (var item in _items)
                ColorConsole.WriteLine(_name, ": ", item.DarkGray());

            enlistment.Done();
        }

        public void Add(string item)
        {
            _items.Add(item);
        }

        public void InDoubt(Enlistment enlistment)
        {
            ColorConsole.WriteLine(_name, ": ", "Executing InDount()".Black().OnDarkGray());

            // Do nothing. This method is used when the transaction is in doubt, i.e: a failure occurred in any agent while commiting
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            ColorConsole.WriteLine(_name, ": ", "Executing Prepare()".Black().OnDarkGray());

            // Do nothing. This method is used to prepare the data for the commit phase.
            preparingEnlistment.Prepared();
        }

        public void Rollback(Enlistment enlistment)
        {
            ColorConsole.WriteLine(_name, ": ", "Executing Rollback()".Black().OnDarkGray());

            // Rollback the transaction
            _items.Clear();

            enlistment.Done();
        }
    }
}
