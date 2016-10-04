using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour {
	public GameObject background;
	public GameObject firstPerson;
	public GameObject colorPicker;
	public GameObject paintEditor;

	void Start()
	{
		hideUI();
		firstPerson.SetActive(true);
	}

	public void hideUI()
	{
		background.SetActive(false);
		firstPerson.SetActive(false);
		colorPicker.SetActive(false);
		paintEditor.SetActive(false);
	}

	public void showUI(GameObject ui)
	{
		hideUI();
		if (ui == firstPerson) {
			firstPerson.SetActive(true);
		} else if (ui == paintEditor) {
			background.SetActive(true);
			paintEditor.SetActive(true);
		} else if (ui == colorPicker) {
			background.SetActive(true);
			colorPicker.SetActive(true);
		}
	}

}
