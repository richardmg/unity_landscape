using UnityEngine;
using System.Collections;

public class PlayerStartupScript : MonoBehaviour {
	public bool moveToGround = true;

	public EntityClass currentEntityClass;

	void Start()
	{
		if (moveToGround) {
			Vector3 worldPos = transform.position;
			worldPos.y = Root.instance.landscapeManager.sampleHeight(worldPos) + 1;
			transform.position = worldPos;
		}
	}

	public void initNewProject()
	{
		currentEntityClass = Root.instance.entityManager.getEntity(0);
		Debug.Assert(currentEntityClass != null);
	}

	public void load(ProjectIO projectIO)
	{
		int id = projectIO.readInt();
		currentEntityClass = Root.instance.entityManager.getEntity(id);
		Debug.Assert(currentEntityClass != null);
	}

	public void save(ProjectIO projectIO)
	{
		projectIO.writeInt(currentEntityClass.id);
	}

}
