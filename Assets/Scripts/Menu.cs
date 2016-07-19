using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	[SerializeField]
	private Button playButton;
	[SerializeField]
	private Button quitButton;

	private const string gameScene = "Game";

	void Awake () {
		playButton.onClick.RemoveAllListeners();
		playButton.onClick.AddListener(() => SceneManager.LoadScene(gameScene));

		quitButton.onClick.RemoveAllListeners();
		quitButton.onClick.AddListener(() => Application.Quit());
	}
}
