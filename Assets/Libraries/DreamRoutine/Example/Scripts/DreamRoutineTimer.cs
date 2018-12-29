using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;

public class DreamRoutineTimer : MonoBehaviour
{
	public event Action<float> Log;

	[SerializeField]
	private float startTime = 2f;
	[SerializeField]
	private float refreshTime = 1f;
	[SerializeField]
	private bool logToConsole;

	private Stopwatch _stopWatch;
	private WaitForSeconds _waitForSeconds;
	private long _totalTicks;
    private long _previousTicks;
	private long _index;

	private void Awake()
	{
		_stopWatch = new Stopwatch();
		_waitForSeconds = new WaitForSeconds(refreshTime);
		StartCoroutine(LogRoutine());
	}

	private void Update ()
	{
		if (Time.time < startTime) return;
		_stopWatch.Start();
	}

	private void LateUpdate()
	{
		if (Time.time < startTime) return;
		_stopWatch.Stop();
		_index++;
		_previousTicks = _stopWatch.ElapsedTicks;
		_totalTicks += _previousTicks;
		_stopWatch.Reset();
	}

	private IEnumerator LogRoutine()
	{
		while (true)
		{
			yield return _waitForSeconds;
			if (_index > 0)
			{
				float previousTime = (float)_previousTicks / Stopwatch.Frequency * 1000f;
				if (logToConsole)
				{
					UnityEngine.Debug.Log("Previous time : " + previousTime + "ms");
				}

				if (Log != null)
				{
					Log(previousTime);
				}
			}
		}
	}
}
