using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public PathfindingObject TestPathfindingObject;

	public List <string> TestList;

	public int Width;
	public int Height;

	public Tile TilePrefab;
	public Board GameBoard;

	void Awake () {
		TestList = new List<string> ();
		TestList.Add ("foo");
		TestList.Remove ("bar");
		GameBoard = new Board (Width, Height, TilePrefab);
	}

	void Update () {
		if (Input.GetMouseButtonUp(0)) {
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			Tile destination = GameBoard.GetTileByPosition (mousePosition);
			TestPathfindingObject.MakePath (destination);
		}
	}
}
