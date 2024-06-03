using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using SharpTimerAPI;
using SharpTimerAPI.Events;
using StoreApi;

namespace SharpTimerRewards;

public class RewardsConfig : BasePluginConfig
{
    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = "{green}[TimerRewards]{white}";
    [JsonPropertyName("reward_amount")]
    public int RewardAmount { get; set; } = 50;

    [JsonPropertyName("sr_reward")] 
    public int SrReward { get; set; } = 100;
    
    [JsonPropertyName("tier_multiplier")] 
    public float TierMultiplier { get; set; } = 1.25f;

    [JsonPropertyName("only_reward_pbs")] 
    public bool PbOnly { get; set; } = false;

}

public class SharpTimerRewards : BasePlugin, IPluginConfig<RewardsConfig>
{
    public override string ModuleName => "SharpTimerRewards";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "Interesting";
    public RewardsConfig Config { get; set; }
    
    public void OnConfigParsed(RewardsConfig config)
    {
        config.Prefix = config.Prefix.ReplaceColorTags();
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        var sender = new PluginCapability<ISharpTimerEventSender>("sharptimer:event_sender").Get();
        if(sender == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[TimerRewards] Failed to get ISharpTimerEventSender");
            return;
        }

        AddTimer(0.5f, () =>
        {
            sender.STEventSender += OnTimer;
        });
    }

    public void OnTimer(object? _, ISharpTimerPlayerEvent @event)
    {
        if (@event is StopTimerEvent e)
        {
            if(Config.PbOnly && !e.IsPb)
                return;
            int reward = e.IsSr ? Config.SrReward : Config.RewardAmount;
            if (e.Tier > 1)
                reward = (int)(reward * Math.Pow(Config.TierMultiplier, e.Tier-1));
            var api = IStoreApi.Capability.Get();
            if (api == null)
                return;
            if (e.Player != null)
            {
                api.GivePlayerCredits(e.Player, reward);
                e.Player.PrintToChat($" {Config.Prefix} you received {ChatColors.Green}{reward} credits{ChatColors.White} for completing the map!");
            }
        }
    }
}