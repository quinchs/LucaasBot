using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace LucaasBot
{
    internal class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;
        Timer t = new Timer();
        internal System.Timers.Timer autoSlowmode = new System.Timers.Timer() { Enabled = false, AutoReset = true, Interval = 1000 };
        private Dictionary<ulong, int> sList = new Dictionary<ulong, int>();
        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;

            _client.SetGameAsync(Global.Status, null, ActivityType.Playing);

            _client.SetStatusAsync(UserStatus.DoNotDisturb);

            _service = new CommandService();

            _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            _client.MessageReceived += MessageRecievedHandler;

            _client.ReactionAdded += _client_ReactionAdded;

            autoSlowmode.Elapsed += AutoSlowmode_Elapsed;
            autoSlowmode.Enabled = true;
        }

        private async Task _client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg2.Id == 618852225602551818)
            {
                var user = _client.GetGuild(Global.GuildID).GetUser(arg3.UserId);
                EmbedBuilder eb = new EmbedBuilder()
                {
                    Title = $"***Welcome to Lucaas's Official Discord server!***",
                    Footer = new EmbedFooterBuilder()
                    {
                        IconUrl = user.GetAvatarUrl(),
                        Text = $"{user.Username}#{user.Discriminator}"
                    },
                    Description = Global.WelcomeMessage,
                    ThumbnailUrl = Global.WelcomeMessageURL,
                    Color = Color.Green
                };
                await _client.GetGuild(Global.GuildID).GetTextChannel(Global.WelcomeMessageChanID).SendMessageAsync("", false, eb.Build());
                Global.ConsoleLog($"WelcomeMessage for {user.Username}#{user.Discriminator}", ConsoleColor.Blue);
            }
        }

        private async void AutoSlowmode_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Global.AutoSlowmodeToggle)
            {
                foreach (var item in sList.ToList())
                {
                    if (item.Value >= Global.AutoSlowmodeTrigger)
                    {
                        var chan = _client.GetGuild(Global.GuildID).GetTextChannel(item.Key);
                        var aChan = _client.GetGuild(Global.GuildID).GetTextChannel(664606058592993281);
                        var mLink = await chan.GetMessagesAsync(1).FlattenAsync();
                        EmbedBuilder b = new EmbedBuilder()
                        {
                            Color = Color.Orange,
                            Title = "Auto Alert",
                            Fields = new List<EmbedFieldBuilder>() { { new EmbedFieldBuilder() { Name = "Reason", Value = $"Message limit of {Global.AutoSlowmodeTrigger}/sec reached" } }, { new EmbedFieldBuilder() { Name = "Channel", Value = $"<#{chan.Id}>" } }, { new EmbedFieldBuilder() { Name = "Message Link", Value = mLink.First().GetJumpUrl() } } }
                        };
                        await chan.ModifyAsync(x => x.SlowModeInterval = 5);
                        await aChan.SendMessageAsync("", false, b.Build());
                        System.Timers.Timer lt = new System.Timers.Timer()
                        {
                            Interval = 60000,
                        };
                        sList.Remove(item.Key);
                        lt.Enabled = true;
                        lt.Elapsed += (object s, ElapsedEventArgs arg) =>
                        {
                            chan.ModifyAsync(x => x.SlowModeInterval = 0);
                        };
                    }
                    else
                    {
                        sList[item.Key] = 0;
                        sList.Remove(item.Key);
                    }
                }
            }
        }
        private async Task responce(SocketMessage arg)
        {
            if (arg.Channel.Id == Global.BotAiChanID && !arg.Author.IsBot)
            {
                var d = arg.Channel.EnterTypingState();
                try
                {
                    string msg = await GenerateAIResponse(arg, new Random());
                    if (msg != "")
                    {
                        if (msg != ("*terminate"))
                            await arg.Channel.SendMessageAsync(msg);
                    }
                    d.Dispose();
                }
                catch (Exception ex)
                {
                    Global.SendExeption(ex);
                    d.Dispose();
                }
            }
        }
        internal async Task<string> GenerateAIResponse(SocketMessage arg, Random r)
        {
            bool dbug = false;
            string dbugmsg = "";


            Regex r1 = new Regex("what time is it in .*");
            if (arg == null)
            {
                dbugmsg += "arg was null... \n";
                string[] filecontUn = File.ReadAllLines(Global.aiResponsePath);
                //var list = filecontUn.ToList();
                //var d = list.FirstOrDefault(x => x.ToLower() == arg.Content.ToLower());
                Regex rg2 = new Regex(".*(\\d{18})>.*");
                string msg = filecontUn[r.Next(0, filecontUn.Length)];
                //if (d != "") { msg = d; }
                if (rg2.IsMatch(msg))
                {
                    dbugmsg += "Found a ping in there, sanitizing..\n";
                    var rm = rg2.Match(msg);
                    var user = _client.GetGuild(Global.GuildID).GetUser(Convert.ToUInt64(rm.Groups[1].Value));
                    msg = msg.Replace(rm.Groups[0].Value, $"**(non-ping: {user.Username}#{user.Discriminator})**");
                }
                if (msg == "") { return filecontUn[r.Next(0, filecontUn.Length)]; }
                else { return msg; }
            }
            else
            {
                string oMsg = arg.Content.ToLower();
                if (arg.Content.StartsWith("*debug "))
                {
                    dbug = true;
                    oMsg = oMsg.Replace("*debug ", "");
                }
                dbugmsg += "Arg was not null. starting AI responces..\n";
                try
                {
                    if (r1.IsMatch(oMsg.ToLower()))
                    {
                        dbugmsg += "User looking for the time. starting up Time API..\n";
                        HttpClient c = new HttpClient();
                        string link = $"https://www.google.com/search?q={oMsg.ToLower().Replace(' ', '+')}";
                        var req = await c.GetAsync(link);
                        var resp = await req.Content.ReadAsStringAsync();
                        Regex x = new Regex(@"<div class=""BNeawe iBp4i AP7Wnd""><div><div class=""BNeawe iBp4i AP7Wnd"">(.*?)<\/div><\/div>");
                        if (x.IsMatch(resp))
                        {
                            string time = x.Match(resp).Groups[1].Value;
                            c.Dispose();
                            dbugmsg += "Found the time to be " + time + "\n";
                            return $"The current time in {oMsg.ToLower().Replace("what time is it in ", "")} is {time}";
                        }
                        else { c.Dispose(); return $"Sorry buddy but could not get the time for {arg.Content.ToLower().Replace("what time is it in ", "")}"; }
                    }
                    //if (oMsg.ToLower() == "are you gay") { return "no ur gay lol"; }
                    //if (oMsg.ToLower() == "how is your day going") { return "kinda bad. my creator beats me and hurts me help"; }
                    //if (oMsg.ToLower() == "are you smart") { return "smarter than your mom lol goteme"; }
                    //if (oMsg.ToLower() == "hi") { return "hello mortal"; }
                    string[] filecontUn = File.ReadAllLines(@"C:\Users\plynch\source\repos\SwissBot\SwissBot\bin\Debug\Data\Responses.AI");
                    for (int i = 0; i != filecontUn.Length; i++)
                        filecontUn[i] = filecontUn[i].ToLower();
                    Regex rg2 = new Regex(".*(\\d{18})>.*");
                    string msg = "";
                    var ar = filecontUn.Select((b, i) => b == oMsg ? i : -1).Where(i => i != -1).ToArray();
                    Random ran = new Random();
                    dbugmsg += $"Found {ar.Length} indexed responces for the question\n";
                    if (ar.Length != 0)
                    {
                        var ind = (ar[ran.Next(0, ar.Length)]);
                        if (ind != 0 && (ind + 1) < filecontUn.Length)
                            msg = filecontUn[ind + 1];
                        dbugmsg += $"Picked the best answer: {msg}\n";
                    }
                    else
                    {
                        var words = oMsg.Split(' ');
                        var query = from state in filecontUn.AsParallel()
                                    let StateWords = state.Split(' ')
                                    select (Word: state, Count: words.Intersect(StateWords).Count());

                        var sortedDict = from entry in query orderby entry.Count descending select entry;
                        string rMsg = sortedDict.First().Word;
                        var reslt = filecontUn.Select((b, i) => b == rMsg ? i : -1).Where(i => i != -1).ToArray();
                        if (reslt.Length != 0)
                        {
                            var ind = (reslt[ran.Next(0, reslt.Length)]);
                            if (ind != 0 && (ind + 1) < filecontUn.Length)
                                msg = filecontUn[ind + 1];
                            dbugmsg += $"Picked the best answer: {msg}\n";
                        }
                        if(msg == "that's business")
                        {
                            msg = filecontUn[ran.Next(0, filecontUn.Length)];
                        }
                        else { msg = rMsg; }
                        //string[] words = oMsg.Split(' ');
                        //Dictionary<string, int> final = new Dictionary<string, int>();
                        //foreach (var state in filecontUn)
                        //{
                        //    int count = 0;
                        //    foreach (var word in state.Split(' '))
                        //    {
                        //        if (words.Contains(word))
                        //            count++;
                        //    }
                        //    if (!final.Keys.Contains(state) && count != 0)
                        //        final.Add(state, count);
                        //}
                        //string res = sortedDict.First().Key;

                    }
                    if (msg == "") { msg = filecontUn[r.Next(0, filecontUn.Length)]; }

                    if (rg2.IsMatch(msg))
                    {
                        var rm = rg2.Match(msg);
                        var user = _client.GetGuild(Global.GuildID).GetUser(Convert.ToUInt64(rm.Groups[1].Value));
                        if (user != null)
                        {
                            msg = msg.Replace(rm.Groups[0].Value, $"**(non-ping: {user.Username}#{user.Discriminator})**");
                            dbugmsg += "Sanitized ping.. \n";
                        }
                        else
                        {
                            try
                            {
                                var em = await _client.GetGuild(Global.GuildID).GetEmoteAsync(Convert.ToUInt64(rm.Groups[1].Value));
                                if (em == null)
                                {
                                    dbugmsg += $"Could not find a user for {rm.Value}, assuming emoji or user is not in server..\n";
                                    msg = msg.Replace(rm.Groups[0].Value, $"**(non-ping: {rm.Value})**");
                                }
                            }
                            catch (Exception ex) { dbugmsg += $"{ex.Message}.. \n"; }
                        }
                    }
                    if (msg.Contains("@everyone")) { msg = msg.Replace("@everyone", "***(Non-ping @every0ne)***"); }
                    if (msg.Contains("@here")) { msg = msg.Replace("@here", "***(Non-ping @h3re)***"); }
                    dbugmsg += "Sanitized for @h3re and @every0ne\n";
                    EmbedBuilder eb = new EmbedBuilder()
                    {
                        Color = Color.Orange,
                        Title = "Ai Debug",
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = _client.CurrentUser.ToString(),
                            IconUrl = _client.CurrentUser.GetAvatarUrl()
                        },
                        Description = "```Ai Debug Log```\n`" + dbugmsg + "`",


                    };
                    if (dbug)
                        await arg.Channel.SendMessageAsync("", false, eb.Build());
                    return msg;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return "uh oh ai broke stinkie";
                }
            }
        }

        private async System.Threading.Tasks.Task MessageRecievedHandler(SocketMessage arg)
        {
            try
            {
                await HandleCommandAsync(arg);
                await LogMessage(arg);
                await responce(arg);
            }
            catch(Exception ex)
            {
                Global.SendExeption(ex);
            }
        }
        public async Task HandleCommandAsync(SocketMessage s)
        {
            try
            {
                //if (s.Channel.Id == 592463507124125706)
                //{
                //    t.Stop();
                //    t.AutoReset = true;
                //    t.Enabled = true;
                //    t.Interval = 300000;
                //    t.Start();
                //}

                var msg = s as SocketUserMessage;
                if (msg == null) return;

                var context = new SocketCommandContext(_client, msg);
               // if (Commands.giveawayinProg) { Commands.checkGiveaway(s); }

                int argPos = 0;
               // if (msg.Channel.GetType() == typeof(SocketDMChannel)) { await checkKey(context); }
                if (msg.HasCharPrefix(Global.Preflix, ref argPos))
                {
                   // if (msg.Content.StartsWith($"{Global.Preflix}echo")) { await EchoMessage(context); return; }
                    var result = await _service.ExecuteAsync(context, argPos, null, MultiMatchHandling.Best);

                    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    {
                        EmbedBuilder b = new EmbedBuilder();
                        b.Color = Color.Red;
                        b.Description = $"The following info is the Command error info, `{msg.Author.Username}#{msg.Author.Discriminator}` tried to use the `{msg}` Command in {msg.Channel}: \n \n **COMMAND ERROR**: ```{result.Error.Value}``` \n \n **COMMAND ERROR REASON**: ```{result.ErrorReason}```";
                        b.Author = new EmbedAuthorBuilder();
                        b.Author.Name = msg.Author.Username + "#" + msg.Author.Discriminator;
                        b.Author.IconUrl = msg.Author.GetAvatarUrl();
                        b.Footer = new EmbedFooterBuilder();
                        b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                        b.Title = "Bot Command Error!";
                        await _client.GetGuild(Global.GuildID).GetTextChannel(Global.DebugChanID).SendMessageAsync("", false, b.Build());
                        await _client.GetGuild(Global.DevGuildID).GetTextChannel(664096941059080202).SendMessageAsync("", false, b.Build());
                    }
                    await HandleCommandresult(result, msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        internal async Task HandleCommandresult(IResult result, SocketUserMessage msg)
        {
            string logMsg = "";
            logMsg += $"[UTC TIME - {DateTime.UtcNow.ToLongDateString() + " : " + DateTime.UtcNow.ToLongTimeString()}] ";
            string completed = resultformat(result.IsSuccess);
            if (!result.IsSuccess)
                logMsg += $"COMMAND: {msg.Content} USER: {msg.Author.Username + "#" + msg.Author.Discriminator} COMMAND RESULT: {completed} ERROR TYPE: {result.Error.Value} EXCEPTION: {result.ErrorReason}";
            else
                logMsg += $"COMMAND: {msg.Content} USER: {msg.Author.Username + "#" + msg.Author.Discriminator} COMMAND RESULT: {completed}";
            var name = DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year;
            if (File.Exists(Global.CommandLogsDir + $"\\{name}.txt"))
            {
                string curr = File.ReadAllText(Global.CommandLogsDir + $"\\{name}.txt");
                File.WriteAllText(Global.CommandLogsDir + $"\\{name}.txt", $"{curr}\n{logMsg}");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Logged Command (from {msg.Author.Username})");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            else
            {
                File.Create(Global.MessageLogsDir + $"\\{name}.txt").Close();
                File.WriteAllText(Global.CommandLogsDir + $"\\{name}.txt", $"{logMsg}");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Logged Command (from {msg.Author.Username}) and created new logfile");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            if (result.IsSuccess)
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.Color = Color.Green;
                eb.Title = "**Command Log**";
                eb.Description = $"The Command {msg.Content.Split(' ').First()} was used in {msg.Channel.Name} by {msg.Author.Username + "#" + msg.Author.Discriminator} \n\n **Full Message** \n `{msg.Content}`\n\n **Result** \n {completed}";
                eb.Footer = new EmbedFooterBuilder();
                eb.Footer.Text = "Command Autogen";
                eb.Footer.IconUrl = _client.CurrentUser.GetAvatarUrl();
                await _client.GetGuild(Global.GuildID).GetTextChannel(Global.DebugChanID).SendMessageAsync("", false, eb.Build());
            }

        }
        internal static string resultformat(bool isSuccess)
        {
            if (isSuccess)
                return "Sucess";
            if (!isSuccess)
                return "Failed";
            return "Unknown";
        }

        private async Task LogMessage(SocketMessage arg)
        {
            try
            {
                if (!arg.Author.IsBot)
                {
                    if (sList.ContainsKey(arg.Channel.Id))
                    {
                        sList[arg.Channel.Id]++;
                    }
                    else
                    {
                        sList.Add(arg.Channel.Id, 1);
                    }
                }
                //Log messages to txt file
                string logMsg = "";
                logMsg += $"[{DateTime.UtcNow.ToLongDateString() + " : " + DateTime.UtcNow.ToLongTimeString()}] ";
                logMsg += $"USER: {arg.Author.Username}#{arg.Author.Discriminator} CHANNEL: {arg.Channel.Name} MESSAGE: {arg.Content}";
                var name = DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year;
                if (File.Exists(Global.MessageLogsDir + $"\\{name}.txt"))
                {
                    string curr = File.ReadAllText(Global.MessageLogsDir + $"\\{name}.txt");
                    File.WriteAllText(Global.MessageLogsDir + $"\\{name}.txt", $"{curr}\n{logMsg}");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"Logged message (from {arg.Author.Username})");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
                else
                {
                    File.Create(Global.MessageLogsDir + $"\\{name}.txt").Close();
                    File.WriteAllText(Global.MessageLogsDir + $"\\{name}.txt", $"{logMsg}");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Logged message (from {arg.Author.Username}) and created new logfile");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
                if (arg.Channel.Id == 465083688795766785)
                {
                    string cont = File.ReadAllText(Global.aiResponsePath);
                    if (cont == "") { cont = arg.Content; }
                    else { cont += $"\n{arg.Content}"; }
                    File.WriteAllText(Global.aiResponsePath, cont);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}