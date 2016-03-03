using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell
{
    public List<Line> edges;
    public List<Cell> neighbors;
    public Vector3 seed;
    public List<Vector3> pointsToRemove;

    public Cell(Vector3 _seed)
    {
        seed = _seed;
        edges = new List<Line>();
        neighbors = new List<Cell>();
        pointsToRemove = new List<Vector3>();
    }

    public bool IsPointInCell(Vector3 point)
    {
        Line testLine = new Line(point, point + new Vector3((float)decimal.MaxValue*0.01f, 0.0f, 0.0f));
        int counter = 0;

        List<Vector3> listOfIntersectionPoint = new List<Vector3>();

        for (int i = 0; i < edges.Count; ++i)
        {
            if (edges[i].pointAEquals(point) || edges[i].pointBEquals(point))
                return true;
            else
            {
                bool crossing = edges[i].IsCrossingOtherLineInBetweenPoints(testLine);

                if (crossing)
                {
                    Vector3 intersectionPoint = edges[i].GetIntersectionPointWithOtherLine(testLine, true);

                    if (edges[i].pointAEquals(intersectionPoint) || edges[i].pointBEquals(intersectionPoint))
                    {
                        if (!listOfIntersectionPoint.Contains(intersectionPoint))
                        {
                            listOfIntersectionPoint.Add(intersectionPoint);
                            ++counter;
                        }
                    }
                    else
                    {
                        ++counter;
                    }


                    //if (!edges[i].pointAEquals(intersectionPoint) && !edges[i].pointBEquals(intersectionPoint))
                        
                }

                //counter += (crossing) ? 1 : 0;
            }
        }

        if (counter % 2 != 0)
            return true;
        else
            return false;
    }

    public void MergeAlignedEdges ()
    {
        for (int i = 0; i < edges.Count; ++i)
        {
            for (int j = i+1; j < edges.Count; ++j)
            {
                if (edges[i].pointA.Equals(edges[j].pointA) && edges[i].IsAlignedWithLine(edges[j]))
                {
                    Line mergedLine = new Line(edges[i].pointB, edges[j].pointB);
                    edges.Insert(i, mergedLine);
                    edges.RemoveAt(i + 1);
                    edges.RemoveAt(j);
                    --j;
                }
                else if (edges[i].pointA.Equals(edges[j].pointB) && edges[i].IsAlignedWithLine(edges[j]))
                {
                    Line mergedLine = new Line(edges[i].pointB, edges[j].pointA);
                    edges.Insert(i, mergedLine);
                    edges.RemoveAt(i + 1);
                    edges.RemoveAt(j);
                    --j;
                }
                else if (edges[i].pointB.Equals(edges[j].pointA) && edges[i].IsAlignedWithLine(edges[j]))
                {
                    Line mergedLine = new Line(edges[i].pointA, edges[j].pointB);
                    edges.Insert(i, mergedLine);
                    edges.RemoveAt(i + 1);
                    edges.RemoveAt(j);
                    --j;
                }
                else if (edges[i].pointB.Equals(edges[j].pointB) && edges[i].IsAlignedWithLine(edges[j]))
                {
                    Line mergedLine = new Line(edges[i].pointA, edges[j].pointA);
                    edges.Insert(i, mergedLine);
                    edges.RemoveAt(i + 1);
                    edges.RemoveAt(j);
                    --j;
                }
            }
        }
    }

    /***
     * perfectMatch = Only remove the edge if pointA = pointA and pointB = pointB
     ***/
    public void RemoveDuplicateEdges(bool perfectMatch)
    {
        for (int i = 0; i < edges.Count; ++i)
        {
            for (int j = i + 1; j < edges.Count; ++j)
            {
                if (edges[i].EqualsOtherLine(edges[j], perfectMatch))
                {
                    edges.RemoveAt(j);
                    edges.RemoveAt(i);
                    --i;
                    break;
                }
            }
        }
    }

    public void RemoveEdgesAsPoint()
    {
        for (int i = 0; i < edges.Count; ++i)
        {
            if (edges[i].pointAEquals(edges[i].pointB, 3))
            {
                edges.Remove(edges[i]);
                --i;
            }
        }
    }

    public void RemoveOrphanEdges()
    {
        List<bool> pointAChecked = new List<bool>();
        List<bool> pointBChecked = new List<bool>();
        List<Line> orphans = new List<Line>(edges);

        for (int i = 0; i < orphans.Count; ++i)
        {
            pointAChecked.Add(false);
            pointBChecked.Add(false);
        }

        for (int i = 0; i < orphans.Count; ++i)
        {
            for (int j = i + 1; j < orphans.Count; ++j)
            {
                if (!pointAChecked[i] && orphans[i].pointAEquals(orphans[j].pointA))
                {
                    pointAChecked[i] = true;
                    pointAChecked[j] = true;
                }
                else if (!pointAChecked[i] && orphans[i].pointAEquals(orphans[j].pointB))
                {
                    pointAChecked[i] = true;
                    pointBChecked[j] = true;
                }

                if (!pointBChecked[i] && orphans[i].pointBEquals(orphans[j].pointB))
                {
                    pointBChecked[i] = true;
                    pointBChecked[j] = true;
                }
                else if (!pointBChecked[i] && orphans[i].pointBEquals(orphans[j].pointA))
                {
                    pointBChecked[i] = true;
                    pointAChecked[j] = true;
                }

                if (pointAChecked[j] && pointBChecked[j])
                {
                    orphans.RemoveAt(j);
                    pointAChecked.RemoveAt(j);
                    pointBChecked.RemoveAt(j);
                    --j;
                }

                if (pointAChecked[i] && pointBChecked[i])
                {
                    orphans.RemoveAt(i);
                    pointAChecked.RemoveAt(i);
                    pointBChecked.RemoveAt(i);
                    --i;
                    break;
                }
            }
        }

        for (int i = 0; i < orphans.Count; ++i)
        {
            edges.Remove(orphans[i]);
        }

    }

    public int HasEdge(Line other, bool perfectMatch)
    {
    	for (int i = 0; i < edges.Count; ++i)
    	{
    		if (edges[i].EqualsOtherLine(other, perfectMatch))
    			return i;
    	}

    	return -1;
    }

    public bool IsStillNeighborWithCell(Cell other)
    { 
        for (int i = 0; i < edges.Count; ++i)
        {
            for (int j = 0; j < other.edges.Count; ++j)
            {
                if (edges[i].EqualsOtherLine(other.edges[j], false))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void RemoveOffNeighbors()
    {
        for (int i = 0; i < neighbors.Count; ++i)
        {
            if (!IsStillNeighborWithCell(neighbors[i]))
            {
                neighbors[i].neighbors.Remove(this);
                neighbors.Remove(neighbors[i]);
                --i;
            }
        }
    }
}

public class Line
{
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 vector;

    public Line(Vector3 _pointA, Vector3 _pointB)
    {
        pointA = _pointA;
        pointB = _pointB;
        vector = pointB - pointA;
    }

    public void EditPointA(Vector3 newPoint)
    {
        pointA = newPoint;
        vector = pointB - pointA;
    }
    
    public void EditPointB(Vector3 newPoint)
    {
        pointB = newPoint;
        vector = pointB - pointA;
    }

    public bool pointAEquals(Vector3 other, int precision = 3)
    {
        if (System.Math.Round((decimal)pointA.x, precision) == System.Math.Round((decimal)other.x, precision) &&
            System.Math.Round((decimal)pointA.y, precision) == System.Math.Round((decimal)other.y, precision) &&
            System.Math.Round((decimal)pointA.z, precision) == System.Math.Round((decimal)other.z, precision))
            return true;

        return false;
    }

    public bool pointBEquals(Vector3 other, int precision = 3)
    {
        if (System.Math.Round((decimal)pointB.x, precision) == System.Math.Round((decimal)other.x, precision) &&
            System.Math.Round((decimal)pointB.y, precision) == System.Math.Round((decimal)other.y, precision) &&
            System.Math.Round((decimal)pointB.z, precision) == System.Math.Round((decimal)other.z, precision))
            return true;

        return false;
    }

    public Vector3 GetMiddle ()
    {
        return ((pointB - pointA) / 2.0f) + pointA;
    }

    public bool IsPointOnLine (Vector3 other)
    {
    	if (pointAEquals(other) || pointBEquals(other))
    		return true;

        Vector3 directionATest = other - pointA;
        
        if (Vector3.Cross(vector, directionATest) == Vector3.zero)
        {
            float dp1 = Vector3.Dot(vector, vector);
            float dp2 = Vector3.Dot(vector, directionATest);

            if (dp2 < 0.0f || dp2 > dp1)
                return false;

            return true;
        }

        return false;
    }

    public bool IsPointOnSegment(Vector3 other)
    {
        if (IsPointOnLine(other))
        {
            if (IsPointInBetween(pointA.x, pointB.x, other.x) &&
                IsPointInBetween(pointA.y, pointB.y, other.y) &&
                IsPointInBetween(pointA.z, pointB.z, other.z))
                return true;
        }

        return false;
    }

    private bool IsPointInBetween(float startingBound, float endingBound, float testingPoint)
    {
        if (System.Math.Round((decimal)testingPoint, 3) <= System.Math.Round((decimal)startingBound, 3) && System.Math.Round((decimal)testingPoint, 3) >= System.Math.Round((decimal)endingBound, 3) ||
            System.Math.Round((decimal)testingPoint, 3) >= System.Math.Round((decimal)startingBound, 3) && System.Math.Round((decimal)testingPoint, 3) <= System.Math.Round((decimal)endingBound, 3))
            return true;

        return false;
    }

    public bool IsCrossingOtherLine (Line other)
    {
        if (float.IsNaN(GetIntersectionPointWithOtherLine(other, false).x))
            return false;
        else
            return true;
    }

    public bool IsCrossingOtherLineInBetweenPoints(Line other)
    {
        if (float.IsNaN(GetIntersectionPointWithOtherLine(other, true).x))
            return false;
        else
            return true;
    }

    public Vector3 GetIntersectionPointWithOtherLine(Line other, bool testAsSegments)
    {
        float k = 0;
        float m = 0;

        // Get the parameter of the equation
        if (System.Math.Round((decimal)other.vector.y, 10) == 0 && decimal.Round((decimal)vector.y, 10) == 0)
        {
            // Test if lines are parallels
            if (System.Math.Round((decimal)(vector.x * other.vector.z - vector.z * other.vector.x), 10) == 0)
                return new Vector3(float.NaN, float.NaN, float.NaN);

            k = -1 * (pointA.x * other.vector.z - other.pointA.x * other.vector.z - other.vector.x * pointA.z + other.vector.x * other.pointA.z) / (vector.x * other.vector.z - vector.z * other.vector.x);
            m = -1 * (-1 * (vector.x * pointA.z) + vector.x * other.pointA.z + vector.z * pointA.x - vector.z * other.pointA.x) / (vector.x * other.vector.z - vector.z * other.vector.x);
        }
        else
        {
            // Test if lines are parallels
            if (System.Math.Round((decimal)(vector.x * other.vector.y - vector.y * other.vector.x), 10) == 0)
                return new Vector3(float.NaN, float.NaN, float.NaN);

            k = -1 * (pointA.x * other.vector.y - other.pointA.x * other.vector.y - other.vector.x * pointA.y + other.vector.x * other.pointA.y) / (vector.x * other.vector.y - vector.y * other.vector.x);
            m = -1 * (-1 * (vector.x * pointA.y) + vector.x * other.pointA.y + vector.y * pointA.x - vector.y * other.pointA.x) / (vector.x * other.vector.y - vector.y * other.vector.x);
        }
        // The intersection point is on the segment if 0 < k < 1 and 0 < m < 1
        if (((k >= 0 && k <= 1) && (m >= 0 && m <= 1) && testAsSegments) || !testAsSegments)
        {
			float x = (float)System.Math.Round((decimal)(pointA.x + vector.x * k), 10);
			float y = (float)System.Math.Round((decimal)(pointA.y + vector.y * k), 10);
			float z = (float)System.Math.Round((decimal)(pointA.z + vector.z * k), 10);

			//float x = pointA.x + vector.x * k;
			//float y = pointA.y + vector.y * k;
			//float z = pointA.z + vector.z * k;

            return new Vector3(x, y, z);
        }
        else
        {
            return new Vector3(float.NaN, float.NaN, float.NaN);
        }
        
    }

    public bool IsAlignedWithLine(Line other)
    {
        if (float.IsNaN(GetIntersectionPointWithOtherLine(other, false).x))
            return true;

        return false;
    }

    public Vector3 Get2DPerpendicularVector()
    {
        return Vector3.Cross(vector.normalized, Vector3.down);
    }

    public bool EqualsOtherLine(Line other, bool perfectMatch)
    {
        if (pointAEquals(other.pointA) && pointBEquals(other.pointB))
            return true;
        else if (!perfectMatch && pointAEquals(other.pointB) && pointBEquals(other.pointA))
            return true;

        return false;
    }

}

public class Voronoi2DScript : MonoBehaviour {

    public string rngSeed;
    public int nbOfPoints;

    private List<Cell> listOfCells;
    private Boundaries boundaries;

	// Use this for initialization
	void Start ()
    {
        listOfCells = new List<Cell>();
        boundaries = new Boundaries();

        Vector3 meshBounds = new Vector3(
           GetComponent<MeshFilter>().mesh.bounds.extents.x * transform.lossyScale.x,
           GetComponent<MeshFilter>().mesh.bounds.extents.y * transform.lossyScale.y,
           GetComponent<MeshFilter>().mesh.bounds.extents.z * transform.lossyScale.z
           );

        boundaries.topRight = new Vector3(transform.position.x + meshBounds.x, transform.position.y + meshBounds.y, transform.position.z + meshBounds.z);
        boundaries.bottomLeft = new Vector3(transform.position.x - meshBounds.x, transform.position.y - meshBounds.y, transform.position.z - meshBounds.z);
        boundaries.topLeft = new Vector3(boundaries.bottomLeft.x, 0f, boundaries.topRight.z);
        boundaries.bottomRight = new Vector3(boundaries.topRight.x, 0f, boundaries.bottomLeft.z);
        Debug.Log(boundaries.topRight);

        /*
        Line topLine = new Line(boundaries.topLeft, boundaries.topRight);
        Line rightLine = new Line(boundaries.topRight, boundaries.bottomRight);
        Line bottomLine = new Line(boundaries.bottomLeft, boundaries.bottomRight);
        Line leftLine = new Line(boundaries.topLeft, boundaries.bottomLeft);

        topLine.pointB = new Vector3(4.0f, 0.0f, 5.0f);
        rightLine.pointA = new Vector3(5.0f, 0.0f, 3.0f);

        Line extra1 = new Line(new Vector3(4.0f, 0.0f, 5.0f), new Vector3(6.0f, 0.0f, 7.0f));
        Line extra2 = new Line(new Vector3(6.0f, 0.0f, 7.0f), new Vector3(5.0f, 0.0f, 3.0f));

        Cell cell = new Cell(new Vector3(0f, 0f, 0f));
        cell.edges.Add(topLine);
        cell.edges.Add(rightLine);
        cell.edges.Add(leftLine);
        cell.edges.Add(bottomLine);

        cell.edges.Add(extra1);
        cell.edges.Add(extra2);

        listOfCells.Add(cell);

        AddNewSeed(new Vector3(1.5f, 0.0f, 2f));
        //AddNewSeed(new Vector3(1.5f, 0.0f, 0.5f));
        AddNewSeed(new Vector3(1f, 0.0f, -2f));
		
		AddNewSeed(new Vector3(-4f, 0.0f, -1f));
		 
        AddNewSeed(new Vector3(4f, 0.0f, -4f));
		
        AddNewSeed(new Vector3(3f, 0.0f, 4f));

		     
		AddNewSeed(new Vector3(-4f, 0.0f, 3f));

        AddNewSeed(new Vector3(-2f, 0.0f, 1f));
		
		    
		AddNewSeed(new Vector3(5f, 0.0f, 5f));
		 
        AddNewSeed(new Vector3(3f, 0.0f, 1f));
         
		 

        AddNewSeed(new Vector3(-2.5f, 0.0f, -1.5f));
        AddNewSeed(new Vector3(-3f, 0.0f, -4f));
        AddNewSeed(new Vector3(-4.3f, 0.0f, -1f));
		
        AddNewSeed(new Vector3(-1f, 0.0f, -4.3f));
        AddNewSeed(new Vector3(-3.5f, 0.0f, -3.5f));

		
        AddNewSeed(new Vector3(-3.5f, 0.0f, 3.5f));

        //Line test = new Line(new Vector3(-4.6f, 0f, 5f), new Vector3(5f, 0f, -2.2f));
        //Line test2 = new Line(new Vector3(1f, 0f, -2f), new Vector3(10001f, 0f, -2f));
        //test.IsCrossingOtherLine(test2);

        Line test = new Line(new Vector3(0f, 0f, 2f), new Vector3(1f, 0f, 0f));
        Line test2 = new Line(new Vector3(1f, 0f, 0f), new Vector3(1f, 0f, -2f));
        Debug.Log(test.IsAlignedWithLine(test2));
        */
        
        StartCoroutine("AddSomeSeeds");

        Debug.Log("ok");
	}

    void AddNewSeed(Vector3 newSeed)
    {
        if (listOfCells.Count != 0)
        {
            for (int i = 0; i < listOfCells.Count; ++i)
            {
                if (listOfCells[i].IsPointInCell(newSeed))
                {
                    SplitMainCell(listOfCells[i], newSeed);
                    return;
                }
            }
        }
        else
        {
            // Create the new cell
            Cell newCell = new Cell(newSeed);

            newCell.edges = GetOutLines();

            listOfCells.Add(newCell);
        }
    }

	public bool ArePointsTheSame(Vector3 pointA, Vector3 pointB, int precision = 3)
    {
        if (System.Math.Round((decimal)pointA.x, precision) == System.Math.Round((decimal)pointB.x, precision) &&
            System.Math.Round((decimal)pointA.y, precision) == System.Math.Round((decimal)pointB.y, precision) &&
            System.Math.Round((decimal)pointA.z, precision) == System.Math.Round((decimal)pointB.z, precision))
            return true;

        return false;
    }

    List<Line> GetOutLines()
    {
        Line topLine = new Line(boundaries.topLeft, boundaries.topRight);
        Line rightLine = new Line(boundaries.topRight, boundaries.bottomRight);
        Line bottomLine = new Line(boundaries.bottomLeft, boundaries.bottomRight);
        Line leftLine = new Line(boundaries.topLeft, boundaries.bottomLeft);

        List<Line> listOfLines = new List<Line>();

        listOfLines.Add(topLine);
        listOfLines.Add(rightLine);
        listOfLines.Add(bottomLine);
        listOfLines.Add(leftLine);

        return listOfLines;
    }

    void SplitCell (Cell mainCell, Cell newCell)
    {
		// Get the line between the seeds
        Line seedsLine = new Line(mainCell.seed, newCell.seed);
        
        // Create the perpendicular line on the line between the seeds
        Vector3 perpendicularVector = seedsLine.Get2DPerpendicularVector();
        Vector3 startingPerpendicularPoint = seedsLine.GetMiddle();
        Vector3 endingPerpendicularPoint = perpendicularVector * 2 + startingPerpendicularPoint;

        Line perpendicularLine = new Line(startingPerpendicularPoint, endingPerpendicularPoint);

        /****************************************************************/
        /*	Get the perpendicular line of the line between the 2 seeds	*/
        /****************************************************************/

        // Create the lists which will be used to hold the edges from the main cell cut by the perpendicular line
        // And the intersection points
        List<Line> cuttingLinesList = new List<Line>();
        List<Vector3> intersectionPointsList = new List<Vector3>();

        // Go through all the edges of the cell to find out which ones are being cut by the perpendicular line
        for (int i = 0; i < mainCell.edges.Count; ++i)
        {
            // Check the lines are crossing
            if (mainCell.edges[i].IsCrossingOtherLine(perpendicularLine))
            {
                Vector3 intersectionPoint = mainCell.edges[i].GetIntersectionPointWithOtherLine(perpendicularLine, false);

                // Check the intersection point is on the segment (so the cut is actually inbetween the boundaries of the segment, not just the line)
                if (mainCell.edges[i].IsPointOnSegment(intersectionPoint))
                {
                    // Save the edge cut by the perpendicular line
                    cuttingLinesList.Add(mainCell.edges[i]);
                    // Save the intersection point
                    intersectionPointsList.Add(intersectionPoint);

                }

            }
        }

		

        // If the perpendicular line cuts some lines at their ends
        // the farthest lines from the new cell's seed will be deleted
		for (int i = 0; i < intersectionPointsList.Count; ++i)
        {
        	for (int j = i + 1; j < intersectionPointsList.Count; ++j)
        	{
        		if (ArePointsTheSame(intersectionPointsList[i], intersectionPointsList[j]))
        		{
        			Vector3 point1 = Vector3.zero;
        			Vector3 point2 = Vector3.zero;

        			if (cuttingLinesList[i].pointA.Equals(intersectionPointsList[i]))
        				point1 = cuttingLinesList[i].pointB;
					else if (cuttingLinesList[i].pointB.Equals(intersectionPointsList[i]))
        				point1 = cuttingLinesList[i].pointA;

					if (cuttingLinesList[j].pointA.Equals(intersectionPointsList[j]))
        				point2 = cuttingLinesList[j].pointB;
					else if (cuttingLinesList[j].pointB.Equals(intersectionPointsList[j]))
        				point2 = cuttingLinesList[j].pointA;

					if (Vector3.Distance(point1, newCell.seed) < Vector3.Distance(point2, newCell.seed))
					{
						intersectionPointsList.Remove(intersectionPointsList[j]);
        				cuttingLinesList.Remove(cuttingLinesList[j]);

        				break;
					}
					else
					{
						intersectionPointsList.Remove(intersectionPointsList[i]);
        				cuttingLinesList.Remove(cuttingLinesList[i]);
        				--i;
        				break;
					}
        		}

        	}
        }

		if (intersectionPointsList.Count < 2)
            return; 

        // The perpendicular line is now defined by the intersection points
        perpendicularLine.EditPointA(intersectionPointsList[0]);
        perpendicularLine.EditPointB(intersectionPointsList[1]);

		// The perpendicular line is now one of the edges of the main and new cell
        mainCell.edges.Add(perpendicularLine);
        newCell.edges.Add(perpendicularLine);

		/*************************************************/
        /*	Get the lines cut by the perpendicular line	 */
        /*************************************************/

        // Go through the edges cut by the perpendicular line to split them into 2 and add them to the appropriate cell (main or new)
        for (int i = 0; i < cuttingLinesList.Count; ++i)
        {
            // If the pointA from the edge being cut is closer to the main cell's seed than the new cell, then it means
            // that the line between pointA and the intersection point should be part of the main cell
            if (Vector3.Distance(cuttingLinesList[i].pointA, mainCell.seed) < Vector3.Distance(cuttingLinesList[i].pointA, newCell.seed))
            {
				Line newLineForMainCell = new Line(cuttingLinesList[i].pointA, intersectionPointsList[i]);
				bool isLinePartOfNeighbors = false;

                // Check the new line is actually not just one point (if both ends are the same) and that the main cell doesnt already have this line
				if (!cuttingLinesList[i].pointAEquals(intersectionPointsList[i]) && mainCell.HasEdge(newLineForMainCell, false) == -1)
                {
					// Add the new line to the main cell
					mainCell.edges.Add(newLineForMainCell);

					// Remove the current edge from the neighbor cell
            		mainCell.edges.Remove(cuttingLinesList[i]);

					// Add pointB as a point potentially linked to some other edges to remove
                	mainCell.pointsToRemove.Add(cuttingLinesList[i].pointB);

					// Check the edge being cut doesnt belong to one of the main cell's neighbors
					// If it does, this edge will be removed from the neighbor cell and the new line will also be added to it.
					for (int j = 0; j < mainCell.neighbors.Count; ++j)
					{
						for (int k = 0; k < mainCell.neighbors [j].edges.Count; ++k)
						{
							// Check the neighbor cell has the line being cut and that it doesn't already have the new line
							if (mainCell.neighbors[j].edges[k].EqualsOtherLine(cuttingLinesList[i],false) && !mainCell.neighbors[j].edges[k].EqualsOtherLine(newLineForMainCell, false))
							{
								isLinePartOfNeighbors = true;

								// Remove the edge being cut and add the new line
								mainCell.neighbors[j].edges.Remove(mainCell.neighbors[j].edges[k]);
								mainCell.neighbors[j].edges.Add(newLineForMainCell);

								// As the nieghbor cell is getting one line removed, it also gets one point to be removed
								if (!mainCell.neighbors[j].pointsToRemove.Contains(cuttingLinesList[i].pointB))
									mainCell.neighbors[j].pointsToRemove.Add(cuttingLinesList[i].pointB);
								 
								break;
							}
						}

						if (isLinePartOfNeighbors)
							break;
					}

					// Add the rest of the line being cut to the new cell (if the line is not reduced to one point)
	                // and if the line being cut is not part of any of the neighbors
					if (!cuttingLinesList[i].pointBEquals(intersectionPointsList[i]) && !isLinePartOfNeighbors)
					{
						Line newLineForNewCell = new Line(cuttingLinesList[i].pointB, intersectionPointsList[i]);
						newCell.edges.Add (newLineForNewCell);
					}             
				}


                

            }
            else
            {
				Line newLineForExistingCell = new Line(cuttingLinesList[i].pointB, intersectionPointsList[i]);
				bool isLinePartOfNeighbors = false;

				if (!cuttingLinesList[i].pointBEquals(intersectionPointsList[i]) && mainCell.HasEdge(newLineForExistingCell, false) == -1)
                {
					mainCell.edges.Add(newLineForExistingCell);

					// Remove the current edge from the neighbor cell
            		mainCell.edges.Remove(cuttingLinesList[i]);

					mainCell.pointsToRemove.Add(cuttingLinesList[i].pointA);


					for (int j = 0; j < mainCell.neighbors.Count; ++j)
					{
						for (int k = 0; k < mainCell.neighbors [j].edges.Count; ++k)
						{
							if (mainCell.neighbors [j].edges [k].EqualsOtherLine (cuttingLinesList [i], false)  && !mainCell.neighbors[j].edges[k].EqualsOtherLine(newLineForExistingCell, false))
							{
								isLinePartOfNeighbors = true;

								mainCell.neighbors[j].edges.Remove(mainCell.neighbors[j].edges[k]);
								mainCell.neighbors[j].edges.Add(newLineForExistingCell);

								if (!mainCell.neighbors[j].pointsToRemove.Contains(cuttingLinesList[i].pointA))
									mainCell.neighbors[j].pointsToRemove.Add(cuttingLinesList[i].pointA);

								break;
							}
						}

						if (isLinePartOfNeighbors)
							break;
					}

					if (!cuttingLinesList[i].pointAEquals(intersectionPointsList[i]) && !isLinePartOfNeighbors)
	                {
						Line newLineForNewCell = new Line(cuttingLinesList[i].pointA, intersectionPointsList[i]);
	                    newCell.edges.Add(newLineForNewCell);
	                }
                }


            }


        }

		/**************************************************/
        /*	Remove the exceding lines from the main cell  */
        /**************************************************/

        // Go through all the edges from the main cell to check if there arent anymore to remove
		for (int i = 0; i < mainCell.pointsToRemove.Count; ++i)
        {
            bool newLineFound = false;

			// If one of the point from 'pointsToRemove' is found, it means there was an extra edge to remove
            // from the neighbor cell and to add to the new cell
            for (int j = 0; j < mainCell.edges.Count; ++j)
            {
            	if (!mainCell.edges[j].EqualsOtherLine(perpendicularLine, false) &&
					!perpendicularLine.pointAEquals(mainCell.pointsToRemove[i]) &&
					!perpendicularLine.pointBEquals(mainCell.pointsToRemove[i]))
            	{
					if (mainCell.edges[j].pointAEquals(mainCell.pointsToRemove[i]))
	                {
	                    // Add the cuurent edge to the new cell
						int indexOfEdge = newCell.HasEdge(mainCell.edges[j], false);

						//if (!mainCell.edges[j].pointBEquals(perpendicularLine.pointA) &&
						//	!mainCell.edges[j].pointBEquals(perpendicularLine.pointB))
						//{
							if (indexOfEdge.Equals(-1))
								newCell.edges.Add(mainCell.edges[j]);
							else
								newCell.edges.RemoveAt(indexOfEdge);
						//}


	                    // Add the other point of the current edge as a point to check
						mainCell.pointsToRemove.Add(mainCell.edges[j].pointB);

	                    // Remove the current edge from the neighbor cell
	                    mainCell.edges.Remove(mainCell.edges[j]);
	                    newLineFound = true;

	                    break;
	                }
					else if (mainCell.edges[j].pointBEquals(mainCell.pointsToRemove[i]))
	                {
						int indexOfEdge = newCell.HasEdge(mainCell.edges[j], false);

						//if (!mainCell.edges[j].pointAEquals(perpendicularLine.pointA) &&
						//	!mainCell.edges[j].pointAEquals(perpendicularLine.pointB))
						//{
							if (indexOfEdge.Equals(-1))
								newCell.edges.Add(mainCell.edges[j]);
							else
								newCell.edges.RemoveAt(indexOfEdge);
						//}

	                    //newCell.edges.Add(mainCell.edges[j]);
						mainCell.pointsToRemove.Add(mainCell.edges[j].pointA);

	                    mainCell.edges.Remove(mainCell.edges[j]);
	                    newLineFound = true;

	                    break;
	                }

            	}
				
            }

            // If no new line has been found, it means all the extra edges has been removed, so there is no need to keep checking
            if (!newLineFound)
                break;
        }

		mainCell.pointsToRemove.Clear();
		
        // Add the new cell as a neighbors of the main cell and vice versa
        mainCell.neighbors.Add(newCell);
        newCell.neighbors.Add(mainCell);

        newCell.MergeAlignedEdges();

		/************************************************/
        /*	Check the neighbor cells of the main cell	*/
        /************************************************/

        for (int i = 0; i < mainCell.neighbors.Count; ++i)
        {
            if (!newCell.neighbors.Contains(mainCell.neighbors[i]) && !mainCell.neighbors[i].Equals(newCell))
            {
                for (int j = 0; j < mainCell.neighbors[i].edges.Count; ++j)
                {
                    //if ((mainCell.neighbors[i].edges[j].IsPointOnSegment(perpendicularLine.pointA) || mainCell.neighbors[i].edges[j].IsPointOnSegment(perpendicularLine.pointB)) &&
                    //    (!mainCell.neighbors[i].edges[j].pointAEquals(perpendicularLine.pointA, 3) && !mainCell.neighbors[i].edges[j].pointAEquals(perpendicularLine.pointB, 3) &&
                    //    !mainCell.neighbors[i].edges[j].pointBEquals(perpendicularLine.pointA, 3) && !mainCell.neighbors[i].edges[j].pointBEquals(perpendicularLine.pointB, 3)))
					if (mainCell.neighbors[i].edges[j].IsPointOnSegment(perpendicularLine.pointA) || mainCell.neighbors[i].edges[j].IsPointOnSegment(perpendicularLine.pointB))
                    {
						SplitCell(mainCell.neighbors[i], newCell);

                        break;
                    }
                }
            }

        }

    }

    void SplitMainCell(Cell cell, Vector3 newSeed)
    {
        // Create the new cell
        Cell newCell = new Cell(newSeed);

        SplitCell(cell, newCell);

        for (int k = 0; k < newCell.edges.Count; ++k)
        {
            for (int i = 0; i < cell.neighbors.Count; ++i)
            {
                if (!newCell.neighbors.Contains(cell.neighbors[i]) && !cell.neighbors[i].Equals(newCell))
                {
                    for (int j = 0; j < cell.neighbors[i].edges.Count; ++j)
                    {
                        if (newCell.edges[k].EqualsOtherLine(cell.neighbors[i].edges[j], false))
                        {
							
							SplitCell(cell.neighbors[i], newCell);
                            cell.neighbors[i].neighbors.Remove(cell);
                            cell.neighbors.Remove(cell.neighbors[i]);
                            --i;
                            --k;
                            break;
                            
                        }
                    }
                }
                
            }
        }
        //newCell.RemoveEdgesAsPoint();
        //newCell.MergeAlignedEdges();
        //newCell.RemoveDuplicateEdges(false);

        //cell.RemoveOffNeighbors();
        //newCell.RemoveOrphanEdges();
        // Add the new cell the global list of cells

        listOfCells.Add(newCell);

        
    }

    struct Boundaries
    {
        public Vector3 topLeft, topRight, bottomLeft, bottomRight;
    }

    List<Vector3> generateListOfPoints(Vector3 minBound, Vector3 maxBound)
    {
        List<Vector3> listOfPoints = new List<Vector3>();

        if (rngSeed != "")
            Random.seed = rngSeed.GetHashCode();

        for (int i = 0; i < nbOfPoints; ++i)
        {
            float x = Random.Range(minBound.x, maxBound.x);
            float z = Random.Range(minBound.z, maxBound.z);

            Vector3 newPoint = new Vector3(x, 0, z);

            listOfPoints.Add(newPoint);
        }

        return listOfPoints;
    }

	// Update is called once per frame
	void Update () {

	}

    IEnumerator AddSomeSeeds()
    {
        List<Vector3> listOfPoints = generateListOfPoints(boundaries.bottomLeft, boundaries.topRight);
        
        for (int i = 0; i < listOfPoints.Count; ++i)
        {
            AddNewSeed(listOfPoints[i]);
            yield return new WaitForSeconds(.01f);
        }

        Debug.Log("Done");
    }

    void OnDrawGizmos()
    {
        if (listOfCells != null)
        {
            for (int i = 0; i < listOfCells.Count; ++i)
            {
                //Gizmos.DrawSphere(listOfCells[i].seed, 0.1f);
                Gizmos.DrawWireSphere(listOfCells[i].seed, 0.1f);
                for (int j = 0; j < listOfCells[i].edges.Count; ++j)
                {
                    Debug.DrawLine(listOfCells[i].edges[j].pointA, listOfCells[i].edges[j].pointB, Color.green);
                }
            }
        }
    }

}