using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dreamtastic;

public class BoardSceneController : MonoBehaviour
{
	[Header("Tokens")]
	[SerializeField]
	private Token[] _tokens;
	[Header("Display")]
	[SerializeField]
	private Button _startButton;
	[SerializeField]
	private Button _stopButton;
	[SerializeField]
	private Text _timeTotalText;
	[SerializeField]
	private Text _timeRemainderText;
	[SerializeField]
	private Text _turnText;
	[Header("Values")]
	[SerializeField]
	private float _moveDistance = 10f;
	// INFO: Time variables are in seconds
	[SerializeField]
	private float _intervalTime = 300f;
	private float _targetTime = 0f;
	private float _currentTime = 0f;
	private int _currentTurn = -1;

	private void Start()
	{
		SetUtilityStates(false);
		
		// INFO: Initialize tokens to capture origin positions of tokens. Note the order of execution.
		// https://docs.unity3d.com/Manual/ExecutionOrder.html
		for (int idx = 0; idx < _tokens.Length; ++idx)
		{
			_tokens[idx].Initialize();
		}

		ResetTurn();
		ResetTime();
		ResetTokens();
	}

	private void StartSimulation()
	{
		SetUtilityStates(true);

		// INFO: Start simulation via DreamRoutine. A Coroutine or Update function can be used to the same effect, however note the optimizations
		// in the documentation. https://bitbucket.org/dreamtasticgames/dreamroutines/src/master/
		DreamRoutine.StartRoutine(gameObject, DoSimuationRoutine());
	}

	private void StopSimulation()
	{
		SetUtilityStates(false);
		DreamRoutine.StopRoutines(gameObject);

		ResetTurn();
		ResetTime();
		ResetTokens();
	}
	
	private void QueueTokenMove(int direction)
	{
		Vector2 moveDelta;
		switch (direction)
		{
			case 0:
			moveDelta = new Vector2(0, _moveDistance);
			break;
			case 1:
			moveDelta = new Vector2(0, -_moveDistance);
			break;
			case 2:
			moveDelta = new Vector2(-_moveDistance, 0);
			break;
			case 3:
			default:
			moveDelta = new Vector2(_moveDistance, 0);
			break;
		}
		
		// TODO: This artificially limits control to a single token. Try adding the ability to control mutiple tokens
		_tokens[0].Queue(moveDelta);
	}

	private IEnumerator<IDRInstruction> DoSimuationRoutine()
	{
		while(true)
		{
			_currentTime += Time.deltaTime;
			SimulateTurn();
			SimulateTime();
			yield return DreamRoutine.update;
		}
	}

	private void SimulateTurn()
	{
		int turn = (int)Math.Floor(_currentTime / _intervalTime);
		if (turn > _currentTurn)
		{
			_currentTurn = turn;
			for (int idx = 0; idx < _tokens.Length; ++idx)
			{
				_tokens[idx].SimulateTurn();
			}
		}

		DisplayTurn();
	}

	private void SimulateTime()
	{
		float turnProgress = (_currentTime % _intervalTime) / _intervalTime;
		for (int idx = 0; idx < _tokens.Length; ++idx)
		{
			_tokens[idx].SimulateTime(turnProgress);
		}

		DisplayTime();
	}

	private void ResetTokens()
	{
		for (int idx = 0; idx < _tokens.Length; ++idx)
		{
			_tokens[idx].Reset();
		}
	}

	private void ResetTime()
	{
		_currentTime = 0f;
		_targetTime = _intervalTime;
		DisplayTime();
	}

	private void ResetTurn()
	{
		_currentTurn = -1;
		DisplayTurn();
	}

	
	private void DisplayTime()
	{
		_timeTotalText.text = TimeSpan.FromSeconds(_currentTime).ToString();
		_timeRemainderText.text = TimeSpan.FromSeconds(_intervalTime - (_currentTime % _intervalTime)).ToString();
	}

	private void DisplayTurn()
	{
		_turnText.text = (_currentTurn + 1).ToString();
	}	

	private void SetUtilityStates(bool simulationOn)
	{
		_startButton.interactable = !simulationOn;
		_stopButton.interactable = simulationOn;
	}
	
	public void OnStartSimulation()
	{
		StartSimulation();
	}
	
	public void OnStopSimulation()
	{
		StopSimulation();
	}

	public void OnQueueTokenMove(int direction)
	{
		QueueTokenMove(direction);
	}
}