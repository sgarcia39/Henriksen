using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Dreamtastic;

public class DreamRoutineSimple : MonoBehaviour
{
	[Header("Display")]
	[SerializeField]
	private Text _updateActiveText;
	[SerializeField]
	private Text _lateUpdateActiveText;
	[SerializeField]
	private Text _fixedUpdateActiveText;
	[SerializeField]
	private Text _combinedActiveText;

	private int _updateActive;
	private int _lateUpdateActive;
	private int _fixedUpdateActive;

	public void OnStartUpdateRoutines(int count)
	{
		for(int idx = 0; idx < count; ++idx)
		{
			DreamRoutine.StartRoutine(gameObject, DoUpdateRoutine());
		}

		_updateActive += count;
		RefreshRoutineDisplay();
	}

	public void OnStartLateUpdateRoutines(int count)
	{
		for(int idx = 0; idx < count; ++idx)
		{
			DreamRoutine.StartRoutine(gameObject, DoLateUpdateRoutine());
		}

		_lateUpdateActive += count;
		RefreshRoutineDisplay();
	}

#if DR_FIXEDUPDATE
	public void OnStartFixedUpdateRoutines(int count)
	{
		for(int idx = 0; idx < count; ++idx)
		{
			DreamRoutine.StartRoutine(gameObject, DoFixedUpdateRoutine());
		}

		_fixedUpdateActive += count;
		RefreshRoutineDisplay();
	}
#endif

	public void OnStopRoutines()
	{
		DreamRoutine.StopRoutines();
		_updateActive = 0;
		_lateUpdateActive = 0;
		_fixedUpdateActive = 0;
		RefreshRoutineDisplay();
	}

	private void Start()
	{
		RefreshRoutineDisplay();
	}

	private void RefreshRoutineDisplay()
	{
		_updateActiveText.text = _updateActive.ToString();
		_lateUpdateActiveText.text = _lateUpdateActive.ToString();
		_fixedUpdateActiveText.text = _fixedUpdateActive.ToString();
		_combinedActiveText.text = (_updateActive + _lateUpdateActive + _fixedUpdateActive).ToString() +
			" / " + DreamRoutine.maxRoutineCount;
	}

	private IEnumerator<IDRInstruction> DoUpdateRoutine()
	{
		while(true)
		{
			yield return DreamRoutine.update;
		}
	}

	private IEnumerator<IDRInstruction> DoLateUpdateRoutine()
	{
		while(true)
		{
			yield return DreamRoutine.lateUpdate;
		}
	}

#if DR_FIXEDUPDATE
	private IEnumerator<IDRInstruction> DoFixedUpdateRoutine()
	{
		while(true)
		{
			yield return DreamRoutine.fixedUpdate;
		}
	}
#endif
}