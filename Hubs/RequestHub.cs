﻿using JustAddOne.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace JustAddOne.Hubs
{
    public class RequestHub : Hub
    {
        private static int _connectionsCount = 0;

        public void AddValue(string number)
        {
            ServerModel.Instance.AddValue(number);
        }

        public async Task Send()
        {
            await Clients.All.SendAsync("Send", ServerModel.Instance.Number);
        }

        public override Task OnConnectedAsync()
        {
            _connectionsCount++;

            //if (_connectionsCount > 0)
            //    ThreadPool.QueueUserWorkItem(RefreshNumber);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _connectionsCount--;
            return base.OnDisconnectedAsync(exception);
        }

        private async void RefreshNumber(object obj)
        {
            while (_connectionsCount > 0)
            {
                try
                {
                    await Send();
                }
                finally
                {
                    await Task.Delay(2000);
                }
            }
        }
    }
}
