using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public int Width;
	public int Height;

	public Tile TilePrefab;
	public Board GameBoard;

	void Start () {
		GameBoard = new Board (Width, Height, TilePrefab);
	}
}
