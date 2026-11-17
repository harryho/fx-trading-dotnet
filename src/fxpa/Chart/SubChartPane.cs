// -----
// MIT License
//
// Copyright (c) 2005-2026 Harry Ho
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// -----

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

using System.ComponentModel;
using System.Windows.Forms;

namespace fxpa
{
    public class SubChartPane : ChartPane
    {
        /// <summary>
        /// If the pane has a master pane, how should it synchronize (in both directions, or x only).
        /// Synchronizing in X only is usefull when slave pane has a different Y scale than the master pane.
        /// </summary>
        public enum MainChartPaneSyncModeEnum
        {
            XAxis
        }

        bool _showMainChartSynchronizationImage = true;
        /// <summary>
        /// Should the pane be showing the image specifying if it is synchronzed with master.
        /// </summary>
        public bool ShowMainChartSynchronizationImage
        {
            get { return _showMainChartSynchronizationImage; }
            set { _showMainChartSynchronizationImage = value; }
        }

        /// <summary>
        /// Image used when a pane is slave pane, to signify if pane is precisely aligned with master or not.
        /// </summary>
        Image _masterSynchronizationImage;
        bool _synchronizedWithMainChart = false;

        MainChartPaneSyncModeEnum _masterPaneSynchronizationMode = MainChartPaneSyncModeEnum.XAxis;
        public MainChartPaneSyncModeEnum MainChartPaneSynchronizationMode
        {
            get { return _masterPaneSynchronizationMode; }
            set { _masterPaneSynchronizationMode = value; }
        }


        MainChartPane mainChartPane;
        /// <summary>
        /// If this pane needs to synchronize its zoom, pan, 
        /// etc. with another pane, assign master pane here.
        /// </summary>
        public MainChartPane MainChartPane
        {
            get { return mainChartPane; }
            set
            {
                if (mainChartPane != null)
                {
                    mainChartPane.DrawingSpaceViewTransformationChangedEvent -= new DrawingSpaceViewTransformationChangedDelegate(mainChartPaneDrawingSpaceViewTransformationChangedEvent);
                    mainChartPane.AppearanceSchemeChangedEvent -= new AppearanceSchemeChangedDelegate(mainChartPaneAppearanceSchemeChangedEvent);
                    mainChartPane.CrossHairShowEvent -= new MainChartPane.CrossHairShowDelegate(mainChartPaneCrossHairShowEvent);
                }

                // SystemMonitor.CheckThrow(_masterPaneSynchronizationMode == MainChartPaneSynchronizationModeEnum.XAxis, "Mode not supported.");

                mainChartPane = value;
                if (mainChartPane != null)
                {
                    //_limitedView = false;
                    mainChartPane.CrossHairShowEvent += new MainChartPane.CrossHairShowDelegate(mainChartPaneCrossHairShowEvent);
                    mainChartPane.DrawingSpaceViewTransformationChangedEvent += new DrawingSpaceViewTransformationChangedDelegate(mainChartPaneDrawingSpaceViewTransformationChangedEvent);
                    mainChartPane.AppearanceSchemeChangedEvent += new AppearanceSchemeChangedDelegate(mainChartPaneAppearanceSchemeChangedEvent);
                    SynchronizeWithMainChartPane();
                }
            }
        }

        Point? _lastCrossHairPosition;

        /// <summary>
        /// 
        /// </summary>
        public SubChartPane()
        {
            this.DrawingSpaceViewTransformationChangedEvent += new DrawingSpaceViewTransformationChangedDelegate(SlaveChartPane_DrawingSpaceViewTransformationChangedEvent);
        }

        protected override void Draw(Graphics g)
        {
            base.Draw(g);
            _lastCrossHairPosition = null;
        }

        //protected override void UpdateActualDrawingSpaceArea()
        //{
        //    if ( ActualDrawingSpaceMarginLeftUpdateEvent != null)
        //    {
        //        _actuablDrawingSpaceAreaMarginLeft = ActualDrawingSpaceMarginLeftUpdateEvent(this, _actualDrawingSpaceAreaMarginLeft);
        //    }

        //    base.UpdateActualDrawingSpaceArea();
        //}

        void mainChartPaneCrossHairShowEvent(MainChartPane pane, Point location)
        {
            // Draw parent cross continuation.
            if (ActualDrawingSpaceArea.X <= location.X
                && location.X <= (ActualDrawingSpaceArea.X + ActualDrawingSpaceArea.Width))
            {
                Point pointUp, pointDown;
                if (_lastCrossHairPosition.HasValue)
                {// Erase previous line
                    pointUp = new Point(_lastCrossHairPosition.Value.X, ActualDrawingSpaceArea.Y);
                    pointDown = new Point(_lastCrossHairPosition.Value.X, ActualDrawingSpaceArea.Y + ActualDrawingSpaceArea.Height);

                    ControlPaint.DrawReversibleLine(this.PointToScreen(pointUp), this.PointToScreen(pointDown), this.BackColor);
                }

                _lastCrossHairPosition = location;

                pointUp = new Point(_lastCrossHairPosition.Value.X, ActualDrawingSpaceArea.Y);
                pointDown = new Point(_lastCrossHairPosition.Value.X, ActualDrawingSpaceArea.Y + ActualDrawingSpaceArea.Height);

                // New line.
                ControlPaint.DrawReversibleLine(this.PointToScreen(pointUp), this.PointToScreen(pointDown), this.BackColor);
            }

        }

        void mainChartPaneAppearanceSchemeChangedEvent(ChartPane pane, AppearanceSchemeEnum scheme)
        {
            this.SetAppearanceScheme(scheme);
            this.Refresh();
        }

        void SlaveChartPane_DrawingSpaceViewTransformationChangedEvent(ChartPane pane, Matrix previousTransformation, Matrix currentTransformation)
        {
            UpdateMainChartSynchronizationState(false);
        }

        protected override void DrawInitialActualSpaceOverlays(GraphicsWrapper g, ChartSeries dateAssignedSeries)
        {
            base.DrawInitialActualSpaceOverlays(g, dateAssignedSeries);

            if (_showMainChartSynchronizationImage && _masterSynchronizationImage != null)
            {
                g.DrawImageUnscaledAndClipped(_masterSynchronizationImage, new Rectangle(4, (int)LabelsTopMargin, _masterSynchronizationImage.Width, _masterSynchronizationImage.Height));
            }
        }


        void mainChartPaneDrawingSpaceViewTransformationChangedEvent(ChartPane pane, Matrix previousTransformation, Matrix currentTransformation)
        {
            SynchronizeWithMainChartPane();
        }

        public void SynchronizeWithMainChartPane()
        {
            if (mainChartPane == null)
            {
                return;
            }

            // SystemMonitor.CheckThrow(_masterPaneSynchronizationMode == MainChartPaneSynchronizationModeEnum.XAxis, "Mode not supported.");
            GraphicsWrapper.SynchronizeDrawingSpaceXAxis(mainChartPane.GraphicsWrapper);

            RectangleF screen = GraphicsWrapper.ActualSpaceToDrawingSpace(ActualDrawingSpaceArea);

            if (screen.X < 0)
            {
                screen.X = 0;
            }

            FitHorizontalAreaToScreen(screen);

            UpdateMainChartSynchronizationState(true);

            this.Refresh();
            //this.Invalidate();
        }

        void UpdateMainChartSynchronizationState(bool isSynchronized)
        {
            _synchronizedWithMainChart = isSynchronized;

            if (isSynchronized)
            {
                // Set the synchronization image to green.
                ComponentResourceManager resources = new ComponentResourceManager(typeof(SubChartPane));
                _masterSynchronizationImage = ((Image)(resources.GetObject("imageComponentGreen")));
            }
            else
            {
                // Set the synchronization image to green.
                ComponentResourceManager resources = new ComponentResourceManager(typeof(SubChartPane));
                _masterSynchronizationImage = ((Image)(resources.GetObject("imageComponentRed")));
            }
        }

        public override void Add(ChartSeries series, bool usePaneColorSelector, bool replaceSeriesWithSameName)
        {
            base.Add(series, usePaneColorSelector, replaceSeriesWithSameName);
            SynchronizeWithMainChartPane();
        }

        public override void series_SeriesUpdatedEvent(ChartSeries series, bool updateUI)
        {
            if (updateUI)
            {
                SynchronizeWithMainChartPane();
            }
            base.series_SeriesUpdatedEvent(series, updateUI);
        }

    }
}
