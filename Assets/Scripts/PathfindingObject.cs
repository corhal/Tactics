using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class PathfindingObject : MonoBehaviour {

	bool shouldMove;
	public Tile currentTile;



	/// <summary>
	/// The radius around from each node that this object will go to. If higher it will walk further away from nodes,
	/// if lower it will walk closer to nodes.
	/// </summary>
	public float SpillDistance = .1f;

	/// <summary>
	/// Determines how fast this object will move.
	/// </summary>
	public float Speed = 10f;

	public Vector2 Offset = new Vector2(0, 0);

	protected List<Tile> Path = new List<Tile>();

	private int moveCounter = 0;

	void Start()
	{
		currentTile = Board.Instance.GetTileByPosition (transform.position);
		transform.position = currentTile.Coordinates;
	}

	void FixedUpdate()
	{
		if (shouldMove) {
			Move ();
		}
	}

	/// <summary>
	/// Creates a path to the desired node, and begins walking the path
	/// </summary>
	/// <param name="destination">Desired desination node</param>
	public void MakePath (Tile destination)
	{		
		SetPath (AStar (currentTile, destination));
	}

	/// <summary>
	/// Hands the object a new path, which it will immediately begin to walk
	/// </summary>
	/// <param name="path">The new path</param>
	public void SetPath (List<Tile> path)
	{
		moveCounter = 0;
		Path = new List<Tile> (path);
		shouldMove = true;
	}

	private void Move()
	{		
		if (moveCounter < Path.Count) {
			transform.position = Vector3.MoveTowards (transform.position, Path [moveCounter].Coordinates + Offset, Time.deltaTime * Speed);
			if (Vector3.Distance (transform.position, Path [moveCounter].Coordinates + Offset) < SpillDistance) {	
				currentTile = Path [moveCounter];
				moveCounter++;
			}
		} else {
			shouldMove = false;
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
		int tileCount = Board.Instance.BoardTiles.Length * Board.Instance.BoardTiles[0].Length;
		FastPriorityQueue<TileNode> openList = new FastPriorityQueue<TileNode> (tileCount);
		//List<Tile> openList = new List<Tile> ();                           // Open list for all candidates(A home for all).
		Tile candidate = start;                                            // The current node candidate which is being analyzed in the algorithm.
		openList.Enqueue (start.MyTileNode, 0);                                              // Start node is added to the openlist
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
				if (!closedList.Contains (n) && !openList.Contains (n.MyTileNode)) {
					n.CalculateTotal (end);
					n.Parent = candidate;
					float priority = n.Total;
					openList.Enqueue (n.MyTileNode, priority);
				}
				// But if in open, then calculate which is the better parent: Candidate or current parent.
				else if (openList.Contains (n.MyTileNode)) {
					if (n.Parent.G > candidate.G) { // g is distance between current and parent...
						// candidate is the better parent as it has a lower combined g value.
						n.Parent = candidate;
					}
				}
			}
			// Calculate h, g and total
			if (openList.Count > 0) {
				candidate = openList.Dequeue ().MyTile;
			}
			if (openList.Count == 0) {
				break;
			}
			// the below for loop, if conditional and method call updates all nodes in openlist.
			/*for (int i = 0; i < openList.Count; i++) {
				openList [i].CalculateTotal (end);
			}
			openList.Sort (delegate(Tile node1, Tile node2) {
				return node1.Total.CompareTo (node2.Total);
			});*/

			closedList.Add (candidate);
		}
		Debug.Log("astar completed in " + astarSteps + " steps. Path found:"+complete);
		path.Reverse ();
		return path;
	}
}
