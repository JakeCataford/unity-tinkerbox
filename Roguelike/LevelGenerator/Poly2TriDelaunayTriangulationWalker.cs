using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Poly2TriDelaunayTriangulationWalker {
	private List<Poly2Tri.DelaunayTriangle> tris;
	private int currentTriangleIndex = 0;
	private int concentricityLimiter = 0;
	private List<int> alreadyWalkedIndecies = new List<int>();

	public Poly2TriDelaunayTriangulationWalker(List<Poly2Tri.DelaunayTriangle> set) {
		tris = set;
		if (set.Count == 1) {
			Debug.LogError("Cannot walk across a set of one triangles.");
		}

		//Get triangle closest to zero.
		List<Poly2Tri.DelaunayTriangle> sortedByDistanceFromOrigin = tris.OrderBy(x => Mathf.Abs(x.Centroid().Xf) + Mathf.Abs (x.Centroid().Yf)).ToList();
		currentTriangleIndex = tris.IndexOf(sortedByDistanceFromOrigin[0]);
	}

	public Poly2Tri.DelaunayTriangle Next() {
		//Get another brownian step
		int index = -1;

		index = GetCurrentNeighborIndexBySide (Mathf.FloorToInt (Random.value * 2.999f));


		if (index == -1) {
			index = GetCurrentNeighborIndexBySide (0);
		}

		if (index == -1) {
			index = GetCurrentNeighborIndexBySide (1);
		}

		if (index == -1) {
			index = GetCurrentNeighborIndexBySide (2);
		}

		if (index == -1) {
			Debug.LogError("Triangle has no neighbors... Awkward....");
		}

		currentTriangleIndex = index;
		return tris [index];
	}

	private int GetCurrentNeighborIndexBySide(int sideNumber) {

		Poly2Tri.DelaunayTriangle currentTriangle = tris [currentTriangleIndex];
		if (sideNumber < 0 || sideNumber > 2) {
			Debug.LogError ("Cannot resolve the " + sideNumber + "th side of a triangle.");
		}

		Poly2Tri.TriangulationPoint p1 = null;
		Poly2Tri.TriangulationPoint p2 = null;
		//Get comparing points
		switch (sideNumber) {
			case 1:
			p1 = currentTriangle.Points[0];
			p2 = currentTriangle.Points[1];
			break;
			case 2:
			p1 = currentTriangle.Points[1];
			p2 = currentTriangle.Points[2];
			break;
			case 3:
			p1 = currentTriangle.Points[2];
			p2 = currentTriangle.Points[0];
			break;
		}

		Poly2Tri.DelaunayTriangle neighbor = null;
		foreach (Poly2Tri.DelaunayTriangle tri in tris) {
			if(tri.Points.Contains(p1) && tri.Points.Contains(p2)) {
				if(tri != currentTriangle) {
					neighbor = tri;
				}
			}
		}

		if (neighbor != null) {
			alreadyWalkedIndecies.Add(tris.IndexOf(neighbor));
			return tris.IndexOf (neighbor);
		} else {
			return -1;
		}

	}

	public void SetCurrentTriangleIndex(int index) {
		currentTriangleIndex = index;
	}

}
