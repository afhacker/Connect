﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Connect.Protobuf;
using Connect.Common;

namespace ProtobufConsoleTesterApp
{
    class Program
    {
        private static string _appId = "154_Uww1S9ZakuuAoLpQdbJGP9FQ1hx5Ux8ks7di2p8ca067fLPise";

        private static string _appSecret = "COGrdN08XFeTE21ZXa1KmGYWxixJfEndhhLeTIkZuUgHuCqAti";

        private static string _accessToken = "agel0TOYYxjkEkNqfJ_4Nx_Vmx3a5wipBcZm0gJW0KY";

        private static long _accountId = 7606472;

        private static Client _client;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Connecting client...");

            _client = new Client();

            //_client.Events.MessageReceivedEvent += Events_MessageReceivedEvent;

            _client.Events.HeartbeatEvent += Events_HeartbeatEvent;
            _client.Events.ErrorEvent += Events_ErrorEvent;
            _client.Events.ListenerStoppedEvent += Events_ListenerStoppedEvent;
            _client.Events.ApplicationAuthResponseEvent += Events_ApplicationAuthResponseEvent;
            _client.Events.VersionResponseEvent += Events_VersionResponseEvent;
            _client.Events.AccountListResponseEvent += Events_AccountListResponseEvent;
            _client.Events.AccountAuthorizationResponseEvent += Events_AccountAuthorizationResponseEvent;
            _client.Events.SymbolsListResponseEvent += Events_SymbolsListResponseEvent;

            await _client.Connect(Mode.Live);

            Console.WriteLine("Client successfully connected");

            Console.WriteLine("--------------------------------------");

            Console.WriteLine("Sending App Auth Req...");

            Console.WriteLine("--------------------------------------");

            await _client.SendMessage(_client.MessagesFactory.CreateAppAuthorizationRequest(_appId, _appSecret));

            Console.ReadKey();
        }

        private async static void Events_AccountAuthorizationResponseEvent(object sender, ProtoOAAccountAuthRes e)
        {
            Console.WriteLine($"AccountAuthorizationResponse: {e}");

            Console.WriteLine("--------------------------------------");

            Console.WriteLine("Sending Account Symbols List Req...");

            await _client.SendMessage(_client.MessagesFactory.CreateSymbolsListRequest(_accountId));

            Console.WriteLine("--------------------------------------");
        }

        private static void Events_SymbolsListResponseEvent(object sender, ProtoOASymbolsListRes e)
        {
            Console.WriteLine($"SymbolsListResponse: {e}");

            Console.WriteLine("--------------------------------------");
        }

        private static void Events_AccountListResponseEvent(object sender, ProtoOAGetAccountListByAccessTokenRes e)
        {
            Console.WriteLine($"AccountListResponse: {e}");

            Console.WriteLine("--------------------------------------");
        }

        private static void Events_VersionResponseEvent(object sender, ProtoOAVersionRes e)
        {
            Console.WriteLine($"VersionResponse: {e}");

            Console.WriteLine("--------------------------------------");
        }

        private static void Events_MessageReceivedEvent(object sender, ProtoMessage e)
        {
            Console.WriteLine($"MessageReceived: {e}");

            Console.WriteLine("--------------------------------------");
        }

        private async static void Events_ApplicationAuthResponseEvent(object sender, ProtoOAApplicationAuthRes e)
        {
            Console.WriteLine($"ApplicationAuthResponse: {e}");

            Console.WriteLine("--------------------------------------");

            Console.WriteLine("Sending Version Req...");

            await _client.SendMessage(_client.MessagesFactory.CreateVersionRequest());

            Console.WriteLine("--------------------------------------");

            Console.WriteLine("Sending Account List Req...");

            await _client.SendMessage(_client.MessagesFactory.CreateAccountListRequest(_accessToken));

            Console.WriteLine("--------------------------------------");

            Console.WriteLine("Sending Account Auth Req...");

            await _client.SendMessage(_client.MessagesFactory.CreateAccountAuthorizationRequest(_accessToken, _accountId));

            Console.WriteLine("--------------------------------------");
        }

        private static void Events_ListenerStoppedEvent(object sender, Exception ex)
        {
            Console.WriteLine($"ListenerStopped: {ex}");

            Console.WriteLine("--------------------------------------");
        }

        private static void Events_ErrorEvent(object sender, ProtoOAErrorRes e)
        {
            Console.WriteLine($"Error: {e.Description}");

            Console.WriteLine("--------------------------------------");
        }

        private static void Events_HeartbeatEvent(object sender, ProtoHeartbeatEvent e)
        {
            Console.WriteLine($"Heartbeat response received: {e}");

            Console.WriteLine("--------------------------------------");
        }
    }
}
