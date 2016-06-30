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
    class ThreadBoundEventHandler<T>
    {
        public TaskFactory Context;
        public EventHandler<T> Handler;

        public async Task SignalAsync(T obj)
        {
            await Context.StartNew(() => Handler.Invoke(null, obj));
        }
    }

    public static class ThreadBroker
    {
        public static IAsyncAction PostConnectionArrived(AppServiceConnection connection)
        {
            return PostToEventSource(connection, _connectionArrivedEventSource);
        }

        public static IAsyncAction PostConnectionDone(AppServiceConnection connection)
        {
            return PostToEventSource(connection, _connectionDoneEventSource);
        }
        
        public static event EventHandler<AppServiceConnection> ConnectionArrived
        {
            add
            {
                return BindHandlerToEventSource(value, _connectionArrivedEventSource);
            }

            remove
            {
                lock(_connectionArrivedEventSource)
                {
                    _connectionArrivedEventSource.RemoveEventHandler(value);
                }
            }
        }

        public static event EventHandler<AppServiceConnection> ConnectionDone
        {
            add
            {
                return BindHandlerToEventSource(value, _connectionDoneEventSource);
            }

            remove
            {
                lock (_connectionArrivedEventSource)
                {
                    _connectionDoneEventSource.RemoveEventHandler(value);
                }
            }
        }

        private static EventRegistrationToken BindHandlerToEventSource<T>(EventHandler<T> handler,
            EventRegistrationTokenTable<EventHandler<T>> eventSource)
        {
            // Wrap the incoming EventHandler such that we will call it
            // back on the same synchronization context in which the event was added
            var boundEventHandler = new ThreadBoundEventHandler<T>
            {
                Handler = handler,
                Context = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext())
            };
            var eventHandlerThunk = new EventHandler<T>(async (_, obj) =>
            {
                await boundEventHandler.SignalAsync(obj);
            });

            lock (eventSource)
            {
                var token = eventSource.AddEventHandler(eventHandlerThunk);
                return token;
            }
        }

        private static IAsyncAction PostToEventSource<T>(T obj, EventRegistrationTokenTable<EventHandler<T>> eventSource)
        {
            var task = Task.Run(() =>
            {
                eventSource.InvocationList?.Invoke(null, obj);
            });
            return task.AsAsyncAction();
        }

        static EventRegistrationTokenTable<EventHandler<AppServiceConnection>> _connectionArrivedEventSource
            = new EventRegistrationTokenTable<EventHandler<AppServiceConnection>>();

        static EventRegistrationTokenTable<EventHandler<AppServiceConnection>> _connectionDoneEventSource
            = new EventRegistrationTokenTable<EventHandler<AppServiceConnection>>();
    }
}
