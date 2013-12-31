using UnityEngine;
using System.Collections.Generic;

public class Poly2TriAdapter {

	public static List<Poly2Tri.TriangulationPoint> Vector2ToP2TList(List<Vector2> points) {
		List<Poly2Tri.TriangulationPoint> p2tPoints = new List<Poly2Tri.TriangulationPoint> ();
		foreach (Vector2 point in points) {
			p2tPoints.Add (new Poly2Tri.TriangulationPoint(point.x, point.y));
		}
		return p2tPoints;
	}

	public static Mesh Poly2TriToMesh(List<Poly2Tri.DelaunayTriangle> triangles) {
		Mesh mesh = new Mesh ();
		List<int> tris = new List<int> ();
		List<Vector3> allPoints = new List<Vector3>();
		foreach (Poly2Tri.DelaunayTriangle tri in triangles) {
			for(int i = 2; i >= 0; i--) { 
				Vector3 point = new Vector3(tri.Points[i].Xf, tri.Points[i].Yf, 0f);
				if(!allPoints.Contains(point)) {
					allPoints.Add(point);
				}
			}
		}

		foreach (Poly2Tri.DelaunayTriangle tri in triangles) {
			for(int i = 2; i >= 0; i--) { 
				Vector3 point = new Vector3(tri.Points[i].Xf, tri.Points[i].Yf, 0f);
				tris.Add(allPoints.IndexOf(point));
			}
		}
		mesh.vertices = allPoints.ToArray();
		mesh.triangles = tris.ToArray();
		return mesh;
	}
}
