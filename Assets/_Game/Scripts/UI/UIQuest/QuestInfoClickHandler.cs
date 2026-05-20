using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Xử lý click trên rich text location trong quest info
/// - Detect click trên location tag (VD: <color=#00FFFF>Làng Rễ Cây</color>)
/// - Extract location name
/// - Trigger QuestObjectiveManager để set waypoint
/// </summary>
public class QuestInfoClickHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questInfoText;
    private Canvas canvas;
    private int currentQuestId = -1;  // Track quest ID for location lookup

    private void Start()
    {
        if (questInfoText == null)
            questInfoText = GetComponent<TextMeshProUGUI>();

        // Tìm Canvas - cách 1: parent
        canvas = GetComponentInParent<Canvas>();
        
        // Cách 2: Nếu không có parent Canvas, tìm Canvas trong scene
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            Debug.Log($"[QuestInfoClickHandler] Canvas found in scene: {canvas?.name}");
        }

        if (canvas != null)
        {
            Debug.Log($"[QuestInfoClickHandler] Canvas found: {canvas.name}, RenderMode: {canvas.renderMode}");
            
            // Kiểm tra GraphicRaycaster
            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogWarning("[QuestInfoClickHandler] Canvas missing GraphicRaycaster! Adding one...");
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
        else
        {
            Debug.LogWarning("[QuestInfoClickHandler] No canvas found!");
        }
    }

    private void Update()
    {
        // Kiểm tra click trên text
        if (Input.GetMouseButtonDown(0))
        {
            // Cách 1: TMP Link detection
            HandleTextClick();
            
            // Cách 2 (Fallback): EventSystem Raycast - nếu cách 1 không work
            // DetectClickViaEventSystem();
        }
    }

    /// <summary>
    /// Alternative method: Detect click sử dụng UI EventSystem + Raycast
    /// </summary>
    private void DetectClickViaEventSystem()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
        {
            raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == questInfoText.gameObject)
                {
                    Debug.Log("[QuestInfoClickHandler] UI Raycast hit questInfoText!");
                    HandleTextClick();
                    break;
                }
            }
        }
    }

    private void HandleTextClick()
    {
        Debug.Log("[QuestInfoClickHandler] Checking for text click...");
        if (questInfoText == null)
        {
            Debug.LogWarning("[QuestInfoClickHandler] questInfoText is null!");
            return;
        }

        if (canvas == null)
        {
            Debug.LogWarning("[QuestInfoClickHandler] canvas is null!");
            return;
        }

        // Force rebuild text layout - IMPORTANT!
        questInfoText.ForceMeshUpdate();

        // Lấy position chuột
        Vector3 mousePos = Input.mousePosition;

        // Debug: Check text bounds
        Bounds textBounds = questInfoText.textBounds;
        Vector3 textWorldPos = questInfoText.transform.position;
        Debug.Log($"[QuestInfoClickHandler] Mouse Pos (Screen): {mousePos}");
        Debug.Log($"[QuestInfoClickHandler] Text Bounds: {textBounds}, World Pos: {textWorldPos}");
        Debug.Log($"[QuestInfoClickHandler] Canvas RenderMode: {canvas.renderMode}");
        Debug.Log($"[QuestInfoClickHandler] Text has {questInfoText.textInfo.linkCount} links, {questInfoText.textInfo.characterCount} characters");

        // Cách 1: Nếu Canvas là ScreenSpaceOverlay, convert screen pos → local UI pos
        int linkIndex = -1;
        Vector2 localClickPos = Vector2.zero;
        
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Convert screen position để match TextMeshPro world position
            // Tính toán local position của click
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                questInfoText.rectTransform,
                mousePos,
                null,  // Camera = null cho ScreenSpaceOverlay
                out localClickPos
            );

            Debug.Log($"[QuestInfoClickHandler] Local Click Pos: {localClickPos}");

            // Check xem click có nằm trong text bounds không
            if (questInfoText.textBounds.Contains(localClickPos))
            {
                Debug.Log("[QuestInfoClickHandler] Click is within text bounds!");
                
                // Find which CHARACTER was clicked
                int charIndex = TMP_TextUtilities.FindIntersectingCharacter(questInfoText, localClickPos, null, false);
                Debug.Log($"[QuestInfoClickHandler] Character index at click: {charIndex}");

                if (charIndex != -1)
                {
                    // Check if this character is part of a link
                    for (int i = 0; i < questInfoText.textInfo.linkCount; i++)
                    {
                        TMP_LinkInfo link = questInfoText.textInfo.linkInfo[i];
                        int linkCharStart = link.linkTextfirstCharacterIndex;
                        int linkCharEnd = linkCharStart + link.linkTextLength;
                        
                        Debug.Log($"[QuestInfoClickHandler] Link {i}: chars {linkCharStart}-{linkCharEnd}, ID: {link.GetLinkID()}");

                        // Check if clicked character is within this link's range
                        if (charIndex >= linkCharStart && charIndex < linkCharEnd)
                        {
                            Debug.Log($"[QuestInfoClickHandler] ✅ Character {charIndex} is within link {i}!");
                            linkIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    Debug.Log("[QuestInfoClickHandler] No character found at click position");
                }
            }
            else
            {
                Debug.Log("[QuestInfoClickHandler] Click is OUTSIDE text bounds!");
            }
        }
        else
        {
            // Cách 2: ScreenSpaceCamera hoặc WorldSpace
            Camera raycastCamera = canvas.worldCamera ?? Camera.main;
            if (raycastCamera == null)
            {
                Debug.LogWarning("[QuestInfoClickHandler] No camera found for raycast!");
                return;
            }
            
            linkIndex = TMP_TextUtilities.FindIntersectingLink(questInfoText, mousePos, raycastCamera);
        }

        Debug.Log($"[QuestInfoClickHandler] linkIndex: {linkIndex}");

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = questInfoText.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();
            string linkText = linkInfo.GetLinkText();

            Debug.Log($"[QuestInfoClickHandler] ✅ Clicked link: {linkID}, Text: {linkText}");

            // Nếu link ID là "objective", extract text và set objective
            if (linkID == "objective")
            {
                SetObjectiveFromText(linkText);
            }
        }
        else
        {
            Debug.Log("[QuestInfoClickHandler] No link found at click position");
            
            // FALLBACK: Nếu chỉ có 1 link và click trong text, auto-trigger nó
            if (questInfoText.textInfo.linkCount == 1 && questInfoText.textBounds.Contains(localClickPos))
            {
                Debug.Log("[QuestInfoClickHandler] FALLBACK: Only 1 link available, auto-triggering...");
                TMP_LinkInfo fallbackLink = questInfoText.textInfo.linkInfo[0];
                string fallbackText = fallbackLink.GetLinkText();
                Debug.Log($"[QuestInfoClickHandler] ✅ Auto-clicked link: {fallbackLink.GetLinkID()}, Text: {fallbackText}");
                SetObjectiveFromText(fallbackText);
            }
            else
            {
                // Debug: Log all available links
                int linkCount = questInfoText.textInfo.linkCount;
                if (linkCount > 0)
                {
                    Debug.Log($"[QuestInfoClickHandler] Available links:");
                    for (int i = 0; i < linkCount; i++)
                    {
                        TMP_LinkInfo link = questInfoText.textInfo.linkInfo[i];
                        Debug.Log($"  [{i}] ID: {link.GetLinkID()}, Text: {link.GetLinkText()}");
                    }
                }
            }
        }
    }

    private void SetObjectiveFromText(string locationName)
    {
        if (QuestObjectiveManager.Instance != null)
        {
            Debug.Log($"[QuestInfoClickHandler] ✅ Setting objective: {locationName} for Quest {currentQuestId}");
            QuestObjectiveManager.Instance.SetObjective(currentQuestId, locationName);
        }
        else
        {
            Debug.LogWarning("[QuestInfoClickHandler] QuestObjectiveManager not found!");
        }
    }

    /// <summary>
    /// Utility: Format location text để clickable
    /// Usage: FormatLocationForClick("Làng Rễ Cây")
    /// Output: <link="objective">Làng Rễ Cây</link>
    /// </summary>
    public static string FormatLocationForClick(string locationName)
    {
        return $"<link=\"objective\">{locationName}</link>";
    }

    /// <summary>
    /// Convert quest text với color tags sang format có link tags
    /// VD: "Đi tới <color=#00FFFF>Làng Rễ Cây</color>"
    /// → "Đi tới <link="objective"><color=#00FFFF>Làng Rễ Cây</color></link>"
    /// </summary>
    public static string ConvertQuestTextToClickable(string questText)
    {
        if (string.IsNullOrEmpty(questText))
            return questText;

        Debug.Log($"[QuestInfoClickHandler] Original text: {questText}");

        // Detect color tags pattern: <color=...>text</color>
        // Replace with: <link="objective"><color=...>text</color></link>
        string pattern = @"<color=([^>]*)>([^<]*)</color>";
        string replacement = @"<link=""objective""><color=$1>$2</color></link>";
        string result = System.Text.RegularExpressions.Regex.Replace(questText, pattern, replacement);
        
        Debug.Log($"[QuestInfoClickHandler] Converted text: {result}");
        
        return result;
    }

    /// <summary>
    /// Set current quest ID - called by UIQuestPanel when displaying quest
    /// </summary>
    public void SetCurrentQuestId(int questId)
    {
        currentQuestId = questId;
        Debug.Log($"[QuestInfoClickHandler] Current Quest ID set to: {questId}");
    }
}
