using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using Zorro.Core;
using UnityEngine.UI;

namespace ItemSpawner
{
    public class ItemSpawnerWindow : MenuWindow
    {
        public override bool openOnStart => false;
        public override bool selectOnOpen => false;
        public override bool closeOnPause => true;
        public override bool closeOnUICancel => true;
        public override bool autoHideOnClose => false;

        private Canvas canvas;

        void Awake()
        {
            canvas = GetComponentInChildren<Canvas>();
            canvas.gameObject.SetActive(false); // Start with the spawner UI hidden
        }

        public override void Initialize()
        {
            Transform itemEntryTemplate = canvas.transform.FindChildRecursive("ItemEntry");

            List<Item> items = ItemDatabase.Instance.Objects;

            foreach (Item item in items)
            {
                Transform itemEntry = GameObject.Instantiate(itemEntryTemplate, itemEntryTemplate.transform.parent);
                TextMeshProUGUI itemName = itemEntry.Find("ItemName").GetComponent<TextMeshProUGUI>();
                itemName.text = item.UIData.itemName;
                itemName.font = Resources.Load<TMP_FontAsset>("Font/DarumaDropOne-Regular SDF");
                itemEntry.Find("ItemIcon").GetComponent<UnityEngine.UI.RawImage>().texture = item.UIData.icon;
                itemEntry.GetComponent<Button>().onClick.AddListener(() => Plugin.Spawn(item));
                itemEntry.gameObject.SetActive(true);
            }
        }

        public void Toggle()
        {
            if (!base.isOpen)
            {
                this.Open();
                this.canvas.gameObject.SetActive(true);
                return;
            }
            base.Close();
        }
        public override void OnClose()
        {
            this.canvas.gameObject.SetActive(false);
        }
    }
}
