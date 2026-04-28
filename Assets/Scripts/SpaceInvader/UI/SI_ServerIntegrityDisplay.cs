using System.Collections;
using UnityEngine;

public class SI_ServerIntegrityDisplay : MonoBehaviour
{
    [SerializeField] private SI_ServerHealth _serverHealth;
    [SerializeField] private int _indicatorCount = 5;
    [SerializeField] private float _indicatorSpacingX = 0.4f;
    [SerializeField] private float _indicatorLocalY = 0.4f;
    [SerializeField] private Vector3 _indicatorScale = new Vector3(0.2f, 0.2f, 1f);
    [SerializeField] private Color _activeColor = new Color(0.2f, 1f, 0.4f, 1f);
    [SerializeField] private Color _destroyedColor = new Color(1f, 0.1f, 0.1f, 1f);
    [SerializeField] private float _flashDuration = 0.8f;
    [SerializeField] private int _flashCount = 4;

    private const string ShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    private SpriteRenderer[] _indicators;
    private int _lastKnownIntegrity = -1;
    private bool _destroyedHandled;

    private void Awake() // Initialise les indicateurs d'intégrité au-dessus du serveur
    {
        if (_serverHealth == null) { Debug.LogError("[SI_ServerIntegrityDisplay] _serverHealth non assigné."); return; }
        BuildIndicators();
    }

    private void Start() // Mémorise l'intégrité initiale pour le polling
    {
        if (_serverHealth == null) return;
        _lastKnownIntegrity = _serverHealth.GetCurrentIntegrity();
    }

    private void Update() // Détecte les changements d'intégrité via polling
    {
        if (_serverHealth == null || _destroyedHandled) return;

        int current = _serverHealth.GetCurrentIntegrity();
        if (current == _lastKnownIntegrity) return;

        _lastKnownIntegrity = current;

        if (current <= 0) // Lance le flash de destruction si intégrité épuisée
        {
            _destroyedHandled = true;
            HandleDestroyed();
        }
        else HandleDamaged(current); // Cache l'indicateur le plus à droite
    }

    private void BuildIndicators() // Instancie les indicateurs circulaires au-dessus du serveur
    {
        _indicators = new SpriteRenderer[_indicatorCount];
        float startX = -(_indicatorCount - 1) * _indicatorSpacingX * 0.5f;

        for (int i = 0; i < _indicatorCount; i++) // Crée chaque indicateur sur la ligne
        {
            GameObject go = new GameObject($"Integrity_{(i + 1):D2}");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(startX + i * _indicatorSpacingX, _indicatorLocalY, 0f);
            go.transform.localScale    = _indicatorScale;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            Texture2D tex     = new Texture2D(1, 1);
            tex.SetPixel(0, 0, _activeColor);
            tex.Apply();
            sr.sprite         = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            sr.sharedMaterial = new Material(Shader.Find(ShaderName));
            sr.sortingOrder   = 3;
            _indicators[i]    = sr;
        }
    }

    private void HandleDamaged(int remainingIntegrity) // Cache l'indicateur correspondant à l'intégrité perdue
    {
        if (remainingIntegrity >= 0 && remainingIntegrity < _indicatorCount)
            _indicators[remainingIntegrity].gameObject.SetActive(false);
    }

    private void HandleDestroyed() // Cache tous les indicateurs et lance le flash rouge
    {
        foreach (SpriteRenderer sr in _indicators) sr.gameObject.SetActive(false); // Masque tous les indicateurs
        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed() // Fait clignoter le serveur en rouge à la destruction
    {
        SpriteRenderer serverSr = GetComponent<SpriteRenderer>();
        if (serverSr == null) yield break;

        Color original    = serverSr.color;
        float halfInterval = _flashDuration / (_flashCount * 2f);

        for (int i = 0; i < _flashCount; i++) // Alterne rouge et couleur originale
        {
            serverSr.color = _destroyedColor;
            yield return new WaitForSeconds(halfInterval);
            serverSr.color = original;
            yield return new WaitForSeconds(halfInterval);
        }
    }
}
