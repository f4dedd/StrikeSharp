using App.Bot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var configFile = File.ReadAllText("./config.json");

Config? config = JsonConvert.DeserializeObject<Config>(configFile);

Console.WriteLine(config.StrikeReset);

Bot bot = new Bot(config!);

bot.Run();

await Task.Delay(-1);