using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridTile : MonoBehaviour, ISelectable
{
	public event Action<SelectableType, string> selected;
	// public event Action<int, int> selected;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;
	private string _id;
	// private int _columnIndex;
	// private int _rowIndex;

	public float size { get { return _spriteRenderer.bounds.size.x; } }
	public Color color { set { _spriteRenderer.color = value; } } 
	public SelectableType type { get { return SelectableType.Tile; } } 
	public Vector2 position { get { return transform.localPosition; } }

	public void Init(Vector3 tilePosition, int tileColumnIdx, int tileRowIdx, Color tileColor)
	{
		_id = string.Format("{0}_{1}", tileColumnIdx, tileRowIdx);
		// _columnIndex = tileColumnIdx;
		// _rowIndex = tileRowIdx;
		_spriteRenderer.color = tileColor;
		transform.localPosition = tilePosition;
	}

	private void OnMouseDown()
	{
		if (selected != null)
		{
			selected(type, _id);
		}
	}
}
