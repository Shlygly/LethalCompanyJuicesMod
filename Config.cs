using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Netcode;

namespace JuicesMod
{
    class Config : SyncedInstance<Config>
    {
        public readonly ConfigEntry<int> cartonsRarity;
        public readonly ConfigEntry<int> premiumsRarity;

        public Config(ConfigFile configFile)
        {
            InitInstance(this);

            configFile.SaveOnConfigSet = false;

            cartonsRarity = configFile.Bind(
                "Items.Rarity",
                "CartonsRarity",
                60,
                "How likely cartons juices spawn (between 1 and 100)"
            );
            premiumsRarity = configFile.Bind(
                "Items.Rarity",
                "PremiumRarity",
                40,
                "How likely premiums juices spawn (between 1 and 100)"
            );

            ClearOrphanedEntries(configFile);
            configFile.Save();
            configFile.SaveOnConfigSet = true;
        }

        static void ClearOrphanedEntries(ConfigFile configFile)
        {
            PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile);
            orphanedEntries.Clear();
        }

        public static void RequestSync()
        {
            if (!IsClient) return;

            using FastBufferWriter stream = new(IntSize, Allocator.Temp);
            MessageManager.SendNamedMessage("ModName_OnRequestConfigSync", 0uL, stream);
        }

        public static void OnRequestSync(ulong clientId, FastBufferReader _)
        {
            if (!IsHost) return;

            Plugin.Logger.LogInfo($"Config sync request received from client: {clientId}");

            byte[] array = SerializeToBytes(Instance);
            int value = array.Length;

            using FastBufferWriter stream = new(value + IntSize, Allocator.Temp);

            try
            {
                stream.WriteValueSafe(in value, default);
                stream.WriteBytesSafe(array);

                MessageManager.SendNamedMessage("ModName_OnReceiveConfigSync", clientId, stream);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogInfo($"Error occurred syncing config with client: {clientId}\n{e}");
            }
        }

        public static void OnReceiveSync(ulong _, FastBufferReader reader)
        {
            if (!reader.TryBeginRead(IntSize))
            {
                Plugin.Logger.LogError("Config sync error: Could not begin reading buffer.");
                return;
            }

            reader.ReadValueSafe(out int val, default);
            if (!reader.TryBeginRead(val))
            {
                Plugin.Logger.LogError("Config sync error: Host could not sync.");
                return;
            }

            byte[] data = new byte[val];
            reader.ReadBytesSafe(ref data, val);

            SyncInstance(data);

            Plugin.Logger.LogInfo("Successfully synced config with host.");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        // [HarmonyPatch(typeof(GameNetworkManager), "SteamMatchmaking_OnLobbyMemberJoined")]
        public static void InitializeLocalPlayer()
        {
            if (IsHost)
            {
                MessageManager.RegisterNamedMessageHandler($"{MyPluginInfo.PLUGIN_NAME}_OnRequestConfigSync", OnRequestSync);
                Synced = true;

                return;
            }

            Synced = false;
            MessageManager.RegisterNamedMessageHandler($"{MyPluginInfo.PLUGIN_NAME}_OnReceiveConfigSync", OnReceiveSync);
            RequestSync();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        public static void PlayerLeave()
        {
            RevertSync();
        }
    }
}
