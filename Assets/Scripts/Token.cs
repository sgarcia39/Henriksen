using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// INFO: BoardSceneController is a Monobehaviour, but Tokens are not. Tokens are managed by the BoardSceneController
[Serializable]
public class Token
{
	[SerializeField]
	private string _name;
	[SerializeField]
	private Image _image;
	private bool _active;

	private Vector2 _originPosition;
	private Queue<Vector2> _queuedPositions = new Queue<Vector2>();

	private Vector2 _startPosition;
	private Vector2 _endPosition;
	
	public void Initialize()
	{
		_originPosition = _image.rectTransform.anchoredPosition;
	}

	public void Queue(Vector2 position)
	{
		_queuedPositions.Enqueue(position);
	}
	
	public void Reset()
	{
		_image.rectTransform.anchoredPosition = _originPosition;
		_startPosition = _originPosition;
		_endPosition = _originPosition;
		_queuedPositions.Clear();
	}

	public void SimulateTurn()
	{
		if (_queuedPositions.Count > 0)
		{
			_active = true;
			_startPosition = _endPosition;
			_endPosition = _queuedPositions.Dequeue() + _endPosition;
		}
		else
		{
			_active = false;
		}
	}
	
	public void SimulateTime(float turnProgress)
	{
		if (_active)
		{
			Vector2 currentPosition = Vector2.Lerp(_startPosition, _endPosition, turnProgress);
			_image.rectTransform.anchoredPosition = currentPosition;
		}
	}
}