using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIEntityClassPicker : MonoBehaviour {

	public GameObject uiEntityPickerCameraGO;
	public GameObject rawImageGO;

	void OnEnable()
	{
		List<EntityClass> entityClasses = Root.instance.entityManager.allEntityClasses;
		for (int i = 0; i < entityClasses.Count; ++i) {
			Texture2D snapshot = takeSnapshot(entityClasses[i]);
			rawImageGO.GetComponent<RawImage>().texture = snapshot;
		}
	}

	void OnDisable()
	{
	}

	Texture2D takeSnapshot(EntityClass entityClass)
	{
		EntityInstance instance = entityClass.createInstance(uiEntityPickerCameraGO.transform, entityClass.name);
		instance.makeStandalone();
		instance.gameObject.layer = LayerMask.NameToLayer("UIEntityPickerLayer");
		instance.transform.localPosition = new Vector3(0, 0, 1);
		float scale = 0.01f;
		instance.gameObject.transform.localScale = new Vector3(scale, scale, scale);

		Camera camera = uiEntityPickerCameraGO.GetComponent<Camera>();
        RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = camera.targetTexture;
		camera.Render();

		Texture2D snapshot = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
		snapshot.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
		snapshot.Apply();

        RenderTexture.active = currentRT;
		instance.gameObject.SetActive(false);
		GameObject.Destroy(instance);
		return snapshot;
	}

	public void onNewTreeButtonClicked()
	{
		EntityClass entityClass = new EntityClass("SquareTree");
		Root.instance.player.currentEntityClass = entityClass;
		Root.instance.uiManager.entityPainter.setEntityClass(entityClass);
		Root.instance.uiManager.push(Root.instance.uiManager.uiPaintEditorGO, (bool accepted) => {});
	}
}
