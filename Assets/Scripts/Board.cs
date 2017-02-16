using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Board {
	
	int width;
	int height;

	public static Board Instance;

	Tile[][] boardTiles;
	public Tile[][] BoardTiles { get { return boardTiles; } }

	public Board (int width, int height, Tile tilePrefab) {
		this.width = width;
		this.height = height;

		boardTiles = new Tile[this.width] [];
		for (int i = 0; i < this.width; i++) {
			boardTiles [i] = new Tile[this.height];
			for (int j = 0; j < this.height; j++) {
				boardTiles [i] [j] = GameObject.Instantiate (tilePrefab).GetComponent <Tile> ();
				boardTiles [i] [j].InitializeTile (i, j);
			}
		}

		List<Tile> neighbors = new List<Tile> ();
		for (int i = 0; i < boardTiles.Length; i++) {
			for (int j = 0; j < boardTiles[i].Length; j++) {
				neighbors.Clear ();
				if (i - 1 >= 0) { // left neighbor					
					neighbors.Add (boardTiles [i - 1] [j]);
				}
				if (i + 1 < boardTiles.Length) { // right neighbor
					neighbors.Add (boardTiles [i + 1] [j]);
				}
				if (j - 1 >= 0) { // bottom neighbor
					neighbors.Add (boardTiles [i] [j - 1]);
				}
				if (j + 1 < boardTiles[i].Length) {
					neighbors.Add (boardTiles [i] [j + 1]);
				}
				if (neighbors.Count == 0) {
					throw new UnityException ("The tile has no neighbors!");
				}
				Tile[] neighborsArray = neighbors.ToArray ();
				boardTiles [i] [j].Neighbors = neighborsArray;
			}
		}

		Instance = this;
	}

	public Tile GetTileByPosition (Vector2 position) {
		Tile tile = null;
		Debug.Log (position);
		int x = (int)Mathf.Round(position.x);
		int y = (int)Mathf.Round(position.y);
		if (boardTiles [x] [y] != null) {
			tile = boardTiles [x] [y];
		}
		Debug.Log ("Returned tile: " + tile.X + ":" + tile.Y);
		return tile;
	}
}
