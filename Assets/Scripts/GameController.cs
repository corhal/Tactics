using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public List <string> TestList;

	public int Width;
	public int Height;

	public Tile TilePrefab;
	public Board GameBoard;

	void Awake () {
		TestList = new List<string> ();
		TestList.Add ("foo");
		TestList.Remove ("bar");
	}

	void Start () {
		GameBoard = new Board (Width, Height, TilePrefab);
	}
}
