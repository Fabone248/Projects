using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DelaunayTriangulationScript : MonoBehaviour {

    public string rngSeed;
    public int nbOfPoints;
    List<Vector3> listOfPoints = new List<Vector3>();
    //List<Triangle> listOfTriangles = new List<Triangle>();

    List<Line> joiningLines = new List<Line>();

    List<List<Line>> subsetsOfLines = new List<List<Line>>();

	// Use this for initialization
	void Start () {
        Vector3 meshBounds = GetComponent<MeshFilter>().mesh.bounds.extents;

        Vector3 maxBound = new Vector3(transform.position.x + meshBounds.x, transform.position.y + meshBounds.y, transform.position.z + meshBounds.z);
        Vector3 minBound = new Vector3(transform.position.x - meshBounds.x, transform.position.y - meshBounds.y, transform.position.z - meshBounds.z);

        generateListOfPoints(minBound, maxBound);

        /*
        listOfPoints.Add(minBound);
        listOfPoints.Add(maxBound);
        listOfPoints.Add(new Vector3(minBound.x, 0f, maxBound.z));
        listOfPoints.Add(new Vector3(maxBound.x, 0f, minBound.z));
        */

        OrderList();

        List<List<Vector3>> subsetsOfPoints = GetSubsetsOfPoints(new List<List<Vector3>>(), listOfPoints);

        subsetsOfLines = GetSubsetsOfLines(subsetsOfPoints);

        MergeSubsetsOfLines(subsetsOfLines);
        //Debug.Log(subsetsOfLines.Count);
	}

    void MergeSubsetsOfLines(List<List<Line>> subsetsOfLines)
    {
        for (int i = 0; i < subsetsOfLines.Count; i += 2)
        {
            //List<Line> joiningLines = new List<Line>();

            Vector3 bottomPointLeftSide = subsetsOfLines[i][0].pointA;
            Vector3 rightPointOnLeftSide = subsetsOfLines[i][0].pointA;
            Vector3 bottomPointRightSide = subsetsOfLines[i + 1][0].pointA;
            Vector3 leftPointOnRightSide = subsetsOfLines[i + 1][0].pointA;

            // Get the lowest point on the left subset
            for (int j = 0; j < subsetsOfLines[i].Count; ++j)
            {
                if (subsetsOfLines[i][j].pointA.z < bottomPointLeftSide.z)
                    bottomPointLeftSide = subsetsOfLines[i][j].pointA;
                else if (subsetsOfLines[i][j].pointB.z < bottomPointLeftSide.z)
                    bottomPointLeftSide = subsetsOfLines[i][j].pointB;
            }

            // Get the lowest point on the right subset
            for (int j = 0; j < subsetsOfLines[i + 1].Count; ++j)
            {
                if (subsetsOfLines[i + 1][j].pointA.z < bottomPointRightSide.z)
                    bottomPointRightSide = subsetsOfLines[i + 1][j].pointA;
                else if (subsetsOfLines[i + 1][j].pointB.z < bottomPointRightSide.z)
                    bottomPointRightSide = subsetsOfLines[i + 1][j].pointB;
            }

            int count = 0;
            while (count < 7)
            {
                leftPointOnRightSide = GetHighestPointInSubset(subsetsOfLines[i + 1]);
                rightPointOnLeftSide = GetHighestPointInSubset(subsetsOfLines[i]);

                // Get the point the most on the left on the right subset
                for (int j = 0; j < subsetsOfLines[i + 1].Count; ++j)
                {
                    if (subsetsOfLines[i + 1][j].pointA.x < leftPointOnRightSide.x && subsetsOfLines[i + 1][j].pointA.z > bottomPointRightSide.z)
                        leftPointOnRightSide = subsetsOfLines[i + 1][j].pointA;
                    else if (subsetsOfLines[i + 1][j].pointB.x < leftPointOnRightSide.x && subsetsOfLines[i + 1][j].pointA.z > bottomPointRightSide.z)
                        leftPointOnRightSide = subsetsOfLines[i + 1][j].pointB;
                }

                // Get the point the most on the right on the left subset
                for (int j = 0; j < subsetsOfLines[i].Count; ++j)
                {
                    if (subsetsOfLines[i][j].pointA.x > rightPointOnLeftSide.x && subsetsOfLines[i][j].pointA.z > bottomPointLeftSide.z)
                        rightPointOnLeftSide = subsetsOfLines[i][j].pointA;
                    else if (subsetsOfLines[i][j].pointB.x > rightPointOnLeftSide.x && subsetsOfLines[i][j].pointA.z > bottomPointLeftSide.z)
                        rightPointOnLeftSide = subsetsOfLines[i][j].pointB;
                }

                // Create the bottom line linking the sides
                Line bottomLine = new Line(bottomPointLeftSide, bottomPointRightSide);
                joiningLines.Add(bottomLine);

                // Store all the lines that will potentially be tested
                List<Line> listOfTestedLines = new List<Line>();

                bool pointOnLeftSide = false;

                /************************/
                /*      Right side      */
                /************************/

                // FinalVertex is the vertex that will represent the third vertex to create the triangle
                Vector3 finalVertex = leftPointOnRightSide;

                // Get the lines which are shared by leftPointOnRightSide
                List<Line> listOfSharedLinesRightSide = GetLinesSharingPoint(leftPointOnRightSide, subsetsOfLines[i + 1]);

                // Add the shared lines to the potentially testes lines
                listOfTestedLines.AddRange(listOfSharedLinesRightSide);

                // Get the points from the shared lines
                List<Vector3> linkedPointsRightSide = GetListOfPointsExcusivelyBelowAndOnRightSide(leftPointOnRightSide, listOfSharedLinesRightSide);

                //bool noValidLeftPointOnRightSide = false;

                if (finalVertex != bottomPointRightSide)
                {
                    // Go through all the linked points to find the one that make a delaunay triangle with the bottom line
                    for (int j = 0; j < linkedPointsRightSide.Count; ++j)
                    {
                        Vector3 centerOfCircle = GetCenterOfCircumCircle(bottomPointLeftSide, bottomPointRightSide, finalVertex);
                        float radius = Vector3.Distance(centerOfCircle, bottomPointRightSide);
                        bool pointInCircle = isPointInCircle(centerOfCircle, radius, linkedPointsRightSide[j]);

                        if (pointInCircle && linkedPointsRightSide[j] != bottomPointRightSide && linkedPointsRightSide[j] != bottomPointLeftSide)
                        {
                            //if (linkedPointsRightSide[j].z < bottomPointRightSide.z)
                            //    noValidLeftPointOnRightSide = true;
                            //else
                            //{
                                finalVertex = linkedPointsRightSide[j];
                            //    noValidLeftPointOnRightSide = false;
                            //}
                        }
                    }

                    
                }
                else
                {
                    finalVertex = rightPointOnLeftSide;
                    pointOnLeftSide = true;
                }

                /***********************/
                /*      Left side      */
                /***********************/

                //if (noValidLeftPointOnRightSide)
                //    finalVertex = rightPointOnLeftSide;

                // Get the lines which are shared by rightPointOnLeftSide
                List<Line> listOfSharedLinesLeftSide = GetLinesSharingPoint(rightPointOnLeftSide, subsetsOfLines[i]);

                // Add the shared lines to the potentially testes lines
                listOfTestedLines.AddRange(listOfSharedLinesLeftSide);

                // Get the points from the shared lines
                List<Vector3> linkedPointsLeftSide = GetListOfPointsExcusivelyBelowAndOnLeftSide(rightPointOnLeftSide, listOfSharedLinesLeftSide);

                // Add rightPointOnLeftSide to the list of linked points because it also will have to be tested
                linkedPointsLeftSide.Insert(0, rightPointOnLeftSide);

                // Starting with the point from the right side, the points on the left will be tested
                // If the current finalVertex doesn't make a delaunay triangle then a point from the left side will be used
                for (int j = 0; j < linkedPointsLeftSide.Count; ++j)
                {
                    Vector3 centerOfCircle = GetCenterOfCircumCircle(bottomPointLeftSide, bottomPointRightSide, finalVertex);
                    float radius = Vector3.Distance(centerOfCircle, bottomPointRightSide);
                    bool pointInCircle = isPointInCircle(centerOfCircle, radius, linkedPointsLeftSide[j]);

                    if (pointInCircle && linkedPointsLeftSide[j] != bottomPointRightSide && linkedPointsLeftSide[j] != bottomPointLeftSide)
                    {
                        pointOnLeftSide = true;
                        finalVertex = linkedPointsLeftSide[j];

                        // Get the lines which are shared by rightPointOnLeftSide
                        listOfSharedLinesLeftSide = GetLinesSharingPoint(finalVertex, subsetsOfLines[i]);

                        // Add the shared lines to the potentially testes lines
                        listOfTestedLines.AddRange(listOfSharedLinesLeftSide);
                    }
                }

                // According to which side the final vertex is, the line between the final vertex and the bottom line gets removed from the subset
                if (pointOnLeftSide)
                {
                    listOfSharedLinesLeftSide = GetLinesSharingPoint(finalVertex, listOfSharedLinesLeftSide);
                    listOfSharedLinesLeftSide = GetLinesSharingPoint(bottomPointLeftSide, listOfSharedLinesLeftSide);

                    subsetsOfLines[i].Remove(listOfSharedLinesLeftSide[0]);
                }
                else
                {
                    listOfSharedLinesRightSide = GetLinesSharingPoint(finalVertex, subsetsOfLines[i + 1]);
                    listOfSharedLinesRightSide = GetLinesSharingPoint(bottomPointRightSide, listOfSharedLinesRightSide);

                    subsetsOfLines[i + 1].Remove(listOfSharedLinesRightSide[0]);
                }

                Line rightLine = new Line(finalVertex, bottomPointRightSide);
                Line leftLine = new Line(finalVertex, bottomPointLeftSide);

                joiningLines.Add(rightLine);
                joiningLines.Add(leftLine);

                for (int k = 0; k < listOfTestedLines.Count; ++k)
                {
                    if (leftLine.pointB != listOfTestedLines[k].pointA &&
                        leftLine.pointA != listOfTestedLines[k].pointB &&
                        leftLine.pointB != listOfTestedLines[k].pointB &&
                        leftLine.pointA != listOfTestedLines[k].pointA &&
                        AreSegmentsCrossing(leftLine.pointA, leftLine.pointB, listOfTestedLines[k].pointA, listOfTestedLines[k].pointB))
                    {
                        subsetsOfLines[i].Remove(listOfTestedLines[k]);
                        subsetsOfLines[i + 1].Remove(listOfTestedLines[k]);
                    }

                    if (rightLine.pointB != listOfTestedLines[k].pointA &&
                        rightLine.pointA != listOfTestedLines[k].pointB &&
                        rightLine.pointB != listOfTestedLines[k].pointB &&
                        rightLine.pointA != listOfTestedLines[k].pointA &&
                        AreSegmentsCrossing(rightLine.pointA, rightLine.pointB, listOfTestedLines[k].pointA, listOfTestedLines[k].pointB))
                    {
                        subsetsOfLines[i].Remove(listOfTestedLines[k]);
                        subsetsOfLines[i + 1].Remove(listOfTestedLines[k]);
                    }

                    if (bottomLine.pointB != listOfTestedLines[k].pointA &&
                        bottomLine.pointA != listOfTestedLines[k].pointB &&
                        bottomLine.pointB != listOfTestedLines[k].pointB &&
                        bottomLine.pointA != listOfTestedLines[k].pointA &&
                        AreSegmentsCrossing(bottomLine.pointA, bottomLine.pointB, listOfTestedLines[k].pointA, listOfTestedLines[k].pointB))
                    {
                        subsetsOfLines[i].Remove(listOfTestedLines[k]);
                        subsetsOfLines[i + 1].Remove(listOfTestedLines[k]);
                    }
                }

                if (pointOnLeftSide)
                {
                    bottomPointLeftSide = finalVertex;
                }
                else
                {
                    bottomPointRightSide = finalVertex;
                }

                ++count;
            }

            rightPointOnLeftSide = GetHighestPointInSubset(subsetsOfLines[i]);

            joiningLines.Add(new Line(bottomPointLeftSide, bottomPointRightSide));
            joiningLines.Add(new Line(rightPointOnLeftSide, bottomPointRightSide));
            joiningLines.Add(new Line(bottomPointLeftSide, rightPointOnLeftSide));
        }

    }

    Vector3 GetHighestPointInSubset(List<Line> searchableList)
    {
        Vector3 highest = searchableList[0].pointA;

        for (int i = 0; i < searchableList.Count; ++i)
        {
            if (searchableList[i].pointA.z > highest.z)
                highest = searchableList[i].pointA;

            if (searchableList[i].pointB.z > highest.z)
                highest = searchableList[i].pointB;
        }

        return highest;
    }

    List<Vector3> GetListOfPointsExcusivelyBelowAndOnRightSide(Vector3 point, List<Line> searchableList)
    {
        List<Vector3> listOfPoints = new List<Vector3>();

        for (int i = 0; i < searchableList.Count; ++i)
        {
            if (point == searchableList[i].pointA && point.x < searchableList[i].pointB.x && point.z > searchableList[i].pointB.z)
                listOfPoints.Add(searchableList[i].pointB);
            else if (point == searchableList[i].pointB && point.x < searchableList[i].pointA.x && point.z > searchableList[i].pointA.z)
                listOfPoints.Add(searchableList[i].pointA);
        }

        return listOfPoints;
    }

    List<Vector3> GetListOfPointsExcusivelyBelowAndOnLeftSide(Vector3 point, List<Line> searchableList)
    {
        List<Vector3> listOfPoints = new List<Vector3>();

        for (int i = 0; i < searchableList.Count; ++i)
        {
            if (point == searchableList[i].pointA && point.x > searchableList[i].pointB.x && point.z > searchableList[i].pointB.z)
                listOfPoints.Add(searchableList[i].pointB);
            else if (point == searchableList[i].pointB && point.x > searchableList[i].pointA.x && point.z > searchableList[i].pointA.z)
                listOfPoints.Add(searchableList[i].pointA);
        }

        return listOfPoints;
    }

    List<Line> GetLinesSharingPoint(Vector3 point, List<Line> searchableList)
    {
        List<Line> listOfSharingLines = new List<Line>();

        for (int i = 0; i < searchableList.Count; ++i)
        {
            if (point == searchableList[i].pointA || point == searchableList[i].pointB)
                listOfSharingLines.Add(searchableList[i]);
        }
        
        return listOfSharingLines;
    }

    List<List<Line>> GetSubsetsOfLines(List<List<Vector3>> subsets)
    {
        List<List<Line>> subsetsOfLines = new List<List<Line>>();

        for (int i = 0; i < subsets.Count; i += 2)
        {
            List<Line> subsetOfLines = new List<Line>();

            // Get lines of the left subset
            for (int j = 0; j < subsets[i].Count-1; ++j)
            {
                for (int k = j + 1; k < subsets[i].Count; ++k)
                {
                    Line line = new Line(subsets[i][j], subsets[i][k]);
                    subsetOfLines.Add(line);
                }
            }

            // Get lines of the right subset
            for (int j = 0; j < subsets[i+1].Count - 1; ++j)
            {
                for (int k = j + 1; k < subsets[i+1].Count; ++k)
                {
                    Line line = new Line(subsets[i + 1][j], subsets[i + 1][k]);
                    subsetOfLines.Add(line);
                }
            }

            // Get the lines between both subsets
            for (int j = 0; j < subsets[i].Count; ++j)
            {
                for (int k = 0; k < subsets[i + 1].Count; ++k)
                {
                    Line line = new Line(subsets[i][j], subsets[i + 1][k]);
                    subsetOfLines.Add(line);
                }
            }

            for (int j = 0; j < subsetOfLines.Count - 1; ++j)
            {
                for (int k = j + 1; k < subsetOfLines.Count; ++k)
                {
                    // Check the 2 segments don't share the same point
                    if (subsetOfLines[j].pointA != subsetOfLines[k].pointA && subsetOfLines[j].pointA != subsetOfLines[k].pointB && subsetOfLines[j].pointB != subsetOfLines[k].pointA && subsetOfLines[j].pointB != subsetOfLines[k].pointB)
                    {
                        bool crossing = AreSegmentsCrossing(subsetOfLines[j].pointA, subsetOfLines[j].pointB, subsetOfLines[k].pointA, subsetOfLines[k].pointB);

                        if (crossing)
                        {
                            // Test line 1
                            Vector3 centerOfCircle1 = GetCenterOfCircumCircle(subsetOfLines[j].pointA, subsetOfLines[j].pointB, subsetOfLines[k].pointA);
                            float radius1 = Vector3.Distance(centerOfCircle1, subsetOfLines[j].pointA);
                            bool isDelaunay1 = isPointInCircle(centerOfCircle1, radius1, subsetOfLines[k].pointB);

                            // Test line 2
                            Vector3 centerOfCircle2 = GetCenterOfCircumCircle(subsetOfLines[k].pointA, subsetOfLines[k].pointB, subsetOfLines[j].pointA);
                            float radius2 = Vector3.Distance(centerOfCircle2, subsetOfLines[k].pointA);
                            bool isDelaunay2 = isPointInCircle(centerOfCircle2, radius2, subsetOfLines[j].pointB);

                            if (isDelaunay1)
                                subsetOfLines.Remove(subsetOfLines[j]);

                            if (isDelaunay2)
                                subsetOfLines.Remove(subsetOfLines[k]);
                        }
                    }
                }

            }

            subsetsOfLines.Add(subsetOfLines);
        }

        return subsetsOfLines;
    }

    List<List<Vector3>> GetSubsetsOfPoints(List<List<Vector3>> originalList, List<Vector3> subset)
    {
        int nbPointsInSubset = subset.Count;

        int sub = (int)Mathf.Ceil((float)nbPointsInSubset / 2.0f);

        int i = 0;
        List<Vector3> list1 = new List<Vector3>();
        List<Vector3> list2 = new List<Vector3>();

        for (; i < sub; ++i)
        {
            list1.Add(subset[i]);
        }

        for (; i < nbPointsInSubset; ++i)
        {
            list2.Add(subset[i]);
        }

        if (sub == 2 || sub == 3)
        {
            originalList.Add(list1);
            originalList.Add(list2);

        }
        else
        {
            originalList = GetSubsetsOfPoints(originalList, list1);
            originalList = GetSubsetsOfPoints(originalList, list2);
        }


        return originalList;
    }

    struct Line
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public Line(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
        
        public static bool operator ==(Line original, Line other)
        {
            return (original.pointA == other.pointA && original.pointB == other.pointB);
        }

        public static bool operator !=(Line original, Line other)
        {
            return (original.pointA != other.pointA || original.pointB != other.pointB);
        }

        
        public bool Equals(Line other)
        {
            return pointA == other.pointA && pointB == other.pointB;
        }

        public override int GetHashCode()
        {
            return (int)pointA.x ^ (int)pointB.x + (int)pointA.y ^ (int)pointB.y + (int)pointA.z ^ (int)pointB.z;
        }

        public Line GetInverse()
        {
            return new Line(pointB, pointA);
        }
        
    }

    struct Triangle
    {
        public Vector3 pointA;
        public Vector3 pointB;
        public Vector3 pointC;

        public Triangle(Vector3 _pointA, Vector3 _pointB, Vector3 _pointC)
        {
            pointA = _pointA;
            pointB = _pointB;
            pointC = _pointC;
        }

        public void DrawTriangle()
        {
            Debug.DrawLine(pointA, pointB, Color.blue);
            Debug.DrawLine(pointB, pointC, Color.blue);
            Debug.DrawLine(pointC, pointA, Color.blue);
        }

    }

    bool IsTriangleFlat(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        Vector3 directionAB = GetDirection(pointA, pointB);
        Vector3 directionAC = GetDirection(pointA, pointC);

        return (Vector3.Cross(directionAB, directionAC) == Vector3.zero);
    }

    bool isPointInCircle(Vector3 centerOfCircle, float radius, Vector3 point)
    {
        return (Vector3.Distance(centerOfCircle, point) < radius);
    }

    Vector3 GetCenterOfCircumCircle(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        float distanceAB = Vector3.Distance(pointA, pointB);
        float distanceBC = Vector3.Distance(pointB, pointC);

        Vector3 directionVector1 = GetDirection(pointA, pointB);
        Vector3 directionVector2 = GetDirection(pointB, pointC);

        Vector3 perpendicularVector1 = Vector3.Cross(directionVector1.normalized, Vector3.down);
        Vector3 perpendicularVector2 = Vector3.Cross(directionVector2.normalized, Vector3.down);

        Vector3 perpendicularStart1 = GetMiddleOfSegment(pointA, pointB);
        Vector3 perpendicularEnd1 = new Vector3(
            perpendicularStart1.x + (perpendicularVector1.x * distanceAB),
            0f,
            perpendicularStart1.z + (perpendicularVector1.z * distanceAB));

        Vector3 perpendicularStart2 = GetMiddleOfSegment(pointB, pointC);
        Vector3 perpendicularEnd2 = new Vector3(
            perpendicularStart2.x + (perpendicularVector2.x * distanceBC),
            0f,
            perpendicularStart2.z + (perpendicularVector2.z * distanceBC));

        return GetIntersectionOfSegments(perpendicularStart1, perpendicularEnd1,
                                         perpendicularStart2, perpendicularEnd2);
    }

    bool AreSegmentsCrossing(Vector3 startPointA, Vector3 endPointA, Vector3 startPointB, Vector3 endPointB)
    {
        bool linesCrossing = AreLinesCrossing(startPointA, endPointA, startPointB, endPointB);

        if (!linesCrossing)
            return linesCrossing;

        Vector3 intersection = GetIntersectionOfSegments(startPointA, endPointA, startPointB, endPointB);

        bool isIntersectionOnSegmentA = IsPointOnSegment(startPointA, endPointA, intersection);
        bool isIntersectionOnSegmentB = IsPointOnSegment(startPointB, endPointB, intersection);

        if (isIntersectionOnSegmentA && isIntersectionOnSegmentB)
            return true;

        return false;
    }

    bool IsPointOnSegment(Vector3 pointA, Vector3 pointB, Vector3 pointToTest)
    {
        Vector3 directionAB = GetDirection(pointA, pointB);
        Vector3 directionATest = GetDirection(pointA, pointToTest);

        if (Vector3.Cross(directionAB, directionATest) == Vector3.zero)
        {
            float dp1 = Vector3.Dot(directionAB, directionAB);
            float dp2 = Vector3.Dot(directionAB, directionATest);

            if (dp2 < 0.0f || dp2 > dp1)
                return false;

            return true;
        }

        return false;
    }

    bool AreLinesCrossing(Vector3 startPointA, Vector3 endPointA, Vector3 startPointB, Vector3 endPointB)
    {
        float aPointA = (startPointA.z - endPointA.z) / (startPointA.x - endPointA.x);
        float aPointB = (startPointB.z - endPointB.z) / (startPointB.x - endPointB.x);

        return ((aPointA - aPointB) != 0f);
    }

    Vector3 GetIntersectionOfSegments(Vector3 startPointA, Vector3 endPointA, Vector3 startPointB, Vector3 endPointB)
    {
        Vector3 intersection = new Vector3(0f, 0f, 0f);

        // y = ax + b
        // y = aPointA + bPointA
        float aPointA = (startPointA.z - endPointA.z) / (startPointA.x - endPointA.x);
        float bPointA = startPointA.z - (aPointA * startPointA.x);

        float aPointB = (startPointB.z - endPointB.z) / (startPointB.x - endPointB.x);
        float bPointB = startPointB.z - (aPointB * startPointB.x);

        float intersecX = (bPointB - bPointA) / (aPointA - aPointB);
        float intersecZ = (aPointA * intersecX) + bPointA;

        intersection.x = intersecX;
        intersection.z = intersecZ;

        return intersection;
    }

    Vector3 GetMiddleOfSegment(Vector3 pointA, Vector3 pointB)
    {
        return ((pointB - pointA) / 2) + pointA;
    }

    Vector3 GetDirection(Vector3 pointA, Vector3 pointB)
    {
        return pointB - pointA;
    }

    void OrderList()
    {
        List<Vector3> orderedList = new List<Vector3>();

        while (listOfPoints.Count > 0)
        {
            Vector3 lowest = listOfPoints[0];

            for (int i = 0; i < listOfPoints.Count; ++i)
                lowest = getLowestVector(lowest, listOfPoints[i]);

            orderedList.Add(lowest);
            listOfPoints.Remove(lowest);
        }

        listOfPoints = orderedList;
    }

    Vector3 getLowestVector(Vector3 vector1, Vector3 vector2)
    {
        Vector3 lowest = vector1;

        if ((vector2.x < lowest.x) || (vector2.x == lowest.x && vector2.z < lowest.z))
            lowest = vector2;

        return lowest;
    }

    void generateListOfPoints(Vector3 minBound, Vector3 maxBound)
    {
        if (rngSeed != "")
            Random.seed = rngSeed.GetHashCode();
        
        for (int i = 0; i < nbOfPoints; ++i)
        {
            float x = Random.Range(minBound.x, maxBound.x);
            float z = Random.Range(minBound.z, maxBound.z);

            Vector3 newPoint = new Vector3(x, 0, z);

            listOfPoints.Add(newPoint);
        }
    }

	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos ()
    {
        Color color = Color.white;
        float radius = .1f;

        Gizmos.color = color;

        for (int i = 0; i < listOfPoints.Count; ++i)
        {
            Gizmos.DrawWireSphere(listOfPoints[i], radius);
        }

        /*
        for (int i = 0; i < listOfTriangles.Count; ++i)
        {
            Debug.DrawLine(listOfTriangles[i].pointA, listOfTriangles[i].pointB, Color.blue);
            Debug.DrawLine(listOfTriangles[i].pointB, listOfTriangles[i].pointC, Color.blue);
            Debug.DrawLine(listOfTriangles[i].pointC, listOfTriangles[i].pointA, Color.blue);
        }
         * */

        for (int i = 0; i < subsetsOfLines.Count; ++i)
        {
            for (int j = 0; j < subsetsOfLines[i].Count; ++j )
                Debug.DrawLine(subsetsOfLines[i][j].pointA, subsetsOfLines[i][j].pointB);
        }

        
        for (int i = 0; i < joiningLines.Count-2; i += 3)
        {
                Debug.DrawLine(joiningLines[i].pointA, joiningLines[i].pointB, Color.blue);
                Debug.DrawLine(joiningLines[i+1].pointA, joiningLines[i+1].pointB, Color.blue);
                Vector3 center = GetCenterOfCircumCircle(joiningLines[i].pointA, joiningLines[i].pointB, joiningLines[i + 1].pointA);
                
            if (i > 15)
                Gizmos.DrawWireSphere(center, Vector3.Distance(center, joiningLines[i].pointA));
        }
         
        for (int i = 0; i < joiningLines.Count; ++i)
        {
           Debug.DrawLine(joiningLines[i].pointA, joiningLines[i].pointB, Color.blue);
        }
    }

}
