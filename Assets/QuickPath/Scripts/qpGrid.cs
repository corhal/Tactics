using UnityEngine;

using System.Collections;
using System;
using System.Collections.Generic;
[AddComponentMenu("QuickPath/Grid")]
[ExecuteInEditMode]
/// <summary>
/// Creates a grid for MoveObjects to move around on.
/// </summary>
public class qpGrid : MonoBehaviour
{
    /// <summary>
    /// Grid map of all nodes generated. Used for finding and setting node connections.
    /// </summary>
    public qpNode[,] gridNodes; // todo: flatten maybe

    /// <summary>
    /// List containing all nodes generated by this grid.
    /// </summary>
	public List<qpNode> allNodes = new List<qpNode>();

    /// <summary>
    /// If true, raycasts will be performed between nodes, and each step along the line will be raycasted to see whether or not its over eligible terrian or not. If not, no connection will be made.
    /// </summary>
    public bool RaycastBetweenNodes = true;

    /// <summary>
    /// Determines how deeply one node will look after other nodes, 1= immediate neighbours, 2=Immediate neighbours, and the neighbours to the immediate neighbours. Etc.
    /// </summary>
    public int NodeConnectionDepth = 2;
    /// <summary>
    /// Start position from which to ray cast for terrain suitable for nodes.
    /// </summary>
    public Vector2 startCoordinate = new Vector3(-50, -50);

    /// <summary>
    /// End position from which to ray cast for terrain suitable for nodes.
    /// </summary>
    public Vector2 endCoordinate = new Vector3(50, 50);

    /// <summary>
    /// List of tags that when raycasted against will not create node.
    /// </summary>
    public List<string> DisallowedTags = new List<string>();
    /// <summary>
    /// List of tags that when raycasted against will not create node.
    /// </summary>
    public List<string> IgnoreTags = new List<string>();

    /// <summary>
    /// Draws a line at each node, and draws a line between each connection between two nodes. Helpful when baking grids.
    /// </summary>
    public bool DrawInEditor = true;

    /// <summary>
    /// The Axises
    /// </summary>
    public enum Axis { Y = 4, Z = 5 };

    /// <summary>
    /// The designated updirection.
    /// </summary>
    public Axis UpDirection = Axis.Y;

    /// <summary>
    /// Indicates where the raycasting for generating nodes should start on the updirection axis.
    /// </summary>
    public float UpRaycastStart = 12;

    /// <summary>
    /// Indicates where the raycasting for generating nodes should end on the updirection axis.
    /// </summary>
    public float UpRayCastEnd = -12;

    /// <summary>
    /// Indicates the distance between each node.
    /// </summary>
    public float spread = 1;

    /// <summary>
    /// Max distance between two nodes before connection is void, this is useful to control the pathing on steep or uneven terrain.
    /// </summary>
    public float MaxNodeDistance = 10;

    private List<Vector3> _lineRayStarts = new List<Vector3>();
    private List<Vector3> _lineRayEnds = new List<Vector3>();
    private qpManager _manager;
    private bool _generateNodes = true;

   void Awake ()
	{
		qpManager.Instance.RegisterNodes (allNodes);

		//Generate new nodes with raycast collision detection
		if (_generateNodes) {
			if (startCoordinate == Vector2.zero && endCoordinate == Vector2.zero && GetComponent<Renderer> () != null) { // start and end are 0 and are in one point
				float width = GetComponent<Renderer> ().bounds.size.x / 50;
				float height = GetComponent<Renderer> ().bounds.size.y / 50;
				startCoordinate = new Vector2 (this.transform.position.x - (width * 0.5f), this.transform.position.y - (height * 0.5f));
				endCoordinate = new Vector2 (startCoordinate.x + width, startCoordinate.y + height);
			}

			_generateGrid ();
		}
		qpManager.Instance.RegisterDisallowedTags (DisallowedTags);
		qpManager.Instance.RegisterIgnoreTags (IgnoreTags);
		qpManager.Instance.KnownUpDirection = UpDirection;
	}

    /// <summary>
    /// Deletes nodes and rebuilds the entire grid.
    /// </summary>
    public void Bake()
    {
        _manager = qpManager.Instance;
        _manager.DelistNodes(allNodes);
        allNodes = new List<qpNode>();
        _generateGrid();
        DontDestroyOnLoad(_manager);
        qpManager.Instance.RegisterDisallowedTags(DisallowedTags);
        qpManager.Instance.RegisterIgnoreTags(IgnoreTags);
        qpManager.Instance.KnownUpDirection = UpDirection;
        Debug.Log("nodes found:" + qpManager.Instance.gridpoints.Count);
    }
   
    void Update ()
	{
		if (DrawInEditor) {
			for (int i = _lineRayStarts.Count; i > 0; i--) {
				Debug.DrawLine (_lineRayStarts [i - 1], _lineRayEnds [i - 1], Color.blue, 0, true);
			}
			foreach (qpNode node in allNodes) {
				foreach (qpNode node2 in node.Contacts) {
					Debug.DrawLine (node.Coordinate, node2.Coordinate, Color.blue, 0, true);
				}
			}
		}
	}

    void _generateGrid () // and here is where the dragon lies
	{
		_lineRayEnds = new List<Vector3> ();
		_lineRayStarts = new List<Vector3> ();
		Vector2 size = new Vector2 (Mathf.Abs (startCoordinate.x - endCoordinate.x), Mathf.Abs (startCoordinate.y - endCoordinate.y));
		Debug.Log ("Size: " + size);
		gridNodes = new qpNode[(int)Mathf.Ceil (size.x / spread) + 1 + NodeConnectionDepth, (int)Mathf.Ceil (size.y / spread) + 1 + NodeConnectionDepth];
		Debug.Log ("gridNodes size: " + gridNodes.GetLength (0) + ":" + gridNodes.GetLength (1));
		Debug.Log ("For loop bound: " + (size.x / spread) * (size.y / spread));
		for (int i = 0; i < (size.x / spread) * (size.y / spread); i++) {
			int row = (int)Mathf.Floor ((i * spread) / size.x);
			float x = ((i * spread) - (row * size.x)) + startCoordinate.x;
			float y = (row * spread) + startCoordinate.y;
			Vector3 rayCastPositionStart = Vector3.zero;
			Vector3 rayDirection = Vector3.zero;
			if (UpDirection == Axis.Z) {
				rayCastPositionStart = new Vector3 (x, y, UpRaycastStart);
				rayDirection = new Vector3 (0, 0, -1f); // axis direction!
			} else if (UpDirection == Axis.Y) {
				rayCastPositionStart = new Vector3 (x, UpRaycastStart, y);
				rayDirection = new Vector3 (0, -1f);
			}
			qpGridNode gridNode = null;
			RaycastHit[] hits;
			bool placeNode = true;
			Vector3 point = Vector3.zero;
			hits = Physics.RaycastAll (rayCastPositionStart, rayDirection, 100.0F);
			int o = 0;
			int countIgnoreHits = 0;
			if (hits.Length == 0) {
				placeNode = false;
			}
			while (o < hits.Length) {
				RaycastHit hit = hits [o];
				if (DisallowedTags.Contains (hit.collider.gameObject.tag)) {
					placeNode = false;
				} else if (IgnoreTags.Contains (hit.collider.gameObject.tag)) {
					countIgnoreHits++;
				} else if (point == Vector3.zero) {
					point = hit.point;
				} else { // судя по всему, тут мы ищем верхний объект, в который попали
					if (UpDirection == Axis.Y) {
						if (hit.point.y > point.y) {
							point = hit.point;
						}
					} else if (UpDirection == Axis.Z) {
						if (hit.point.z > point.z) {
							point = hit.point;
						}
					}
				}
				o++;
			}
			if (o == countIgnoreHits) {
				placeNode = false;
			}
			if (placeNode) {
				//Collision detected on floor // ...on walkable surface?..
				gridNode = new qpGridNode (point);
				allNodes.Add (gridNode);

				if (UpDirection == Axis.Z) { // кажется, чисто косметическая хрень
					rayCastPositionStart.z = point.z + 1;
				} else if (UpDirection == Axis.Y) {
					rayCastPositionStart.y = point.y + 1;
				}

				if (point != Vector3.zero) {
					_lineRayEnds.Add (point);
					_lineRayStarts.Add (rayCastPositionStart);
				}
			}

			int yindex = (int)Mathf.Floor (i / (size.x / spread)); // чувак ловко итерирует двумерный массив плоским образом о_О
			int xindex = (int)(i - (Mathf.Ceil (yindex * (size.x / spread))));

			gridNodes [xindex, yindex] = gridNode;
			//Set connection for gridNode
			if (gridNode != null) {
				Dictionary<qpNode, bool> isDiagonalsByCandidateNodes = new Dictionary<qpNode, bool> ();
				bool isDiagonal = false;

				for (int c = 1; c < (NodeConnectionDepth + 1); c++) {
					if ((xindex - c) > 0) {
						//left
						if (gridNodes [xindex - c, yindex] != null) {
							isDiagonalsByCandidateNodes.Add (gridNodes [xindex - c, yindex], false);
						}
						if ((yindex - c) > 0) {
							//top Left
							for (int nc = c; nc > 0; nc--) { // dude, wtf								
								isDiagonal = true;
								if (c == 0 || nc == 0) {
									isDiagonal = false;
								}
								if (gridNodes [xindex - c, yindex - nc] != null) {
									isDiagonalsByCandidateNodes.Add (gridNodes [xindex - c, yindex - nc], isDiagonal);
								}
							}
						}
					}
					if ((yindex - c) > 0) {
						//top
						if (gridNodes [xindex, yindex - c] != null) {
							isDiagonalsByCandidateNodes.Add (gridNodes [xindex, yindex - c], false);
						}
						for (int nc = c - 1; nc > 0; nc--) {
							isDiagonal = true;
							if ((xindex - nc) < 0 || (yindex - c) < 0) {
								continue;
							}
							if (nc == 0 || c == 0) {
								isDiagonal = false;
							}
							if (gridNodes [xindex - nc, yindex - c] != null) {
								isDiagonalsByCandidateNodes.Add (gridNodes [xindex - nc, yindex - c], isDiagonal);
							}
						}
						//top right
						if (gridNodes [xindex + c, yindex - c] != null) {
							isDiagonalsByCandidateNodes.Add (gridNodes [xindex + c, yindex - c], true);
						}
						for (int nc = c - 1; nc > 0; nc--) {
							isDiagonal = true;
							if ((xindex + nc) == xindex || (yindex - c) == yindex) {
								isDiagonal = false;
							}
							if (gridNodes [xindex + nc, yindex - c] != null) {
								isDiagonalsByCandidateNodes.Add (gridNodes [xindex + nc, yindex - c], isDiagonal);
							}
						}
						//below top right // wat
						for (int nc = c - 1; nc > 0; nc--) {
							isDiagonal = true;
							if ((xindex + c) == xindex || (yindex - nc) == yindex) {
								isDiagonal = false;
							}
							if (gridNodes [xindex + c, yindex - nc] != null) {
								isDiagonalsByCandidateNodes.Add (gridNodes [xindex + c, yindex - nc], isDiagonal);
							}
						}
					}

					foreach (var candidate in isDiagonalsByCandidateNodes) {
						if (RaycastBetweenNodes) {
							if (gridNode.CanConnectTo (candidate.Key) && Vector3.Distance (gridNode.Coordinate, candidate.Key.Coordinate) < MaxNodeDistance) {
								gridNode.SetMutualConnection (candidate.Key, candidate.Value);
							}
						} else if (!RaycastBetweenNodes) {
							if (Vector3.Distance (gridNode.Coordinate, candidate.Key.Coordinate) < MaxNodeDistance) {
								gridNode.SetMutualConnection (candidate.Key, isDiagonal); // looks like it shouldn't be just "isDiagonal"
							}
						}
					}
				}
			}
		}
		qpManager.Instance.RegisterNodes (allNodes);
	}
}