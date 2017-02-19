using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class TileNode : FastPriorityQueueNode {

	Tile myTile;
	public Tile MyTile { get { return myTile; } }

	public TileNode (Tile myTile) {
		this.myTile = myTile;
	}
}
