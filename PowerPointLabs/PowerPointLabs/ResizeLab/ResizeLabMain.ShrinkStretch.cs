﻿using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using PowerPointLabs.Utils;


namespace PowerPointLabs.ResizeLab
{
    /// <summary>
    /// Handles the stretching and shrinking of shapes in resize lab
    /// </summary>
    public partial class ResizeLabMain
    {
        private const int ModShapesIndex = 2;

        public enum RefType
        {
            FirstSelected = 1,
            Outermost = 2
        }

        private enum StretchType
        {
            Left, Right, Top, Bottom
        }

        /// <summary>
        /// Stretches a given shape to match an edge of the reference shape.
        /// </summary>
        /// <param name="referenceEdge">The edge to refer to</param>
        /// <param name="stretchShape">The shape to stretch</param>
        /// <returns>True if shape is stretched successfully, false otherwise</returns>
        private delegate void StretchAction(float referenceEdge, PPShape stretchShape);
        
        /// <summary>
        /// Checks whether a shape can be stretched in a particular direction towards reference shape
        /// </summary>
        /// <param name="referenceEdge">The edge to refer to. This may be modified to match the apporiate stretch action</param>
        /// <param name="checkShape">The shape to check</param>
        /// <returns>The appropriate stretch action to use</returns>
        private delegate StretchAction GetAppropriateStretchAction(float referenceEdge, PPShape checkShape);

        /// <summary>
        /// Returns the default reference edge for given shape
        /// </summary>
        /// <param name="referenceShape">The shape to get the reference edge from</param>
        /// <returns>The point determining the reference edge</returns>
        private delegate float GetDefaultReferenceEdge(PPShape referenceShape);
        #region API

        /// <summary>
        /// Stretches all selected shapes to the left edge of reference shape
        /// </summary>
        /// <param name="stretchShapes">The shapes to stretch</param>
        public void StretchLeft(PowerPoint.ShapeRange stretchShapes)
        {
            var appropriateStretch = new GetAppropriateStretchAction((referenceEdge, checkShape) =>
            {
                // Opposite stretch
                if (GetRight(checkShape) < referenceEdge)
                {
                    return StretchRightAction;
                }
                return StretchLeftAction;
            });
            var defaultReferenceEdge = new GetDefaultReferenceEdge(referenceShape => referenceShape.Left);
            Stretch(stretchShapes, appropriateStretch, defaultReferenceEdge, StretchType.Left);
        }

        /// <summary>
        /// Stretches all selected shapes to the right edge of reference shape
        /// </summary>
        /// <param name="stretchShapes">The shapes to stretch</param>
        public void StretchRight(PowerPoint.ShapeRange stretchShapes)
        {
            var appropriateStretch = new GetAppropriateStretchAction((referenceEdge, checkShape) =>
            {
                // Opposite stretch
                if (checkShape.Left > referenceEdge)
                {
                    return StretchLeftAction;
                }
                return StretchRightAction;
            });
            var defaultReferenceEdge = new GetDefaultReferenceEdge(referenceShape => referenceShape.Left + referenceShape.AbsoluteWidth);
            Stretch(stretchShapes, appropriateStretch, defaultReferenceEdge, StretchType.Right);
        }

        /// <summary>
        /// Stretches all selected shapes to the top edge of reference shape
        /// </summary>
        /// <param name="stretchShapes">The shapes to stretch</param>
        public void StretchTop(PowerPoint.ShapeRange stretchShapes)
        {
            var appropriateStretch = new GetAppropriateStretchAction((referenceEdge, checkShape) =>
            {
                // Opposite stretch
                if (GetBottom(checkShape) < referenceEdge)
                {
                    return StretchBottomAction;
                }
                return StretchTopAction;
            });
            var defaultReferenceEdge = new GetDefaultReferenceEdge(referenceShape => referenceShape.Top);
            Stretch(stretchShapes, appropriateStretch, defaultReferenceEdge, StretchType.Top);
        }

        /// <summary>
        /// Stretches all selected shapes to the left edge of reference shape
        /// </summary>
        /// <param name="stretchShapes">The shapes to stretch</param>
        public void StretchBottom(PowerPoint.ShapeRange stretchShapes)
        {
            var appropriateStretch = new GetAppropriateStretchAction((referenceEdge, checkShape) =>
            {
                // Opposite stretch
                if (checkShape.Top > referenceEdge)
                {
                    return StretchTopAction;
                }
                return StretchBottomAction;
            });
            var defaultReferenceEdge = new GetDefaultReferenceEdge(referenceShape => referenceShape.Top + referenceShape.AbsoluteHeight);
            Stretch(stretchShapes, appropriateStretch, defaultReferenceEdge, StretchType.Bottom);
        }

        private static void StretchLeftAction(float referenceEdge, PPShape stretchShape)
        {
            stretchShape.AbsoluteWidth += stretchShape.Left - referenceEdge;
            stretchShape.Left = referenceEdge;
        }

        private static void StretchRightAction(float referenceEdge, PPShape stretchShape)
        {
            stretchShape.AbsoluteWidth += referenceEdge - GetRight(stretchShape);
        }

        private static void StretchTopAction(float referenceEdge, PPShape stretchShape)
        {
            stretchShape.AbsoluteHeight += stretchShape.Top - referenceEdge;
            stretchShape.Top = referenceEdge;
        }

        private static void StretchBottomAction(float referenceEdge, PPShape stretchShape)
        {
            stretchShape.AbsoluteHeight += referenceEdge - GetBottom(stretchShape);
        }

        /// <summary>
        /// Stretch shapes in the list
        /// </summary>
        /// <param name="stretchShapes">The shapes to stretch</param>
        /// <param name="stretchAction">The function to use to stretch</param>
        /// <param name="defaultReferenceEdge">The function to return the default reference edge</param>
        /// <param name="stretchType">The type of stretch to perform</param>
        private void Stretch(PowerPoint.ShapeRange stretchShapes, GetAppropriateStretchAction stretchAction, 
            GetDefaultReferenceEdge defaultReferenceEdge, StretchType stretchType)
        {
            if (!ValidateSelection(stretchShapes))
            {
                return;
            }

            var referenceShape = GetReferenceShape(stretchShapes, defaultReferenceEdge, stretchType);
            var referenceEdge = defaultReferenceEdge(new PPShape(referenceShape));

            for (var i = 1; i <= stretchShapes.Count; i++)
            {
                if (referenceShape.Equals(stretchShapes[i]))
                {
                    continue;
                }
                var stretchShape = new PPShape(stretchShapes[i]);
                var sa = stretchAction(referenceEdge, stretchShape);
                sa(referenceEdge, stretchShape);
            }
        }

        #endregion

        #region Helper Functions

        private bool ValidateSelection(PowerPoint.ShapeRange shapes)
        {
            return IsMoreThanOneShape(shapes);
        }

        private PowerPoint.Shape GetReferenceShape(PowerPoint.ShapeRange shapes, GetDefaultReferenceEdge getReferenceEdge,
            StretchType stretchType)
        {
            var refShapeIndex = 1;

            if (ReferenceType == RefType.Outermost)
            {
                var refPpShape = new PPShape(shapes[1]);
                for (var i = 2; i <= shapes.Count; i++)
                {
                    var tempPpShape = new PPShape(shapes[i]);
                    if (((stretchType == StretchType.Left || stretchType == StretchType.Top) &&
                        getReferenceEdge(refPpShape) > getReferenceEdge(tempPpShape)) ||
                        ((stretchType == StretchType.Right || stretchType == StretchType.Bottom) &&
                        getReferenceEdge(refPpShape) < getReferenceEdge(tempPpShape)))
                    {
                        refPpShape = tempPpShape;
                        refShapeIndex = i;
                    }
                }
            }

            return shapes[refShapeIndex];

        }

        private static float GetRight(PPShape aShape)
        {
            return aShape.Left + aShape.AbsoluteWidth;
        }

        private static float GetBottom(PPShape aShape)
        {
            return aShape.Top + aShape.AbsoluteHeight;
        }

        #endregion

        #region Settings

        public RefType ReferenceType { get; set; }

        #endregion

    }
}
