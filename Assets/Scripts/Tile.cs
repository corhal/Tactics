using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	int x;
	int y;
	public int X { get { return x; } }
	public int Y { get { return y; } }

	public void InitializeTile (int x, int y) {
		this.x = x;
		this.y = y;
		transform.position = new Vector2 (x, y);
	}
}
