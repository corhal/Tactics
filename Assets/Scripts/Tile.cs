using UnityEngine;
using System.Collections;
using Priority_Queue;

public class Tile : MonoBehaviour {

	TileNode myTileNode;
	public TileNode MyTileNode { get { return myTileNode; } }

	int x;
	int y;
	public int X { get { return x; } }
	public int Y { get { return y; } }

	float h;
	float g;
	public float G { get { return g; } }
	float total;
	public float Total { get { return total; } }
	Tile parent;
	public Tile Parent { get { return parent; } set { this.parent = value; } }

	Vector2 coordinates;
	public Vector2 Coordinates { get { return coordinates; } }

	Tile[] neighbors;
	public Tile[] Neighbors { 
		get {
			return neighbors; 
		}
		set {
			neighbors = new Tile[value.Length];
			value.CopyTo (neighbors, 0);
		} 
	}

	public void InitializeTile (int x, int y) {
		this.x = x;
		this.y = y;
		transform.position = new Vector2 (x, y);
		coordinates = transform.position;
		myTileNode = new TileNode (this);
	}

	/// <summary>
	/// Used for A*. Calculates total - sum of distances between current and end, and current and parent nodes.
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	/// <returns></returns>
	public float CalculateTotal (Tile end) {
		h = CalculateH (end);
		if (parent != null) {
			g = CalculateG (parent) + parent.G;
		} else {
			g = 1;
		}
		total = g + h;
		return total;
	}
	/// <summary>
	/// Used for A*. Calculates h - distance between current and end nodes.
	/// </summary>
	public float CalculateH (Tile end) {
		//return Vector2.Distance(coordinates, end.Coordinates);
		return ManhattanDistance (this, end);
	}
	/// <summary>
	/// Used for A*. Calculates g - distance between current and parent nodes.
	/// </summary>
	public float CalculateG (Tile parent) {
		//return Vector2.Distance(coordinates, parent.Coordinates);
		return ManhattanDistance (this, parent);
	}

	public override string ToString ()
	{
		return string.Format ("Tile {0}:{1}", X, Y);
	}

	public float ManhattanDistance (Tile start, Tile finish) {
		return (Mathf.Abs (finish.X - start.X) + Mathf.Abs (finish.Y - start.Y));
	}
}
