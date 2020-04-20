using ColoredConsole;
using System;
using System.Collections.Generic;
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
            if (_throws) throw new InvalidOperationException("Repository throws");

            foreach (var item in _items)
                ColorConsole.WriteLine(_name, ": ", item.DarkGray());
        }

        public void Add(string item)
        {
            _items.Add(item);
        }

        public void InDoubt(Enlistment enlistment)
        {
            // Do nothing. This method is used when the transaction is in dount, i.e: a failure occurred in any agent while commiting
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            // Do nothing. This method is used to prepare the data for the commit phase.
            preparingEnlistment.Prepared();
        }

        public void Rollback(Enlistment enlistment)
        {
            // Rollback the transaction
            _items.Clear();
        }
    }
}
