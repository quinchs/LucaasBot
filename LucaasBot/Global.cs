using System;
using System.Collections.Generic;
using System.IO;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace LucaasBot
{
    public class Global
    {
        public static string Token { get; set; }
        public static char Preflix { get; set; }
        public static string Status { get; set; }
        public static ulong GuildID { get; set; }
        public static ulong DevGuildID { get; set; }
        public static ulong DeveloperRoleId { get; set; }
        public static ulong DebugChanID { get; set; }
        public static ulong WelcomeMessageChanID { get; set; }
        public static string WelcomeMessage { get; set; }
        public static string WelcomeMessageURL { get; set; }
        public static ulong ModeratorRoleID { get; set; }
        public static ulong MemberRoleID { get; set; }
        public static bool AutoSlowmodeToggle { get; set; }
        public static int AutoSlowmodeTrigger { get; set; }
        public static ulong BotAiChanID { get; set; }
        public static string MessageLogsDir = $"{Environment.CurrentDirectory}\\Messagelogs";
        public static string CommandLogsDir = $"{Environment.CurrentDirectory}\\Commandlogs";
        public static string ButterFile = $"{Environment.CurrentDirectory}\\Data\\Landings.butter";
        public static string aiResponsePath = $"{Environment.CurrentDirectory}\\Data\\Responses.AI";
        private static string ConfigSettingsPath = $"{Environment.CurrentDirectory}\\Data\\ConfigPerms.json";
        internal static JsonItems CurrentJsonData;
        internal static Dictionary<string, string> jsonItemsList { get; private set; }
        internal static Dictionary<string, string> JsonItemsListDevOps { get; private set; }
        public static Dictionary<string, bool> ConfigSettings { get; set; }

        public static DiscordSocketClient Client { get; internal set; }
        private static string ConfigPath = $"{Environment.CurrentDirectory}\\Data\\Config.json";

        public static void ConsoleLog(string ConsoleMessage, ConsoleColor FColor = ConsoleColor.Green, ConsoleColor BColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = FColor;
            Console.BackgroundColor = BColor;
            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + ConsoleMessage);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        internal static void ReadConfig()
        {
            //read the .json
            if (!Directory.Exists(MessageLogsDir)) { Directory.CreateDirectory(MessageLogsDir); }
            if (!Directory.Exists(CommandLogsDir)) { Directory.CreateDirectory(CommandLogsDir); }
            if (!File.Exists(aiResponsePath)) { File.Create(aiResponsePath); }

            var data = JsonConvert.DeserializeObject<JsonItems>(File.ReadAllText(ConfigPath));
            jsonItemsList = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ConfigPath));
            JsonItemsListDevOps = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ConfigPath));
            ConfigSettings = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(ConfigSettingsPath));
            //foreach (var item in ConfigSettings)
            //    if (item.Value == false)
            //        jsonItemsList.Remove(item.Key);

            JsonItemsListDevOps.Remove("Token");
            CurrentJsonData = data;
            Preflix = data.Preflix;
            WelcomeMessageChanID = data.WelcomeMessageChanID;
            WelcomeMessage = data.WelcomeMessage;
            WelcomeMessageURL = data.WelcomeMessageURL;
            Status = data.Status;
            //giveawayChanID = data.giveawayChanID;
            //giveawayCreatorChanId = data.giveawayCreatorChanId;
            Token = data.Token;
            //StatsChanID = data.StatsChanID;
            GuildID = data.GuildID;
            DeveloperRoleId = data.DeveloperRoleId;
            DevGuildID = data.DevGuildID;
            //LogsChannelID = data.LogsChannelID;
            DebugChanID = data.DebugChanID;
            //SubmissionChanID = data.SubmissionChanID;
            //TestingCat = data.TestingCatigoryID;
            ModeratorRoleID = data.ModeratorRoleID;
            MemberRoleID = data.MemberRoleID;
            AutoSlowmodeTrigger = data.AutoSlowmodeTrigger;
            //ApiKey = data.ApiKey;
            AutoSlowmodeToggle = data.AutoSlowmodeToggle;
            //UnverifiedRoleID = data.UnverifiedRoleID;
            //VerificationChanID = data.VerificationChanID;
            //VerificationLogChanID = data.VerificationLogChanID;
            //SubmissionsLogChanID = data.SubmissionsLogChanID;
            //MilestonechanID = data.MilestonechanID;
            BotAiChanID = data.BotAiChanID;
            //StatsTotChanID = data.StatsTotChanID;

        }
        public class JsonItems
        {
            public string Token { get; set; }
            public string Status { get; set; }
            public char Preflix { get; set; }
            public ulong GuildID { get; set; }
            public ulong DevGuildID { get; set; }
            public ulong DeveloperRoleId { get; set; }
            public ulong DebugChanID { get; set; }
            public ulong WelcomeMessageChanID { get; set; }
            public string WelcomeMessage { get; set; }
            public string WelcomeMessageURL { get; set; }
            //public ulong VerificationLogChanID { get; set; }
            public ulong ModeratorRoleID { get; set; }
            public ulong MemberRoleID { get; set; }
            //public ulong UnverifiedRoleID { get; set; }
            //public ulong VerificationChanID { get; set; }
            //public ulong SubmissionsLogChanID { get; set; }
            //public ulong MilestonechanID { get; set; }
            public bool AutoSlowmodeToggle { get; set; }
            public ulong BotAiChanID { get; set; }
            //public ulong giveawayChanID { get; set; }
            //public ulong giveawayCreatorChanId { get; set; }
            //public string ApiKey { get; set; }
            public int AutoSlowmodeTrigger { get; set; }
        }
        public static async void SendExeption(Exception ex)
        {
            EmbedBuilder b = new EmbedBuilder();
            b.Color = Color.Red;
            b.Description = ex.StackTrace;
            b.Fields = new List<EmbedFieldBuilder>()
            {
                {new EmbedFieldBuilder()
                {
                    Name = "Source",
                    Value = ex.Source
                } },
                {new EmbedFieldBuilder()
                {
                    Name = "Message",
                    Value = ex.Message
                } },
                {new EmbedFieldBuilder()
                {
                    Name = "TargetSite",
                    Value = ex.TargetSite
                } }
            };
            b.Footer = new EmbedFooterBuilder();
            b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
            b.Title = "Bot Command Error!";
            await Client.GetGuild(Global.GuildID).GetTextChannel(Global.DebugChanID).SendMessageAsync("", false, b.Build());
            await Client.GetGuild(Global.DevGuildID).GetTextChannel(664096941059080202).SendMessageAsync("", false, b.Build());
        }
        public static void SaveConfig(JsonItems newData)
        {
            string jsonS = JsonConvert.SerializeObject(newData, Formatting.Indented);
            newData.Token = "N#########################";
            string conJson = JsonConvert.SerializeObject(newData, Formatting.Indented);
            File.WriteAllText(ConfigPath, jsonS);
            ConsoleLog("Saved New config items. here is the new JSON \n " + conJson + "\n Saving...", ConsoleColor.DarkYellow);
            ReadConfig();
        }
    }
}