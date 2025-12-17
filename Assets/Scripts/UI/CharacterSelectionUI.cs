using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject characterButtonPrefab;
    public Transform characterButtonContainer;

    [Header("Scene")]
    public string gameSceneName = "GameScene";

    void Start()
    {
        PopulateCharacterSelection();
    }

    void PopulateCharacterSelection()
    {
        if (CharacterManager.Instance == null)
        {
            Debug.LogError("CharacterManager not found! Make sure it exists in the scene.");
            return;
        }

        foreach (CharacterData character in CharacterManager.Instance.availableCharacters)
        {
            GameObject btnObj = Instantiate(characterButtonPrefab, characterButtonContainer);

            // ---- SOLO IMMAGINE ----
            Image image = btnObj.GetComponent<Image>();

            if (image != null && character.portrait != null)
            {
                image.sprite = character.portrait;
            }

            // ---- CLICK ----
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnCharacterSelected(character));
            }
        }
    }

    void OnCharacterSelected(CharacterData character)
    {
        CharacterManager.Instance.SelectCharacter(character);
        SceneManager.LoadScene(gameSceneName);
    }
}
