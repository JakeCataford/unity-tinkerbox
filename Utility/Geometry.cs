using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Tinkerbox {
	namespace Geometry {

		public class Utils {
			public static Mesh PolygonsToMesh(List<Polygon> polygons) {
				//Convert Clipper points to constrained point sets.
				Mesh mesh = new Mesh ();
				List<Vector3> verts = new List<Vector3> ();
				List<int> triangles = new List<int> ();
				polygons.OrderByDescending(x=> ClipperLib.Clipper.Area(x));
				List<Poly2Tri.Polygon> paths = new List<Poly2Tri.Polygon> ();
				paths.Add ((Poly2Tri.Polygon) polygons[0]);
				polygons.RemoveAt (0);
				foreach (Polygon polygon in polygons) {
					if(ClipperLib.Clipper.Orientation(polygon)) {
						//Clockwise wound points are not holes and are valid paths.
						paths.Add(polygon);
					} else {
						paths[0].AddHole(polygon);
					}
				}
				
				Poly2Tri.PolygonSet set = new Poly2Tri.PolygonSet ();
				foreach (Poly2Tri.Polygon polygon in paths) {
					set.Add (polygon);
				}
				
				Poly2Tri.P2T.Triangulate (set);
				
				foreach(Poly2Tri.Polygon polygon in set.Polygons) {
					foreach(Poly2Tri.DelaunayTriangle tri in polygon.Triangles) {
						for(int i = 0; i < 3; i ++) {
							Vector3 vert = new Vector3(tri.Points[i].Xf,tri.Points[i].Yf,0);
							if(!verts.Contains(vert)) {
								verts.Add(vert);
							}
						}
					}
				}
				
				foreach(Poly2Tri.Polygon polygon in set.Polygons) {
					foreach(Poly2Tri.DelaunayTriangle tri in polygon.Triangles) {
						for(int i = 2; i >= 0; i--) {
							triangles.Add(verts.IndexOf(new Vector3(tri.Points[i].Xf,tri.Points[i].Yf,0)));
						}
					}
				}
				
				mesh.vertices = verts.ToArray();
				mesh.triangles = triangles.ToArray();
				
				return mesh;
			}
		}

		public class Constants {
			public static int ROBUST_PRECISION = 10000;
		}

		[System.Serializable]
		public class Polygon {

			/// <summary>
			/// List of points in the polygon. Ordered by their 
			/// </summary>
			[SerializeField]
			public List<Point> points = new List<Point>();

			/// <summary>
			/// Default Constructor
			/// </summary>
			public Polygon() {

			}

			/// <summary>
			/// Default Constructor
			/// </summary>
			public Polygon(List<ClipperLib.IntPoint> clipperPoints) {
				points = clipperPoints.Select(x=>(Point)x).ToList();
			}

			public Polygon(List<Vector2> points) {
				this.points = points.Cast<Point>().ToList();
			}

			public static implicit operator List<ClipperLib.IntPoint>(Polygon p) {
				return p.points.Select(x=> (ClipperLib.IntPoint) x).ToList ();
			}

			public static implicit operator Polygon(List<ClipperLib.IntPoint> p) {
				return new Polygon (p);
			}

			public static implicit operator Poly2Tri.Polygon(Polygon p) {
				return new Poly2Tri.Polygon(p.points.Select(x=> (Poly2Tri.PolygonPoint)x).ToList());
			}
		}

		[System.Serializable]
		public class Point {
			[SerializeField]
			public float x = 0.0f;
			[SerializeField]
			public float y = 0.0f;

			public Point(float x, float y) {
				this.x = x;
				this.y = y;
			}

			public static implicit operator Vector2(Point p) {
				return new Vector2(p.x,p.y);
			}

			public static implicit operator ClipperLib.IntPoint(Point p) {
				return new ClipperLib.IntPoint(p.x * Constants.ROBUST_PRECISION,p.y * Constants.ROBUST_PRECISION);
			}

			public static implicit operator Point(ClipperLib.IntPoint p) {
				return new Point((float) p.X / Constants.ROBUST_PRECISION,(float) p.Y / Constants.ROBUST_PRECISION);
			}

			public static implicit operator Poly2Tri.PolygonPoint(Point p) {
				return new Poly2Tri.PolygonPoint (p.x, p.y);
			}

			public static implicit operator Point(Poly2Tri.PolygonPoint p) {
				return new Point(p.Xf,p.Yf);
			}

			public static implicit operator Poly2Tri.TriangulationPoint(Point p) {
				return new Poly2Tri.TriangulationPoint (p.x, p.y);
			}
			
			public static implicit operator Point(Poly2Tri.TriangulationPoint p) {
				return new Point(p.Xf,p.Yf);
			}

		}

	}
}