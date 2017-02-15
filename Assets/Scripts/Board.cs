using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Board {
	
	int width;
	int height;

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
	}

	/// <summary>
	/// Performs an A* algorithm
	/// </summary>
	/// <param name="start">Starting node</param>
	/// <param name="end">Destination Node</param>
	/// <returns>The fastest path from start node to the end node</returns>
	public List<Tile> AStar(Tile start, Tile end)
	{
		List<Tile> path = new List<Tile> ();                               // will hold the final path
		bool complete = (end == null || start == null) ? true : false;     // Regulates the main while loop of the algorithm
		List<Tile> closedList = new List<Tile> ();                         // Closed list for the best candidates.
		List<Tile> openList = new List<Tile> ();                           // Open list for all candidates(A home for all).
		Tile candidate = start;                                            // The current node candidate which is being analyzed in the algorithm.
		openList.Add (start);                                              // Start node is added to the openlist
		if (start == null || end == null) {
			return null;                                                   // algorithm cannot be executed if either start or end node are null.
		}

		int astarSteps = 0;
		while (openList.Count > 0 && !complete) {                          // ALGORITHM STARTS HERE.
			astarSteps++;
			if (candidate == end) {                                        // If current candidate is end, the algorithm has been completed and the path can be built.
				// DestinationNode = end;
				complete = true;
				bool pathComplete = false;
				Tile node = end;
				while (!pathComplete) {
					path.Add (node);
					if (node == start) {
						pathComplete = true;
					}
					node = node.Parent;
				}
			}

			foreach (Tile n in candidate.Neighbors) { // отсюда можно убирать тайлы, если через них нельзя пройти
				// Mark candidate as parent if not in open nor closed.
				if (!closedList.Contains (n) && !openList.Contains (n)) {
					n.Parent = candidate;
					openList.Add (n);
				}
				// But if in open, then calculate which is the better parent: Candidate or current parent.
				else if (openList.Contains (n)) {
					if (n.Parent.G > candidate.G) { // g is distance between current and parent...
						// candidate is the better parent as it has a lower combined g value.
						n.Parent = candidate;
					}
				}
			}
			// Calculate h, g and total
			if (openList.Count > 0) {
				openList.RemoveAt (0);
			}
			if (openList.Count == 0) {
				break;
			}
			// the below for loop,if conditional and method call updates all nodes in openlist.
			for (int i = 0; i < openList.Count; i++) {
				openList [i].CalculateTotal (end);
			}
			openList.Sort (delegate(Tile node1, Tile node2) {
				return node1.Total.CompareTo (node2.Total);
			});

			candidate = openList [0];
			closedList.Add (candidate);
		}
		// Debug.Log("astar completed in " + astarSteps + " steps. Path found:"+complete);
		path.Reverse ();
		return path;
	}
}
