using UnityEngine;
using UnityEngine.UI;

/*
* @JulienLopez
* @InputFieldPseudo.cs
* @Le script s'attache sur un InputFiled.
*   - Permet de gerer le pseudo du joueur et le référencer à Photon
*/

[RequireComponent(typeof(InputField))]
public class InputFieldPseudo : MonoBehaviour {

    static string playerNamePrefKey = "PlayerName";

    // Use this for initialization
    void Start () {
        string defaultName = "";
        InputField _inputField = GetComponent<InputField>();

        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }

        PhotonNetwork.playerName = defaultName;
    }

    /// <summary>
    /// Set le pseudo du joueur à Photon.
    /// </summary>
    /// <param name="value"></param>
    public void SetPlayerName(string value)
    {
        PhotonNetwork.playerName = value + " ";

        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
