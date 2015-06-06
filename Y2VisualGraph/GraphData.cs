using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Y2VisualGraph
{
	[Serializable]
	class GraphData
	{
		public List<Point> NodeLocations;
		public EdgeCollection Edges;
		public int FormNode;
		public int ToNode;
		public bool IsUndirectedGraph;
		
		public GraphData(int fromNode,int toNode)
		{
			FormNode=fromNode;
			ToNode=toNode;
			NodeLocations = new List<Point>();

		}
	}
}
