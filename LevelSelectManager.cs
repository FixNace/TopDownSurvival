using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static LevelConfigSO;
public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private Button[] levelButtons; // 5 кнопок
    [SerializeField] private TextMeshProUGUI progressText;
    
    private int maxUnlockedLevel = 1;
    
    void Start()
    {
        LoadProgress();
        UpdateButtons();
    }

    void UpdateButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool isUnlocked = (i + 1 <= maxUnlockedLevel);
            levelButtons[i].interactable = isUnlocked;

            // Текст на кнопке
            TextMeshProUGUI buttonText = levelButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Уровень {i + 1}\n{(isUnlocked ? "Открыт" : "Закрыт")}";
            }
        }

        if (progressText != null)
            progressText.text = $"Пройдено: {maxUnlockedLevel}/5";
    }
  
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex <= maxUnlockedLevel)
        {
            PlayerPrefs.SetInt("CurrentLevel", levelIndex);

            if (levelIndex < 4)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(1); // 1–3 уровень → сцена 1
            }
            else if (levelIndex < 7)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(2); // 4–6 уровень → сцена 2
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(3); // 7–9 уровень → сцена 3
            }

        }
    }

    public static void UnlockNextLevel()
    {
        int currentMax = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
        if (currentMax < 9)
        {
            PlayerPrefs.SetInt("MaxUnlockedLevel", currentMax + 1);
            PlayerPrefs.Save();
        }
    }

    void LoadProgress()
    {
        maxUnlockedLevel = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
    }
}
