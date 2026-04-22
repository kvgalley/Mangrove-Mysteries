using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Trash count only increments when the player collects a trash Collectable in the world.
/// </summary>
public class HUDBuilder : MonoBehaviour
{
    public static HUDBuilder Instance { get; private set; }

    int wildlifePts = 50;
    // Default per-trash display value shown in the score-panel subtitle.
    // Actual pts per pickup come from each Collectable's pointValue.
    int trashPtsDisplay = 10;

    string[] wildlifeNames = {
        "Manatee", "Horse Shoe Crab",
        "Egret", "White Ibis", "Turtle",
        "Mangrove tree", "Coconut palm"
    };

    // true = fauna, false = flora
    bool[] isFauna = { true, true, true, true, true, false, false };

    int _spotted;
    int _trash;
    int _trashPoints;
    Coroutine _notifyRoutine;

    // Built UI references
    TextMeshProUGUI _scoreTMP;
    TextMeshProUGUI _wildlifeBreakTMP;
    TextMeshProUGUI _trashBreakTMP;
    TextMeshProUGUI _trashCountTMP;
    TextMeshProUGUI _notifyTMP;
    TextMeshProUGUI _interactHintTMP;
    TextMeshProUGUI _wlProgressTMP;
    CanvasGroup     _notifyGroup;
    GameObject      _interactHintRoot;

    class WildlifeRow
    {
        public string name;
        public bool   spotted;
        public Image  checkbox;
        public TextMeshProUGUI label;
    }
    List<WildlifeRow> _rows = new List<WildlifeRow>();

    // Colors
    // Panels
    Color panelBg        = new Color(0.02f, 0.10f, 0.08f, 0.82f);
    Color panelBorder    = new Color(1f, 1f, 1f, 0.35f);

    // Text
    Color textPrimary    = new Color(1.00f, 1.00f, 1.00f, 1.00f);   
    Color textMuted      = new Color(0.85f, 0.95f, 0.90f, 1.00f);  
    Color textDim        = new Color(0.75f, 0.90f, 0.82f, 1.00f);  

    // Accents
    Color green          = new Color(0.20f, 0.90f, 0.45f, 1f);
    Color salmon         = new Color(1.00f, 0.55f, 0.45f, 1f);
    Color darkRed = new Color(0.55f, 0.08f, 0.08f, 1f);
    Color checkUnchecked = new Color(0.18f, 0.70f, 0.54f, 0.40f);

    // Tags
    Color faunaTag       = new Color(0.22f, 0.63f, 0.86f, 0.35f);
    Color floraTag       = new Color(0.31f, 0.75f, 0.47f, 0.35f);
    Color faunaText      = new Color(0.75f, 0.92f, 1.00f, 1.00f);   // solid light blue
    Color floraText      = new Color(0.75f, 1.00f, 0.82f, 1.00f);   // solid light green

    // Notification
    Color notifyBorder   = new Color(0.20f, 0.90f, 0.45f, 0.50f);

    // Trash
    Color trashColor     = new Color(0.88f, 0.44f, 0.35f, 0.30f);

    // Spotted/struck row text
    Color textStruck     = new Color(0.65f, 0.80f, 0.72f, 0.70f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        BuildHUD();
        RefreshScore();
        HideNotify();
        SetInteractHint("", false);
    }

    //  BUILD

    void BuildHUD()
    {
        // Canvas
        var canvasGO = new GameObject("HUD_Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Score panel (top right)
        GameObject scorePanel = MakePanel(canvasGO, "ScorePanel", new Vector2(400, 200), new Vector2(1,1), new Vector2(-20,-20));

        TextMeshProUGUI scoreLabel = MakeSimpleText(scorePanel, "ScoreLabel", "SCORE", 20, textMuted);
        RectTransform slRT = scoreLabel.GetComponent<RectTransform>();
        slRT.anchorMin = new Vector2(0,1); slRT.anchorMax = new Vector2(1,1);
        slRT.pivot     = new Vector2(0.5f,1);
        slRT.sizeDelta = new Vector2(0, 36);
        slRT.anchoredPosition = new Vector2(0, -16);
        scoreLabel.alignment = TextAlignmentOptions.Center;

        _scoreTMP = MakeSimpleText(scorePanel, "ScoreValue", "0", 56, textPrimary);
        RectTransform svRT = _scoreTMP.GetComponent<RectTransform>();
        svRT.anchorMin = new Vector2(0,1); svRT.anchorMax = new Vector2(1,1);
        svRT.pivot     = new Vector2(0.5f,1);
        svRT.sizeDelta = new Vector2(0, 72);
        svRT.anchoredPosition = new Vector2(0, -52);
        _scoreTMP.alignment = TextAlignmentOptions.Center;
        _scoreTMP.fontStyle = FontStyles.Bold;

        _wildlifeBreakTMP = MakeSimpleText(scorePanel, "WildlifeBreak", "Wildlife: 50 points", 20, textDim);
        RectTransform wbRT = _wildlifeBreakTMP.GetComponent<RectTransform>();
        wbRT.anchorMin = new Vector2(0,1); wbRT.anchorMax = new Vector2(1,1);
        wbRT.pivot     = new Vector2(0.5f,1);
        wbRT.sizeDelta = new Vector2(-32, 32);
        wbRT.anchoredPosition = new Vector2(0, -128);
        _wildlifeBreakTMP.alignment = TextAlignmentOptions.Center;

        _trashBreakTMP = MakeSimpleText(scorePanel, "TrashBreak", $"Trash: {trashPtsDisplay} points", 20, textDim);
        RectTransform tbRT = _trashBreakTMP.GetComponent<RectTransform>();
        tbRT.anchorMin = new Vector2(0,1); tbRT.anchorMax = new Vector2(1,1);
        tbRT.pivot     = new Vector2(0.5f,1);
        tbRT.sizeDelta = new Vector2(-32, 32);
        tbRT.anchoredPosition = new Vector2(0, -160);
        _trashBreakTMP.alignment = TextAlignmentOptions.Center;

        // Trash panel (top right, below score)
        GameObject trashPanel = MakePanel(canvasGO, "TrashPanel", new Vector2(400, 116), new Vector2(1,1), new Vector2(-20,-216));
        trashPanel.GetComponent<Image>().color = new Color(trashColor.r, trashColor.g, trashColor.b, 0.35f);
        BuildTrashIcon(trashPanel);

        _trashCountTMP = MakeSimpleText(trashPanel, "TrashCount", "0", 50, darkRed);
        RectTransform tcRT = _trashCountTMP.GetComponent<RectTransform>();
        tcRT.anchorMin = new Vector2(0.4f,0); tcRT.anchorMax = new Vector2(1,1);
        tcRT.offsetMin = new Vector2(0,16);   tcRT.offsetMax = new Vector2(-20,-16);
        _trashCountTMP.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI trashLabel = MakeSimpleText(trashPanel, "TrashLabel", "Trash collected", 25, new Color(darkRed.r, darkRed.g, darkRed.b, 1f));
        RectTransform tlRT = trashLabel.GetComponent<RectTransform>();
        tlRT.anchorMin = new Vector2(0.4f,0); tlRT.anchorMax = new Vector2(1,0);
        tlRT.pivot     = new Vector2(0.5f,0);
        tlRT.sizeDelta = new Vector2(0,28);
        tlRT.anchoredPosition = new Vector2(0, 16);
        trashLabel.alignment = TextAlignmentOptions.Center;

        // Wildlife panel (bottom left)
        float rowH    = 56f;
        float headerH = 60f;
        float panelH  = headerH + (rowH * wildlifeNames.Length) + 24f;
        GameObject wlPanel = MakePanel(canvasGO, "WildlifePanel", new Vector2(400, panelH), new Vector2(0,0), new Vector2(20,20));

        TextMeshProUGUI wlHeader = MakeSimpleText(wlPanel, "WLHeader", "Wildlife spotted", 20, textMuted);
        RectTransform whRT = wlHeader.GetComponent<RectTransform>();
        whRT.anchorMin = new Vector2(0,1); whRT.anchorMax = new Vector2(0.65f,1);
        whRT.pivot     = new Vector2(0,1);
        whRT.sizeDelta = new Vector2(0,40);
        whRT.anchoredPosition = new Vector2(20,-16);

        _wlProgressTMP = MakeSimpleText(wlPanel, "WLProgress", "0 / " + wildlifeNames.Length, 20, green);
        RectTransform wpRT = _wlProgressTMP.GetComponent<RectTransform>();
        wpRT.anchorMin = new Vector2(0.65f,1); wpRT.anchorMax = new Vector2(1,1);
        wpRT.pivot     = new Vector2(1,1);
        wpRT.sizeDelta = new Vector2(0,40);
        wpRT.anchoredPosition = new Vector2(-20,-16);
        _wlProgressTMP.alignment = TextAlignmentOptions.Right;

        // Divider
        Image divImg = MakeImage(wlPanel, "Divider", new Color(1,1,1,0.2f));
        RectTransform drt = divImg.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0,1); drt.anchorMax = new Vector2(1,1);
        drt.offsetMin = new Vector2(16,-headerH); drt.offsetMax = new Vector2(-16,-headerH+2);

        for (int i = 0; i < wildlifeNames.Length; i++)
            BuildWildlifeRow(wlPanel, i, rowH, headerH);

        // Interact hint (center)
        _interactHintRoot = MakePanel(canvasGO, "InteractHint", new Vector2(480,60), new Vector2(0.5f,0.5f), new Vector2(0,-80));
        _interactHintTMP  = MakeSimpleText(_interactHintRoot, "HintTMP", "", 22, textPrimary);
        RectTransform ihRT = _interactHintTMP.GetComponent<RectTransform>();
        ihRT.anchorMin = Vector2.zero; ihRT.anchorMax = Vector2.one;
        ihRT.offsetMin = Vector2.zero; ihRT.offsetMax = Vector2.zero;
        _interactHintTMP.alignment = TextAlignmentOptions.Center;

        // Notification (bottom center)
        GameObject notifyGO = MakePanel(canvasGO, "Notification", new Vector2(560,64), new Vector2(0.5f,0), new Vector2(0,240));
        notifyGO.GetComponent<Image>().color = new Color(panelBg.r, panelBg.g, panelBg.b, 0.90f);
        AddOutline(notifyGO, notifyBorder);
        _notifyTMP = MakeSimpleText(notifyGO, "NotifyTMP", "", 22, textPrimary);
        RectTransform nRT = _notifyTMP.GetComponent<RectTransform>();
        nRT.anchorMin = Vector2.zero; nRT.anchorMax = Vector2.one;
        nRT.offsetMin = new Vector2(16,0); nRT.offsetMax = new Vector2(-16,0);
        _notifyTMP.alignment = TextAlignmentOptions.Center;
        _notifyGroup = notifyGO.AddComponent<CanvasGroup>();
    }

    //  WILDLIFE ROW BUILDER

    void BuildWildlifeRow(GameObject parent, int index, float rowH, float headerH)
    {
        float yOffset = -(headerH + 8 + index * rowH);

        GameObject row = new GameObject($"Row_{wildlifeNames[index]}");
        row.transform.SetParent(parent.transform, false);
        RectTransform rowRT = row.AddComponent<RectTransform>();
        rowRT.anchorMin = new Vector2(0,1); rowRT.anchorMax = new Vector2(1,1);
        rowRT.offsetMin = new Vector2(16,  yOffset - rowH);
        rowRT.offsetMax = new Vector2(-16, yOffset);

        Button btn   = row.AddComponent<Button>();
        Image  rowBG = row.AddComponent<Image>();
        rowBG.color  = Color.clear;

        // Checkbox
        Image cbImage = MakeImage(row, "Checkbox", checkUnchecked);
        cbImage.raycastTarget = false;
        RectTransform cbRT = cbImage.GetComponent<RectTransform>();
        cbRT.anchorMin = new Vector2(0,0.5f); cbRT.anchorMax = new Vector2(0,0.5f);
        cbRT.sizeDelta = new Vector2(28,28);  cbRT.anchoredPosition = new Vector2(16,0);

        // Checkmark
        GameObject cmGO = new GameObject("Checkmark");
        cmGO.transform.SetParent(cbImage.gameObject.transform, false);
        RectTransform cmRT = cmGO.AddComponent<RectTransform>();
        cmRT.anchorMin = Vector2.zero; cmRT.anchorMax = Vector2.one;
        cmRT.offsetMin = new Vector2(6,6); cmRT.offsetMax = new Vector2(-6,-6);
        cmGO.AddComponent<Image>().color = new Color(0.02f, 0.10f, 0.08f, 1f);
        cmGO.SetActive(false);

        // Name label
        TextMeshProUGUI nameTMP = MakeSimpleText(row, "Name", wildlifeNames[index], 24, textPrimary);
        RectTransform nameRT = nameTMP.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0,0); nameRT.anchorMax = new Vector2(0.72f,1);
        nameRT.offsetMin = new Vector2(56,0); nameRT.offsetMax = Vector2.zero;

        // Tag pill
        bool fauna     = isFauna[index];
        Image tagImage = MakeImage(row, "Tag", fauna ? faunaTag : floraTag);
        tagImage.raycastTarget = false;
        RectTransform tagRT = tagImage.GetComponent<RectTransform>();
        tagRT.anchorMin = new Vector2(1,0.5f); tagRT.anchorMax = new Vector2(1,0.5f);
        tagRT.sizeDelta = new Vector2(76,32);  tagRT.anchoredPosition = new Vector2(-42,0);

        TextMeshProUGUI tagTMP = MakeSimpleText(tagImage.gameObject, "TagText",
            fauna ? "fauna" : "flora", 18, fauna ? faunaText : floraText);
        RectTransform tagTRT = tagTMP.GetComponent<RectTransform>();
        tagTRT.anchorMin = Vector2.zero; tagTRT.anchorMax = Vector2.one;
        tagTRT.offsetMin = Vector2.zero; tagTRT.offsetMax = Vector2.zero;
        tagTMP.alignment = TextAlignmentOptions.Center;

        // Row separator
        if (index < wildlifeNames.Length - 1)
        {
            Image sep = MakeImage(row, "Sep", new Color(1,1,1,0.12f));
            RectTransform srt = sep.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0,0); srt.anchorMax = new Vector2(1,0);
            srt.offsetMin = Vector2.zero;     srt.offsetMax = new Vector2(0,2);
        }

        _rows.Add(new WildlifeRow {
            name     = wildlifeNames[index],
            spotted  = false,
            checkbox = cbImage,
            label    = nameTMP
        });

        int captured = index;
        btn.onClick.AddListener(() => ToggleWildlife(captured, cbImage, cmGO, nameTMP));

        ColorBlock cb = btn.colors;
        cb.normalColor      = Color.clear;
        cb.highlightedColor = new Color(1,1,1,0.08f);
        cb.pressedColor     = new Color(1,1,1,0.15f);
        cb.selectedColor    = Color.clear;
        btn.colors        = cb;
        btn.targetGraphic = rowBG;
    }

    //  LOGIC

    void ToggleWildlife(int index, Image checkbox, GameObject checkmark, TextMeshProUGUI label)
    {
        WildlifeRow row = _rows[index];
        row.spotted  = !row.spotted;
        _spotted     = Mathf.Max(0, _spotted + (row.spotted ? 1 : -1));

        checkbox.color = row.spotted ? green : checkUnchecked;
        checkmark.SetActive(row.spotted);
        label.color    = row.spotted ? textStruck : textPrimary;
        label.text     = row.spotted ? $"<s>{row.name}</s>" : row.name;

        _wlProgressTMP.text = $"{_spotted} / {wildlifeNames.Length}";
        RefreshScore();
        ShowNotify(row.spotted
            ? $"{row.name} spotted · +{wildlifePts} pts"
            : $"{row.name} removed · -{wildlifePts} pts");
    }

    /// <summary>Called automatically by Collectable.OnCollected() when isTrash = true.
    /// The caller supplies the point value from the Collectable itself — the HUD
    /// is not the source of truth for per-item points.</summary>
    public void OnTrashCollected(int points)
    {
        _trash++;
        _trashPoints += points;
        if (_trashCountTMP) _trashCountTMP.text = _trash.ToString();
        RefreshScore();
        ShowNotify($"Trash collected · +{points} pts");
    }

    void RefreshScore()
    {
        int total = (_spotted * wildlifePts) + _trashPoints;
        if (_scoreTMP)         _scoreTMP.text         = total.ToString("N0");
        if (_wildlifeBreakTMP) _wildlifeBreakTMP.text = $"Wildlife: {_spotted} x {wildlifePts} = {_spotted * wildlifePts}";
        if (_trashBreakTMP)    _trashBreakTMP.text    = $"Trash: {_trash} collected = {_trashPoints}";
    }

    public void SetInteractHint(string text, bool visible)
    {
        if (_interactHintRoot) _interactHintRoot.SetActive(visible);
        if (_interactHintTMP)  _interactHintTMP.text = text;
    }

    public void ShowNotify(string message)
    {
        if (_notifyTMP) _notifyTMP.text = message;
        if (_notifyRoutine != null) StopCoroutine(_notifyRoutine);
        _notifyRoutine = StartCoroutine(NotifyRoutine());
    }

    IEnumerator NotifyRoutine()
    {
        if (!_notifyGroup) yield break;
        _notifyGroup.gameObject.SetActive(true);
        _notifyGroup.alpha = 1f;
        yield return new WaitForSeconds(2.0f);
        float t = 0f;
        while (t < 0.3f) { _notifyGroup.alpha = 1f - (t / 0.3f); t += Time.deltaTime; yield return null; }
        HideNotify();
    }

    void HideNotify()
    {
        if (!_notifyGroup) return;
        _notifyGroup.alpha = 0f;
        _notifyGroup.gameObject.SetActive(false);
    }

    //  UI HELPERS

    GameObject MakePanel(GameObject parent, string name, Vector2 size, Vector2 anchor, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = panelBg;
        AddOutline(go, panelBorder);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
        rt.sizeDelta = size;   rt.anchoredPosition = anchoredPos;
        return go;
    }

    Image MakeImage(GameObject parent, string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    TextMeshProUGUI MakeSimpleText(GameObject parent, string name, string text, float size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = text;
        tmp.fontSize      = size;
        tmp.color         = color;
        tmp.raycastTarget = false;
        tmp.overflowMode  = TextOverflowModes.Overflow;
        return tmp;
    }

    void BuildTrashIcon(GameObject parent)
    {
        GameObject icon = new GameObject("TrashIcon");
        icon.transform.SetParent(parent.transform, false);
        RectTransform iconRT = icon.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0,0.5f); iconRT.anchorMax = new Vector2(0,0.5f);
        iconRT.sizeDelta = new Vector2(56,56);  iconRT.anchoredPosition = new Vector2(40,0);
        icon.AddComponent<Image>().color = new Color(0.88f,0.31f,0.24f,0.25f);

        Image body = MakeImage(icon, "Body", new Color(1f,0.55f,0.45f,1f));
        RectTransform brt = body.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.25f,0.1f); brt.anchorMax = new Vector2(0.75f,0.85f);
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;

        Image lid = MakeImage(icon, "Lid", new Color(1f,0.55f,0.45f,1f));
        RectTransform lrt = lid.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.15f,0.8f); lrt.anchorMax = new Vector2(0.85f,0.95f);
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
    }

    void AddOutline(GameObject go, Color color)
    {
        Outline o = go.AddComponent<Outline>();
        o.effectColor    = color;
        o.effectDistance = new Vector2(1f,1f);
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}