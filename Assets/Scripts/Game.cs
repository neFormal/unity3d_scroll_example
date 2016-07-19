using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using System;

public class Game : MonoBehaviour {

	[Serializable]
	private struct UserData {
		[Serializable]
		public struct User {
			public ulong uid;
			public string first_name;
			public string last_name;
			public string photo_200;
			public bool hidden;
		}

		public User[] users;
	}

	[SerializeField]
	private Button runButton;
	[SerializeField]
	private Button backButton;
	[SerializeField]
	private ScrollRect usersList;
	[SerializeField]
	private GameObject userIconPrefab;

	private const string backScene = "Menu";
	private const float scrollVelocity = 100.0f;

	private bool run;
	private float oldDeceleration;

	void Awake () {

		runButton.onClick.RemoveAllListeners();
		runButton.onClick.AddListener(() => {
			if (run)
				return;

			oldDeceleration = usersList.decelerationRate;
			usersList.decelerationRate = 1.0f;
			usersList.velocity = new Vector2(0.0f, scrollVelocity);
			run = true;
		});

		usersList.onValueChanged.RemoveAllListeners();
		usersList.onValueChanged.AddListener(v => {
			if (!run || v.y != 0)
				return;

			usersList.decelerationRate = oldDeceleration;
			run = false;
		});

		backButton.onClick.RemoveAllListeners();
		backButton.onClick.AddListener(() => SceneManager.LoadScene(backScene));

		LoadUserData();
	}
	
	private void LoadUserData() {

		if (userIconPrefab == null) {
			Debug.LogError("user icon prefab is not set");
			return;
		}

		var usersAsset = Resources.Load<TextAsset>("users");
		if (usersAsset == null) {
			Debug.LogError("cant load users.json asset");
			return;
		}

		string usersJson = string.Format("{{\"users\": {0} }}", usersAsset.text);
		var userData = JsonUtility.FromJson<UserData>(usersJson);
		var users = userData.users;

		// set content area size
		var listTransformRect = usersList.GetComponent<RectTransform>();
		var iconTransformRect = userIconPrefab.GetComponent<RectTransform>();

		float listHeight = iconTransformRect.rect.height * users.Length;
		usersList.content.offsetMin = new Vector2(usersList.content.offsetMin.x, -listHeight);
		usersList.content.offsetMax = new Vector2(usersList.content.offsetMax.x, 0);
		////

		for (int i = 0; i < users.Length; i++) {
			var u = users[i];

			var icon = GameObject.Instantiate(userIconPrefab);
			icon.transform.SetParent(usersList.content.transform, false);

			// set item position and size
			var rectTransform = icon.GetComponent<RectTransform>();
			rectTransform.offsetMin = new Vector2(
				listTransformRect.rect.x + listTransformRect.rect.width/2 - iconTransformRect.rect.width/2,
				usersList.content.rect.height/2 - (iconTransformRect.rect.height * (i+1))
			);
			rectTransform.offsetMax = new Vector2(
				rectTransform.offsetMin.x + iconTransformRect.rect.width,
				rectTransform.offsetMin.y + iconTransformRect.rect.height
			);
			////

			var userIcon = icon.GetComponent<UserIcon>();
			userIcon.uid.text = u.uid.ToString();
			userIcon.firstName.text = u.first_name;

			if (userIcon.avatar != null)
				StartCoroutine(downloadImage(u.photo_200, userIcon.avatar));
			else
				Debug.LogError("no image component on user avatar element");
		}
	}

	private IEnumerator downloadImage(string url, Image targetImage) {
		var www = new WWW(url);
		yield return www;

		var sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
		targetImage.sprite = sprite;
		targetImage.color = Color.white;
		www.Dispose();
	}
}
