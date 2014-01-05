using UnityEngine;
using System.Collections.Generic;
using Tinkerbox.Geometry;
using System.Linq;

public class DestructibleLevel : MonoBehaviour {

	public List<Polygon> booleanPolygons = new List<Polygon>();
	private Mesh mesh;
	public bool debug = true;
	public bool debugUnion = true;

	private Polygon levelArea = new Polygon();

	public List<Polygon> solutions;
	public List<List<ClipperLib.IntPoint>> booleanUnionSolution = new List<List<ClipperLib.IntPoint>> ();
	public int solutionCount = 0;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (debug) {
			if(Input.GetMouseButtonDown(0)) {
				Debug.Log("got...");
				Vector2 center = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				DestroyLevel(center, Random.value);
			}
		}
	}

	public void AddBooleanPolygon(Polygon polygon) {
		if(!booleanPolygons.Contains(polygon)) {
			booleanPolygons.Add(polygon);
			UpdateMesh();
		}
	}

	public void DestroyLevel(Vector2 centerOfDestruction, float destructionRadius, int sides = 5) {
		float step = (2 * Mathf.PI)/ sides;
		Polygon polygon = new Polygon ();
		for(int i = sides - 1; i >= 0; i--) {
			polygon.points.Add(new Point(centerOfDestruction.x + Mathf.Sin (step * i) * destructionRadius, centerOfDestruction.y + Mathf.Cos (step * i) * destructionRadius));
		}
		AddBooleanPolygon(polygon);
	}

	public void OnDrawGizmos() {
		Gizmos.color = Color.red;
		foreach (Polygon polygon in solutions) {
			if(ClipperLib.Clipper.Orientation(polygon)) {
				Gizmos.color = Color.red;
			} else {
				Gizmos.color = Color.yellow;
			}

			for(int i = 1; i < polygon.points.Count; i++)  {
				Gizmos.DrawLine((Vector2) polygon.points[i-1],(Vector2) polygon.points[i]);
			}
			Gizmos.DrawLine((Vector2) polygon.points.Last(),(Vector2) polygon.points.First());
		}

		Gizmos.color = Color.cyan;
		foreach (Polygon polygon in booleanPolygons) {
			for(int i = 1; i < polygon.points.Count; i++)  {
				//Gizmos.DrawLine((Vector2) polygon.points[i-1],(Vector2) polygon.points[i]);
			}
		}

		if (debugUnion) {
			foreach (Polygon polygon in booleanUnionSolution) {
					if (ClipperLib.Clipper.Orientation (polygon)) {
							Gizmos.color = Color.magenta;
					} else {
							Gizmos.color = Color.blue;
					}
					for (int i = 1; i < polygon.points.Count; i++) {
						Gizmos.DrawWireSphere((Vector2) polygon.points[i], 0.2f);
						Gizmos.DrawLine ((Vector2)polygon.points [i - 1], (Vector2)polygon.points [i]);
					}
					Gizmos.DrawLine ((Vector2)polygon.points.Last (), (Vector2)polygon.points.First ());
			}
		}
	}

	public void UpdateMesh() {
		//clean up
		booleanUnionSolution.Clear ();
		//solutions.Clear ();
		EdgeCollider2D[] cols = GetComponents<EdgeCollider2D> ();
		foreach (EdgeCollider2D col in cols) {
			DestroyImmediate(col);
		}
		
		PolygonCollider2D[] polyCols = GetComponents<PolygonCollider2D> ();
		foreach (PolygonCollider2D polyCol in polyCols) {
			DestroyImmediate(polyCol);
		}

		//Wind the square to boolean from.
		levelArea.points.Clear ();
		levelArea.points.Add (new Point (-1000f, 1000f));
		levelArea.points.Add (new Point (1000f, 1000f));
		levelArea.points.Add (new Point (1000f, -1000f));
		levelArea.points.Add (new Point (-1000f, -1000f));
		ClipperLib.Clipper clipper = new ClipperLib.Clipper ();
		//Strictly simple is computationally expensive, but prevents points from appearing too close together and making Poly2Tri Shit the bed.
		clipper.StrictlySimple = true;
		foreach (Polygon polygon in booleanPolygons) {
			clipper.AddPath(polygon,ClipperLib.PolyType.ptClip, true);
		}

		clipper.Execute (ClipperLib.ClipType.ctUnion, booleanUnionSolution, ClipperLib.PolyFillType.pftNonZero,ClipperLib.PolyFillType.pftNonZero);
		clipper.Clear ();
		clipper.AddPath (levelArea, ClipperLib.PolyType.ptSubject, true);
		foreach (List<ClipperLib.IntPoint> polygon in booleanUnionSolution) {
			clipper.AddPath(polygon,ClipperLib.PolyType.ptClip, true);
		}
		List<List<ClipperLib.IntPoint>> solution = new List<List<ClipperLib.IntPoint>> ();
		clipper.Execute (ClipperLib.ClipType.ctDifference, solution, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero);
		mesh = Utils.PolygonsToMesh (solution.Select(x=> (Polygon)x).ToList());
		solutions = solution.Select (x => (Polygon)x).ToList ();
		GetComponent<MeshFilter> ().sharedMesh = mesh;



		foreach (Polygon polygon in solution) {
			Vector2[] unityPoints = new Vector2[polygon.points.Count + 1];
			for(int i = 0; i < polygon.points.Count; i++) {
				unityPoints[i] = new Vector2(polygon.points[i].x,polygon.points[i].y);
			}
			unityPoints[polygon.points.Count] = new Vector2(polygon.points[0].x,polygon.points[0].y);
			EdgeCollider2D collider = gameObject.AddComponent<EdgeCollider2D>();
			collider.points = unityPoints;
		}

		solutionCount = booleanUnionSolution.Count;
		//No need to store polygons that we won't need to reference...
		booleanPolygons = booleanUnionSolution.Select(x=>(Polygon)x).ToList();
	}
}
