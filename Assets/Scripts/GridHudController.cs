using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dreamtastic;
public class GridHudController : MonoBehaviour 
{
	public GridManager gridManager;
	public Image actor1Image;
	public Text actor1SpeedText;
	public Image actor2Image;
	public Text actor2SpeedText;
	public Text activeTimeText;

	public void Start()
	{
		actor1Image.color = gridManager.actorDefs[0].color;
		actor2Image.color = gridManager.actorDefs[1].color;

		actor1SpeedText.text = gridManager.actorDefs[0].speed.ToString() + " Ti/Sec";
		actor2SpeedText.text = gridManager.actorDefs[1].speed.ToString() + " Ti/Sec";

		DreamRoutine.StartRoutine(gameObject, DoSimulation());
	}

	private IEnumerator<IDRInstruction> DoSimulation()
	{
		while(true)
		{
			activeTimeText.text = Time.time.ToString("#.00");
			yield return DreamRoutine.update;
		}
	}
}
