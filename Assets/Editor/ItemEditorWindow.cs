#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Fenêtre d'édition des ItemData du projet.
public class ItemEditorWindow : EditorWindow
{
    // -------------------------------------------------------------------------
    // Constantes de chemin et d'interface
    // -------------------------------------------------------------------------

    // Dossier de sauvegarde des assets ItemData.
    private const string ITEMS_FOLDER = "Assets/Resources/Items";

    // Dossier parent pour la création du sous-dossier Items.
    private const string RESOURCES_FOLDER = "Assets/Resources";

    // Nom du sous-dossier contenant les ItemData.
    private const string ITEMS_SUBFOLDER = "Items";

    // Chemin par défaut d'un nouvel asset ItemData.
    private const string NEW_ITEM_PATH = "Assets/Resources/Items/NewItem.asset";

    // Largeur fixe du panneau de liste gauche.
    private const float LEFT_PANEL_WIDTH = 220f;

    // Hauteur du bouton de sauvegarde principal.
    private const float SAVE_BUTTON_HEIGHT = 36f;

    // Largeur des boutons d'action en bas du panneau gauche.
    private const float ACTION_BUTTON_WIDTH = 210f;

    // Hauteur de l'aperçu du sprite dans le panneau droit.
    private const float PREVIEW_SIZE = 64f;

    // Valeur minimale du slider de durée d'effet.
    private const float DURATION_MIN = 0.5f;

    // Valeur maximale du slider de durée d'effet.
    private const float DURATION_MAX = 60f;

    // Valeur minimale du slider de temps de consommation.
    private const float CONSUMPTION_MIN = 0.1f;

    // Valeur maximale du slider de temps de consommation.
    private const float CONSUMPTION_MAX = 10f;

    // -------------------------------------------------------------------------
    // Champs privés
    // -------------------------------------------------------------------------

    // Liste de tous les ItemData chargés depuis le dossier Items.
    private List<ItemData> _allItems = new List<ItemData>();

    // ItemData actuellement sélectionné dans la liste.
    private ItemData _selectedItem = null;

    // Position de défilement du panneau de liste gauche.
    private Vector2 _listScrollPos = Vector2.zero;

    // Position de défilement du panneau d'édition droit.
    private Vector2 _editorScrollPos = Vector2.zero;

    // -------------------------------------------------------------------------
    // Initialisation de la fenêtre
    // -------------------------------------------------------------------------

    // Ouvre la fenêtre depuis le menu Unity.
    [MenuItem("Game Tools/Item Editor")]
    public static void OpenWindow()
    {
        // Récupère ou crée la fenêtre et définit son titre.
        ItemEditorWindow window = GetWindow<ItemEditorWindow>("Éditeur d'Objets");

        // Définit la taille minimale de la fenêtre.
        window.minSize = new Vector2(700, 500);

        // Affiche la fenêtre à l'écran.
        window.Show();
    }

    // Charge la liste au démarrage de la fenêtre.
    private void OnEnable()
    {
        // Initialise la liste des objets au premier affichage.
        LoadAllItems();
    }

    // -------------------------------------------------------------------------
    // Boucle principale d'interface
    // -------------------------------------------------------------------------

    // Dessine l'interface complète de la fenêtre.
    private void OnGUI()
    {
        // Démarre la disposition horizontale principale.
        EditorGUILayout.BeginHorizontal();

        // Panneau gauche : liste des objets.
        EditorGUILayout.BeginVertical(GUILayout.Width(LEFT_PANEL_WIDTH));
        DrawItemList();
        EditorGUILayout.EndVertical();

        // Séparateur vertical entre les deux panneaux.
        EditorGUILayout.LabelField("", GUI.skin.verticalSlider,
            GUILayout.Width(4), GUILayout.ExpandHeight(true));

        // Panneau droit : éditeur de l'objet sélectionné.
        EditorGUILayout.BeginVertical();
        DrawItemEditor();
        EditorGUILayout.EndVertical();

        // Termine la disposition horizontale principale.
        EditorGUILayout.EndHorizontal();
    }

    // -------------------------------------------------------------------------
    // Panneau gauche — liste des objets
    // -------------------------------------------------------------------------

    // Charge tous les ItemData depuis le dossier Resources/Items.
    private void LoadAllItems()
    {
        // Trouve tous les GUIDs d'assets ItemData dans le dossier cible.
        string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { ITEMS_FOLDER });

        // Convertit les GUIDs en références ItemData valides.
        _allItems = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<ItemData>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .Where(item => item != null)
            .ToList();
    }

    // Dessine la liste des objets dans le panneau gauche.
    private void DrawItemList()
    {
        // En-tête du panneau gauche en gras.
        EditorGUILayout.LabelField("Objets sauvegardés", EditorStyles.boldLabel);

        // Démarre la zone de défilement de la liste.
        _listScrollPos = EditorGUILayout.BeginScrollView(_listScrollPos,
            GUILayout.Width(LEFT_PANEL_WIDTH), GUILayout.ExpandHeight(true));

        // Parcourt chaque objet de la liste pour l'afficher.
        foreach (ItemData item in _allItems)
        {
            // Détermine la couleur de rareté de l'objet courant.
            Color rarityColor = GetRarityColor(item.Rarity);

            // Surligne l'objet sélectionné en bleu.
            bool isSelected = item == _selectedItem;
            if (isSelected)
                GUI.backgroundColor = new Color(0.3f, 0.6f, 1f, 1f);

            // Démarre la ligne horizontale de l'entrée.
            EditorGUILayout.BeginHorizontal();

            // Affiche le point coloré représentant la rareté.
            GUIStyle dotStyle = new GUIStyle(GUI.skin.label);
            dotStyle.normal.textColor = rarityColor;
            EditorGUILayout.LabelField("●", dotStyle, GUILayout.Width(16));

            // Bouton cliquable avec le nom de l'objet pour le sélectionner.
            if (GUILayout.Button(item.ItemName, EditorStyles.label))
                _selectedItem = item;

            // Termine la ligne horizontale de l'entrée.
            EditorGUILayout.EndHorizontal();

            // Remet la couleur de fond à la valeur par défaut.
            GUI.backgroundColor = Color.white;
        }

        // Termine la zone de défilement de la liste.
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8);

        // Bouton de création d'un nouvel objet.
        if (GUILayout.Button("+ Nouvel objet", GUILayout.Width(ACTION_BUTTON_WIDTH)))
            CreateNewItem();

        // Désactive le bouton supprimer si rien n'est sélectionné.
        GUI.enabled = _selectedItem != null;

        // Bouton de suppression en rouge, actif seulement si sélection.
        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f, 1f);
        if (GUILayout.Button("🗑 Supprimer", GUILayout.Width(ACTION_BUTTON_WIDTH)))
            DeleteSelectedItem();

        // Remet les états visuels par défaut.
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
    }

    // Retourne la couleur associée à une rareté donnée.
    private Color GetRarityColor(LootRarity rarity)
    {
        // Mappe chaque rareté à une couleur distincte.
        return rarity switch
        {
            LootRarity.Common    => Color.white,
            LootRarity.Uncommon  => new Color(0.3f, 1f, 0.3f, 1f),
            LootRarity.Rare      => new Color(0.3f, 0.5f, 1f, 1f),
            LootRarity.Legendary => new Color(1f, 0.6f, 0f, 1f),
            _                    => Color.white
        };
    }

    // -------------------------------------------------------------------------
    // Panneau droit — éditeur de l'objet sélectionné
    // -------------------------------------------------------------------------

    // Dessine le panneau d'édition de l'objet sélectionné.
    private void DrawItemEditor()
    {
        // Affiche un message centré si rien n'est sélectionné.
        if (_selectedItem == null)
        {
            // Pousse le contenu au centre vertical.
            GUILayout.FlexibleSpace();

            // Affiche le message d'invite en gris centré.
            EditorGUILayout.LabelField("Sélectionnez un objet",
                EditorStyles.centeredGreyMiniLabel);

            // Complète le centrage vertical.
            GUILayout.FlexibleSpace();
            return;
        }

        // Démarre la zone de défilement du panneau droit.
        _editorScrollPos = EditorGUILayout.BeginScrollView(_editorScrollPos);

        // Commence la détection des changements pour marquer l'asset dirty.
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.Space(10);

        // ── IDENTITÉ ─────────────────────────────────────────────────────────

        // Section identité de l'objet.
        EditorGUILayout.LabelField("Identité", EditorStyles.boldLabel);

        // Nom de l'objet fouillable.
        _selectedItem.ItemName = EditorGUILayout.TextField("Nom", _selectedItem.ItemName);

        // Description affichée dans l'inventaire du joueur.
        _selectedItem.Description = EditorGUILayout.TextArea(
            _selectedItem.Description, GUILayout.Height(60));

        // Sprite représentant l'objet dans l'inventaire.
        _selectedItem.Icon = (Sprite)EditorGUILayout.ObjectField(
            "Sprite", _selectedItem.Icon, typeof(Sprite), false);

        // Rareté déterminant la couleur et la valeur de l'objet.
        _selectedItem.Rarity = (LootRarity)EditorGUILayout.EnumPopup(
            "Rareté", _selectedItem.Rarity);

        EditorGUILayout.Space(10);

        // ── EFFET ─────────────────────────────────────────────────────────────

        // Section effet appliqué au joueur.
        EditorGUILayout.LabelField("Effet sur le joueur", EditorStyles.boldLabel);

        // Type d'effet que l'objet confère au joueur.
        _selectedItem.EffectType = (ItemEffectType)EditorGUILayout.EnumPopup(
            "Type d'effet", _selectedItem.EffectType);

        // Valeur numérique associée à l'effet.
        _selectedItem.EffectValue = EditorGUILayout.FloatField(
            "Valeur", _selectedItem.EffectValue);

        EditorGUILayout.Space(10);

        // ── TEMPORALITÉ ───────────────────────────────────────────────────────

        // Section temporalité de l'effet et de la consommation.
        EditorGUILayout.LabelField("Temporalité", EditorStyles.boldLabel);

        // Bascule indiquant si l'effet est limité dans le temps.
        _selectedItem.HasDuration = EditorGUILayout.Toggle(
            "Effet temporaire", _selectedItem.HasDuration);

        // Affiche le slider de durée uniquement si l'effet est temporaire.
        if (_selectedItem.HasDuration)
        {
            // Slider pour régler la durée de l'effet en secondes.
            _selectedItem.EffectDuration = EditorGUILayout.Slider(
                "Durée (sec)", _selectedItem.EffectDuration, DURATION_MIN, DURATION_MAX);
        }

        EditorGUILayout.Space(6);

        // Bascule indiquant si la consommation prend du temps.
        _selectedItem.HasConsumptionTime = EditorGUILayout.Toggle(
            "Consommation progressive", _selectedItem.HasConsumptionTime);

        // Affiche le slider de consommation seulement si activé.
        if (_selectedItem.HasConsumptionTime)
        {
            // Slider pour régler le temps de consommation en secondes.
            _selectedItem.ConsumptionTime = EditorGUILayout.Slider(
                "Temps de consommation (sec)", _selectedItem.ConsumptionTime,
                CONSUMPTION_MIN, CONSUMPTION_MAX);
        }

        EditorGUILayout.Space(16);

        // ── APERÇU ────────────────────────────────────────────────────────────

        // Section aperçu visuel de l'objet.
        EditorGUILayout.LabelField("Aperçu", EditorStyles.boldLabel);
        DrawItemPreview();

        EditorGUILayout.Space(16);

        // ── SAUVEGARDE ────────────────────────────────────────────────────────

        // Bouton de sauvegarde mis en valeur en vert.
        GUI.backgroundColor = new Color(0.3f, 1f, 0.4f, 1f);
        if (GUILayout.Button("💾 Sauvegarder", GUILayout.Height(SAVE_BUTTON_HEIGHT)))
            SaveSelectedItem();

        // Remet la couleur de fond par défaut après le bouton.
        GUI.backgroundColor = Color.white;

        // Marque l'asset dirty si un champ a été modifié.
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(_selectedItem);

        // Termine la zone de défilement du panneau droit.
        EditorGUILayout.EndScrollView();
    }

    // Affiche un aperçu visuel de l'objet sélectionné.
    private void DrawItemPreview()
    {
        // Démarre la disposition horizontale de l'aperçu.
        EditorGUILayout.BeginHorizontal();

        // Affiche le sprite si un icon est assigné à l'objet.
        if (_selectedItem.Icon != null)
        {
            // Récupère la prévisualisation Unity du sprite sélectionné.
            Texture2D tex = AssetPreview.GetAssetPreview(_selectedItem.Icon);

            // Affiche la texture uniquement si elle est disponible.
            if (tex != null)
                GUILayout.Label(tex, GUILayout.Width(PREVIEW_SIZE),
                    GUILayout.Height(PREVIEW_SIZE));
        }
        else
        {
            // Dessine un rectangle gris en remplacement du sprite absent.
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(PREVIEW_SIZE, PREVIEW_SIZE),
                Color.gray);
        }

        // Démarre la colonne de texte à droite de l'aperçu.
        EditorGUILayout.BeginVertical();

        // Affiche le nom avec la couleur de rareté correspondante.
        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
        nameStyle.normal.textColor = GetRarityColor(_selectedItem.Rarity);
        EditorGUILayout.LabelField(_selectedItem.ItemName, nameStyle);

        // Affiche la description en texte compact et enveloppant.
        EditorGUILayout.LabelField(_selectedItem.Description,
            EditorStyles.wordWrappedMiniLabel);

        // Résumé textuel de l'effet selon sa durée configurée.
        string effectSummary = _selectedItem.HasDuration
            ? $"{_selectedItem.EffectType} +{_selectedItem.EffectValue} ({_selectedItem.EffectDuration}s)"
            : $"{_selectedItem.EffectType} +{_selectedItem.EffectValue} (permanent)";

        // Affiche le résumé de l'effet en mini-label.
        EditorGUILayout.LabelField(effectSummary, EditorStyles.miniLabel);

        // Termine la colonne de texte de l'aperçu.
        EditorGUILayout.EndVertical();

        // Termine la disposition horizontale de l'aperçu.
        EditorGUILayout.EndHorizontal();
    }

    // -------------------------------------------------------------------------
    // Création / Sauvegarde / Suppression
    // -------------------------------------------------------------------------

    // Crée un nouvel objet ItemData dans le dossier Items.
    private void CreateNewItem()
    {
        // Crée le dossier Resources s'il n'existe pas encore.
        if (!AssetDatabase.IsValidFolder(RESOURCES_FOLDER))
            AssetDatabase.CreateFolder("Assets", "Resources");

        // Crée le dossier Items s'il n'existe pas encore.
        if (!AssetDatabase.IsValidFolder(ITEMS_FOLDER))
            AssetDatabase.CreateFolder(RESOURCES_FOLDER, ITEMS_SUBFOLDER);

        // Rafraîchit la base de données pour enregistrer les nouveaux dossiers.
        AssetDatabase.Refresh();

        // Génère un chemin unique pour éviter d'écraser un asset existant.
        string path = AssetDatabase.GenerateUniqueAssetPath(NEW_ITEM_PATH);

        // Interrompt si le chemin généré est vide ou invalide.
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("[ItemEditor] Impossible de générer un chemin valide.");
            return;
        }

        // Crée l'instance du ScriptableObject en mémoire.
        ItemData newItem = CreateInstance<ItemData>();

        // Sauvegarde l'asset sur le disque au chemin généré.
        AssetDatabase.CreateAsset(newItem, path);
        AssetDatabase.SaveAssets();

        // Recharge la liste et sélectionne automatiquement le nouvel objet.
        LoadAllItems();
        _selectedItem = newItem;
    }

    // Sauvegarde les modifications de l'objet sélectionné.
    private void SaveSelectedItem()
    {
        // Marque l'asset comme modifié pour forcer la sérialisation Unity.
        EditorUtility.SetDirty(_selectedItem);
        AssetDatabase.SaveAssets();

        // Confirme la sauvegarde dans la console Unity.
        Debug.Log($"[ItemEditor] Objet sauvegardé : {_selectedItem.ItemName}");

        // Recharge la liste pour refléter les changements de nom éventuels.
        LoadAllItems();
    }

    // Supprime l'objet sélectionné après confirmation utilisateur.
    private void DeleteSelectedItem()
    {
        // Affiche une boîte de dialogue de confirmation avant suppression.
        bool confirm = EditorUtility.DisplayDialog(
            "Supprimer l'objet",
            $"Supprimer \"{_selectedItem.ItemName}\" définitivement ?",
            "Supprimer", "Annuler");

        // Annule l'opération si l'utilisateur a refusé la suppression.
        if (!confirm)
            return;

        // Récupère le chemin sur le disque de l'asset à supprimer.
        string path = AssetDatabase.GetAssetPath(_selectedItem);

        // Supprime l'asset du projet Unity.
        AssetDatabase.DeleteAsset(path);

        // Efface la sélection courante après suppression.
        _selectedItem = null;

        // Recharge la liste pour retirer l'entrée supprimée.
        LoadAllItems();
    }
}

#endif
