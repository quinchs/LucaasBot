using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LucaasBot.Global;

namespace LucaasBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("slowmode")]
        public async Task slowmode(string value)
        {
            //check user perms
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == 583302940467527683).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null || r.FirstOrDefault(x => x.Id == Global.DeveloperRoleId) != null)
            {
                try
                {
                    int val = 0;
                    try
                    {
                        val = Convert.ToInt32(value);
                    }
                    catch { }
                    var chan = Context.Guild.GetTextChannel(Context.Channel.Id);
                    await chan.ModifyAsync(x =>
                    {
                        x.SlowModeInterval = val;
                    });
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Color = Color.Green,
                        Title = $"Set the slowmode to {value}!",
                        Description = $"{Context.Message.Author.Mention} successfully modified the slowmode of <#{Context.Channel.Id}> to {value} seconds!",
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = Context.Message.Author.ToString(),
                            IconUrl = Context.Message.Author.GetAvatarUrl(),
                            Url = Context.Message.GetJumpUrl()
                        }
                    }.Build());
                }
                catch (Exception ex)
                {
                    Global.SendExeption(ex);
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Color = Color.Red,
                    Title = "You dont have Permission!",
                    Description = $"Sorry {Context.Message.Author.Mention} but you do not have permission to change the slowmode of <#{Context.Channel.Id}> !",
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = Context.Message.Author.ToString(),
                        IconUrl = Context.Message.Author.GetAvatarUrl(),
                        Url = Context.Message.GetJumpUrl()
                    }
                }.Build());
            }
        }
        [Command("welcome")]
        public async Task welcome()
        {
            var arg = Context.Guild.GetUser(Context.Message.Author.Id);
            string welcomeMessage = Global.WelcomeMessage;

            EmbedBuilder eb = new EmbedBuilder()
            {
                Title = $"***Welcome to Lucaas's Official Discord server!***",
                Footer = new EmbedFooterBuilder()
                {
                    IconUrl = arg.GetAvatarUrl(),
                    Text = $"{arg.Username}#{arg.Discriminator}"
                },
                Description = welcomeMessage,
                ThumbnailUrl = Global.WelcomeMessageURL,
                Color = Color.Green
            };
            await Context.Channel.SendMessageAsync("", false, eb.Build());
            Global.ConsoleLog($"WelcomeMessage for {arg.Username}#{arg.Discriminator}", ConsoleColor.Blue);
        }
        [Command("modify")]
        public async Task modify(string configItem, params string[] input)
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == 635039634992136204).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null || r.FirstOrDefault(x => x.Id == Global.DeveloperRoleId) != null)
            {
                if (input.Length == 0)
                {
                    if (configItem == "list")
                    {
                        if (Context.Guild.Id == Global.DevGuildID)
                        {
                            EmbedBuilder b = new EmbedBuilder();
                            b.Footer = new EmbedFooterBuilder();
                            b.Footer.Text = "**Dev Config**";
                            b.Title = "Dev Config List";
                            string list = "**Here is the current config file** \n";
                            foreach (var item in Global.JsonItemsListDevOps) { list += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n"; }
                            b.Description = list;
                            b.Color = Color.Green;
                            b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                            await Context.Channel.SendMessageAsync("", false, b.Build());
                        }
                        else
                        {

                            if (Context.Channel.Id == 561218509342769173)
                            {
                                EmbedBuilder b = new EmbedBuilder();
                                b.Footer = new EmbedFooterBuilder();
                                b.Footer.Text = "**Admin Config**";
                                b.Title = "Admin Config List";
                                string list = "**Here is the current config file, not all items are here, if you wish to view more items please contact Thomas or Swiss, because they control the config items you can modify!** \n";
                                string itemsL = "";
                                foreach (var item in Global.jsonItemsList) { itemsL += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n"; }
                                if (itemsL == "") { list = "**Sorry but there is nothing here or you do not have permission to change anything yet :/**"; }
                                b.Description = list + itemsL;
                                b.Color = Color.Green;
                                b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                                await Context.Channel.SendMessageAsync("", false, b.Build());
                            }
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"No value was provided for the variable `{configItem}`");
                    }
                }
                else
                {
                    var value = string.Join(" ", input);
                    string newvalue = value;
                    if (Context.Guild.Id == Global.DevGuildID)//allow full modify
                    {
                        if (Global.JsonItemsListDevOps.Keys.Contains(configItem))
                        {
                            JsonItems data = Global.CurrentJsonData;
                            data = modifyJsonData(data, configItem, newvalue);
                            if (data.Token != null)
                            {
                                Global.SaveConfig(data);
                                await Context.Channel.SendMessageAsync($"Sucessfuly modified the config, Updated the item {configItem} with the new value of {value}");
                                EmbedBuilder b = new EmbedBuilder();
                                b.Footer = new EmbedFooterBuilder();
                                b.Footer.Text = "**Dev Config**";
                                b.Title = "Dev Config List";
                                string list = "**Here is the current config file** \n";
                                foreach (var item in Global.JsonItemsListDevOps) { list += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n"; }
                                b.Description = list;
                                b.Color = Color.Green;
                                b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                                await Context.Channel.SendMessageAsync("", false, b.Build());
                            }

                        }
                        else { await Context.Channel.SendMessageAsync($"Could not find the config item {configItem}! Try `{Global.Preflix}modify list` for a list of the Config!"); }
                    }
                    if (Context.Guild.Id == Global.GuildID)
                    {
                        if (Context.Channel.Id == 561218509342769173)//allow some modify
                        {
                            if (Global.jsonItemsList.Keys.Contains(configItem))
                            {
                                JsonItems data = Global.CurrentJsonData;
                                data = modifyJsonData(data, configItem, newvalue);
                                if (data.Token != null)
                                {
                                    Global.SaveConfig(data);
                                    await Context.Channel.SendMessageAsync($"Sucessfuly modified the config, Updated the item {configItem} with the new value of {value}");
                                    EmbedBuilder b = new EmbedBuilder();
                                    b.Footer = new EmbedFooterBuilder();
                                    b.Footer.Text = "**Admin Config**";
                                    b.Title = "Admin Config List";
                                    string list = "**Here is the current config file** \n";
                                    foreach (var item in Global.jsonItemsList) { list += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n"; }
                                    b.Description = list;
                                    b.Color = Color.Green;
                                    b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                                    await Context.Channel.SendMessageAsync("", false, b.Build());
                                }
                            }
                            else
                            {
                                if (Global.JsonItemsListDevOps.Keys.Contains(configItem))
                                {
                                    EmbedBuilder b = new EmbedBuilder();
                                    b.Color = Color.Red;
                                    b.Title = "You need Better ***PERMISSION***";
                                    b.Description = "You do not have permission to modify this item, if you think this is incorrect you can DM quin#3017 for help";

                                    await Context.Channel.SendMessageAsync("", false, b.Build());
                                }
                                else { await Context.Channel.SendMessageAsync($"Could not find the config item {configItem}! Try `{Global.Preflix}modify list` for a list of the Config!"); }
                            }
                        }
                    }
                }
            }
        }
        internal JsonItems modifyJsonData(JsonItems data, string iName, object iValue)
        {
            try
            {
                var prop = data.GetType().GetProperty(iName);
                if (prop != null)
                {
                    Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    object safeValue = (iValue == null) ? null : Convert.ChangeType(iValue, t);
                    prop.SetValue(data, safeValue, null);
                    return data;
                }
                else { throw new Exception($"Could not find the config item {iName}!"); }

            }
            catch (Exception ex)
            {
                EmbedBuilder b = new EmbedBuilder()
                {
                    Color = Color.Red,
                    Title = "Exeption!",
                    Description = $"**{ex}**"
                };
                Context.Channel.SendMessageAsync("", false, b.Build());
                return data;
            }
        }

    }
}
