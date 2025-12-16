using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject characterButtonPrefab;
    public Transform characterButtonContainer;
    public TMP_Text titleText;
    
    [Header("Scene")]
    public string gameSceneName = "GameScene"; // Name of your main game scene
    
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
            
            // Setup button visuals
            TMP_Text nameText = btnObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
            TMP_Text descText = btnObj.transform.Find("DescriptionText")?.GetComponent<TMP_Text>();
            TMP_Text weaponText = btnObj.transform.Find("WeaponText")?.GetComponent<TMP_Text>();
            TMP_Text passiveText = btnObj.transform.Find("PassiveText")?.GetComponent<TMP_Text>();
            Image portrait = btnObj.transform.Find("Portrait")?.GetComponent<Image>();
            
            if (nameText != null)
                nameText.text = $"{character.arcanaName}\n{character.characterName}";
            
            if (descText != null)
                descText.text = character.description;
            
            if (weaponText != null && character.starterWeapon != null)
                weaponText.text = $"Weapon: {character.starterWeapon.weaponName}";
            
            if (passiveText != null)
                passiveText.text = $"Passive: {character.passiveDescription}";
            
            if (portrait != null && character.portrait != null)
                portrait.sprite = character.portrait;
            
            // Setup button click
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnCharacterSelected(character));
            }
        }
    }
    
    void OnCharacterSelected(CharacterData character)
    {
        Debug.Log($"Selected character: {character.characterName}");
        CharacterManager.Instance.SelectCharacter(character);
        SceneManager.LoadScene(gameSceneName);
    }
}