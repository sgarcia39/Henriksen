using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct GridActorDef
{
	public int columnIndex;
	public int rowIndex;
	public Color color;
	public float speed;
}

public class GridActor : MonoBehaviour, ISelectable
{
	public event Action<SelectableType, string> selected;
	// public event Action<int> selected;

	public bool active;
	public Vector2 targetPosition;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;
	private string _id;

	public float speed { get; private set; }
	public SelectableType type { get { return SelectableType.Actor; } } 
	public Vector2 position { get { return transform.localPosition; } set { transform.localPosition = value; }}

	public void Init(Vector3 actorPosition, int actorId, Color actorColor, float actorSpeed)
	{
		_id = actorId.ToString();
		_spriteRenderer.color = actorColor;
		speed = actorSpeed;
		transform.localPosition = actorPosition;
	}

	private void OnMouseDown()
	{
		if (selected != null)
		{
			selected(type, _id);
		}
	}
}
