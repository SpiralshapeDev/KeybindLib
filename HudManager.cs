using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Rewired;
using Thor;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KeybindLib
{
    internal class HudManager
    {
        private static PopupManager popupManager => Game.Instance.PopupManager;
        private static Transform optionsPopup => popupManager?.gameObject.transform.Find("OptionsPopup(Clone)");
        private static UIButton defaultsButton => optionsPopup.Find("Input/DefaultsButton")?.GetComponent<UIButton>();
        private static EventTrigger defaultsButtonOriginalTrigger = null;
        private static TextMeshProUGUI modKeybindsButtonText => modKeybindsButton?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
        private static readonly Dictionary<(string, string), Button> modKeybindsPageKeyButtons = new Dictionary<(string, string), Button>();
        private static UIButton modKeybindsButton = null;
        private static GameObject modKeybindsPage = null;

        [HarmonyPatch(typeof(HUD))]
        [HarmonyPatch("Process")]
        [HarmonyPostfix]
        internal static void Update()
        {
            if (popupManager == null)
            {
                modKeybindsButton = null;
                modKeybindsPage = null;
                defaultsButtonOriginalTrigger = null;
                return;
            }
            if (optionsPopup == null) return;
            if (defaultsButton == null) return;
            if (defaultsButtonOriginalTrigger == null) {
                defaultsButtonOriginalTrigger = defaultsButton.GetComponent<EventTrigger>();
            }

            if (modKeybindsButton == null)
            {
                modKeybindsButton = Object.Instantiate<UIButton>(defaultsButton, defaultsButton.transform.parent, true);

                // Account for other mods adding buttons
                Vector3 buttonOffset = new Vector3(-200, 0, 0);
                modKeybindsButton.transform.localPosition -= buttonOffset;
                int buttonCount = optionsPopup.Find("Input").transform
                    .Cast<Transform>()
                    .Count(child => child.name.Contains("Button"));
                modKeybindsButton.transform.localPosition += buttonOffset * (buttonCount - 1);

                modKeybindsButton.transform.gameObject.name = "ModKeybindsButton";
                Object.Destroy(modKeybindsButton.GetComponent<EventTrigger>());

                EventTrigger configButtonTrigger = modKeybindsButton.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry configButtonEntry = new EventTrigger.Entry {
                    eventID = EventTriggerType.PointerClick
                };
                configButtonEntry.callback.AddListener(data => {
                    defaultsButtonOriginalTrigger.enabled = !defaultsButtonOriginalTrigger.enabled;
                    ToggleConfigUI();
                });
                configButtonTrigger.triggers.Add(configButtonEntry);

                EventTrigger defaultButtonTrigger = defaultsButton.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry defaultButtonEntry= new EventTrigger.Entry {
                    eventID = EventTriggerType.PointerClick
                };
                defaultButtonEntry.callback.AddListener(data => {
                    GameObject textContainer = optionsPopup.Find("Input/TextContainer")?.gameObject;
                    if (textContainer == null) return;
                    if (modKeybindsPage == null) return;
                    textContainer.SetActive(!modKeybindsPage.activeSelf);

                    UpdateButtons();
                });

                defaultButtonTrigger.triggers.Add(defaultButtonEntry);
                return;
            }
            modKeybindsButtonText.text = modKeybindsButtonText.text == "Defaults" ? "Mod Keybinds" : modKeybindsButtonText.text;
        }

        private static void ToggleConfigUI()
        {
            if (optionsPopup == null) return;
            if (optionsPopup.Find("Input") == null) return;

            GameObject textContainer = optionsPopup.Find("Input/TextContainer")?.gameObject;
            GameObject actionLayoutGroup = optionsPopup.Find("Input/ActionLayoutGroup")?.gameObject;
            GameObject ControllerButtonGroup = optionsPopup.Find("Input/Header/ControllerButtonGroup")?.gameObject;
            if (textContainer == null) return;
            if (actionLayoutGroup == null) return;
            if (ControllerButtonGroup == null) return;
            textContainer.SetActive(!textContainer.activeSelf);
            actionLayoutGroup.SetActive(!actionLayoutGroup.activeSelf);
            ControllerButtonGroup.SetActive(!ControllerButtonGroup.activeSelf);

            if (modKeybindsPage != null)
            {
                modKeybindsPage.SetActive(!modKeybindsPage.activeSelf);
                if (modKeybindsPage.activeSelf) UpdateButtons();
                return;
            }
            modKeybindsPage = new GameObject("ModKeybindsPage")
            {
                layer = LayerMask.NameToLayer("UI")
            };
            modKeybindsPage.transform.position += new Vector3(0,-9,0);
            modKeybindsPage.transform.SetParent(optionsPopup.Find("Input").transform, false);

            RectTransform modConfigPageRect = modKeybindsPage.GetComponent<RectTransform>() ?? modKeybindsPage.AddComponent<RectTransform>();
            modConfigPageRect.sizeDelta = new Vector2(1000, 600);
            modConfigPageRect.localPosition = Vector3.zero;

            GameObject modConfigPageViewport = new GameObject("Viewport");
            RectTransform modConfigPageViewportRect = modConfigPageViewport.GetComponent<RectTransform>() ?? modConfigPageViewport.AddComponent<RectTransform>();
            modConfigPageViewportRect.sizeDelta = modConfigPageRect.sizeDelta;
            modConfigPageViewport.transform.SetParent(modKeybindsPage.transform, false);

            GameObject modConfigPageContentBox = new GameObject("Content");
            RectTransform modConfigPageContentBoxRect = modConfigPageContentBox.GetComponent<RectTransform>() ?? modConfigPageContentBox.AddComponent<RectTransform>();
            modConfigPageContentBoxRect.sizeDelta = modConfigPageRect.sizeDelta;
            modConfigPageContentBox.transform.SetParent(modConfigPageViewport.transform, false);

            VerticalLayoutGroup modConfigPageContentBoxLayout = modConfigPageContentBox.AddComponent<VerticalLayoutGroup>();
            modConfigPageContentBoxLayout.childForceExpandWidth = true;
            modConfigPageContentBoxLayout.childForceExpandHeight = false;
            modConfigPageContentBoxLayout.spacing = 20;
            modConfigPageContentBoxLayout.padding = new RectOffset(10, 10, 10, 10);

            // Fix raycasting
            Image modConfigPageContentBoxImage = modConfigPageContentBox.AddComponent<Image>();
            modConfigPageContentBoxImage.color = Color.clear;
            modConfigPageContentBoxImage.raycastTarget = true;

            GameObject modConfigPageScrollbarBox = new GameObject("Scrollbar");
            RectTransform modConfigPageScrollbarBoxRect = modConfigPageScrollbarBox.AddComponent<RectTransform>();
            modConfigPageScrollbarBoxRect.SetParent(modKeybindsPage.transform, false);
            modConfigPageScrollbarBoxRect.anchorMin = new Vector2(1, 0);
            modConfigPageScrollbarBoxRect.anchorMax = new Vector2(1, 1);
            modConfigPageScrollbarBoxRect.pivot = new Vector2(1, 0.5f);
            modConfigPageScrollbarBoxRect.sizeDelta = new Vector2(8, 0);
            modConfigPageScrollbarBoxRect.anchoredPosition = new Vector2(-10, 0);
            modConfigPageScrollbarBoxRect.localPosition += new Vector3(25,0,0);

            Image modConfigPageScrollbarBackground = modConfigPageScrollbarBox.AddComponent<Image>();
            modConfigPageScrollbarBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

            GameObject modConfigPageScrollbarHandleBox = new GameObject("Handle");
            RectTransform modConfigPageScrollbarHandleRect = modConfigPageScrollbarHandleBox.AddComponent<RectTransform>();
            modConfigPageScrollbarHandleRect.SetParent(modConfigPageScrollbarBoxRect, false);
            modConfigPageScrollbarHandleRect.anchorMin = Vector2.zero;
            modConfigPageScrollbarHandleRect.anchorMax = new Vector2(1, 1);
            modConfigPageScrollbarHandleRect.offsetMin = Vector2.zero;
            modConfigPageScrollbarHandleRect.offsetMax = Vector2.zero;

            Image modConfigPageScrollbarHandleBackground = modConfigPageScrollbarHandleBox.AddComponent<Image>();
            modConfigPageScrollbarHandleBackground.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);

            Scrollbar modConfigPageScrollbar = modConfigPageScrollbarBox.AddComponent<Scrollbar>();
            modConfigPageScrollbar.handleRect = modConfigPageScrollbarHandleRect;
            modConfigPageScrollbar.direction = Scrollbar.Direction.BottomToTop;

            ScrollRect modConfigPageScrollRect = modKeybindsPage.AddComponent<ScrollRect>();
            modConfigPageScrollRect.viewport = modConfigPageViewport.GetComponent<RectTransform>();
            modConfigPageScrollRect.content = modConfigPageContentBox.GetComponent<RectTransform>();
            modConfigPageScrollRect.vertical = true;
            modConfigPageScrollRect.horizontal = false;
            modConfigPageScrollRect.verticalScrollbar = modConfigPageScrollbar;
            modConfigPageScrollRect.scrollSensitivity = 20f;
            modConfigPageScrollRect.elasticity = 0;

            modConfigPageViewport.AddComponent<RectMask2D>();

            Image standardKeybindContentButtonImage = optionsPopup.Find("Input/ActionLayoutGroup")?.gameObject.transform.GetChild(0)?.Find("Layout/InputButtonTemplate/Background")?.GetComponent<Image>();
            if (standardKeybindContentButtonImage == null)
            {
                Debug.LogError($"{KeybindLibBase.modGUID}: Failed to find background image for default InputButton!");
                return;
            }
            Material standardKeybindContentTextMaterial = optionsPopup.Find("Input/ActionLayoutGroup")?.gameObject.transform.GetChild(0)?.Find("NameText")?.GetComponent<TextMeshProUGUI>().fontMaterial;
            if (standardKeybindContentTextMaterial == null)
            {
                Debug.LogError($"{KeybindLibBase.modGUID}: Failed to find background image for default InputButton font material!");
                return;
            }
            Dictionary<string,List<string>> activeKeys = KeybindManager.GetAllActiveKeys();
            if (activeKeys == null || activeKeys.Count == 0)
            {
                Debug.LogWarning($"{KeybindLibBase.modGUID}: Failed to find any active mod keybinds");

                GameObject noKeybindHeader = new GameObject($"No-Keybinds-Header");
                TextMeshProUGUI noKeybindHeaderText = noKeybindHeader.AddComponent<TextMeshProUGUI>();
                noKeybindHeaderText.text = $"No custom mod keybinds are loaded.";
                noKeybindHeaderText.fontSize = 40f;
                noKeybindHeaderText.fontMaterial = standardKeybindContentTextMaterial;
                RectTransform modTitleHeaderRect = noKeybindHeader.GetComponent<RectTransform>() ?? noKeybindHeader.AddComponent<RectTransform>();
                modTitleHeaderRect.sizeDelta = new Vector2(modConfigPageRect.sizeDelta.x/2, 50);
                noKeybindHeader.transform.SetParent(modConfigPageContentBox.transform, false);
                return;
            }

            int totalHeight = activeKeys.Keys.Count * 120;
            foreach (var activeKey in activeKeys)
            {
                totalHeight += activeKey.Value.Count * 25;
            }
            modConfigPageContentBoxRect.sizeDelta = new Vector2(1000, totalHeight + 200);

            LayoutElement modConfigPageContentBoxLayoutElement = modConfigPageContentBox.AddComponent<LayoutElement>();
            modConfigPageContentBoxLayoutElement.preferredHeight = totalHeight + 200;

            foreach (string modId in activeKeys.Keys)
            {
                GameObject modTitleHeader = new GameObject($"Title-Header {modId}");
                TextMeshProUGUI modTitleHeaderText = modTitleHeader.AddComponent<TextMeshProUGUI>();
                modTitleHeaderText.text = $"# {modId}";
                modTitleHeaderText.fontSize = 40f;
                modTitleHeaderText.fontMaterial = standardKeybindContentTextMaterial;
                RectTransform modTitleHeaderRect = modTitleHeader.GetComponent<RectTransform>() ?? modTitleHeader.AddComponent<RectTransform>();
                modTitleHeaderRect.sizeDelta = new Vector2(modConfigPageRect.sizeDelta.x/2, 50);
                modTitleHeader.transform.SetParent(modConfigPageContentBox.transform, false);

                List<string> modKeybinds = activeKeys[modId];

                GameObject modKeybindBox = new GameObject($"Content {modId}");
                modKeybindBox.transform.SetParent(modConfigPageContentBox.transform, false);
                RectTransform modKeybindBoxRect = modKeybindBox.GetComponent<RectTransform>() ?? modKeybindBox.AddComponent<RectTransform>();
                modKeybindBoxRect.sizeDelta = new Vector2(1000, 60 * modKeybinds.Count + 70);

                GridLayoutGroup modKeybindBoxGrid = modKeybindBox.AddComponent<GridLayoutGroup>();
                modKeybindBoxGrid.constraintCount = 2;
                modKeybindBoxGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                modKeybindBoxGrid.cellSize = new Vector2(modConfigPageRect.sizeDelta.x/2, 50);
                modKeybindBoxGrid.spacing = new Vector2(10, 10);

                foreach (var keybindId in modKeybinds)
                {
                    GameObject keybindContentBox = new GameObject($"KeybindContent {modId}");
                    keybindContentBox.transform.SetParent(modKeybindBox.transform, false);
                    RectTransform keybindContentBoxRect = keybindContentBox.GetComponent<RectTransform>() ?? keybindContentBox.AddComponent<RectTransform>();
                    keybindContentBoxRect.sizeDelta = modKeybindBoxGrid.cellSize;

                    GameObject keybindContentNameBox = new GameObject($"NameText");
                    TextMeshProUGUI keybindBoxContentNameText = keybindContentNameBox.AddComponent<TextMeshProUGUI>();
                    keybindBoxContentNameText.text = KeybindManager.GetDisplayName(modId, keybindId);
                    keybindBoxContentNameText.fontSize = 28f;
                    keybindBoxContentNameText.fontMaterial = standardKeybindContentTextMaterial;
                    RectTransform keyIdRect = keybindContentNameBox.GetComponent<RectTransform>() ?? keybindContentNameBox.AddComponent<RectTransform>();
                    keyIdRect.sizeDelta = new Vector2(200, 50);
                    keybindContentNameBox.transform.SetParent(keybindContentBox.transform, false);

                    HorizontalLayoutGroup keybindContentBoxHLG = keybindContentBox.AddComponent<HorizontalLayoutGroup>();
                    keybindContentBoxHLG.spacing = 50f;
                    keybindContentBoxHLG.childForceExpandWidth = true;
                    keybindContentBoxHLG.childForceExpandHeight = false;
                    keybindContentBoxHLG.padding = new RectOffset(0, 20, 0, 0);

                    LayoutElement keybindContentNameBoxLayout = keybindContentNameBox.AddComponent<LayoutElement>();
                    keybindContentNameBoxLayout.flexibleWidth = 0;

                    GameObject spacer = new GameObject("Spacer");
                    spacer.transform.SetParent(keybindContentBox.transform, false);
                    LayoutElement spacerLayout = spacer.AddComponent<LayoutElement>();
                    spacerLayout.preferredWidth = -1;

                    GameObject keybindContentButtonBox = new GameObject($"Button");
                    keybindContentButtonBox.transform.SetParent(keybindContentBox.transform, false);

                    Button keybindContentButton = keybindContentButtonBox.AddComponent<Button>();
                    RectTransform keybindContentButtonRect = keybindContentButtonBox.GetComponent<RectTransform>() ?? keybindContentButtonBox.AddComponent<RectTransform>();
                    keybindContentButtonRect.sizeDelta = new Vector2(125, 50);
                    modKeybindsPageKeyButtons[(modId,keybindId)] = keybindContentButton;

                    LayoutElement keybindContentButtonBoxLayout = keybindContentButtonBox.AddComponent<LayoutElement>();
                    keybindContentButtonBoxLayout.preferredWidth = keybindContentButtonRect.sizeDelta.x;
                    keybindContentButtonBoxLayout.preferredHeight = keybindContentButtonRect.sizeDelta.y;
                    keybindContentButtonBox.transform.localPosition += new Vector3(0,10,0);

                    Image keybindContentButtonImage = Object.Instantiate(standardKeybindContentButtonImage, keybindContentButtonBox.transform, false);
                    keybindContentButtonImage.name = "Background";
                    RectTransform keybindContentButtonImageRect = keybindContentButtonImage.GetComponent<RectTransform>();
                    keybindContentButtonImageRect.anchorMin = Vector2.zero;
                    keybindContentButtonImageRect.anchorMax = Vector2.one;
                    keybindContentButtonImageRect.offsetMin = Vector2.zero;
                    keybindContentButtonImageRect.offsetMax = Vector2.zero;
                    keybindContentButtonImage.raycastTarget = true;
                    keybindContentButtonImage.transform.SetAsFirstSibling();

                    GameObject keybindContentButtonTextBox = new GameObject("Text");
                    keybindContentButtonTextBox.transform.SetParent(keybindContentButtonBox.transform, false);
                    keybindContentButtonTextBox.transform.SetAsLastSibling();

                    RectTransform keybindContentButtonTextRect = keybindContentButtonTextBox.AddComponent<RectTransform>();
                    keybindContentButtonTextRect.anchorMin = Vector2.zero;
                    keybindContentButtonTextRect.anchorMax = Vector2.one;
                    keybindContentButtonTextRect.offsetMin = Vector2.zero;
                    keybindContentButtonTextRect.offsetMax = Vector2.zero;

                    TextMeshProUGUI keybindContentButtonText = keybindContentButtonTextBox.AddComponent<TextMeshProUGUI>();
                    keybindContentButtonText.text = KeybindManager.GetKey(modId, keybindId).ToString();
                    keybindContentButtonText.alignment = TextAlignmentOptions.Center;
                    keybindContentButtonText.fontMaterial = standardKeybindContentTextMaterial;
                    keybindContentButtonText.color = Color.white;
                    keybindContentButtonText.raycastTarget = false;
                    keybindContentButtonText.fontSize = 24f;

                    keybindContentButton.targetGraphic = keybindContentButtonImage;

                    string capturedModId = modId;
                    string capturedKeybindId = keybindId;
                    keybindContentButton.onClick.AddListener(() =>
                    {
                        float fontSize = keybindContentButtonText.fontSize;
                        keybindContentButtonText.text = "Waiting for input...";
                        keybindContentButtonText.fontSize = 18f;
                        KeybindLibBase.Instance.StartCoroutine(WaitForKeyInput(capturedKey =>
                        {
                            if (capturedKey == KeyCode.None || capturedKey == KeyCode.Escape) return;
                            KeybindManager.SetKey(capturedModId, capturedKeybindId, capturedKey);
                            keybindContentButtonText.text = KeybindManager.GetKey(capturedModId, capturedKeybindId).ToString();
                            keybindContentButtonText.fontSize = fontSize;
                        }));
                    });
                }
            }
            modConfigPageScrollRect.verticalNormalizedPosition = 1f;
        }

        private static IEnumerator WaitForKeyInput(System.Action<KeyCode> onKeyReceived)
        {
            while (true)
            {
                foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (ReInput.controllers.Keyboard.GetKeyDown(key))
                    {
                        onKeyReceived?.Invoke(key);
                        yield break;
                    }
                    if (modKeybindsPage) continue;

                    onKeyReceived?.Invoke(KeyCode.None);
                    yield break;
                }
                yield return null;
            }
        }

        private static void UpdateButtons()
        {
            if (!modKeybindsButton) return;
            foreach (var (modId, keyId) in modKeybindsPageKeyButtons.Keys)
            {
                Button button = modKeybindsPageKeyButtons[(modId, keyId)];
                button.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = KeybindManager.GetKey(modId, keyId).ToString();
            }
        }
    }
}