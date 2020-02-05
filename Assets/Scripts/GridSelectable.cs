using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SelectableType
{
	Tile,
	Actor
}

interface ISelectable
{
	event Action<SelectableType, string> selected;
	Transform transform { get; } 
	SelectableType type { get; }
	Vector2 position { get; } 
}