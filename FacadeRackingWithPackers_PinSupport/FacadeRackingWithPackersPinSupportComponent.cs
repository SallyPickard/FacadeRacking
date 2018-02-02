using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;

namespace FacadeRackingWithPackers_PinSupport
{
    public class FacadeRackingWithPackersPinSupportComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FacadeRackingWithPackersPinSupportComponent()
          : base("FacadeRackingWithPackers_PinSupport", "ASpi",
              "Construct an Archimedean, or arithmetic, spiral given its radii and number of turns.",
              "FacadeToolKit", "FacadeRacking")
        {
        }

        /// Registers all the input parameters for this component.
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Span", "Span", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Number of Points On Curve", "Number of Points On Curve", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Deflection", "Deflection", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("No.IGUs", "no.IGUs", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height of IGUs", "Height of IGUs", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Sway height/ X", "Sway height/ X", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Nosing Width", "Nosing Width", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Clearance", "Max clearance", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Min Clearance", "Min clearance", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Installation Clearance", "Installation Clearance", "", GH_ParamAccess.item);
        }


        /// Registers all the output parameters for this component.
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("PolyLine Bottom", "Top Polyline", "PolyLine Bottom", GH_ParamAccess.item);
            pManager.AddCurveParameter("PolyLine Top", "Bottom Polyline", "PolyLine Top", GH_ParamAccess.item);
            pManager.AddLineParameter("Mullions", "Mullions", "PolyLine Top", GH_ParamAccess.list);
            pManager.AddLineParameter("Top Transom", "Top Transom", "Top Transom", GH_ParamAccess.list);
            pManager.AddLineParameter("Base Transom", "Base Transom", "Base Transom", GH_ParamAccess.list);
            pManager.AddLineParameter("max vertical Limits", "max vertical Limits", "max vertical Limits", GH_ParamAccess.list);
            pManager.AddLineParameter("min vertical Limits", "min vertical Limits", "min vertical Limits", GH_ParamAccess.list);
            pManager.AddLineParameter("top transom Limits", "top transom limits", "top transom Limits", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Glass plane", "Glass plane", "Glass plane", GH_ParamAccess.item);
            pManager.AddRectangleParameter("Glass panels", "Glass panels", "Glass panels", GH_ParamAccess.list);
            pManager.AddMeshParameter("Coloured Mesh Panels", "Coloured Mesh Panels", "Coloured Mesh Panels", GH_ParamAccess.list);
            pManager.AddPointParameter("Glass Corners", "Glass Corners", "Glass Corners", GH_ParamAccess.list);


        }

        /// This is the method that actually does the work.
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Prevent the plugin from running if nothing is connected to the inputParams
            bool run = false;
            if (!DA.GetData(0, ref run)) return;
            if (!run) return;
            double span = 0;
            if (!DA.GetData(1, ref span)) return;
            double numberOfPointsOnCurve = 0;
            if (!DA.GetData(2, ref numberOfPointsOnCurve)) return;
            double deflectionSlider = 0;
            if (!DA.GetData(3, ref deflectionSlider)) return;
            int noIGUs = 2;
            if (!DA.GetData(4, ref noIGUs)) return;
            double heightOfIGU = 0;
            if (!DA.GetData(5, ref heightOfIGU)) return;
            double sway = 0;
            if (!DA.GetData(6, ref sway)) return;
            double nosingWidth = 0;
            if (!DA.GetData(7, ref nosingWidth)) return;
            double maxClearance = 0;
            if (!DA.GetData(8, ref maxClearance)) return;
            double minClearance = 0;
            if (!DA.GetData(9, ref minClearance)) return;
            double installationClearance = 0;
            if (!DA.GetData(10, ref installationClearance)) return;


            double NumPointsInSeries = FindNumPointsInSeries(numberOfPointsOnCurve);
            List<double> ZeroToOneList = XSeries(numberOfPointsOnCurve, NumPointsInSeries);
            List<double> BaseXCoordinateList = XCoordinates(ZeroToOneList, span);
            List<double> BaseYCoordinateList = YBaseCoordinates(ZeroToOneList, deflectionSlider);


            //double TopCalc = TopBitOutput(noIGUs);
            List<double> TopYCoordinateList = YTopCoords(BaseYCoordinateList, heightOfIGU);
            List<double> TopXCoordinateList = XTopCoords(BaseXCoordinateList, heightOfIGU, sway);

            // set outputs

            //Draw curves curve
            Polyline BasePolyLine = CreatePLine(BaseXCoordinateList, BaseYCoordinateList);
            DA.SetData(0, BasePolyLine);

            Polyline TopPolyLine = CreatePLine(TopXCoordinateList, TopYCoordinateList);
            DA.SetData(1, TopPolyLine);

            Point3d[] TransomPointsTop = TopTransomPoints(TopPolyLine, noIGUs);
            Point3d[] TransomPointsBase = BaseTransomPoints(BasePolyLine, noIGUs);

            //draw mullions
            List<Line> Mullions = MullionOutput(TransomPointsTop, TransomPointsBase, noIGUs);
            DA.SetDataList(2, Mullions);

            //draw transoms
            List<Line> TopTransom = TraansomLines(TopPolyLine, noIGUs);
            DA.SetDataList(3, TopTransom);

            List<Line> BaseTransom = TraansomLines(BasePolyLine, noIGUs);
            DA.SetDataList(4, BaseTransom);

            //Draw vertical limits
            List<Line> MaxVerticalLimits = LineLimits(nosingWidth, maxClearance, Mullions, TransomPointsTop, TransomPointsBase);
            DA.SetDataList(5, MaxVerticalLimits);

            List<Line> MinVerticalLimits = LineLimits(nosingWidth, minClearance, Mullions, TransomPointsTop, TransomPointsBase);
            DA.SetDataList(6, MinVerticalLimits);

            //Draw top transom limits
            List<Line> TopLimit = TransomLineLimits(nosingWidth, minClearance, TransomPointsTop);
            DA.SetDataList(7, TopLimit);

            //Draw plane
            Plane GlassPlane = GlassPlaneCalc(nosingWidth, installationClearance, heightOfIGU, span, noIGUs, TransomPointsBase, TransomPointsTop);
            DA.SetData(8, GlassPlane);

            //Draw corner points
            List<Point3d> GlassCornerPoints = GlassPoints(nosingWidth, installationClearance, heightOfIGU, span, noIGUs, TransomPointsBase, TransomPointsTop);
            DA.SetDataList(11, GlassCornerPoints);


            List<Rectangle3d> GlassPanelShape = GlassPanels(GlassCornerPoints, nosingWidth, installationClearance, heightOfIGU, span, noIGUs, TransomPointsBase, TransomPointsTop);
            DA.SetDataList(9, GlassPanelShape);

            //Detect clashes
            List<bool> Clashes = DetectClashes(noIGUs, GlassCornerPoints, MaxVerticalLimits, MinVerticalLimits, TopLimit);

            //Colour panels
            List<Mesh> ColouredMesh = ColourRectangles(Clashes, GlassPanelShape);
            DA.SetDataList(10, ColouredMesh);




        }



        //Find number of points on the curve

        public double FindNumPointsInSeries(double numberPointsOnCurve)
        {
            double NumPointsInSeries = new double();

            NumPointsInSeries = numberPointsOnCurve + 1;

            return NumPointsInSeries;

        }

        //Find 0 to 1 series
        public List<double> XSeries(double numberPointsOnCurve, double NumPointsInSeries)
        {

            double Increment = new double();

            Increment = 1 / numberPointsOnCurve;

            List<double> ListZeroToOne = new List<double>();

            for (int i = 0; i < NumPointsInSeries; i++)
            {
                ListZeroToOne.Add(Increment * i);
            }

            return ListZeroToOne;

        }

        //Find X Coordinates
        public List<double> XCoordinates(List<double> ZeroToOneList, double span)
        {
            List<double> xCoordinates = new List<double>();

            for (int i = 0; i < ZeroToOneList.Count; i++)
            {
                xCoordinates.Add(ZeroToOneList[i] * span);
            }

            return xCoordinates;
        }

        //Find base Y Coordinates
        public List<double> YBaseCoordinates(List<double> ZeroToOneList, double deflection)
        {
            List<double> deflectedYCoordinates = new List<double>();

            for (int i = 0; i < ZeroToOneList.Count; i++)
            {
                double temp_y = ZeroToOneList[i] - (2 * Math.Pow(ZeroToOneList[i], 3)) + (Math.Pow(ZeroToOneList[i], 4));

                deflectedYCoordinates.Add(temp_y);
            }

            int total = deflectedYCoordinates.Count;
            int middleIndex = total / 2;

            double theMiddleValue = deflectedYCoordinates[middleIndex];

            List<double> seriesDividedByMiddle = new List<double>();

            for (int i = 0; i < deflectedYCoordinates.Count; i++)
            {
                seriesDividedByMiddle.Add(deflectedYCoordinates[i] / theMiddleValue);

            }

            //Multiply by deflection and -1
            List<double> yCoordinates = new List<double>();

            for (int i = 0; i < seriesDividedByMiddle.Count; i++)
            {
                double temp = seriesDividedByMiddle[i] * deflection * -1;

                yCoordinates.Add(temp);

            }

            return yCoordinates;

        }

        //Find top y coords
        public List<double> YTopCoords(List<double> BaseYCoordinateList, double heightOfIGU)
        {
            List<double> topYCoordinateList = new List<double>();


            for (int a = 0; a < BaseYCoordinateList.Count; a++)
            {
                topYCoordinateList.Add(BaseYCoordinateList[a] + heightOfIGU);
            }

            return topYCoordinateList;

        }

        //Find top x coords
        public List<double> XTopCoords(List<double> baseXCoordinateList, double heightOfIGU, double sway)
        {
            List<double> topXCoordinateList = new List<double>();
            if (sway != 0 && sway != heightOfIGU)
            {
                double temp = heightOfIGU / sway;

                for (int i = 1; i < baseXCoordinateList.Count; i++)
                {
                    topXCoordinateList.Add(baseXCoordinateList[i] + temp);
                }

            }

            else
            {
                for (int a = 0; a < baseXCoordinateList.Count; a++)
                {
                    topXCoordinateList.Add(baseXCoordinateList[a]);
                }

            }

            return topXCoordinateList;

        }
        /*
        //Top Calculation
        public double TopBitOutput(int noIGUs)
        {
            List<double> series = new List<double>();

            for (int i = 0; i < noIGUs + 1; i++)
            {
                series.Add(i / noIGUs);
            }

            //insert list of yvalues called "yvalues"
            //shift down (B)

            List<double> yvaluesB = new List<double>();

            for (int i = 0; i < noIGUs; i++)
            {
                yvaluesB.Add(i);
            }

            List<double> newYList = new List<double>();

            for (int i = 1; i < yvaluesB.Count; i++)
            {

                newYList.Add(yvaluesB[i]);
            }

            //List where end one is removed (A)
            List<double> yvaluesA = new List<double>();

            for (int i = 0; i < noIGUs; i++)
            {
                yvaluesA.Add(i);
            }

            yvaluesA.Remove(yvaluesA.Count - 1);

            //  A-B

            List<double> aMinusB = new List<double>();

            for (int i = 0; i < yvaluesA.Count; i++)
            {
                aMinusB.Add(Math.Abs(yvaluesA[i] - yvaluesB[i]));
            }

            aMinusB.Sort();

            double topBitOutput = aMinusB[aMinusB.Count - 1];

            return topBitOutput;
        }*/

        //Draw PolyLine
        public Polyline CreatePLine(List<double> xCoordinateList, List<double> yCoordinateList)
        {
            Polyline PL = new Polyline();
            for (int i = 0; i < xCoordinateList.Count; i++)
            {
                Point3d pt = new Point3d(xCoordinateList[i], yCoordinateList[i], 0);
                PL.Add(pt);
            }

            return PL;
        }

        //TopTransom points
        public Point3d[] TopTransomPoints(Polyline topPolyLine, int noIGUs)
        {
            Point3d[] Points = new Point3d[noIGUs + 1];
            double[] PointArray = topPolyLine.ToNurbsCurve().DivideByCount(noIGUs, true, out Points);

            return Points;
        }

        // BaseTransom
        public Point3d[] BaseTransomPoints(Polyline BasePolyLine, int noIGUs)
        {
            Point3d[] Points = new Point3d[noIGUs + 1];
            double[] PointArray = BasePolyLine.ToNurbsCurve().DivideByCount(noIGUs, true, out Points);

            return Points;
        }

        //Mullions 
        public List<Line> MullionOutput(Point3d[] topTransomPoints, Point3d[] baseTransomPoints, int noIGUs)
        {

            List<Line> listOfLines = new List<Line>();
            for (int i = 0; i < topTransomPoints.Length; i++)
            {
                Line temp = new Line(baseTransomPoints[i], topTransomPoints[i]);

                listOfLines.Add(temp);
            }

            return listOfLines;

        }

        //transom Lines
        public List<Line> TraansomLines(Polyline topOrBasePolyLine, int noIGUs)
        {
            Point3d[] Points = new Point3d[noIGUs + 1];
            double[] PointArray = topOrBasePolyLine.ToNurbsCurve().DivideByCount(noIGUs, true, out Points);

            List<Point3d> PointList = new List<Point3d>(Points);

            List<Line> listOfLines = new List<Line>();
            for (int i = 0; i < PointArray.Length - 1; i++)
            {
                Line temp = new Line(Points[i], Points[i + 1]);

                listOfLines.Add(temp);
            }

            return listOfLines;

        }

        //Vertical limits mullions
        public List<Line> LineLimits(double nosingSize, double minmaxClearance, List<Line> mullionOutputLines, Point3d[] topTransomPoints, Point3d[] baseTransomPoints)
        {
            double nosingDiv2 = nosingSize / 2;
            double maxValRight = (nosingDiv2 + minmaxClearance);
            double maxValLeft = (nosingDiv2 + minmaxClearance) * -1;

            List<Line> limitLines = new List<Line>();
            Vector3d limitLeft = new Vector3d(maxValLeft, 0, 0);
            Vector3d limitRight = new Vector3d(maxValRight, 0, 0);

            List<Line> safeLimits = new List<Line>();
            for (int i = 0; i < mullionOutputLines.Count; i++)
            {

                Point3d tempBaseL = baseTransomPoints[i] + limitLeft;
                Point3d tempTopL = topTransomPoints[i] + limitLeft;

                Line tempL = new Line(tempBaseL, tempTopL);

                Point3d tempBaseR = baseTransomPoints[i] + limitRight;
                Point3d tempTopR = topTransomPoints[i] + limitRight;

                Line tempR = new Line(tempBaseR, tempTopR);

                safeLimits.Add(tempL);
                safeLimits.Add(tempR);
            }

            return safeLimits;
        }

        //Transom limits
        public List<Line> TransomLineLimits(double nosingSize, double minClearance, Point3d[] topTransomPoints)
        {
            double nosingDiv2 = nosingSize / 2;
            double maxValdown = (nosingDiv2 + minClearance) * -1;

            List<Line> limitLines = new List<Line>();
            Vector3d limitDown = new Vector3d(0, maxValdown, 0);

            List<Line> safeLimits = new List<Line>();
            for (int i = 0; i < topTransomPoints.Length - 1; i++)
            {
                Point3d tempTop1 = topTransomPoints[i] + limitDown;
                Point3d tempTop2 = topTransomPoints[i + 1] + limitDown;

                Line tempLine = new Line(from: tempTop1, to: tempTop2);

                safeLimits.Add(tempLine);
            }
            return safeLimits;
        }

        //Glass plane
        public Plane GlassPlaneCalc(double nosingSize, double installationClearance, double heightOfPanel, double span, int noIGUs, Point3d[] transomPointBase, Point3d[] transomPointTop)
        {

            double glassHeight = heightOfPanel - ((nosingSize / 2) + installationClearance);

            double glassWidth_tempA = span / noIGUs;

            double glassWidth_tempB = (installationClearance * 2) + nosingSize;

            //glassWidth = A-B

            double glassWidth = glassWidth_tempA - glassWidth_tempB;

            List<Vector3d> transomPointVList = new List<Vector3d>();


            for (int i = 0; i < noIGUs; i++)
            {
                Vector3d temp = transomPointBase[i + 1] - transomPointBase[i];

                Vector3d temp2 = new Vector3d();

                temp2.PerpendicularTo(temp);
                Vector3d rotate = transomPointBase[i + 1] - transomPointBase[i];
                temp2.Rotate(0, rotate);

                Vector3d temp3 = new Vector3d();
                temp3.PerpendicularTo(temp2);

                transomPointVList.Add(temp);
            }




            //Vector3d panelHeight = new Vector3d(0,0, glassWidth);
            List<Rectangle3d> glassRecs = new List<Rectangle3d>();

            for (int i = 0; i < noIGUs; i++)
            {
                Plane pln = new Plane(transomPointBase[i], transomPointVList[i]);
                Rectangle3d glassRectangle = new Rectangle3d(pln, glassWidth, glassHeight);
                glassRecs.Add(glassRectangle);


            }
            Plane pln2 = new Plane(transomPointBase[1], transomPointVList[1]);



            return pln2;

        }

        //Glass points
        public List<Point3d> GlassPoints(double nosingSize, double installationClearance, double heightOfPanel, double span, int noIGUs, Point3d[] transomPointBase, Point3d[] transomPointTop)
        {
            double glassHeight = heightOfPanel - ((nosingSize / 2) + installationClearance);

            double glassWidth_tempA = span / noIGUs;

            double glassWidth_tempB = (installationClearance * 2) + nosingSize;

            //glassWidth = A-B

            double glassWidth = glassWidth_tempA - glassWidth_tempB;

            double gWRedEachSide = glassWidth_tempB / 2;

            //X base location

            List<Point3d> startPoints = new List<Point3d>();
            List<Point3d> endPoints = new List<Point3d>();
            List<Point3d> topPoints = new List<Point3d>();
            List<Point3d> allPoints = new List<Point3d>();

            double halfNoPanels = noIGUs / 2;
            List<Point3d> glassBasePoints = new List<Point3d>();
            int noIGUint = Convert.ToInt32(noIGUs);
            int jLower = ((noIGUint * 2) + 2);
            int i = 0;
            int jLower2 = ((noIGUint * 2) + 4);

            if ((halfNoPanels % 1) == 0)  // No. panels is even i.e. where no panels/2 is an integer
            {

                for (int j = 0; j < noIGUs / 2; j++)
                {
                    //Find vector along transom to glass start point

                    Vector3d temp = new Vector3d();
                    temp = transomPointBase[j + 1] - transomPointBase[j];

                    double originalLength = temp.Length;
                    temp.Unitize();
                    double unitLength = temp.Length;

                    Vector3d moveAlongLine = new Vector3d();
                    moveAlongLine = temp * gWRedEachSide;

                    //Find start point of glass
                    Point3d xStartPoint = new Point3d();
                    xStartPoint = transomPointBase[j] + moveAlongLine;
                    startPoints.Add(xStartPoint);

                    //Find end point of glass

                    Point3d xEndPoint = new Point3d();
                    Vector3d moveGlassWidth = new Vector3d(glassWidth, 0, 0);
                    xEndPoint = xStartPoint + moveGlassWidth;
                    endPoints.Add(xEndPoint);

                    //Vector 
                    Vector3d xVector = new Vector3d(moveGlassWidth);
                    Vector3d zAxis = new Vector3d(0, 0, 1);

                    Vector3d yVector = new Vector3d(0, glassHeight, 0);

                    //Find Top points
                    Point3d topLeft = new Point3d();

                    topLeft = xStartPoint + yVector;

                    topPoints.Add(topLeft);

                    Point3d topRight = new Point3d();

                    topRight = xEndPoint + yVector;

                    topPoints.Add(topRight);

                    //FINAL LIST OF POINTS TOP AND BOTTOM

                    allPoints.Add(xStartPoint);
                    allPoints.Add(topLeft);
                    allPoints.Add(xEndPoint);
                    allPoints.Add(topRight);

                    i = i + 1;
                }


                for (int j = (noIGUs / 2); j < noIGUs; j++)
                {
                    //Find vector along transom to glass start point

                    Vector3d temp = new Vector3d();
                    temp = transomPointBase[j + 1] - transomPointBase[j];

                    double originalLength = temp.Length;
                    temp.Unitize();
                    double unitLength = temp.Length;

                    Vector3d moveAlongLine = new Vector3d();
                    moveAlongLine = temp * gWRedEachSide;

                    //Find end point of glass ******temp***** (moves on base transom)
                    Point3d xEndPointTemp = new Point3d();
                    xEndPointTemp = transomPointBase[j] + moveAlongLine;


                    //Find start point of glass

                    Point3d xStartPoint = new Point3d();
                    Vector3d moveGlassWidth = new Vector3d();
                    moveGlassWidth = Vector3d.Multiply(glassWidth, temp);
                    xStartPoint = xEndPointTemp + moveGlassWidth;
                    startPoints.Add(xStartPoint);

                    //End point to plot
                    Point3d xEndPoint = new Point3d();
                    Vector3d xVector = new Vector3d(glassWidth, 0, 0);
                    xEndPoint = xStartPoint - xVector;
                    endPoints.Add(xEndPoint);

                    //Find Top points
                    Vector3d yVector = new Vector3d(0, glassHeight, 0);

                    Point3d topLeft = new Point3d();

                    topLeft = xEndPoint + yVector;

                    topPoints.Add(topLeft);

                    Point3d topRight = new Point3d();

                    topRight = xStartPoint + yVector;

                    topPoints.Add(topRight);

                    //FINAL LIST OF POINTS TOP AND BOTTOM

                    allPoints.Add(xEndPoint);
                    allPoints.Add(topLeft);
                    allPoints.Add(xStartPoint);
                    allPoints.Add(topRight);

                    i = i + 1;
                }



            }

            if ((halfNoPanels % 1) != 0)  // No. panels is even i.e. where no panels/2 is an integer
            {
                double halfNoIGUs = noIGUs / 2;
                double noPanelRndUp = Math.Round(halfNoIGUs + 0.1);
                for (int j = 0; j < noPanelRndUp; j++)
                {
                    //Find vector along transom to glass start point

                    Vector3d temp = new Vector3d();
                    temp = transomPointBase[j + 1] - transomPointBase[j];

                    double originalLength = temp.Length;
                    temp.Unitize();
                    double unitLength = temp.Length;

                    Vector3d moveAlongLine = new Vector3d();
                    moveAlongLine = temp * gWRedEachSide;

                    //Find start point of glass
                    Point3d xStartPoint = new Point3d();
                    xStartPoint = transomPointBase[j] + moveAlongLine;
                    startPoints.Add(xStartPoint);

                    //Find end point of glass

                    Point3d xEndPoint = new Point3d();
                    Vector3d moveGlassWidth = new Vector3d(glassWidth, 0, 0);
                    xEndPoint = xStartPoint + moveGlassWidth;
                    endPoints.Add(xEndPoint);

                    //Vector 
                    Vector3d xVector = new Vector3d(moveGlassWidth);
                    Vector3d zAxis = new Vector3d(0, 0, 1);

                    Vector3d yVector = new Vector3d(0, glassHeight, 0);

                    //Find Top points
                    Point3d topLeft = new Point3d();

                    topLeft = xStartPoint + yVector;

                    topPoints.Add(topLeft);

                    Point3d topRight = new Point3d();

                    topRight = xEndPoint + yVector;

                    topPoints.Add(topRight);

                    //FINAL LIST OF POINTS TOP AND BOTTOM

                    allPoints.Add(xStartPoint);
                    allPoints.Add(topLeft);
                    allPoints.Add(xEndPoint);
                    allPoints.Add(topRight);

                    i = i + 1;
                }

                for (int j = Convert.ToInt32(noPanelRndUp); j < noIGUs; j++)
                {
                    //Find vector along transom to glass start point

                    Vector3d temp = new Vector3d();
                    temp = transomPointBase[j + 1] - transomPointBase[j];

                    double originalLength = temp.Length;
                    temp.Unitize();
                    double unitLength = temp.Length;

                    Vector3d moveAlongLine = new Vector3d();
                    moveAlongLine = temp * gWRedEachSide;

                    //Find end point of glass
                    Point3d xEndPoint = new Point3d();
                    xEndPoint = transomPointBase[j] + moveAlongLine;
                    endPoints.Add(xEndPoint);

                    //Find start point of glass

                    Point3d xStartPoint = new Point3d();
                    Vector3d moveGlassWidth = new Vector3d();
                    moveGlassWidth = Vector3d.Multiply(glassWidth, temp);
                    xStartPoint = xEndPoint + moveGlassWidth;
                    startPoints.Add(xStartPoint);

                    Vector3d yVector = new Vector3d(0, glassHeight, 0);

                    //Find Top points

                    Point3d topLeft = new Point3d();

                    topLeft = xEndPoint + yVector;

                    topPoints.Add(topLeft);

                    Point3d topRight = new Point3d();

                    topRight = xStartPoint + yVector;

                    topPoints.Add(topRight);

                    //FINAL LIST OF POINTS TOP AND BOTTOM

                    allPoints.Add(xEndPoint);
                    allPoints.Add(topLeft);
                    allPoints.Add(xStartPoint);
                    allPoints.Add(topRight);

                    i = i + 1;

                }


            }

            return allPoints;
        }

        //Draw Panels
        public List<Rectangle3d> GlassPanels(List<Point3d> gBasePoints, double nosingSize, double installationClearance, double heightOfPanel, double span, double noIGUs, Point3d[] transomPointBase, Point3d[] transomPointTop)
        {
            double glassHeight = heightOfPanel - ((nosingSize / 2) + installationClearance);

            double glassWidth_tempA = span / noIGUs;

            double glassWidth_tempB = (installationClearance * 2) + nosingSize;

            //glassWidth = A-B

            double glassWidth = glassWidth_tempA - glassWidth_tempB;

            double gWRedEachSide = glassWidth_tempB / 2;

            List<Rectangle3d> glassRecs = new List<Rectangle3d>();

            double halfNoPanels = noIGUs / 2;
            int k = 0;
            int n = 4;
            List<Point3d> glassBasePoints = new List<Point3d>();
            int noIGUint = Convert.ToInt32(noIGUs);
            int jLower = ((noIGUint * 2) + 2);
            int l = 4;
            int jLower2 = ((noIGUint * 2) + 4);

            if ((halfNoPanels % 1) == 0)  // No. panels is even i.e. where no panels/2 is an integer
            {
                double panelNoOv2 = noIGUs / 2;

                for (int j = 0; j < (noIGUs * 2) - 1; j += n)
                {
                    glassBasePoints.Add(gBasePoints[j]);

                    //x vector 
                    Vector3d xVector = new Vector3d(glassWidth, 0, 0);

                    Vector3d yVector = new Vector3d(0, glassHeight, 0);

                    Plane pln = new Plane(glassBasePoints[k], xVector, yVector);

                    Rectangle3d glassRectangle = new Rectangle3d(pln, glassWidth, glassHeight);
                    glassRecs.Add(glassRectangle);


                    k = k + 1;
                }


                for (int j = jLower; j < gBasePoints.Count; j += l)
                {
                    glassBasePoints.Add(gBasePoints[j]);

                    //x vector 

                    Vector3d xVector = new Vector3d(-glassWidth, 0, 0);

                    Vector3d yVector = new Vector3d(0, glassHeight, 0);

                    Plane pln = new Plane(glassBasePoints[k], xVector, yVector);

                    Rectangle3d glassRectangle = new Rectangle3d(pln, glassWidth, glassHeight);
                    glassRecs.Add(glassRectangle);

                    k = k + 1;
                }



            }

            if ((halfNoPanels % 1) != 0)  // No. panels is even i.e. where no panels/2 is an integer
            {
                int t = 4;
                for (int j = 0; j < ((noIGUs * 2) + 1); j += t)
                {
                    double noPanelRndUp = Math.Round(noIGUs / 2, 0, MidpointRounding.AwayFromZero);

                    glassBasePoints.Add(gBasePoints[j]);

                    //x vector 
                    Vector3d xVector = new Vector3d(glassWidth, 0, 0);

                    Vector3d yVector = new Vector3d(0, glassHeight, 0);

                    Plane pln = new Plane(glassBasePoints[k], xVector, yVector);

                    Rectangle3d glassRectangle = new Rectangle3d(pln, glassWidth, glassHeight);
                    glassRecs.Add(glassRectangle);


                    k = k + 1;
                }

                for (int j = jLower2; j < gBasePoints.Count; j += l)
                {
                    glassBasePoints.Add(gBasePoints[j]);

                    //x vector 

                    Vector3d xVector = new Vector3d(-glassWidth, 0, 0);

                    Vector3d yVector = new Vector3d(0, glassHeight, 0);

                    Plane pln = new Plane(glassBasePoints[k], xVector, yVector);

                    Rectangle3d glassRectangle = new Rectangle3d(pln, glassWidth, glassHeight);
                    glassRecs.Add(glassRectangle);

                    k = k + 1;
                }


            }


            return glassRecs;
        }

        //Detect clashes
        private List<bool> DetectClashes(int noIGUs, List<Point3d> glassCornerPoints, List<Line> maxVertLimit, List<Line> minVertLimit, List<Line> topLimitLines)
        {

            //Separate Lists
            List<Point3d> bottomLeft = new List<Point3d>();
            List<Point3d> topLeft = new List<Point3d>();
            List<Point3d> bottomRight = new List<Point3d>();
            List<Point3d> topRight = new List<Point3d>();

            //checking x values - checking in min-max zones

            int n = 4;
            for (int i = 0; i < glassCornerPoints.Count; i += n)
            {
                bottomLeft.Add(glassCornerPoints[i]);
            }

            for (int i = 1; i < glassCornerPoints.Count; i += n)
            {
                topLeft.Add(glassCornerPoints[i]);
            }

            for (int i = 2; i < glassCornerPoints.Count; i += n)
            {
                bottomRight.Add(glassCornerPoints[i]);
            }

            for (int i = 3; i < glassCornerPoints.Count; i += n)
            {
                topRight.Add(glassCornerPoints[i]);
            }

            int limitCount = 1;
            List<bool> clashing = new List<bool>();

            //checking top transom offset is not exceeded
            //X coords of left and right
            List<double> xLeftLineList = new List<double>();
            for (int j = 0; j < noIGUs; j++)
            {

                xLeftLineList.Add(topLeft[j].X);

            }

            List<double> xRightLineList = new List<double>();
            for (int j = 0; j < noIGUs; j++)
            {

                xRightLineList.Add(topRight[j].X);

            }

            // Y coords of top limit line Left
            List<double> yCoordListLeft = new List<double>();
            for (int j = 0; j < noIGUs; j++)
            {
                Vector3d longVect = new Vector3d(0, Int64.MaxValue, 0);
                Point3d endofVertLine = bottomLeft[j] + longVect;
                Line vertLine = new Line(bottomLeft[j], endofVertLine);


                Point3d startPoint = topLimitLines[j].From;
                Point3d endPoint = topLimitLines[j].To;
                Vector3d lineV = endPoint - startPoint;
                Line vectorLine = new Line(startPoint, endPoint);

                Point3d pointOnLimitLine = vectorLine.ClosestPoint(topLeft[j], true);


                // Point3d aPoint = topLimitLines[i].PointAt(xLeftLineList[i]);


                yCoordListLeft.Add(pointOnLimitLine.Y);

            }

            // Y coords of top limit line Right
            List<double> yCoordListRight = new List<double>();
            for (int j = 0; j < noIGUs; j++)
            {
                Vector3d longVect = new Vector3d(0, Int64.MaxValue, 0);
                Point3d endofVertLine = bottomLeft[j] + longVect;
                Line vertLine = new Line(bottomLeft[j], endofVertLine);


                Point3d startPoint = topLimitLines[j].From;
                Point3d endPoint = topLimitLines[j].To;
                Vector3d lineV = endPoint - startPoint;
                Line vectorLine = new Line(startPoint, endPoint);

                Point3d pointOnLimitLine = vectorLine.ClosestPoint(topRight[j], true);

                yCoordListRight.Add(pointOnLimitLine.Y);

            }


            for (int i = 0; i < (noIGUs); i++)
            {
                //checking all rules
                if
                (
                    topLeft[i].Y < yCoordListLeft[i] && //top line limit left 
                    topRight[i].Y < yCoordListRight[i] && // top line limit right
                    bottomLeft[i].X > minVertLimit[limitCount].FromX &&  //here and below are for min and max vertical limits
                    bottomLeft[i].X < maxVertLimit[limitCount].FromX &&
                    bottomRight[i].X < minVertLimit[1 + limitCount].FromX &&
                    bottomRight[i].X > maxVertLimit[1 + limitCount].FromX &&
                    topLeft[i].X > minVertLimit[limitCount].FromX &&
                    topLeft[i].X < maxVertLimit[limitCount].FromX &&
                    topRight[i].X < minVertLimit[1 + limitCount].FromX &&
                    topRight[i].X > maxVertLimit[1 + limitCount].FromX &&

                    topLeft[i].X > minVertLimit[limitCount].ClosestPoint(topLeft[i], true).X &&
                    topLeft[i].X < maxVertLimit[limitCount].ClosestPoint(topLeft[i], true).X &&
                    topRight[i].X < minVertLimit[1 + limitCount].ClosestPoint(topRight[i], true).X &&
                    topRight[i].X > maxVertLimit[1 + limitCount].ClosestPoint(topRight[i], true).X)
                {
                    clashing.Add(false);
                }
                else
                {
                    clashing.Add(true);
                }

                limitCount = limitCount + 2;

            }
            return clashing;

        }

        //Make and colour panel meshes
        private List<Mesh> ColourRectangles(List<bool> clashList, List<Rectangle3d> glassPanels)
        {
            List<Mesh> meshes = new List<Mesh>();
            for (int panel = 0; panel < glassPanels.Count; panel++)
            {
                Polyline rectangleLine = glassPanels[panel].ToPolyline();
                Mesh newmesh = Mesh.CreateFromClosedPolyline(rectangleLine);
                meshes.Add(newmesh);


                if (clashList[panel] == true)
                {
                    Color meshColor = Color.Red;
                    newmesh.VertexColors.CreateMonotoneMesh(meshColor);

                }
                else
                {
                    Color meshColor = Color.Green;
                    newmesh.VertexColors.CreateMonotoneMesh(meshColor);
                }

            }
            return meshes;
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                Bitmap myBitmap = new Bitmap("C:\Users\spickard\OneDrive\Documents\Code\FacadeToolkit\FacadeRackingWithPackers_PinSupport\facadeRackingWithPackers_PinSupport");
                Icon myIcon = ;
                return Resources.IconForThisComponent;
                return null;
            }
        }

        //protected override System.Drawing.Bitmap Icon
        //{
        //    get
        //    {
        //        // You can add image files to your project resources and access them like this:
        //        return Resources.IconForThisComponent;
        //        return null;
        //    }
        //}

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("27a52b09-a6aa-4fc8-9bcb-6f65f12d54a0"); }
        }
    }
}
