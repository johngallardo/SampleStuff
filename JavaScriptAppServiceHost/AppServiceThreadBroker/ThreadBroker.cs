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
    class PostedMessage<T>
    {
        public PostedMessage()
        {
            _fanoutTasks.Add(this._completion.Task);
        }

        public T Argument;

        public PostedMessage<T> Fanout()
        {
            var fanout = new PostedMessage<T> { Argument = this.Argument };
            _fanoutTasks.Add(fanout._completion.Task);
            return fanout;
        }

        public void Complete()
        {
            _completion.SetResult(true);
        }

        public void MarkError(Exception ex)
        {
            _completion.SetException(ex);
        }

        public Task GetAggregatedTask()
        {
            return Task.WhenAll(_fanoutTasks.ToArray());
        }

        private List<Task> _fanoutTasks = new List<Task>();
        private TaskCompletionSource<bool> _completion = new TaskCompletionSource<bool>();
    }

    class ThreadBoundEventHandler<T>
    {
        public TaskFactory Context;
        public EventHandler<T> Handler;

        public void Post(PostedMessage<T> message)
        {
            var instance = message.Fanout();
            Context.StartNew(() =>
            {
                try
                {
                    Handler.Invoke(null, instance.Argument);
                    instance.Complete();
                }
                catch(Exception ex)
                {
                    instance.MarkError(ex);
                }
            });
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
            EventRegistrationTokenTable<EventHandler<PostedMessage<T>>> eventSource)
        {
            // Wrap the incoming EventHandler such that we will call it
            // back on the same synchronization context in which the event was added
            var boundEventHandler = new ThreadBoundEventHandler<T>
            {
                Handler = handler,
                Context = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext())
            };
            var eventHandlerThunk = new EventHandler<PostedMessage<T>>((_, message) =>
            {
                boundEventHandler.Post(message);
            });

            lock (eventSource)
            {
                var token = eventSource.AddEventHandler(eventHandlerThunk);
                return token;
            }
        }

        private static IAsyncAction PostToEventSource<T>(T obj, EventRegistrationTokenTable<EventHandler<PostedMessage<T>>> eventSource)
        {
            PostedMessage<T> message = new PostedMessage<T> { Argument = obj };
            var task = Task.Run(() =>
            {
                // If we have delegates bound to the event source, each one
                // will get a fanned out PostedMessage. They will all get aggregated
                // and returned via GetAggregatedTask() below.
                eventSource.InvocationList?.Invoke(null, message);

                // And mark the "parent" PostedMessage as being completed. This
                // also handles the case of not having any delegates bound to the
                // invocation list.
                message.Complete();
            });
            return message.GetAggregatedTask().AsAsyncAction();
        }

        static EventRegistrationTokenTable<EventHandler<PostedMessage<AppServiceConnection>>> _connectionArrivedEventSource
            = new EventRegistrationTokenTable<EventHandler<PostedMessage<AppServiceConnection>>>();

        static EventRegistrationTokenTable<EventHandler<PostedMessage<AppServiceConnection>>> _connectionDoneEventSource
            = new EventRegistrationTokenTable<EventHandler<PostedMessage<AppServiceConnection>>>();
    }
}
