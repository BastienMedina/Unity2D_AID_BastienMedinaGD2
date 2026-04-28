#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ItemEditorWindow : EditorWindow
{
    private const string ITEMS_FOLDER       = "Assets/Resources/Items";
    private const string RESOURCES_FOLDER   = "Assets/Resources";
    private const string ITEMS_SUBFOLDER    = "Items";
    private const string NEW_ITEM_PATH      = "Assets/Resources/Items/NewItem.asset";
    private const float  LEFT_PANEL_WIDTH   = 220f;
    private const float  SAVE_BUTTON_HEIGHT = 36f;
    private const float  ACTION_BUTTON_WIDTH = 210f;
    private const float  PREVIEW_SIZE       = 64f;
    private const float  DURATION_MIN       = 0.5f;
    private const float  DURATION_MAX       = 60f;
    private const float  CONSUMPTION_MIN    = 0.1f;
    private const float  CONSUMPTION_MAX    = 10f;

    private List<ItemData> _allItems       = new List<ItemData>();
    private ItemData       _selectedItem   = null;
    private Vector2        _listScrollPos  = Vector2.zero;
    private Vector2        _editorScrollPos = Vector2.zero;

    [MenuItem("Game Tools/Item Editor")]
    public static void OpenWindow() // Ouvre la fenêtre depuis le menu Unity
    {
        ItemEditorWindow window = GetWindow<ItemEditorWindow>("Éditeur d'Objets");
        window.minSize = new Vector2(700, 500);
        window.Show();
    }

    private void OnEnable() // Charge la liste au démarrage de la fenêtre
    {
        LoadAllItems();
    }

    private void OnGUI() // Dessine l'interface complète de la fenêtre
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(LEFT_PANEL_WIDTH));
        DrawItemList();
        EditorGUILayout.EndVertical();

        EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(4), GUILayout.ExpandHeight(true));

        EditorGUILayout.BeginVertical();
        DrawItemEditor();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private void LoadAllItems() // Charge tous les ItemData depuis Resources/Items
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { ITEMS_FOLDER });
        _allItems = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(item => item != null)
            .ToList();
    }

    private void DrawItemList() // Dessine la liste des objets dans le panneau gauche
    {
        EditorGUILayout.LabelField("Objets sauvegardés", EditorStyles.boldLabel);
        _listScrollPos = EditorGUILayout.BeginScrollView(_listScrollPos, GUILayout.Width(LEFT_PANEL_WIDTH), GUILayout.ExpandHeight(true));

        foreach (ItemData item in _allItems) // Parcourt chaque item de la liste
        {
            Color rarityColor = GetRarityColor(item.Rarity);
            bool isSelected   = item == _selectedItem;
            if (isSelected)
                GUI.backgroundColor = new Color(0.3f, 0.6f, 1f, 1f);

            EditorGUILayout.BeginHorizontal();

            GUIStyle dotStyle = new GUIStyle(GUI.skin.label);
            dotStyle.normal.textColor = rarityColor;
            EditorGUILayout.LabelField("●", dotStyle, GUILayout.Width(16));

            if (GUILayout.Button(item.ItemName, EditorStyles.label))
                _selectedItem = item;

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space(8);

        if (GUILayout.Button("+ Nouvel objet", GUILayout.Width(ACTION_BUTTON_WIDTH)))
            CreateNewItem();

        GUI.enabled = _selectedItem != null;
        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f, 1f);
        if (GUILayout.Button("🗑 Supprimer", GUILayout.Width(ACTION_BUTTON_WIDTH)))
            DeleteSelectedItem();

        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
    }

    private Color GetRarityColor(LootRarity rarity) // Retourne la couleur associée à la rareté
    {
        return rarity switch
        {
            LootRarity.Common    => Color.white,
            LootRarity.Uncommon  => new Color(0.3f, 1f, 0.3f, 1f),
            LootRarity.Rare      => new Color(0.3f, 0.5f, 1f, 1f),
            LootRarity.Legendary => new Color(1f, 0.6f, 0f, 1f),
            _                    => Color.white
        };
    }

    private void DrawItemEditor() // Dessine le panneau d'édition de l'objet sélectionné
    {
        if (_selectedItem == null)
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Sélectionnez un objet", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();
            return;
        }

        _editorScrollPos = EditorGUILayout.BeginScrollView(_editorScrollPos);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Identité", EditorStyles.boldLabel);
        _selectedItem.ItemName    = EditorGUILayout.TextField("Nom", _selectedItem.ItemName);
        _selectedItem.Description = EditorGUILayout.TextArea(_selectedItem.Description, GUILayout.Height(60));
        _selectedItem.Icon        = (Sprite)EditorGUILayout.ObjectField("Sprite", _selectedItem.Icon, typeof(Sprite), false);
        _selectedItem.Rarity      = (LootRarity)EditorGUILayout.EnumPopup("Rareté", _selectedItem.Rarity);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Effet sur le joueur", EditorStyles.boldLabel);
        _selectedItem.EffectType  = (ItemEffectType)EditorGUILayout.EnumPopup("Type d'effet", _selectedItem.EffectType);
        _selectedItem.EffectValue = EditorGUILayout.FloatField("Valeur", _selectedItem.EffectValue);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Temporalité", EditorStyles.boldLabel);
        _selectedItem.HasDuration = EditorGUILayout.Toggle("Effet temporaire", _selectedItem.HasDuration);

        if (_selectedItem.HasDuration) // Affiche durée uniquement si temporaire
            _selectedItem.EffectDuration = EditorGUILayout.Slider("Durée (sec)", _selectedItem.EffectDuration, DURATION_MIN, DURATION_MAX);

        EditorGUILayout.Space(6);
        _selectedItem.HasConsumptionTime = EditorGUILayout.Toggle("Consommation progressive", _selectedItem.HasConsumptionTime);

        if (_selectedItem.HasConsumptionTime) // Affiche le temps de conso si activé
            _selectedItem.ConsumptionTime = EditorGUILayout.Slider("Temps de consommation (sec)", _selectedItem.ConsumptionTime, CONSUMPTION_MIN, CONSUMPTION_MAX);

        EditorGUILayout.Space(16);
        EditorGUILayout.LabelField("Aperçu", EditorStyles.boldLabel);
        DrawItemPreview();
        EditorGUILayout.Space(16);

        GUI.backgroundColor = new Color(0.3f, 1f, 0.4f, 1f);
        if (GUILayout.Button("💾 Sauvegarder", GUILayout.Height(SAVE_BUTTON_HEIGHT)))
            SaveSelectedItem();

        GUI.backgroundColor = Color.white;

        if (EditorGUI.EndChangeCheck()) // Marque l'asset dirty si modifié
            EditorUtility.SetDirty(_selectedItem);

        EditorGUILayout.EndScrollView();
    }

    private void DrawItemPreview() // Affiche l'aperçu visuel de l'objet sélectionné
    {
        EditorGUILayout.BeginHorizontal();

        if (_selectedItem.Icon != null)
        {
            Texture2D tex = AssetPreview.GetAssetPreview(_selectedItem.Icon);
            if (tex != null)
                GUILayout.Label(tex, GUILayout.Width(PREVIEW_SIZE), GUILayout.Height(PREVIEW_SIZE));
        }
        else
        {
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(PREVIEW_SIZE, PREVIEW_SIZE), Color.gray);
        }

        EditorGUILayout.BeginVertical();

        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
        nameStyle.normal.textColor = GetRarityColor(_selectedItem.Rarity);
        EditorGUILayout.LabelField(_selectedItem.ItemName, nameStyle);
        EditorGUILayout.LabelField(_selectedItem.Description, EditorStyles.wordWrappedMiniLabel);

        string effectSummary = _selectedItem.HasDuration
            ? $"{_selectedItem.EffectType} +{_selectedItem.EffectValue} ({_selectedItem.EffectDuration}s)"
            : $"{_selectedItem.EffectType} +{_selectedItem.EffectValue} (permanent)";
        EditorGUILayout.LabelField(effectSummary, EditorStyles.miniLabel);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void CreateNewItem() // Crée un nouvel ItemData dans le dossier Items
    {
        if (!AssetDatabase.IsValidFolder(RESOURCES_FOLDER))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder(ITEMS_FOLDER))
            AssetDatabase.CreateFolder(RESOURCES_FOLDER, ITEMS_SUBFOLDER);

        AssetDatabase.Refresh();

        string path = AssetDatabase.GenerateUniqueAssetPath(NEW_ITEM_PATH);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("[ItemEditor] Impossible de générer un chemin valide.");
            return;
        }

        ItemData newItem = CreateInstance<ItemData>();
        AssetDatabase.CreateAsset(newItem, path);
        AssetDatabase.SaveAssets();

        LoadAllItems();
        _selectedItem = newItem;
    }

    private void SaveSelectedItem() // Sauvegarde les modifications de l'objet sélectionné
    {
        EditorUtility.SetDirty(_selectedItem);
        AssetDatabase.SaveAssets();
        Debug.Log($"[ItemEditor] Objet sauvegardé : {_selectedItem.ItemName}");
        LoadAllItems();
    }

    private void DeleteSelectedItem() // Supprime l'objet sélectionné après confirmation
    {
        bool confirm = EditorUtility.DisplayDialog(
            "Supprimer l'objet",
            $"Supprimer \"{_selectedItem.ItemName}\" définitivement ?",
            "Supprimer", "Annuler");

        if (!confirm) return;

        string path = AssetDatabase.GetAssetPath(_selectedItem);
        AssetDatabase.DeleteAsset(path);
        _selectedItem = null;
        LoadAllItems();
    }
}

#endif
