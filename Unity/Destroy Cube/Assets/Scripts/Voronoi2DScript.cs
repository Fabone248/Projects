using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell
{
    public List<Line> edges;
    public List<Cell> neighbors;
    public Vector3 germ;

    public Cell(Vector3 _germ)
    {
        germ = _germ;
        edges = new List<Line>();
        neighbors = new List<Cell>();
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
            float x = pointA.x + vector.x * k;
            float y = pointA.y + vector.y * k;
            float z = pointA.z + vector.z * k;

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
        float f = 3.13288451f;
        Debug.Log(f);
        Debug.Log(System.Math.Round((decimal)f, 2));
        Debug.Log(System.Math.Round((decimal)f, 3));
        Debug.Log(System.Math.Round((decimal)f, 4));
        Debug.Log(System.Math.Round((decimal)f, 5));
        Debug.Log(System.Math.Round((decimal)f, 6));



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

        //topLine.pointB = new Vector3(4.0f, 0.0f, 5.0f);
        //rightLine.pointA = new Vector3(5.0f, 0.0f, 3.0f);

        //Line extra1 = new Line(new Vector3(4.0f, 0.0f, 5.0f), new Vector3(6.0f, 0.0f, 7.0f));
        //Line extra2 = new Line(new Vector3(6.0f, 0.0f, 7.0f), new Vector3(5.0f, 0.0f, 3.0f));

        Cell cell = new Cell(new Vector3(0f, 0f, 0f));
        cell.edges.Add(topLine);
        cell.edges.Add(rightLine);
        cell.edges.Add(leftLine);
        cell.edges.Add(bottomLine);

        //cell.edges.Add(extra1);
        //cell.edges.Add(extra2);

        listOfCells.Add(cell);

        AddNewGerm(new Vector3(1.5f, 0.0f, 2f));
        //AddNewGerm(new Vector3(1.5f, 0.0f, 0.5f));
        AddNewGerm(new Vector3(1f, 0.0f, -2f));
        AddNewGerm(new Vector3(-4f, 0.0f, -1f));
        AddNewGerm(new Vector3(4f, 0.0f, -4f));
        AddNewGerm(new Vector3(3f, 0.0f, 4f));
        AddNewGerm(new Vector3(-4f, 0.0f, 3f));
        AddNewGerm(new Vector3(-2f, 0.0f, 1f));
        AddNewGerm(new Vector3(5f, 0.0f, 5f));
        AddNewGerm(new Vector3(3f, 0.0f, 1f));
        /* 
         * 
        
        
        AddNewGerm(new Vector3(-2.5f, 0.0f, -1.5f));
        AddNewGerm(new Vector3(-3f, 0.0f, -4f));
        AddNewGerm(new Vector3(-4.3f, 0.0f, -1f));
        AddNewGerm(new Vector3(-1f, 0.0f, -4.3f));
        AddNewGerm(new Vector3(-3.5f, 0.0f, -3.5f));
        AddNewGerm(new Vector3(-3.5f, 0.0f, 3.5f));

        //Line test = new Line(new Vector3(-4.6f, 0f, 5f), new Vector3(5f, 0f, -2.2f));
        //Line test2 = new Line(new Vector3(1f, 0f, -2f), new Vector3(10001f, 0f, -2f));
        //test.IsCrossingOtherLine(test2);

        Line test = new Line(new Vector3(0f, 0f, 2f), new Vector3(1f, 0f, 0f));
        Line test2 = new Line(new Vector3(1f, 0f, 0f), new Vector3(1f, 0f, -2f));
        Debug.Log(test.IsAlignedWithLine(test2));
         * */
        
        StartCoroutine("AddSomeGerms");

        Debug.Log("ok");
	}

    void AddNewGerm(Vector3 newGerm)
    {
        if (listOfCells.Count != 0)
        {
            for (int i = 0; i < listOfCells.Count; ++i)
            {
                if (listOfCells[i].IsPointInCell(newGerm))
                {
                    SplitMainCell(listOfCells[i], newGerm);
                    return;
                }
            }
        }
        else
        {
            // Create the new cell
            Cell newCell = new Cell(newGerm);

            newCell.edges = GetOutLines();

            listOfCells.Add(newCell);
        }
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

    void SplitMainCell(Cell cell, Vector3 newGerm)
    {
        // Create the new cell
        Cell newCell = new Cell(newGerm);

        // Get the line between the germs
        Line germsLine = new Line(cell.germ, newGerm);

        // Create the perpendicular line on the line between the germs
        Vector3 perpendicularVector = germsLine.Get2DPerpendicularVector();
        Vector3 startingPerpendicularPoint = germsLine.GetMiddle();
        Vector3 endingPerpendicularPoint = perpendicularVector * 2 + startingPerpendicularPoint;

        Line perpendicularLine = new Line(startingPerpendicularPoint, endingPerpendicularPoint);

        // Create the lists which will be used to hold the edges from the main cell cut by the perpendicular line
        // And the intersection points
        List<Line> cuttingLinesList = new List<Line>();
        List<Vector3> intersectionPointsList = new List<Vector3>();

        // Go through all the edges of the cell to find out which ones are being cut by the perpendicular line
        for (int i = 0; i < cell.edges.Count; ++i)
        {
            // Check the lines are crossing
            if (cell.edges[i].IsCrossingOtherLine(perpendicularLine))
            {
                Vector3 intersectionPoint = cell.edges[i].GetIntersectionPointWithOtherLine(perpendicularLine, false);

                // Check the intersection point is on the segment (so the cut is actually inbetween the boundaries of the segment, not just the line)
                if (cell.edges[i].IsPointOnSegment(intersectionPoint))
                {
                    // Save the edge cut by the perpendicular line
                    cuttingLinesList.Add(cell.edges[i]);
                    // Save the intersection point
                    intersectionPointsList.Add(intersectionPoint);
                }
                
            }
        }

        // The perpendicular line is now defined by the intersection points
        perpendicularLine.EditPointA(intersectionPointsList[0]);
        perpendicularLine.EditPointB(intersectionPointsList[1]);

        // The perpendicular line is now one of the edges of the main and new cell
        cell.edges.Add(perpendicularLine);
        newCell.edges.Add(perpendicularLine);

        // Create a list that will hold the points of the edges to remove
        // A-------\-----B
        //  |       \   |
        //  |        \  |
        // If A will still be part of the main cell, B will be saved because it might be linked to some other edges to remove

        List<Vector3> existingCellPointsLinkedToLinesToRemove = new List<Vector3>();

        // Go through the edges cut by the perpendicular line to split them into 2 and add them to the proper cell
        for (int i = 0; i < cuttingLinesList.Count; ++i)
        {
            // If the pointA from the edge being cut is closer the main cell's germ than the new cell, then it means
            // that the line between pointA and the intersection point should be part of the main cell
            if (Vector3.Distance(cuttingLinesList[i].pointA, cell.germ) < Vector3.Distance(cuttingLinesList[i].pointA, newCell.germ))
            {
                // Add the new line to the main cell
                if (!cuttingLinesList[i].pointAEquals(intersectionPointsList[i]))
                {
                    Line newLineForExistingCell = new Line(cuttingLinesList[i].pointA, intersectionPointsList[i]);
                    cell.edges.Add(newLineForExistingCell);
                }

                // Add pointB as a point potentially linked to some other edges to remove
                existingCellPointsLinkedToLinesToRemove.Add(cuttingLinesList[i].pointB);

                // Add the rest of the line the new cell
                if (!cuttingLinesList[i].pointBEquals(intersectionPointsList[i]))
                {
                    Line newLineForNewCell = new Line(cuttingLinesList[i].pointB, intersectionPointsList[i]);
                    newCell.edges.Add(newLineForNewCell);
                }

            }
            else
            {
                if (!cuttingLinesList[i].pointBEquals(intersectionPointsList[i]))
                {
                    Line newLineForExistingCell = new Line(cuttingLinesList[i].pointB, intersectionPointsList[i]);
                    cell.edges.Add(newLineForExistingCell);
                }

                existingCellPointsLinkedToLinesToRemove.Add(cuttingLinesList[i].pointA);

                if (!cuttingLinesList[i].pointAEquals(intersectionPointsList[i]))
                {
                    Line newLineForNewCell = new Line(cuttingLinesList[i].pointA, intersectionPointsList[i]);
                    newCell.edges.Add(newLineForNewCell);
                }
            }

            // Remove the current edge from the main cell
            cell.edges.Remove(cuttingLinesList[i]);
        }

        // Go through all the edges from the main cell to check if there arent anymore to remove
        for (int i = 0; i < existingCellPointsLinkedToLinesToRemove.Count; ++i)
        {
            bool newLineFound = false;

            // If one of the point from 'existingCellPointsLinkedToLinesToRemove' is found, it means there was an extra edge to remove
            // from the main cell and to add to the new cell
            for (int j = 0; j < cell.edges.Count; ++j)
            {
                if (cell.edges[j].pointAEquals(existingCellPointsLinkedToLinesToRemove[i]))
                {
                    // Add the cuurent edge to the new cell
                    newCell.edges.Add(cell.edges[j]);
                    // Add the other point of the current edge as a point to check
                    existingCellPointsLinkedToLinesToRemove.Add(cell.edges[j].pointB);

                    // Remove the current edge from the main cell
                    cell.edges.Remove(cell.edges[j]);
                    newLineFound = true;
                    
                    break;
                }
                else if (cell.edges[j].pointBEquals(existingCellPointsLinkedToLinesToRemove[i]))
                {
                    newCell.edges.Add(cell.edges[j]);
                    existingCellPointsLinkedToLinesToRemove.Add(cell.edges[j].pointA);

                    cell.edges.Remove(cell.edges[j]);
                    newLineFound = true;
                    
                    break;
                }
            }

            // If no new line has been found, it means all the extra edges has been removed, so there is no need to keep checking
            if (!newLineFound)
                break;
        }

        // Add the new cell as a neighbors of the main cell and vice versa
        cell.neighbors.Add(newCell);
        newCell.neighbors.Add(cell);

        List<Cell> listOfNeighborsToNotCheck = new List<Cell>();

        for (int i = 0; i < cell.neighbors.Count; ++i)
        {
            if (!cell.neighbors[i].Equals(newCell) && !newCell.neighbors.Contains(cell.neighbors[i]))
            {
                for (int j = 0; j < cell.neighbors[i].edges.Count; ++j)
                {
                    if ((cell.neighbors[i].edges[j].IsPointOnSegment(perpendicularLine.pointA) || cell.neighbors[i].edges[j].IsPointOnSegment(perpendicularLine.pointB)) &&
                        (!cell.neighbors[i].edges[j].pointAEquals(perpendicularLine.pointA, 3) && !cell.neighbors[i].edges[j].pointAEquals(perpendicularLine.pointB, 3) &&
                        !cell.neighbors[i].edges[j].pointBEquals(perpendicularLine.pointA, 3) && !cell.neighbors[i].edges[j].pointBEquals(perpendicularLine.pointB, 3)))
                    //if (cell.neighbors[i].edges[j].IsPointOnSegment(perpendicularLine.pointA) || cell.neighbors[i].edges[j].IsPointOnSegment(perpendicularLine.pointB))
                    {
                        SplitNeighbors(cell.neighbors[i], newCell);

                        if (!cell.IsStillNeighborWithCell(cell.neighbors[i]))
                        {
                            //cell.neighbors[i].neighbors.Remove(cell);
                            //cell.neighbors.Remove(cell.neighbors[i]);
                        }
                        listOfNeighborsToNotCheck.Add(cell.neighbors[i]);
                        break;
                    }
                }
            }
            
        }

        listOfNeighborsToNotCheck.Add(newCell);

        
        for (int k = 0; k < newCell.edges.Count; ++k)
        {
            for (int i = 0; i < cell.neighbors.Count; ++i)
            {
                if (!listOfNeighborsToNotCheck.Contains(cell.neighbors[i]) && !newCell.neighbors.Contains(cell.neighbors[i]))
                {
                    for (int j = 0; j < cell.neighbors[i].edges.Count; ++j)
                    {
                        if (newCell.edges[k].EqualsOtherLine(cell.neighbors[i].edges[j], false))
                        {
                            SplitNeighbors(cell.neighbors[i], newCell);
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

    void SplitNeighbors(Cell neighborCell, Cell newCell)
    {
        // Get the line between the germs
        Line germsLine = new Line(neighborCell.germ, newCell.germ);
        
        // Create the perpendicular line on the line between the germs
        Vector3 perpendicularVector = germsLine.Get2DPerpendicularVector();
        Vector3 startingPerpendicularPoint = germsLine.GetMiddle();
        Vector3 endingPerpendicularPoint = perpendicularVector * 2 + startingPerpendicularPoint;

        Line perpendicularLine = new Line(startingPerpendicularPoint, endingPerpendicularPoint);

        // Create the lists which will be used to hold the edges from the neighbor cell cut by the perpendicular line
        // And the intersection points
        List<Line> cuttingLinesList = new List<Line>();
        List<Vector3> intersectionPointsList = new List<Vector3>();

        // Go through all the edges of the cell to find out which ones are being cut by the perpendicular line
        for (int i = 0; i < neighborCell.edges.Count; ++i)
        {
            // Check the lines are crossing
            if (neighborCell.edges[i].IsCrossingOtherLine(perpendicularLine))
            {
                Vector3 intersectionPoint = neighborCell.edges[i].GetIntersectionPointWithOtherLine(perpendicularLine, false);

                // Check the intersection point is on the segment (so the cut is actually inbetween the boundaries of the segment, not just the line)
                if (neighborCell.edges[i].IsPointOnSegment(intersectionPoint))
                {
                    // Save the edge cut by the perpendicular line
                    cuttingLinesList.Add(neighborCell.edges[i]);
                    // Save the intersection point
                    intersectionPointsList.Add(intersectionPoint);
                }

            }
        }

        if (intersectionPointsList.Count != 2)
            return;

        // The perpendicular line is now defined by the intersection points
        perpendicularLine.EditPointA(intersectionPointsList[0]);
        perpendicularLine.EditPointB(intersectionPointsList[1]);

        // The perpendicular line is now one of the edges of the main and new cell
        neighborCell.edges.Add(perpendicularLine);
        newCell.edges.Add(perpendicularLine);

        // Create a list that will hold the points of the edges to remove
        // A-------\-----B
        //  |       \   |
        //  |        \  |
        // If A will still be part of the main cell, B will be saved because it might be linked to some other edges to remove

        List<Vector3> existingCellPointsLinkedToLinesToRemove = new List<Vector3>();

        // Go through the edges cut by the perpendicular line to split them into 2 and add them to the proper cell
        for (int i = 0; i < cuttingLinesList.Count; ++i)
        {
            // If the pointA from the edge being cut is closer the neighbor cell's germ than the new cell, then it means
            // that the line between pointA and the intersection point should be part of the neighbor cell
            if (Vector3.Distance(cuttingLinesList[i].pointA, neighborCell.germ) < Vector3.Distance(cuttingLinesList[i].pointA, newCell.germ))
            {
                // Add the new line to the neighbor cell
                if (!cuttingLinesList[i].pointAEquals(intersectionPointsList[i]))
                {
                    Line newLineForExistingCell = new Line(cuttingLinesList[i].pointA, intersectionPointsList[i]);
                    neighborCell.edges.Add(newLineForExistingCell);
                }

                // Add pointB as a point potentially linked to some other edges to remove
                existingCellPointsLinkedToLinesToRemove.Add(cuttingLinesList[i].pointB);

                // Add the rest of the line the new cell
                if (!cuttingLinesList[i].pointBEquals(intersectionPointsList[i]))
                {
                    Line newLineForNewCell = new Line(cuttingLinesList[i].pointB, intersectionPointsList[i]);
                    newCell.edges.Add(newLineForNewCell);
                }

            }
            else
            {
                if (!cuttingLinesList[i].pointBEquals(intersectionPointsList[i]))
                {
                    Line newLineForExistingCell = new Line(cuttingLinesList[i].pointB, intersectionPointsList[i]);
                    neighborCell.edges.Add(newLineForExistingCell);
                }

                existingCellPointsLinkedToLinesToRemove.Add(cuttingLinesList[i].pointA);

                if (!cuttingLinesList[i].pointAEquals(intersectionPointsList[i]))
                {
                    Line newLineForNewCell = new Line(cuttingLinesList[i].pointA, intersectionPointsList[i]);
                    newCell.edges.Add(newLineForNewCell);
                }
                
            }

            // Remove the current edge from the neighbor cell
            neighborCell.edges.Remove(cuttingLinesList[i]);
        }

        // Go through all the edges from the neighbor cell to check if there arent anymore to remove
        for (int i = 0; i < existingCellPointsLinkedToLinesToRemove.Count; ++i)
        {
            bool newLineFound = false;

            // If one of the point from 'existingCellPointsLinkedToLinesToRemove' is found, it means there was an extra edge to remove
            // from the neighbor cell and to add to the new cell
            for (int j = 0; j < neighborCell.edges.Count; ++j)
            {
                if (neighborCell.edges[j].pointAEquals(existingCellPointsLinkedToLinesToRemove[i]))
                {
                    // Add the cuurent edge to the new cell
                    newCell.edges.Add(neighborCell.edges[j]);
                    // Add the other point of the current edge as a point to check
                    existingCellPointsLinkedToLinesToRemove.Add(neighborCell.edges[j].pointB);

                    // Remove the current edge from the neighbor cell
                    neighborCell.edges.Remove(neighborCell.edges[j]);
                    newLineFound = true;

                    break;
                }
                else if (neighborCell.edges[j].pointBEquals(existingCellPointsLinkedToLinesToRemove[i]))
                {
                    newCell.edges.Add(neighborCell.edges[j]);
                    existingCellPointsLinkedToLinesToRemove.Add(neighborCell.edges[j].pointA);

                    neighborCell.edges.Remove(neighborCell.edges[j]);
                    newLineFound = true;

                    break;
                }
            }

            // If no new line has been found, it means all the extra edges has been removed, so there is no need to keep checking
            if (!newLineFound)
                break;
        }

        // Add the new cell as a neighbors of the neighbor cell and vice versa
        neighborCell.neighbors.Add(newCell);
        newCell.neighbors.Add(neighborCell);

        newCell.RemoveDuplicateEdges(false);
        //newCell.RemoveEdgesAsPoint();
        newCell.MergeAlignedEdges();
        


        for (int i = 0; i < neighborCell.neighbors.Count; ++i)
        {
            if (!newCell.neighbors.Contains(neighborCell.neighbors[i]) && !neighborCell.neighbors[i].Equals(newCell))
            {
                for (int j = 0; j < neighborCell.neighbors[i].edges.Count; ++j)
                {
                    if ((neighborCell.neighbors[i].edges[j].IsPointOnSegment(perpendicularLine.pointA) || neighborCell.neighbors[i].edges[j].IsPointOnSegment(perpendicularLine.pointB)) &&
                        (!neighborCell.neighbors[i].edges[j].pointAEquals(perpendicularLine.pointA, 3) && !neighborCell.neighbors[i].edges[j].pointAEquals(perpendicularLine.pointB, 3) &&
                        !neighborCell.neighbors[i].edges[j].pointBEquals(perpendicularLine.pointA, 3) && !neighborCell.neighbors[i].edges[j].pointBEquals(perpendicularLine.pointB, 3)))
                    {
                        SplitNeighbors(neighborCell.neighbors[i], newCell);

                        if (!neighborCell.IsStillNeighborWithCell(neighborCell.neighbors[i]))
                        {
                            //neighborCell.neighbors[i].neighbors.Remove(neighborCell);
                            //neighborCell.neighbors.Remove(neighborCell.neighbors[i]);
                        }
                        break;
                    }
                }
            }

        }

        

        //neighborCell.RemoveOffNeighbors();
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

    IEnumerator AddSomeGerms()
    {
        List<Vector3> listOfPoints = generateListOfPoints(boundaries.bottomLeft, boundaries.topRight);
        
        for (int i = 0; i < listOfPoints.Count; ++i)
        {
            AddNewGerm(listOfPoints[i]);
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
                //Gizmos.DrawSphere(listOfCells[i].germ, 0.1f);
                Gizmos.DrawWireSphere(listOfCells[i].germ, 0.1f);
                for (int j = 0; j < listOfCells[i].edges.Count; ++j)
                {
                    Debug.DrawLine(listOfCells[i].edges[j].pointA, listOfCells[i].edges[j].pointB, Color.green);
                }
            }
        }
    }

}