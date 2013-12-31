using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(MeshFilter))]
public class LevelGenerator : MonoBehaviour {

	// Use this for initialization
	public int numberOfPoints = 30;
	public float levelRadius = 10f;
	public int numberOfStepsForBrownianWalk = 10;
	public int numberOfTendrilWalksToComplete = 3;
	public float delaunayPointSnap = 0.2f;

	public int decorationsToSpawn = 10;
	public GameObject[] decorations;

	private string generationMessage = "Starting...";




	void Start () {
		Generate ();
	}

	public void Generate(){
		//clean up
		EdgeCollider2D[] cols = GetComponents<EdgeCollider2D> ();
		foreach (EdgeCollider2D col in cols) {
			DestroyImmediate(col);
		}

		PolygonCollider2D[] polyCols = GetComponents<PolygonCollider2D> ();
		foreach (PolygonCollider2D polyCol in polyCols) {
			DestroyImmediate(polyCol);
		}
		generationMessage = "Generating points...";

		//Generate some points...
		List<Vector2> generationPoints = new List<Vector2>();

		for(int i = 0; i < numberOfPoints; i++) {
			Vector2 randomPoint = new Vector2(Random.Range(-levelRadius, levelRadius), Random.Range(-levelRadius, levelRadius));
			randomPoint = new Vector2( Mathf.Round(randomPoint.x * delaunayPointSnap) / delaunayPointSnap, Mathf.Round(randomPoint.y * delaunayPointSnap) / delaunayPointSnap);
			if(!generationPoints.Contains(randomPoint)) {
				generationPoints.Add(randomPoint);
			}
		}

		//Clean up duplicates

		generationMessage = "Triangulating...";
		//Generate a delaunay triangulation of the points 
		Poly2Tri.PointSet ps = new Poly2Tri.PointSet(Poly2TriAdapter.Vector2ToP2TList(generationPoints));
		Poly2Tri.P2T.Triangulate (ps);
		List<Poly2Tri.DelaunayTriangle> delaunayTriangles = (List<Poly2Tri.DelaunayTriangle>) ps.Triangles;

		generationMessage = "Performing Brownian Walk through triangulation...";
		//Brownian Walk through the triangles.
		Poly2TriDelaunayTriangulationWalker walker = new Poly2TriDelaunayTriangulationWalker (delaunayTriangles);
		List<Poly2Tri.DelaunayTriangle> levelTriangleSet = new List<Poly2Tri.DelaunayTriangle>();

		for (int i = 0; i < numberOfStepsForBrownianWalk; i++) {
			Poly2Tri.DelaunayTriangle nextTriangle = walker.Next();
			if(!levelTriangleSet.Contains(nextTriangle)) {
				levelTriangleSet.Add(nextTriangle);
			}
		}

		generationMessage = "Performing Tendril Walks...";
		for (int i = 0; i < numberOfTendrilWalksToComplete; i++) {
			generationMessage = "Performing Tendril Walk: " + i;
			walker.SetCurrentTriangleIndex(delaunayTriangles.IndexOf(levelTriangleSet[Mathf.FloorToInt(Random.value * levelTriangleSet.Count)]));
			for (int j = 0; j < numberOfStepsForBrownianWalk; j++) {
				Poly2Tri.DelaunayTriangle nextTriangle = walker.Next();
				if(!levelTriangleSet.Contains(nextTriangle)) {
					levelTriangleSet.Add(nextTriangle);
				}
			}
		}

		generationMessage = "Getting level boolean...";
		List<Poly2Tri.DelaunayTriangle> booleanFromLevelSet = delaunayTriangles.Where (x => !levelTriangleSet.Contains (x)).ToList (); 

		/* Debugging delaunay triangulation
		foreach (Poly2Tri.DelaunayTriangle tri in booleanFromLevelSet) {
			Gizmos.DrawLine(new Vector2(tri.Points[0].Xf, tri.Points[0].Yf), new Vector2(tri.Points[1].Xf, tri.Points[1].Yf));
			Gizmos.DrawLine(new Vector2(tri.Points[1].Xf, tri.Points[1].Yf), new Vector2(tri.Points[2].Xf, tri.Points[2].Yf));
			Gizmos.DrawLine(new Vector2(tri.Points[2].Xf, tri.Points[2].Yf), new Vector2(tri.Points[0].Xf, tri.Points[0].Yf));
		}
		*/

		generationMessage = "Generating mesh from boolean...";
		Mesh mesh = Poly2TriAdapter.Poly2TriToMesh (booleanFromLevelSet);
		generationMessage = "Optimizing...";
		mesh.Optimize ();
		GetComponent<MeshFilter> ().sharedMesh = mesh;

		//Generate colliders...
		ClipperLib.Clipper clipper = new ClipperLib.Clipper ();
		foreach(Poly2Tri.DelaunayTriangle tri in booleanFromLevelSet) {
			List<ClipperLib.IntPoint> trianglePoints = new List<ClipperLib.IntPoint>();
			for(int i = 0; i < 3; i++) {
				trianglePoints.Add(new ClipperLib.IntPoint(tri.Points[i].Xf,tri.Points[i].Yf));
			}
			clipper.AddPath(trianglePoints,ClipperLib.PolyType.ptClip, true);
		}
		List<List<ClipperLib.IntPoint>> solution = new List<List<ClipperLib.IntPoint>> ();
		clipper.Execute (ClipperLib.ClipType.ctUnion, solution);


		foreach (List<ClipperLib.IntPoint> polygon in solution) {
			Vector2[] unityPoints = new Vector2[polygon.Count + 1];
			for(int i = 0; i < polygon.Count; i++) {
				unityPoints[i] = new Vector2(polygon[i].X,polygon[i].Y);
			}
			unityPoints[polygon.Count] = new Vector2(polygon[0].X,polygon[0].Y);
			EdgeCollider2D collider = gameObject.AddComponent<EdgeCollider2D>();
			collider.points = unityPoints;
		}

		generationMessage = "Calling Callback..";
		OnFinishedGeneration();
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		Poly2Tri.DelaunayTriangle spawnTriangle = levelTriangleSet [Mathf.FloorToInt (Random.value * levelTriangleSet.Count)];

		generationMessage = "Spawning Players...";
		Vector2 spawnPoint = new Vector2 (spawnTriangle.Centroid ().Xf, spawnTriangle.Centroid ().Yf);

		foreach (GameObject player in players) {
			player.transform.position = spawnPoint + Random.insideUnitCircle;

		}

		generationMessage = "Spawning Decorations...";
		for (int i = 0; i < decorationsToSpawn; i++) {
			GameObject decoration = (GameObject) Instantiate(decorations[Mathf.FloorToInt(Random.value * decorations.Length)]);
			Poly2Tri.DelaunayTriangle decorSpawnTriangle = levelTriangleSet [Mathf.FloorToInt (Random.value * levelTriangleSet.Count)];
			Vector2 decorSpawnPoint = new Vector2 (decorSpawnTriangle.Centroid ().Xf, decorSpawnTriangle.Centroid ().Yf);
			decoration.transform.position = decorSpawnPoint + Random.insideUnitCircle;
		}


		//yield return 0;
	}

	void OnFinishedGeneration() {
		generationMessage = "Done.";

	}

	void OnGUI() {
		GUI.Label (new Rect (10f, 10f, Screen.width, 100f), generationMessage);
	}

	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos() {

	}
}
