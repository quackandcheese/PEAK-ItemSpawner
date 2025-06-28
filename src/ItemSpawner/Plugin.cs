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
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    internal static AssetBundle Bundle { get; set; } = null!;
    internal static KeyCode ToggleKey { get; set; } = KeyCode.F4;

    private void Awake()
    {
        Log = Logger;
        string AssetBundlePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "itemspawnerui");
        Bundle = AssetBundle.LoadFromFile(AssetBundlePath);

        ToggleKey = Config.Bind("General", "ToggleKey", KeyCode.F4, "The button to toggle the item spawner UI.").Value;

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

        InputAction toggleAction;
        toggleAction = new InputAction(
            name: "ToggleItemSpawner",
            type: InputActionType.Button,
            binding: "<Keyboard>/" + ToggleKey.ToString().ToLower()
        );

        toggleAction.performed += ctx => spawnerWindow.Toggle();
        toggleAction.Enable();
    }

    public static void Spawn(Item item)
    {
        if (MainCamera.instance == null)
        {
            return;
        }
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        Transform transform = MainCamera.instance.transform;
        RaycastHit raycastHit;
        if (!Physics.Raycast(transform.position, transform.forward, out raycastHit, 10f, HelperFunctions.terrainMapMask))
        {
            raycastHit = default(RaycastHit);
        }
        Vector3 point = raycastHit.point + raycastHit.normal;

        Debug.Log(string.Format("Spawn item: {0} at {1}", item, point));
        
        Item spawnedItem = PhotonNetwork.Instantiate("0_Items/" + item.name, point, Quaternion.identity, 0, null).GetComponent<Item>();
        spawnedItem.view.RPC("RequestPickup", RpcTarget.MasterClient, Character.localCharacter.GetComponent<PhotonView>());
    }
}