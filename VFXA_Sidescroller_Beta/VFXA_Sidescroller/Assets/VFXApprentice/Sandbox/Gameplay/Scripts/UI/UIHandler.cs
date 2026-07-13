using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIHandler : MonoBehaviour
{
    [SerializeField] private UIThemeScriptableObject _uiThemeData;
    [SerializeField] private CurrentSpellScriptableObject _currentSpellData;
    
    private VisualTreeAsset _treeAsset;
    private UIDocument _uiDocument;
    private VisualElement _root;

    private VisualElement _panel;
    private VisualElement _uiToggle;
    private Label _spellName;
    private Label _spellID;
    private VisualElement _spellIcon;
    private VisualElement _slash;
    private VisualElement _cast;
    private VisualElement _summon;
    
    
    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _root = _uiDocument.rootVisualElement;
        
        _panel = _root.Q<VisualElement>("VE-Panel");
        _uiToggle = _root.Q<VisualElement>("VE-Toggle");
        _spellName = _root.Q<Label>("LB-Spell-Name");
        _spellID = _root.Q<Label>("LB-Spell-ID");
        _spellIcon = _root.Q<VisualElement>("VE-Spell-Icon");
        _slash = _root.Q<VisualElement>("VE-Slash");
        _cast = _root.Q<VisualElement>("VE-Cast");
        _summon = _root.Q<VisualElement>("VE-Summon");

        UpdateUI();
    }

    internal void UpdateUI()
    {
        _spellName.text = _currentSpellData.spellName;
        _spellID.text = _currentSpellData.spellID + 1 + "/" + _currentSpellData.spellMaxID;
        
        switch (_currentSpellData.spellTypeID)
        {
            case 1: // Slash
                _spellIcon.style.backgroundImage = new StyleBackground(_uiThemeData._slashIcon);
                _slash.style.backgroundImage = new StyleBackground(_uiThemeData._slashActive);
                _cast.style.backgroundImage = new StyleBackground(_uiThemeData._cast);
                _summon.style.backgroundImage = new StyleBackground(_uiThemeData._summon);
                break;
            case 2: // Cast
                _spellIcon.style.backgroundImage = new StyleBackground(_uiThemeData._castIcon);
                _slash.style.backgroundImage = new StyleBackground(_uiThemeData._slash);
                _cast.style.backgroundImage = new StyleBackground(_uiThemeData._castActive);
                _summon.style.backgroundImage = new StyleBackground(_uiThemeData._summon);
                break;
            case 3: // Summon
                _spellIcon.style.backgroundImage = new StyleBackground(_uiThemeData._summonIcon);
                _slash.style.backgroundImage = new StyleBackground(_uiThemeData._slash);
                _cast.style.backgroundImage = new StyleBackground(_uiThemeData._summon);
                _summon.style.backgroundImage = new StyleBackground(_uiThemeData._summonActive);
                break;
            default:
                break;
        }
    }
}
