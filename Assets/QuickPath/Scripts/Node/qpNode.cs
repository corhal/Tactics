using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Basic Node
/// </summary>
public abstract class qpNode
{
    /// <summary>
    /// The nodes with which this node is connected to.
    /// </summary>
    public List<qpNode> Contacts = new List<qpNode>();

    public List<qpNode> NonDiagonalContacts = new List<qpNode>();
    /// <summary>
    /// Whether or not objects may traverse this node.
    /// </summary>
    public bool traverseable = true;

    /// <summary>
    /// Whether or not this node is outdated.
    /// </summary>
    public bool outdated = false;

    private float _g = 1;
	public float G { get { return _g; } }

    private float _h;
    private float _total;
	public float Total { get { return _total; } }

    private qpNode _parent;
	public qpNode Parent { get { return _parent; } set { this._parent = value; } }

    private Vector3 _coordinate;
	public Vector3 Coordinate { get { return _coordinate; } protected set { this._coordinate = value; } }
	public List<Vector3> ScanRayCasts = new List<Vector3> ();

    /// <summary>
    /// Creates a mutual connection between this node and another node.
    /// </summary>
    /// <param name="node">the other node.</param>
    /// <param name="diagonal">Is the other node diagonally placed from this?</param>
    public void SetMutualConnection (qpNode node, bool diagonal = false)
	{
		if (node != null) {
			if (!Contacts.Contains (node)) {
				Contacts.Add (node);
			}
			if (!node.Contacts.Contains (this)) {
				node.Contacts.Add (this);
			}
			if (!diagonal) {
				if (!NonDiagonalContacts.Contains (node)) {
					NonDiagonalContacts.Add (node);
				}
				if (!node.NonDiagonalContacts.Contains (this)) {
					node.NonDiagonalContacts.Add (this);
				}
			}
		}
	}

    /// <summary>
    /// Sets connection to another node.
    /// </summary>
    /// <param name="node">The other node</param>
    public void SetConnection (qpNode node)
	{
		if (node != null) {
			if (!Contacts.Contains (node)) {
				Contacts.Add (node);
			}
		}
	}
    /// <summary>
    /// Remove a mutual connection between this node and another node.
    /// </summary>
    /// <param name="otherNode">The other node that this node is currently connected to.</param>
    public void RemoveMutualConnection(qpNode otherNode)
    {
		if (otherNode != null) {			
			Contacts.Remove (otherNode);
			NonDiagonalContacts.Remove (otherNode);
			otherNode.Contacts.Remove (this);
			otherNode.NonDiagonalContacts.Add (this);
		}
    }

    public bool CanConnectTo(qpNode candidate)
	{
		int steps = (int)(Vector3.Distance (candidate.Coordinate, _coordinate) * 2);
		float castDistance = Mathf.Abs ((candidate.Coordinate - _coordinate).y) + 4;
		if (qpManager.Instance.KnownUpDirection == qpGrid.Axis.Z) {
			castDistance = Mathf.Abs ((candidate.Coordinate - _coordinate).z) + 4;
		}

		Vector3 difference = (candidate.Coordinate - _coordinate) / steps;
		RaycastHit info;
		Vector3 upDirection = qpManager.Instance.UpVector;
		Vector3 downDirection = -upDirection;
		Vector3 myCoordinate = _coordinate + (upDirection / 5); // ...why?
		Vector3 destinationPoint = candidate.Coordinate - difference + (upDirection / 5);
		if (Physics.Linecast (myCoordinate, destinationPoint, out info)) { // обскурная херня, без которой образуются лишние связи
			return false;
		}

		//Ray cast downward along the previously casted straight line
		for (int i = 1; i < steps; i++) {
			int ignoreHits = 0;
			RaycastHit[] hits;

			Vector3 rayCastPositionStart = new Vector3 (myCoordinate.x + (difference.x * i), myCoordinate.y + (castDistance / 2), myCoordinate.z + (difference.z * i));
			if (upDirection == new Vector3 (0, 0, 1)) {
				rayCastPositionStart.y = myCoordinate.y + (difference.y * i);
				rayCastPositionStart.z = myCoordinate.z + (castDistance / 2);
			}
			ScanRayCasts.Add (rayCastPositionStart);

			hits = Physics.RaycastAll (rayCastPositionStart, downDirection, castDistance);
			if (hits.Length == 0) {
				return false;
			}
			List<float> heightPoints = new List<float> ();
			foreach (RaycastHit hit in hits) {
				if (qpManager.Instance.disallowedTags.Contains (hit.collider.gameObject.tag)) {
					return false;
				} else if (qpManager.Instance.ignoreTags.Contains (hit.collider.gameObject.tag)) {
					ignoreHits++;
				} else {
					if (qpManager.Instance.KnownUpDirection == qpGrid.Axis.Y) {
						heightPoints.Add (hit.point.y);
					} else if (qpManager.Instance.KnownUpDirection == qpGrid.Axis.Z) {
						heightPoints.Add (hit.point.z);
					}
				}
			}
			if (hits.Length == ignoreHits) {
				return false;
			}
		}
		return true;
	}

    /// <summary>
    /// Used for A*. Calculates _total - sum of distances between current and end, and current and parent nodes.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public float CalculateTotal(qpNode end)
    {
		_h = CalculateH (end);
		if (_parent != null) {
			_g = CalculateG (_parent) + _parent.G;
		} else {
			_g = 1;
		}
        _total = _g + _h;
        return _total;
    }
    /// <summary>
    /// Used for A*. Calculates _h - distance between current and end nodes.
    /// </summary>
    public float CalculateH(qpNode end)
    {
		return Vector3.Distance(_coordinate, end.Coordinate);
    }
    /// <summary>
    /// Used for A*. Calculates _g - distance between current and parent nodes.
    /// </summary>
    public float CalculateG(qpNode parent)
    {
		return Vector3.Distance(_coordinate, parent.Coordinate);
    }
}
