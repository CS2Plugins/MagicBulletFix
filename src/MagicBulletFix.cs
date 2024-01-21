using System.Numerics;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace MagicBulletFix;

public enum FixMethod_t
{
    ALLOW = 0,
    IGNORE = 1,
    REFLECT = 2,
    REFLECT_SAFE = 3
}

public class MagicBulletFixConfig : BasePluginConfig
{
    [JsonPropertyName("ChatMessage")] public string ChatMessage { get; set; } = " \x02 Applied Magic Bullet penalty.";
    [JsonPropertyName("FixMethod")] public FixMethod_t FixMethod { get; set; } = FixMethod_t.IGNORE;
    [JsonPropertyName("ReflectScale")] public float ReflectScale { get; set; } = 1f;
}

public class CMagicBulletFix : BasePlugin, IPluginConfig<MagicBulletFixConfig>
{
    public override string ModuleName => "Magic Bullet Fix";

    public override string ModuleVersion => "1.0";

    public override string ModuleAuthor => "jon & sapphyrus";

    public override string ModuleDescription => "Blocks magic bullet with configurability";

    public MagicBulletFixConfig Config { get; set; }

    public HashSet<uint> magicBullets = new();
    public Dictionary<uint, int> magicBulletWarnings = new();

    public void OnConfigParsed(MagicBulletFixConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        Console.WriteLine("Loaded MagicBulletFix");

        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(h =>
        {
            var damageInfo = h.GetParam<CTakeDamageInfo>(1);
            

            if (magicBullets.Contains(damageInfo.Attacker.Index) && damageInfo.Attacker.Value != null)
            {
                switch (Config.FixMethod)
                {
                    case FixMethod_t.ALLOW:
                        break;
                    case FixMethod_t.IGNORE:
                        damageInfo.Damage = 0;
                        break;
                    case FixMethod_t.REFLECT:
                    case FixMethod_t.REFLECT_SAFE:
                        damageInfo.Damage *= Config.ReflectScale;
                        h.SetParam<CEntityInstance>(0, damageInfo.Attacker.Value);
                        if (Config.FixMethod == FixMethod_t.REFLECT_SAFE)
                            damageInfo.DamageFlags = (TakeDamageFlags_t)((int)damageInfo.DamageFlags | 8); //https://docs.cssharp.dev/api/CounterStrikeSharp.API.Core.TakeDamageFlags_t.html
                        break;
                }

                return HookResult.Changed;
            }

            return HookResult.Continue;
        }, HookMode.Pre);

        RegisterEventHandler<EventBulletFlightResolution>((evt, info) =>
        {
            if (evt.StartX == 0 && evt.StartY == 0 && evt.StartZ == 0)
            {
                if (magicBullets.Count == 0)
                {
                    Server.NextFrame(() =>
                    {
                        magicBullets.Clear();
                    });
                }

                magicBullets.Add(evt.Userid.Pawn.Index);

                if (!magicBulletWarnings.ContainsKey(evt.Userid.Pawn.Index) || Server.TickCount - magicBulletWarnings[evt.Userid.Pawn.Index] > Server.TickInterval * 5)
                {
                    magicBulletWarnings[evt.Userid.Pawn.Index] = Server.TickCount;
                    CCSPlayerController penaltyReceiver = evt.Userid;
                    Server.NextFrame(() =>
                    {
                        penaltyReceiver.PrintToChat(Config.ChatMessage);
                    });
                }

                return HookResult.Handled;
            }
            return HookResult.Continue;
        });
    }
}