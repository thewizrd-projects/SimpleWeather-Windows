﻿#if WINDOWS_UWP
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace SimpleWeather.Utils
{
    public static class DispatcherExtensions
    {
        public static Task RunOnUIThread(this CoreDispatcher Dispatcher, Action action)
        {
            if (Dispatcher.HasThreadAccess)
            {
                action?.Invoke();
                return Task.CompletedTask;
            }
            else
            {
                return Dispatcher.AwaitableRunAsync(action);
            }
        }

        public static Task<T> RunOnUIThread<T>(this CoreDispatcher Dispatcher, Func<T> function)
        {
            if (Dispatcher.HasThreadAccess)
            {
                return Task.FromResult(function.Invoke());
            }
            else
            {
                return Dispatcher.AwaitableRunAsync(function);
            }
        }

        public static void LaunchOnUIThread(this CoreDispatcher Dispatcher, Action action)
        {
            Dispatcher.AwaitableRunAsync(action);
        }

        private static CoreDispatcher CoreDispatcher = null;

        private static CoreDispatcher GetDispatcher()
        {
            CoreDispatcher Dispatcher = null;

            try
            {
                try
                {
                    Dispatcher = CoreApplication.MainView?.Dispatcher;
                }
                catch (Exception) { }

                if (Dispatcher == null)
                {
                    Dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Dispatcher unavailable");
            }

            return Dispatcher;
        }

        public static Task TryRunOnUIThread(Action action)
        {
            if (CoreDispatcher == null)
            {
                CoreDispatcher = GetDispatcher();
            }

            if (CoreDispatcher != null)
            {
                if (CoreDispatcher.HasThreadAccess)
                {
                    action?.Invoke();
                    return Task.CompletedTask;
                }
                else
                {
                    return CoreDispatcher.AwaitableRunAsync(action);
                }
            }
            else
            {
                // Dispatcher is not available
                return Task.CompletedTask;
            }
        }
    }
}
#endif