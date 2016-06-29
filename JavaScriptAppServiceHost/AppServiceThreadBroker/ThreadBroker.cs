using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.UI.Core;

namespace AppServiceThreadBroker
{
    class ThreadBoundEventHandler
    {
        public TaskFactory Context;
        public EventHandler<AppServiceConnection> Handler;
        public async Task SignalAsync(AppServiceConnection connection)
        {
            await Context.StartNew(() => Handler.Invoke(null, connection));
        }
    }

    public static class ThreadBroker
    {
        public static void SignalNewConnectionArrived(AppServiceConnection connection)
        {
            _handlerTable.InvocationList?.Invoke(null, connection);
        }

        public static event EventHandler<AppServiceConnection> ConnectionArrived
        {
            add
            {
                // Wrap the incoming EventHandler such that we will call it
                // back on the same synchronization context in which the event was added
                var handler = new ThreadBoundEventHandler
                {
                    Handler = value,
                    Context = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext())
                };
                var thunkedHandler = new EventHandler<AppServiceConnection>(async (_, connection) =>
                {
                    await handler.SignalAsync(connection);
                });

                lock (_handlerTable)
                {
                    var token = _handlerTable.AddEventHandler(thunkedHandler);
                    return token;
                }
            }

            remove
            {
                lock(_handlerTable)
                {
                    _handlerTable.RemoveEventHandler(value);
                }
            }
        }

        static EventRegistrationTokenTable<EventHandler<AppServiceConnection>> _handlerTable
            = new EventRegistrationTokenTable<EventHandler<AppServiceConnection>>();
    }
}
