using UnityEngine;
using System.Collections;

[System.Serializable]
public class Board {
	
	int width;
	int height;

	Tile[][] boardTiles;

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
	}
}
