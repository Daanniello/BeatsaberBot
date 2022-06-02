using AnimatedGif;
using BumpKit;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Commands.Functions
{
    public class PatternCatalog
    {
        private RestUserMessage _msg;
        public List<PatternInfo> PatternsToDisplay;
        private string _gridSelection;
        private string _noteTypeSelection;
        private bool _isBlueSelection;
        private string _timingSelection;
        private bool _isBackhandSelection;
        private ulong _msgCreatorID;
        public List<PatternInfo> _patternsSelection;
        private string[][] _grid;
        private string _patternSavePath = "../../../Resources/PatternCatalog/PaternCatalog.json";
        private DiscordSocketClient _discord;
        public async Task AddNewPattern(DiscordSocketClient discord, SocketSlashCommand command)
        {
            _discord = discord;
            _patternsSelection = new List<PatternInfo>();
            _msgCreatorID = command.User.Id;

            var componentBuilder = new ComponentBuilder();
            componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog00", style: ButtonStyle.Primary);
            componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog01", style: ButtonStyle.Primary);
            componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog02", style: ButtonStyle.Primary);
            componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog03", style: ButtonStyle.Primary);
            componentBuilder.WithButton("Next", customId: "Next", style: ButtonStyle.Success);
            componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog10", style: ButtonStyle.Primary, row: 1);
            componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog11", style: ButtonStyle.Primary, row: 1);
            componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog12", style: ButtonStyle.Primary, row: 1);
            componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog13", style: ButtonStyle.Primary, row: 1);
            componentBuilder.WithButton("Go Back", customId: "Back", style: ButtonStyle.Danger, row: 1);
            componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog20", style: ButtonStyle.Primary, row: 2);
            componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog21", style: ButtonStyle.Primary, row: 2);
            componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog22", style: ButtonStyle.Primary, row: 2);
            componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog23", style: ButtonStyle.Primary, row: 2);
            componentBuilder.WithButton("Red", customId: "red", style: ButtonStyle.Secondary, row: 2);

            componentBuilder.WithButton(emote: Emote.Parse("<:bluenotedot:926423586653233183>"), customId: "bluenotedot_926423586653233183", style: ButtonStyle.Primary, row: 3);
            componentBuilder.WithButton(emote: Emote.Parse("<:bluenotedown:926423586640629801>"), customId: "bluenotedown_926423586640629801", style: ButtonStyle.Primary, row: 3);
            componentBuilder.WithButton(emote: Emote.Parse("<:bluenotedownleft:926423586548351057>"), customId: "bluenotedownleft_926423586548351057", style: ButtonStyle.Primary, row: 3);
            componentBuilder.WithButton(emote: Emote.Parse("<:bluenotedownright:926423586858737674>"), customId: "bluenotedownright_926423586858737674", style: ButtonStyle.Primary, row: 3);
            componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteleft:926423586988777482>"), customId: "bluenoteleft_926423586988777482", style: ButtonStyle.Primary, row: 3);
            componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteright:926423586992947220>"), customId: "bluenoteright_926423586992947220", style: ButtonStyle.Primary, row: 3);
            componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteup:926423587018125332>"), customId: "bluenoteup_926423587018125332", style: ButtonStyle.Primary, row: 3);
            componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteupleft:926423586988761118>"), customId: "bluenoteupleft_926423586988761118", style: ButtonStyle.Primary, row: 3);
            componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteupright:926423586628050985>"), customId: "bluenoteupright_926423586628050985", style: ButtonStyle.Primary, row: 3);
            componentBuilder.WithButton(emote: Emote.Parse("<:bomb:445582400383090717>"), customId: "bomb_445582400383090717", style: ButtonStyle.Primary, row: 3);

            _grid = new string[3][];
            _grid[0] = new string[] { "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>" };
            _grid[1] = new string[] { "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>" };
            _grid[2] = new string[] { "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>" };

            var gridString = "";
            for (var y = 0; y < 3; y++)
            {
                for (var x = 0; x < 4; x++)
                {
                    gridString += _grid[y][x];
                }
                gridString += "\n";
            }

            _msg = await command.Channel.SendMessageAsync($"{gridString}", false, EmbedBuilderExtension.NullEmbed("Add pattern to the catalog", $"- Choose a grid position by pressing a button and then press the button with the note type you want to place there.\n- Selecting a grid position twice resets the note type on that position.\n").Build(), components: componentBuilder.Build());

            discord.ButtonExecuted += Discord_ButtonExecuted;
        }

        private async Task Discord_ButtonExecuted(SocketMessageComponent arg)
        {
            if (arg.User.Id == _msgCreatorID)
            {
                if (arg.Data.CustomId == "red")
                {
                    var componentBuilder = new ComponentBuilder();
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog00", style: ButtonStyle.Primary);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog01", style: ButtonStyle.Primary);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog02", style: ButtonStyle.Primary);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog03", style: ButtonStyle.Primary);
                    componentBuilder.WithButton("Next", customId: "Next", style: ButtonStyle.Success);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog10", style: ButtonStyle.Primary, row: 1);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog11", style: ButtonStyle.Primary, row: 1);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog12", style: ButtonStyle.Primary, row: 1);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog13", style: ButtonStyle.Primary, row: 1);
                    componentBuilder.WithButton("Go Back", customId: "Back", style: ButtonStyle.Danger, row: 1);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog20", style: ButtonStyle.Primary, row: 2);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog21", style: ButtonStyle.Primary, row: 2);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog22", style: ButtonStyle.Primary, row: 2);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog23", style: ButtonStyle.Primary, row: 2);
                    componentBuilder.WithButton("Blue", customId: "blue", style: ButtonStyle.Secondary, row: 2);
                    componentBuilder.WithButton(emote: Emote.Parse("<:rednotedot:926423586535800833>"), customId: "rednotedot_926423586535800833", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:rednoteupright:926423588289015829>"), customId: "rednoteupright_926423588289015829", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:rednoteupleft:926423586510618645>"), customId: "rednoteupleft_926423586510618645", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:rednoteup:926423587047505970>"), customId: "rednoteup_926423587047505970", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:rednoteright:926423586846158938>"), customId: "rednoteright_926423586846158938", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:rednoteleft:926423586917462036>"), customId: "rednoteleft_926423586917462036", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:rednotedownright:926423586946818058>"), customId: "rednotedownright_926423586946818058", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:rednotedownleft:926423586846175233>"), customId: "rednotedownleft_926423586846175233", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:rednotedown:926423586581913612>"), customId: "rednotedown_926423586581913612", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bomb:445582400383090717>"), customId: "bomb_445582400383090717", style: ButtonStyle.Primary, row: 3);
                    await _msg.ModifyAsync(x => x.Components = componentBuilder.Build());
                    _isBlueSelection = false;
                }
                else if (arg.Data.CustomId == "blue")
                {
                    var componentBuilder = new ComponentBuilder();
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog00", style: ButtonStyle.Primary);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog01", style: ButtonStyle.Primary);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog02", style: ButtonStyle.Primary);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog03", style: ButtonStyle.Primary);
                    componentBuilder.WithButton("Next", customId: "Next", style: ButtonStyle.Success);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog10", style: ButtonStyle.Primary, row: 1);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog11", style: ButtonStyle.Primary, row: 1);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog12", style: ButtonStyle.Primary, row: 1);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog13", style: ButtonStyle.Primary, row: 1);
                    componentBuilder.WithButton("Go Back", customId: "Back", style: ButtonStyle.Danger, row: 1);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog20", style: ButtonStyle.Primary, row: 2);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog21", style: ButtonStyle.Primary, row: 2);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog22", style: ButtonStyle.Primary, row: 2);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog23", style: ButtonStyle.Primary, row: 2);
                    componentBuilder.WithButton("Red", customId: "red", style: ButtonStyle.Secondary, row: 2);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenotedot:926423586653233183>"), customId: "bluenotedot_926423586653233183", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenotedown:926423586640629801>"), customId: "bluenotedown_926423586640629801", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenotedownleft:926423586548351057>"), customId: "bluenotedownleft_926423586548351057", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenotedownright:926423586858737674>"), customId: "bluenotedownright_926423586858737674", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteleft:926423586988777482>"), customId: "bluenoteleft_926423586988777482", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteright:926423586992947220>"), customId: "bluenoteright_926423586992947220", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteup:926423587018125332>"), customId: "bluenoteup_926423587018125332", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteupleft:926423586988761118>"), customId: "bluenoteupleft_926423586988761118", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteupright:926423586628050985>"), customId: "bluenoteupright_926423586628050985", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bomb:445582400383090717>"), customId: "bomb_445582400383090717", style: ButtonStyle.Primary, row: 3);
                    await _msg.ModifyAsync(x => x.Components = componentBuilder.Build());
                    _isBlueSelection = true;
                }
                else if (arg.Data.CustomId == "patternCatalog00") _gridSelection = "00";
                else if (arg.Data.CustomId == "patternCatalog01") _gridSelection = "01";
                else if (arg.Data.CustomId == "patternCatalog02") _gridSelection = "02";
                else if (arg.Data.CustomId == "patternCatalog03") _gridSelection = "03";
                else if (arg.Data.CustomId == "patternCatalog10") _gridSelection = "10";
                else if (arg.Data.CustomId == "patternCatalog11") _gridSelection = "11";
                else if (arg.Data.CustomId == "patternCatalog12") _gridSelection = "12";
                else if (arg.Data.CustomId == "patternCatalog13") _gridSelection = "13";
                else if (arg.Data.CustomId == "patternCatalog20") _gridSelection = "20";
                else if (arg.Data.CustomId == "patternCatalog21") _gridSelection = "21";
                else if (arg.Data.CustomId == "patternCatalog22") _gridSelection = "22";
                else if (arg.Data.CustomId == "patternCatalog23") _gridSelection = "23";
                if (arg.Data.CustomId.Contains("patternCatalog") && _gridSelection != null)
                {
                    string y_ = _gridSelection.Substring(0, 1);
                    string x_ = _gridSelection.Substring(1, 1);
                    _grid[Convert.ToInt32(y_)][Convert.ToInt32(x_)] = "<:emptynote:926428079591682048>";
                    var gridString = "";
                    for (var y = 0; y < 3; y++)
                    {
                        for (var x = 0; x < 4; x++)
                        {
                            gridString += _grid[y][x];
                        }
                        gridString += "\n";
                    }
                    await _msg.ModifyAsync(x => x.Content = gridString);
                }
                else if (arg.Data.CustomId.Contains("note") || arg.Data.CustomId.Contains("bomb") && _gridSelection != null)
                {
                    var emote = $"<:{arg.Data.CustomId.Replace("_", ":")}>";
                    string y_ = _gridSelection.Substring(0, 1);
                    string x_ = _gridSelection.Substring(1, 1);
                    _grid[Convert.ToInt32(y_)][Convert.ToInt32(x_)] = emote;

                    var gridString = "";
                    for (var y = 0; y < 3; y++)
                    {
                        for (var x = 0; x < 4; x++)
                        {
                            gridString += _grid[y][x];
                        }
                        gridString += "\n";
                    }

                    _gridSelection = null;

                    await _msg.ModifyAsync(x => x.Content = gridString);
                }
                else if (arg.Data.CustomId == "Next")
                {
                    var componentBuilder = new ComponentBuilder();
                    componentBuilder.WithButton("1", customId: "one", style: ButtonStyle.Primary);
                    componentBuilder.WithButton("1/2", customId: "half", style: ButtonStyle.Primary);
                    componentBuilder.WithButton("1/3", customId: "third", style: ButtonStyle.Primary);
                    componentBuilder.WithButton("1/4", customId: "fourth", style: ButtonStyle.Primary);
                    componentBuilder.WithButton("1/8", customId: "eight", style: ButtonStyle.Primary);
                    componentBuilder.WithButton("1/16", customId: "sixteen", style: ButtonStyle.Primary);
                    componentBuilder.WithButton("Front hand", customId: "fronthand", style: ButtonStyle.Primary);
                    componentBuilder.WithButton("Back hand", customId: "backhand", style: ButtonStyle.Primary);
                    componentBuilder.WithButton("Add next pattern", customId: "addpattern", style: ButtonStyle.Success);
                    componentBuilder.WithButton("Complete pattern", customId: "completepattern", style: ButtonStyle.Success);

                    await _msg.ModifyAsync(x => x.Components = componentBuilder.Build());
                    await _msg.ModifyAsync(x => x.Content = "");
                    await _msg.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("What is the timing before the next note comes? \nAnd is this swing back hand or front hand? \nreact with a button", "").Build());
                }
                else if (arg.Data.CustomId == "one") { _timingSelection = "1"; }
                else if (arg.Data.CustomId == "half") { _timingSelection = "1/2"; }
                else if (arg.Data.CustomId == "third") { _timingSelection = "1/3"; }
                else if (arg.Data.CustomId == "fourth") { _timingSelection = "1/4"; }
                else if (arg.Data.CustomId == "eight") { _timingSelection = "1/8"; }
                else if (arg.Data.CustomId == "sixteen") { _timingSelection = "1/16"; }
                else if (arg.Data.CustomId == "backhand") { _isBackhandSelection = true; }
                else if (arg.Data.CustomId == "fronthand") { _isBackhandSelection = false; }
                else if (arg.Data.CustomId == "addpattern" && _timingSelection != "")
                {
                    var a = Enum.Parse<Patterns>(_grid[0][0].Split(':')[1]);
                    var b = Enum.Parse<Patterns>(_grid[0][1].Split(':')[1]);
                    var c = Enum.Parse<Patterns>(_grid[0][2].Split(':')[1]);
                    var d = Enum.Parse<Patterns>(_grid[0][3].Split(':')[1]);
                    var e = Enum.Parse<Patterns>(_grid[1][0].Split(':')[1]);
                    var f = Enum.Parse<Patterns>(_grid[1][1].Split(':')[1]);
                    var g = Enum.Parse<Patterns>(_grid[1][2].Split(':')[1]);
                    var h = Enum.Parse<Patterns>(_grid[1][3].Split(':')[1]);
                    var i = Enum.Parse<Patterns>(_grid[2][0].Split(':')[1]);
                    var j = Enum.Parse<Patterns>(_grid[2][1].Split(':')[1]);
                    var k = Enum.Parse<Patterns>(_grid[2][2].Split(':')[1]);
                    var l = Enum.Parse<Patterns>(_grid[2][3].Split(':')[1]);

                    _patternsSelection.Add(new PatternInfo(new Patterns[][] {
                        new Patterns[]{ a, b, c, d},
                        new Patterns[]{ e, f, g, h},
                        new Patterns[]{ i, j, k, l}
                    }, _timingSelection, 500, _isBackhandSelection));

                    _timingSelection = "";
                    _grid[0] = new string[] { "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>" };
                    _grid[1] = new string[] { "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>" };
                    _grid[2] = new string[] { "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>", "<:emptynote:926428079591682048>" };

                    var componentBuilder = new ComponentBuilder();
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog00", style: ButtonStyle.Primary);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog01", style: ButtonStyle.Primary);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog02", style: ButtonStyle.Primary);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog03", style: ButtonStyle.Primary);
                    componentBuilder.WithButton("Next", customId: "Next", style: ButtonStyle.Success);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog10", style: ButtonStyle.Primary, row: 1);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog11", style: ButtonStyle.Primary, row: 1);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog12", style: ButtonStyle.Primary, row: 1);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog13", style: ButtonStyle.Primary, row: 1);
                    componentBuilder.WithButton("Go Back", customId: "Back", style: ButtonStyle.Danger, row: 1);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog20", style: ButtonStyle.Primary, row: 2);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog21", style: ButtonStyle.Primary, row: 2);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog22", style: ButtonStyle.Primary, row: 2);
                    componentBuilder.WithButton(emote: Emote.Parse("<:emptynote:926428079591682048>"), customId: "patternCatalog23", style: ButtonStyle.Primary, row: 2);
                    componentBuilder.WithButton("Red", customId: "red", style: ButtonStyle.Secondary, row: 2);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenotedot:926423586653233183>"), customId: "bluenotedot_926423586653233183", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenotedown:926423586640629801>"), customId: "bluenotedown_926423586640629801", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenotedownleft:926423586548351057>"), customId: "bluenotedownleft_926423586548351057", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenotedownright:926423586858737674>"), customId: "bluenotedownright_926423586858737674", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteleft:926423586988777482>"), customId: "bluenoteleft_926423586988777482", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteright:926423586992947220>"), customId: "bluenoteright_926423586992947220", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteup:926423587018125332>"), customId: "bluenoteup_926423587018125332", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteupleft:926423586988761118>"), customId: "bluenoteupleft_926423586988761118", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bluenoteupright:926423586628050985>"), customId: "bluenoteupright_926423586628050985", style: ButtonStyle.Primary, row: 3);
                    componentBuilder.WithButton(emote: Emote.Parse("<:bomb:445582400383090717>"), customId: "bomb_445582400383090717", style: ButtonStyle.Primary, row: 3);
                    var gridString = "";
                    for (var y = 0; y < 3; y++)
                    {
                        for (var x = 0; x < 4; x++)
                        {
                            gridString += _grid[y][x];
                        }
                        gridString += "\n";
                    }

                    _gridSelection = null;
                    _isBlueSelection = true;
                    await _msg.ModifyAsync(x => x.Components = componentBuilder.Build());
                    await _msg.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Add pattern to the catalog", $"- Choose a grid position by pressing a button and then press the button with the note type you want to place there.\n- Selecting a grid position twice resets the note type on that position.\n").Build());
                    await _msg.ModifyAsync(x => x.Content = gridString);
                }
                else if (arg.Data.CustomId == "completepattern" && _timingSelection != "")
                {
                    var a = Enum.Parse<Patterns>(_grid[0][0].Split(':')[1]);
                    var b = Enum.Parse<Patterns>(_grid[0][1].Split(':')[1]);
                    var c = Enum.Parse<Patterns>(_grid[0][2].Split(':')[1]);
                    var d = Enum.Parse<Patterns>(_grid[0][3].Split(':')[1]);
                    var e = Enum.Parse<Patterns>(_grid[1][0].Split(':')[1]);
                    var f = Enum.Parse<Patterns>(_grid[1][1].Split(':')[1]);
                    var g = Enum.Parse<Patterns>(_grid[1][2].Split(':')[1]);
                    var h = Enum.Parse<Patterns>(_grid[1][3].Split(':')[1]);
                    var i = Enum.Parse<Patterns>(_grid[2][0].Split(':')[1]);
                    var j = Enum.Parse<Patterns>(_grid[2][1].Split(':')[1]);
                    var k = Enum.Parse<Patterns>(_grid[2][2].Split(':')[1]);
                    var l = Enum.Parse<Patterns>(_grid[2][3].Split(':')[1]);

                    _patternsSelection.Add(new PatternInfo(new Patterns[][] {
                        new Patterns[]{ a, b, c, d},
                        new Patterns[]{ e, f, g, h},
                        new Patterns[]{ i, j, k, l}
                    }, _timingSelection, 500, _isBackhandSelection));

                    var componentBuilder = new ComponentBuilder();
                    componentBuilder.WithButton("Save", customId: "save", style: ButtonStyle.Success);
                    componentBuilder.WithButton("Exit", customId: "exit", style: ButtonStyle.Danger);
                    var embedBuilder = EmbedBuilderExtension.NullEmbed("Are you happy with the results?", $"Press save to save the pattern in the catalog or exit to delete it\nPlease make sure its decent quality since this is public to everyone");
                    var guid = Guid.NewGuid();
                    CreateGifFromPatternList(_patternsSelection, $"{guid}.gif");
                    embedBuilder.WithImageUrl($"{GlobalConfiguration.BotImageStorageLink}{guid}.gif");

                    await _msg.ModifyAsync(x => x.Components = componentBuilder.Build());
                    await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                    await _msg.ModifyAsync(x => x.Content = "");
                }
                else if (arg.Data.CustomId == "save" && _timingSelection != "")
                {
                    await _msg.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Give a good name for the pattern", "Be serious and think a little for this\nAnswer the name here in chat").Build());
                    var now = DateTime.Now;
                    var end = DateTime.Now.AddMinutes(20);
                    var name = "";
                    var description = "";

                    bool isDone = false;
                    var patternList = await OpenPatternList();
                    do
                    {
                        var reaction = await _msg.Channel.GetMessagesAsync(1).FlattenAsync();
                        if (reaction.First().Author.Id == _msgCreatorID && reaction.First().CreatedAt > now && reaction.First().Author.IsBot == false && reaction.First().Content != "")
                        {
                            name = reaction.First().Content;
                            if (patternList.All(x => x.Name != name))
                            {
                                isDone = true;

                            }
                            else
                            {
                                var feedback = await _msg.Channel.SendMessageAsync("There is already a pattern with this name in the catalog.");
                                await Task.Delay(1000);
                                await feedback.DeleteAsync();
                            }
                            await reaction.First().DeleteAsync();
                        }
                    } while (end > now && !isDone);

                    await _msg.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Give a good description about the pattern", "Think about how you can use it in mapping, how comfortable it is, how other patterns should fit after it etc etc..").Build());
                    now = DateTime.Now;
                    end = DateTime.Now.AddMinutes(20);

                    isDone = false;
                    do
                    {
                        var reaction = await _msg.Channel.GetMessagesAsync(1).FlattenAsync();
                        if (reaction.First().Author.Id == _msgCreatorID && reaction.First().CreatedAt > now && reaction.First().Author.IsBot == false && reaction.First().Content != "")
                        {
                            description = reaction.First().Content;
                            isDone = true;
                            await reaction.First().DeleteAsync();
                        }
                    } while (end > now && !isDone);

                    if (name != "" && description != "")
                    {

                        await SavePatternInPatternList(new Pattern(name, description, _patternsSelection));
                        await _msg.DeleteAsync();
                        await _msg.Channel.SendMessageAsync("Pattern added to the catalog! Thanks for your help <3 \nYour created pattern will be shown up sooner or later in the catalog");

                        _discord.ButtonExecuted -= Discord_ButtonExecuted;
                    }
                }
                else if (arg.Data.CustomId == "exit")
                {                    
                    await _msg.DeleteAsync();                    
                    _discord.ButtonExecuted -= Discord_ButtonExecuted;
                }
            }

            return;
        }

        private async Task<bool> SavePatternInPatternList(Pattern pattern)
        {
            try
            {
                var patternList = await OpenPatternList();
                if (patternList == null) patternList = new List<Pattern>();
                patternList.Add(pattern);
                var json = JsonConvert.SerializeObject(patternList);
                if (File.Exists(_patternSavePath)) File.WriteAllText(_patternSavePath, json);
                else File.AppendAllText(_patternSavePath, json);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<Pattern>> OpenPatternList()
        {
            try
            {
                var json = File.ReadAllText(_patternSavePath);
                return JsonConvert.DeserializeObject<List<Pattern>>(json);
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public async Task Function(DiscordSocketClient discord, SocketSlashCommand command)
        {
            if (command.Data.Options.Count != 0)
            {
                if (command.Data.Options.First().Name == "action")
                {
                    AddNewPattern(discord, command);
                    return;
                }
                else if (command.Data.Options.First().Name.Contains("patterns"))
                {
                }
                else
                {
                    return;
                }
            }

            var patterns = await OpenPatternList();
            var pattern = patterns.FirstOrDefault(x => x.Name == command.Data.Options.First().Value.ToString());
            if (pattern != null)
            {
                CreateGifFromPatternList(pattern.PatternFrames, $"patterncatalog_{pattern.Name.Replace(" ", "_").Replace("/", "-").Trim()}.gif");

                var embedBuilder = EmbedBuilderExtension.NullEmbed($"{pattern.Name}", $"{pattern.Description}");
                embedBuilder.WithImageUrl($"{GlobalConfiguration.BotImageStorageLink}patterncatalog_{pattern.Name.Replace(" ", "_").Replace("/","-").Trim()}.gif");
                var msg = await command.Channel.SendMessageAsync("", false, embedBuilder.Build());
            }
        }


        private void AddPatterFrameToPatternList(Patterns[] topRow, Patterns[] midRow, Patterns[] downRow, string timeTillNextNote, int duration, bool isBackHand)
        {
            PatternsToDisplay.Add(new PatternInfo(new Patterns[][] {
                topRow,
                midRow,
                downRow
            }, timeTillNextNote, duration, isBackHand));
        }

        private void CreateGifFromPatternList(List<PatternInfo> patternsToDisplay, string gifPath)
        {
            //Create all patternframes
            var timing = "";
            foreach (var patternFrame in patternsToDisplay)
            {
                timing = patternFrame.TimeTillNextNote;
                var imageCreator = new ImageCreator("../../../Resources/img/patterncatalog_template.png");

                for (var x = 0; x < 4; x++)
                {
                    for (var y = 0; y < 3; y++)
                    {
                        var type = patternFrame.PatternFrame[y][x].ToString();
                        imageCreator.AddImage($"../../../Resources/img/{type}.png", (x * 140) + 30, (y * 120) + 20, 120, 120, isLocalFile: true);
                        imageCreator.AddText($"{timing}", System.Drawing.Color.White, 12, 10, 370);
                        imageCreator.AddTextFloatRight($"{patternFrame.SwingType}", System.Drawing.Color.White, 12, 10, 370);
                    }
                }
                var bitmap = imageCreator.GetBitmap();
                patternFrame.patternFrameAsBitmap = bitmap;
            }

            //Create empty frame
            var emptyFrameCreator = new ImageCreator("../../../Resources/img/patterncatalog_template.png");

            for (var x = 0; x < 4; x++)
            {
                for (var y = 0; y < 3; y++)
                {
                    emptyFrameCreator.AddImage($"../../../Resources/img/emptynote.png", (x * 140) + 30, y * 120 + 20, 120, 120, isLocalFile: true);
                }
            }
            var emptyFrame = emptyFrameCreator.GetBitmap();


            // 33ms delay (~30fps)
            using (var gif = AnimatedGif.AnimatedGif.Create($"{GlobalConfiguration.BotImageStoragePath}{gifPath}", 33))
            {
                foreach (var patternFrame in patternsToDisplay)
                {
                    gif.AddFrame(patternFrame.patternFrameAsBitmap, delay: patternFrame.Duration, quality: GifQuality.Bit8);
                    int timeTillNextNote = 1000;
                    switch (patternFrame.TimeTillNextNote)
                    {
                        case "1":
                            timeTillNextNote = 1000;
                            break;
                        case "1/2":
                            timeTillNextNote = 500;
                            break;
                        case "1/3":
                            timeTillNextNote = 333;
                            break;
                        case "1/4":
                            timeTillNextNote = 250;
                            break;
                        case "1/8":
                            timeTillNextNote = 125;
                            break;
                        case "1/16":
                            timeTillNextNote = 75;
                            break;
                        default:
                            break;
                    }

                    gif.AddFrame(emptyFrame, delay: timeTillNextNote, quality: GifQuality.Bit8);
                }
                gif.AddFrame(emptyFrame, delay: 2000, quality: GifQuality.Bit8);
            }

            //using (var image = bitmapList.First())
            //using (var gif = File.OpenWrite($"{GlobalConfiguration.BotImageStoragePath}{gifPath}"))
            //using (var encoder = new GifEncoder(gif))                
            //    foreach (var bitmap in bitmapList)
            //    {                    
            //        encoder.AddFrame(bitmap, frameDelay: TimeSpan.FromMilliseconds(1000));
            //    }

        }

        public enum Patterns
        {
            emptynote = 0,
            bluenotedot = 1,
            bluenoteup = 2,
            bluenoteupright = 3,
            bluenoteupleft = 4,
            bluenoteright = 5,
            bluenoteleft = 6,
            bluenotedown = 7,
            bluenotedownright = 8,
            bluenotedownleft = 9,
            rednotedot = 10,
            rednoteup = 11,
            rednoteupright = 12,
            rednoteupleft = 13,
            rednoteright = 14,
            rednoteleft = 15,
            rednotedown = 16,
            rednotedownright = 17,
            rednotedownleft = 18,
            bomb = 19
        }

        public class PatternInfo
        {
            public int Duration;
            public string TimeTillNextNote;
            public Patterns[][] PatternFrame;
            [JsonIgnore]
            public Bitmap patternFrameAsBitmap;
            public string SwingType;

            public PatternInfo(Patterns[][] patternFrame, string timeTillNextNote, int duration, bool isBackhand)
            {
                PatternFrame = patternFrame;
                Duration = duration;
                TimeTillNextNote = timeTillNextNote;
                SwingType = isBackhand ? "Back Hand" : "Front Hand";
            }
        }

        public class Pattern
        {
            public string Name;
            public string Description;
            public List<PatternInfo> PatternFrames;

            public Pattern(string name, string description, List<PatternInfo> patternFrames)
            {
                Name = name;
                Description = description;
                PatternFrames = patternFrames;
            }
        }


    }
}
