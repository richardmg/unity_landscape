using UnityEngine;
using System.Collections;

public class PlayerStartupScript : MonoBehaviour, IProjectIOMember {
	public bool moveToGround = true;

	void Start()
	{
		if (moveToGround) {
			Vector3 worldPos = transform.position;
			worldPos.y = Root.instance.landscapeManager.sampleHeight(worldPos) + 1;
			transform.position = worldPos;
		}
	}

	public EntityClass currentEntityClass()
	{
		return Root.instance.uiManager.entityClassPicker.getSelectedEntityClass();
	}

	public void initNewProject()
	{
	}

	public void load(ProjectIO projectIO)
	{
		int id = projectIO.readInt();
	}

	public void save(ProjectIO projectIO)
	{
		projectIO.writeInt(currentEntityClass().id);
	}

}
