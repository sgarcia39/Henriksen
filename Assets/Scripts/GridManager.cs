using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dreamtastic;

public class GridManager : MonoBehaviour 
{
	[SerializeField]
	private Transform gridTileTransform;
	[SerializeField]
	private Transform gridActorTransform;
	[SerializeField]
	private Transform gridSelectedTransform;

	[SerializeField]
	private int totalRows;
	[SerializeField]
	private int totalColumns;
	[SerializeField]
	private GridTile tilePrefab;
	[SerializeField]
	private GridActor actorPrefab;
	[SerializeField]
	private SpriteRenderer selectedRendererPrefab;
	[SerializeField]
	private Color tileColor1;
	[SerializeField]
	private Color tileColor2;
	[SerializeField]
	private Color selectedTileColor;
	[SerializeField]
	public GridActorDef[] actorDefs;
	
	private GridTile[,] tiles;
	private GridActor[] actors;
	private GridActor selectedActor;
	private GridTile selectedTile;
	
	private ISelectable selectableSource;
	private ISelectable selectableTarget;

	private SpriteRenderer selectedSourceRenderer;
	private SpriteRenderer selectedTargetRenderer;
	private void Start () 
	{
		GenerateGrid();
		GenerateActors();
		GenerateSelectedRenderers();
		DreamRoutine.StartRoutine(gameObject, DoSimulation());
	}

	private IEnumerator<IDRInstruction> DoSimulation()
	{
		while(true)
		{
			MoveActors();
			yield return DreamRoutine.update;
		}
	}

	public void MoveActors()
	{
		for (int idx = 0; idx < actors.Length; ++idx)
		{
			GridActor actor = actors[idx];
			if (actor.active)
			{
				if (actor.position == actor.targetPosition)
				{
					actor.active = false;
					break;;
				}

				float distance = tilePrefab.size * actor.speed * Time.deltaTime;
				actor.position = Vector2.MoveTowards(actor.position, actor.targetPosition, distance);
			}
		}
	}

	private void GenerateGrid() 
	{
		tiles = new GridTile[totalRows, totalColumns];
		for (int column = 0; column < totalColumns; ++column)
		{
			for (int row = 0; row < totalRows; ++row)
			{
				GridTile tile = Instantiate(tilePrefab, gridTileTransform, false);
				float positionX = column * tilePrefab.size;
				float positionY = row * -tilePrefab.size;
				Vector3 position = new Vector3(positionX, positionY, 0);
				Color color = (row + column) % 2 == 0 ? tileColor1 : tileColor2;
				tile.selected += OnSelected;
				// tile.selected += OnTileSelected;
				tile.Init(position, column, row, color);
				tiles[column, row] = tile;
			}
		}
	}

	private void GenerateActors()
	{
		actors = new GridActor[actorDefs.Length];
		for (int idx = 0; idx < actorDefs.Length; ++idx)
		{
			GridActor actor = Instantiate(actorPrefab, gridActorTransform, false);
			GridActorDef actorDef = actorDefs[idx];
			Vector3 position = tiles[actorDef.columnIndex, actorDef.rowIndex].position;
			actor.selected += OnSelected;
			// actor.selected += OnActorSelected;
			actor.Init(position, idx, actorDef.color, actorDef.speed);
			actors[idx] = actor;
		}
	}

	private void GenerateSelectedRenderers()
	{
		selectedSourceRenderer= Instantiate(selectedRendererPrefab, gridSelectedTransform, false);
		selectedSourceRenderer.enabled = false;
		selectedSourceRenderer.color = selectedTileColor;
		
		selectedTargetRenderer = Instantiate(selectedRendererPrefab, gridSelectedTransform, false);
		selectedTargetRenderer.enabled = false;
		selectedTargetRenderer.color = selectedTileColor;
	}

	private void OnSelected(SelectableType selectableType, string selectableId)
	{
		switch(selectableType)
		{
			case SelectableType.Tile:
				if(selectableSource == null)
				{
					selectableTarget = null;
					selectedTargetRenderer.enabled = false;
					selectableSource = null;
					selectedSourceRenderer.enabled = false;
					return;
				}

				string[] tileId = selectableId.Split('_');
				int tileColumnIdx = Int32.Parse(tileId[0]);
				int tileRowIdx = Int32.Parse(tileId[1]);
				selectableTarget = tiles[tileColumnIdx, tileRowIdx];
				selectedTargetRenderer.transform.SetParent(selectableTarget.transform, false);
				selectedTargetRenderer.enabled = true;

				GridActor actor = (GridActor)selectableSource;
				actor.targetPosition = selectableTarget.position;
				actor.active = true;

				selectableSource = null;
			break;
			case SelectableType.Actor:
				if (selectableSource != null)
				{
					selectableTarget = null;
					selectedTargetRenderer.enabled = false;
					selectableSource = null;
					selectedSourceRenderer.enabled = false;
					return;
				}

				selectableTarget = null;
				selectedTargetRenderer.enabled = false;

				int actorIdx = Int32.Parse(selectableId);
				selectableSource = actors[actorIdx];
				selectedSourceRenderer.transform.SetParent(selectableSource.transform, false);
				selectedSourceRenderer.enabled = true;
			break;
		}
	}

	private void OnTileSelected(int tileColumnIdx, int tileRowIdx)
	{
		if(selectableSource == null)
		{
			selectableTarget = null;
			selectedTargetRenderer.enabled = false;
			selectableSource = null;
			selectedSourceRenderer.enabled = false;
			return;
		}

		selectableTarget = tiles[tileColumnIdx, tileRowIdx];
		selectedTargetRenderer.transform.SetParent(selectableTarget.transform, false);
		selectedTargetRenderer.enabled = true;

		GridActor actor = (GridActor)selectableSource;
		actor.targetPosition = selectableTarget.position;
		actor.active = true;

		selectableSource = null;
	}

	private void OnActorSelected(int actorId)
	{
		// INFO: Deselect target and source if selectables source exists. Needs an update for pursuit
		if (selectableSource != null)
		{
			selectableTarget = null;
			selectedTargetRenderer.enabled = false;
			selectableSource = null;
			selectedSourceRenderer.enabled = false;
			return;
		}

		selectableTarget = null;
		selectedTargetRenderer.enabled = false;

		selectableSource = actors[actorId];
		selectedSourceRenderer.transform.SetParent(selectableSource.transform, false);
		selectedSourceRenderer.enabled = true;
	}
}
