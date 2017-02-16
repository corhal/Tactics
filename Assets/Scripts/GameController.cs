using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Priority_Queue;

public class GameController : MonoBehaviour {
	
	public PathfindingObject TestPathfindingObject;

	public int Width;
	public int Height;

	public Tile TilePrefab;
	public Board GameBoard;

	void Awake () {
		GameBoard = new Board (Width, Height, TilePrefab);
	}

	void Start () {
		
	}

	void Update () {
		if (Input.GetMouseButtonUp(0)) {
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			Tile destination = GameBoard.GetTileByPosition (mousePosition);
			TestPathfindingObject.MakePath (destination);
		}
	}
}
