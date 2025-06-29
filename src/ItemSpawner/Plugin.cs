using BepInEx;
using BepInEx.Logging;
using Photon.Pun;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zorro.Core.CLI;

namespace ItemSpawner;

[BepInAutoPlugin]
[ConsoleClassCustomizer("ItemSpawner")]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    internal static AssetBundle Bundle { get; set; } = null!;
    internal static KeyCode ToggleKey { get; set; } = KeyCode.F5;

    private void Awake()
    {
        Log = Logger;
        string AssetBundlePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "itemspawnerui");
        Bundle = AssetBundle.LoadFromFile(AssetBundlePath);

        ToggleKey = Config.Bind("General", "ToggleKey", KeyCode.F5, "The button to toggle the item spawner UI.").Value;

        On.GUIManager.Start += On_GUIManager_Start;

        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private void On_GUIManager_Start(On.GUIManager.orig_Start orig, GUIManager self)
    {
        orig(self);

        GameObject itemSpawnerUIPrefab = Bundle.LoadAsset<GameObject>("ItemSpawnerUI");

        if (self.GetComponentInChildren<ItemSpawnerWindow>() != null)
        {
            Log.LogWarning("ItemSpawner UI already exists, skipping instantiation.");
            return;
        }
        GameObject itemSpawnerUI = GameObject.Instantiate(itemSpawnerUIPrefab, self.transform);
        ItemSpawnerWindow spawnerWindow = itemSpawnerUI.AddComponent<ItemSpawnerWindow>();
        spawnerWindow.gameObject.SetActive(true);
    }

    [ConsoleCommand]
    public static void Spawn(Item item)
    {
        if (!PhotonNetwork.IsConnected || !Character.localCharacter)
        {
            return;
        }

        Debug.Log(string.Format("Spawn item: {0}", item));

        Character.localCharacter.refs.items.SpawnItemInHand(item.name);
    }
}