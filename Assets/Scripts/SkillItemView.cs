using Game;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SkillItemView : MonoBehaviour
{
    [FormerlySerializedAs("nameText")] [SerializeField, Tooltip("The TMPro Text that will be used to display the skill tier")]
    private TMPro.TMP_Text tierText;
    [SerializeField, Tooltip("The TMPro Text that will be used to display the points required in the skill tree")]
    private TMPro.TextMeshProUGUI pointRequirementText;
    [SerializeField, Tooltip("The game object that will be used to display the lock icon")]
    private GameObject lockedIcon;
    [SerializeField, Tooltip("The game object that will be used to display the connector icon")]
    private GameObject connectorIcon;
    [SerializeField, Tooltip("The game object that will be used to display the selection background")]
    private GameObject selectionBackground;
    [SerializeField, Tooltip("The game object that will be used to display the purchased background")]
    private GameObject purchasedBackground;
    
    private int unlockCost;
    

    public void Bind(SkillData _data, System.Action<SkillData> _onSelect)
    {
        if (!TryGetComponent(out Toggle toggle))
            throw new System.Exception("No Button component found on SkillItemView GameObject.");
        var skillToggleGroupParent = GameObject.Find("SkillToggleGroup");
        if (!skillToggleGroupParent)
            throw new System.Exception("No SkillToggleGroupParent GameObject found in scene.");
        if (!skillToggleGroupParent.TryGetComponent(out ToggleGroup toggleGroup))
            throw new System.Exception("No ToggleGroup found in SkillToggleGroup GameObject.");
        
        toggle.group = toggleGroup;
        toggle.SetIsOnWithoutNotify(false);
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener((_isOn) =>
        {
            selectionBackground.SetActive(_isOn);
            if (_isOn)
                _onSelect(_data);
        });
        
        tierText.text = _data.TierRoman;
        unlockCost = _data.TreeRequirement.y;
        pointRequirementText.text = $"{_data.TreeRequirement.x} / {unlockCost}";
        
        if (_data.UseLock) {
            connectorIcon.SetActive(false);
            tierText.gameObject.SetActive(false);
            pointRequirementText.gameObject.SetActive(false);
            selectionBackground.SetActive(false);
            purchasedBackground.SetActive(false);
            toggle.interactable = false;
            toggle.group = toggleGroup;
            toggle.SetIsOnWithoutNotify(false);
            lockedIcon.SetActive(true);
            return;
        }

        lockedIcon.SetActive(false);
        selectionBackground.SetActive(false);
        purchasedBackground.SetActive(false);
        toggle.interactable = true;
        connectorIcon.SetActive(_data.UseConnector);
    }

    public void Purchase()
    {
        selectionBackground.SetActive(false);
        pointRequirementText.gameObject.SetActive(false);
        connectorIcon.SetActive(true);
        if (!TryGetComponent(out Toggle toggle))
            throw new System.Exception("No Button component found on SkillItemView GameObject.");
        
        toggle.interactable = false;
        toggle.group = null;
        toggle.SetIsOnWithoutNotify(false);
        purchasedBackground.SetActive(true);
    }

    public void Unlock()
    {
        lockedIcon.SetActive(false);
        tierText.gameObject.SetActive(true);
        pointRequirementText.gameObject.SetActive(true);
        if (!TryGetComponent(out Toggle toggle))
            throw new System.Exception("No Button component found on SkillItemView GameObject.");
        toggle.interactable = true;
    }

    public void UpdatePoints(int _pointsSpent)
    {
        pointRequirementText.text = $"{_pointsSpent} / {unlockCost}";
    }
}
