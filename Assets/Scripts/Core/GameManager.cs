using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Ensure These Exist")]
    public CharacterManager characterManager;
    public CardGenerator cardGenerator;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Ensure CharacterManager exists
            if (characterManager == null)
            {
                characterManager = FindObjectOfType<CharacterManager>();
                if (characterManager == null)
                {
                    GameObject cm = new GameObject("CharacterManager");
                    characterManager = cm.AddComponent<CharacterManager>();
                    Debug.Log("Created CharacterManager");
                }
            }
            
            // Ensure CardGenerator exists
            if (cardGenerator == null)
            {
                cardGenerator = FindObjectOfType<CardGenerator>();
                if (cardGenerator == null)
                {
                    GameObject cg = new GameObject("CardGenerator");
                    cardGenerator = cg.AddComponent<CardGenerator>();
                    Debug.Log("Created CardGenerator");
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}