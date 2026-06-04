using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Панели")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private GameObject characterPanel; // Панель выбора персонажей
    [Header("Admin Panel")]
    [SerializeField] private GameObject adminPanel; // Перетащи панель в инспекторе
    [SerializeField] private Button openAdminBtn;    // Кнопка для открытия (например, в углу)
    [SerializeField] private Button unlockAllBtn;    // Кнопка "Unlock All" внутри панели
    [Header("Кнопки Меню")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button charactersButton; // Кнопка "Герои"
    [SerializeField] private Button quitButton;

    [Header("Кнопки Персонажей")]
    [SerializeField] private Button selectNormalBtn;
    [SerializeField] private Button selectTankBtn;
    [SerializeField] private Button selectVampireBtn;
    [SerializeField] private Button backFromCharBtn;

    [Header("Кнопки Уровней")]
    [SerializeField] private Button level1Button;
    [SerializeField] private Button backFromLevelBtn;

    // Цвета для выделения
    private Color selectedColor = Color.green;
    private Color normalColor = Color.white;

    void Start()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
        ShowPanel(mainPanel);
        UpdateCharacterButtons(); // Подсвечиваем текущего
    }

    void Awake()
    {
        playButton.onClick.AddListener(() => ShowPanel(levelPanel));
        charactersButton.onClick.AddListener(() => ShowPanel(characterPanel));
        quitButton.onClick.AddListener(() => Application.Quit());

        backFromCharBtn.onClick.AddListener(() => ShowPanel(mainPanel));
        backFromLevelBtn.onClick.AddListener(() => ShowPanel(mainPanel));

        level1Button.onClick.AddListener(() => SceneManager.LoadScene(1));

        // Выбор персонажей
        selectNormalBtn.onClick.AddListener(() => SelectCharacter("Normal"));
        selectTankBtn.onClick.AddListener(() => SelectCharacter("Tank"));
        selectVampireBtn.onClick.AddListener(() => SelectCharacter("Vampire"));
        if (openAdminBtn) openAdminBtn.onClick.AddListener(() => adminPanel.SetActive(true));
        if (unlockAllBtn) unlockAllBtn.onClick.AddListener(UnlockEverything);
    }
    public void UnlockEverything()
    {
        PlayerPrefs.SetInt("MaxUnlockedLevel", 9); // Разблокируем все 9
        PlayerPrefs.Save();
        Debug.Log("Все уровни разблокированы!");
        SceneManager.LoadScene(0); // Перезагружаем меню, чтобы кнопки обновились
    }
    void SelectCharacter(string charName)
    {
        PlayerPrefs.SetString("SelectedCharacter", charName);
        UpdateCharacterButtons();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.clickUI);
    }

    void UpdateCharacterButtons()
    {
        string selected = PlayerPrefs.GetString("SelectedCharacter", "Normal");

        SetBtnColor(selectNormalBtn, selected == "Normal");
        SetBtnColor(selectTankBtn, selected == "Tank");
        SetBtnColor(selectVampireBtn, selected == "Vampire");
    }

    void SetBtnColor(Button btn, bool isSelected)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor = isSelected ? selectedColor : normalColor;
        cb.selectedColor = isSelected ? selectedColor : normalColor;
        btn.colors = cb;
    }

    void ShowPanel(GameObject panelToShow)
    {
        mainPanel.SetActive(false);
        levelPanel.SetActive(false);
        characterPanel.SetActive(false);
        panelToShow.SetActive(true);
    }
}