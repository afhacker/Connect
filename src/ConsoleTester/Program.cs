﻿using Connect.Common.Enums;
using Connect.Oauth.Factories;
using Connect.Oauth.Models;
using Connect.Protobuf;
using Connect.Protobuf.Helpers;
using Connect.Protobuf.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleTester
{
    internal class Program
    {
        private static App _app;

        private static Token _token;

        private static Client _client;

        private static readonly List<IDisposable> _streamDisposables = new List<IDisposable>();

        private static async Task Main()
        {
            Console.Write("Enter App ID: ");

            var appId = Console.ReadLine();

            Console.Write("Enter App Secret: ");

            var appSecret = Console.ReadLine();

            Console.Write("Enter App Redirect URL: ");

            var redirectUrl = Console.ReadLine();

            _app = new App(appId, appSecret, redirectUrl);

            Console.Write("Enter Connection Mode (Live or Demo): ");

            var connectionMode = Console.ReadLine();

            var mode = (Mode)Enum.Parse(typeof(Mode), connectionMode, true);

            var auth = new Auth(_app, mode: mode);

            System.Diagnostics.Process.Start("explorer.exe", $"\"{auth.AuthUri}\"");

            ShowDashLine();

            Console.WriteLine("Follow the authentication steps on your browser, then copy the authentication code from redirect" +
                " URL and paste it here.");

            Console.WriteLine("The authentication code is at the end of redirect URL and it starts after '?code=' parameter.");

            ShowDashLine();

            Console.Write("Enter Authentication Code: ");

            var code = Console.ReadLine();

            var authCode = new AuthCode(code, _app);

            _token = TokenFactory.GetToken(authCode);

            Console.WriteLine("Access token generated");

            ShowDashLine();

            _client = new Client();

            _streamDisposables.Add(_client.Streams.MessageStream.Subscribe(OnMessageReceived));
            _streamDisposables.Add(_client.Streams.ErrorStream.Subscribe(OnError));

            _streamDisposables.Add(_client.Streams.ListenerExceptionStream.Subscribe(OnListenerException));

            _streamDisposables.Add(_client.Streams.SenderExceptionStream.Subscribe(OnSenderException));

            _streamDisposables.Add(_client.Streams.RefreshTokenResponseStream.Subscribe(OnRefreshTokenResponse));

            Console.WriteLine("Connecting Client...");

            await _client.Connect(mode);

            ShowDashLine();

            Console.WriteLine("Client successfully connected");

            ShowDashLine();

            Console.WriteLine("Sending App Auth Req...");

            Console.WriteLine("Please wait...");

            ShowDashLine();

            var applicationAuthReq = new ProtoOAApplicationAuthReq
            {
                ClientId = _app.ClientId,
                ClientSecret = _app.Secret,
            };

            await _client.SendMessage(applicationAuthReq, ProtoOAPayloadType.ProtoOaApplicationAuthReq);

            await Task.Delay(5000);

            Console.WriteLine("You should see the application auth response message before entering any command");

            Console.WriteLine("For commands list and description use 'help' command");

            ShowDashLine();

            GetCommand();
        }

        private static void OnMessageReceived(ProtoMessage e)
        {
            if (e.PayloadType == (int)ProtoPayloadType.HeartbeatEvent)
            {
                return;
            }

            Console.WriteLine($"MessageReceived:\n{e.GetTextPresentation()}");

            ShowDashLine();
        }

        private static void OnListenerException(Exception ex)
        {
            Console.WriteLine($"ListenerExceptionEvent");
            Console.WriteLine($"Exception\n: {ex}");

            ShowDashLine();
        }

        private static void OnSenderException(Exception ex)
        {
            Console.WriteLine($"SenderExceptionEvent");
            Console.WriteLine($"Exception\n: {ex}");

            ShowDashLine();
        }

        private static void OnError(ProtoOAErrorRes e)
        {
            Console.WriteLine($"Error:\n{e}");

            ShowDashLine();
        }

        private static void OnRefreshTokenResponse(StreamMessage<ProtoOARefreshTokenRes> response)
        {
            _token = new Token
            {
                AccessToken = response.Message.AccessToken,
                RefreshToken = response.Message.RefreshToken,
                ExpiresIn = DateTimeOffset.FromUnixTimeMilliseconds(response.Message.ExpiresIn),
                TokenType = response.Message.TokenType,
                Mode = _token.Mode
            };

            Console.WriteLine($"New token received: {_token.AccessToken}");
            Console.WriteLine($"As you refreshed your access token, now you have to re-authorize all previously authorized" +
                $" trading accounts");
        }

        private static void ProcessCommand(string command)
        {
            Console.WriteLine();

            var commandSplit = command.Split(' ');
            try
            {
                switch (commandSplit[0].ToLowerInvariant())
                {
                    case "help":
                        Console.WriteLine("For getting accounts list type: accountlist\n");
                        Console.WriteLine("For authorizing an account type: accountauth {Account ID}\n");
                        Console.WriteLine("For getting an account symbols list type (Requires account authorization): symbolslist {Account ID}\n");
                        Console.WriteLine("For subscribing to symbol(s) spot quotes type (Requires account authorization): subscribe spot {Account ID} {Symbol ID,}\n");
                        Console.WriteLine("For subscribing to symbol(s) trend bar type (Requires account authorization and spot subscription): subscribe trendbar {Period} {Account ID} {Symbol ID}\n");
                        Console.WriteLine("For trend bar period parameter, you can use these values:\n");

                        var trendbars = Enum.GetValues(typeof(ProtoOATrendbarPeriod)).Cast<ProtoOATrendbarPeriod>();

                        var isFirst = true;

                        foreach (var trendBar in trendbars)
                        {
                            Console.Write(isFirst ? $"{trendBar}" : $", {trendBar}");

                            if (isFirst) { isFirst = false; }
                        }

                        Console.WriteLine();

                        Console.WriteLine("\nTo refresh access token, type: refreshtoken\n");

                        Console.WriteLine("\nTo exit the app and disconnect the client type: disconnect\n");

                        Console.WriteLine("Commands aren't case sensitive\n");

                        break;

                    case "accountlist":
                        AccountListRequest();
                        break;

                    case "reconcile":
                        ReconcileRequest(commandSplit);
                        break;

                    case "accountauth":
                        AccountAuthRequest(commandSplit);
                        break;

                    case "symbolslist":
                        SymbolListRequest(commandSplit);
                        break;

                    case "subscribe":
                        ProcessSubscriptionCommand(commandSplit);
                        break;

                    case "refreshtoken":
                        RefreshToken();
                        break;

                    case "disconnect":
                        Disconnect();

                        break;

                    default:
                        Console.WriteLine($"'{command}' is not recognized as a command, please use help command to get all available commands list");
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is IndexOutOfRangeException)
                {
                    Console.WriteLine(ex);
                }
                else
                {
                    throw;
                }
            }

            Task.Delay(3000).Wait();

            GetCommand();
        }

        private static void ProcessSubscriptionCommand(string[] commandSplit)
        {
            switch (commandSplit[1].ToLowerInvariant())
            {
                case "spot":
                    SubscribeToSymbolSpot(commandSplit);
                    break;

                case "trendbar":
                    SubscribeToSymbolTrendBar(commandSplit);
                    break;

                default:
                    Console.WriteLine($"'{commandSplit[1]}' is not recognized as a subscription command, please use help command to get all available commands list");
                    break;
            }
        }

        private async static void RefreshToken()
        {
            Console.WriteLine("Refreshing access token...");

            var refreshTokenReq = new ProtoOARefreshTokenReq
            {
                RefreshToken = _token.RefreshToken
            };

            await _client.SendMessage(refreshTokenReq, ProtoOAPayloadType.ProtoOaRefreshTokenReq);
        }

        private async static void SubscribeToSymbolTrendBar(string[] commandSplit)
        {
            Console.WriteLine("Subscribing to symbol trend bar event...");

            var subscribeLiveTrendbarReq = new ProtoOASubscribeLiveTrendbarReq()
            {
                Period = (ProtoOATrendbarPeriod)Enum.Parse(typeof(ProtoOATrendbarPeriod), commandSplit[2], true),
                CtidTraderAccountId = long.Parse(commandSplit[3]),
                SymbolId = long.Parse(commandSplit[4]),
            };

            await _client.SendMessage(subscribeLiveTrendbarReq, ProtoOAPayloadType.ProtoOaSubscribeLiveTrendbarReq);
        }

        private async static void SubscribeToSymbolSpot(string[] commandSplit)
        {
            Console.WriteLine("Subscribing to symbol spot event...");

            var subscribeSpotsReq = new ProtoOASubscribeSpotsReq()
            {
                CtidTraderAccountId = long.Parse(commandSplit[2]),
            };

            subscribeSpotsReq.SymbolId.AddRange(commandSplit.Skip(3).Select(iSymbolId => long.Parse(iSymbolId)));

            await _client.SendMessage(subscribeSpotsReq, ProtoOAPayloadType.ProtoOaSubscribeSpotsReq);
        }

        private async static void SymbolListRequest(string[] commandSplit)
        {
            var accountId = long.Parse(commandSplit[1]);

            Console.WriteLine("Sending symbols list req...");

            var symbolsListReq = new ProtoOASymbolsListReq
            {
                CtidTraderAccountId = accountId,
            };

            await _client.SendMessage(symbolsListReq, ProtoOAPayloadType.ProtoOaSymbolsListReq);
        }

        private async static void ReconcileRequest(string[] commandSplit)
        {
            var accountId = long.Parse(commandSplit[1]);

            Console.WriteLine("Sending reconcile req...");

            var reconcileReq = new ProtoOAReconcileReq
            {
                CtidTraderAccountId = accountId,
            };

            await _client.SendMessage(reconcileReq, ProtoOAPayloadType.ProtoOaReconcileReq);
        }
        private async static void AccountListRequest()
        {
            Console.WriteLine("Sending account list req...");

            var accountListByAccessTokenReq = new ProtoOAGetAccountListByAccessTokenReq
            {
                AccessToken = _token.AccessToken,
            };

            await _client.SendMessage(accountListByAccessTokenReq, ProtoOAPayloadType.ProtoOaGetAccountsByAccessTokenReq);
        }

        private async static void AccountAuthRequest(string[] commandSplit)
        {
            var accountId = long.Parse(commandSplit[1]);

            Console.WriteLine("Sending account auth req...");

            var accountAuthReq = new ProtoOAAccountAuthReq
            {
                CtidTraderAccountId = accountId,
                AccessToken = _token.AccessToken
            };

            await _client.SendMessage(accountAuthReq, ProtoOAPayloadType.ProtoOaAccountAuthReq);
        }

        private static void GetCommand()
        {
            Console.Write("Enter command: ");

            var command = Console.ReadLine();

            ProcessCommand(command);
        }

        private static void ShowDashLine() => Console.WriteLine("--------------------------------------------------");

        private static void Disconnect()
        {
            Console.WriteLine("Disconnecting...");

            _streamDisposables.ForEach(iDisposable => iDisposable.Dispose());

            _client.Dispose();

            Console.WriteLine("Disconnected, exiting...");

            Task.Delay(3000).Wait();

            Environment.Exit(0);
        }
    }
}