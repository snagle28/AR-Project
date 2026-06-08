using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 여러 PLY 파일(TextAsset)을 등록하고, 매 세션마다 랜덤으로 하나를 선택한다.
/// 파일명 → 표시명 매핑을 통해 정답 판정에 사용할 표시명을 반환한다.
/// </summary>
public class CaptchaShapePool : MonoBehaviour
{
    // ───────────────────────────────────────────────
    // Inspector Fields
    // ───────────────────────────────────────────────

    [Header("PLY Shape Pool")]
    [Tooltip("랜덤 선택 대상 PLY 파일 목록. 2개 이상 등록 권장.")]
    public TextAsset[] plyShapes;

    [Header("Filename → Display Name Mapping")]
    [Tooltip("파일명과 표시명(정답 판정용)을 쌍으로 등록한다.\n" +
             "예) fileName: 02_Key  displayName: Key")]
    public ShapeNameEntry[] shapeNameMap;

    [Header("State (Read-Only)")]
    [Tooltip("직전에 선택된 PLY 파일 (연속 중복 방지용)")]
    [SerializeField] private TextAsset _lastSelectedShape;

    /// <summary>직전 선택 항목 (읽기 전용 프로퍼티)</summary>
    public TextAsset LastSelectedShape => _lastSelectedShape;

    // ───────────────────────────────────────────────
    // Internal
    // ───────────────────────────────────────────────

    private Dictionary<string, string> _nameDict;

    void Awake()
    {
        BuildDictionary();
    }

    /// <summary>shapeNameMap 배열을 Dictionary로 빌드한다.</summary>
    void BuildDictionary()
    {
        _nameDict = new Dictionary<string, string>();

        if (shapeNameMap == null) return;

        foreach (ShapeNameEntry entry in shapeNameMap)
        {
            if (string.IsNullOrEmpty(entry.fileName)) continue;
            if (!_nameDict.ContainsKey(entry.fileName))
                _nameDict.Add(entry.fileName, entry.displayName);
        }
    }

    // ───────────────────────────────────────────────
    // Public API
    // ───────────────────────────────────────────────

    /// <summary>
    /// 배열에서 랜덤 TextAsset을 반환한다.
    /// 직전 선택 항목은 후보에서 제외 (배열이 1개짜리일 때는 그대로 반환).
    /// </summary>
    public TextAsset GetRandomShape()
    {
        if (plyShapes == null || plyShapes.Length == 0)
        {
            Debug.LogError("[CaptchaShapePool] plyShapes 배열이 비어 있습니다.");
            return null;
        }

        if (plyShapes.Length == 1)
        {
            _lastSelectedShape = plyShapes[0];
            return _lastSelectedShape;
        }

        List<TextAsset> candidates = new List<TextAsset>(plyShapes.Length);

        foreach (TextAsset asset in plyShapes)
        {
            if (asset != null && asset != _lastSelectedShape)
                candidates.Add(asset);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("[CaptchaShapePool] 유효한 후보가 없어 lastSelectedShape를 반환합니다.");
            return _lastSelectedShape;
        }

        TextAsset selected = candidates[Random.Range(0, candidates.Count)];
        _lastSelectedShape = selected;

        Debug.Log("[CaptchaShapePool] 선택된 형태: " + selected.name
                  + " → 표시명: " + GetCurrentDisplayName());
        return selected;
    }

    /// <summary>
    /// 현재 선택된 형태의 파일명을 반환한다.
    /// lastSelectedShape가 null이면 "unknown" 반환.
    /// </summary>
    public string GetCurrentShapeName()
    {
        return _lastSelectedShape != null ? _lastSelectedShape.name : "unknown";
    }

    /// <summary>
    /// 현재 선택된 형태의 표시명(displayName)을 반환한다.
    /// 매핑이 없으면 파일명을 그대로 반환.
    /// </summary>
    public string GetCurrentDisplayName()
    {
        if (_lastSelectedShape == null) return "unknown";

        // Dictionary가 아직 빌드되지 않은 경우 방어
        if (_nameDict == null) BuildDictionary();

        string fileName = _lastSelectedShape.name;
        string displayName;

        if (_nameDict != null && _nameDict.TryGetValue(fileName, out displayName))
            return displayName;

        Debug.LogWarning("[CaptchaShapePool] 매핑 없음: " + fileName + " → 파일명으로 대체");
        return fileName;
    }

    /// <summary>
    /// 모든 등록된 형태의 표시명 목록을 반환한다.
    /// CaptchaAnswerUI의 오답 후보 구성에 사용한다.
    /// </summary>
    public List<string> GetAllDisplayNames()
    {
        if (_nameDict == null) BuildDictionary();

        List<string> names = new List<string>();

        if (shapeNameMap == null) return names;

        foreach (ShapeNameEntry entry in shapeNameMap)
        {
            if (!string.IsNullOrEmpty(entry.displayName))
                names.Add(entry.displayName);
        }

        return names;
    }
}

// ───────────────────────────────────────────────
// Data Class
// ───────────────────────────────────────────────

/// <summary>파일명 → 표시명 매핑 엔트리</summary>
[System.Serializable]
public class ShapeNameEntry
{
    [Tooltip("TextAsset 파일명 (확장자 제외, 예: 02_Key)")]
    public string fileName;

    [Tooltip("표시명 및 정답 판정에 사용할 이름 (예: Key)")]
    public string displayName;
}
