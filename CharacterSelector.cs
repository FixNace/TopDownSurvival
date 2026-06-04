using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    public void SelectNormal()
    {
        PlayerPrefs.SetString("SelectedCharacter", "Normal");
        Debug.Log("Выбран: Normal");
    }

    public void SelectTank()
    {
        PlayerPrefs.SetString("SelectedCharacter", "Tank");
        Debug.Log("Выбран: Tank");
    }

    public void SelectVampire()
    {
        PlayerPrefs.SetString("SelectedCharacter", "Vampire");
        Debug.Log("Выбран: Vampire");
    }
}