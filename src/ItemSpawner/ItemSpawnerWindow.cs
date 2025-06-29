using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using Zorro.Core;
using UnityEngine.UI;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;
using System.Linq;

namespace ItemSpawner
{
    public class ItemSpawnerWindow : MenuWindow
    {
        public override bool openOnStart => false;
        public override bool selectOnOpen => false;
        public override bool closeOnPause => true;
        public override bool closeOnUICancel => true;
        public override bool autoHideOnClose => true;
        public override GameObject panel => canvas.gameObject;

        private Canvas canvas;
        private InputAction toggleAction;

        void Awake()
        {
            canvas = GetComponentInChildren<Canvas>();
            canvas.gameObject.SetActive(false); // Start with the spawner UI hidden
        }

        public override void Initialize()
        {
            Transform itemEntryTemplate = canvas.transform.FindChildRecursive("ItemEntry");
            TMP_FontAsset darumaFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().ToList().Find(x => x.name == "DarumaDropOne-Regular SDF");
            List<Item> items = ItemDatabase.Instance.Objects;

            foreach (Item item in items)
            {
                Transform itemEntry = GameObject.Instantiate(itemEntryTemplate, itemEntryTemplate.transform.parent);
                TextMeshProUGUI itemName = itemEntry.Find("ItemName").GetComponent<TextMeshProUGUI>();
                itemName.text = item.UIData.itemName;
                itemName.font = darumaFont;
                itemEntry.Find("ItemIcon").GetComponent<UnityEngine.UI.RawImage>().texture = item.UIData.icon;
                itemEntry.GetComponent<Button>().onClick.AddListener(() => Plugin.Spawn(item));
                itemEntry.gameObject.SetActive(true);
            }
        }

        public void Toggle(InputAction.CallbackContext _)
        {
            if (!base.isOpen)
            {
                this.Open();
                return;
            }
            base.Close();
        }

        void OnEnable()
        {
            toggleAction = new InputAction(
                name: "ToggleItemSpawner",
                type: InputActionType.Button,
                binding: "<Keyboard>/" + Plugin.ToggleKey.ToString().ToLower()
            );

            toggleAction.performed += Toggle;
            toggleAction.Enable();
        }

        void OnDisable()
        {
            toggleAction.Disable();
            toggleAction.performed -= Toggle;
            toggleAction.Dispose();
        }
    }
}
