using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ID : MonoBehaviour
{
    public Image photo;
    public TextMeshProUGUI name;
    public TextMeshProUGUI age;
    public TextMeshProUGUI height;
    public TextMeshProUGUI eyeColor;
    public TextMeshProUGUI gender;
    public TextMeshProUGUI address;
    public TextMeshProUGUI expiryDate;
    public Transform container;
    public Button hideButton;

    public void Init(Sprite photo, string name, int age, int height, string eyeColor, string gender)
    {
        this.photo.sprite = photo;
        this.name.text = "Name: " + name;
        this.age.text = "Age: " + age.ToString();
        this.height.text = "Height: " + height + "cm";
        this.eyeColor.text = "Eye Color: " + eyeColor;
        this.gender.text = "Gender: " + gender;
    }

    public void ToggleContainer()
            {
        container.gameObject.SetActive(!container.gameObject.activeSelf);
    }
}
