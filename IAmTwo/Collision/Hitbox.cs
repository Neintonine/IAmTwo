using System;
using IAmTwo.Game;
using OpenTK;

namespace KWEngine.Hitbox
{
    /// <summary>
    /// Collision detection helper class that uses the SAT (separate axes theorem) algorithm to check for collisions of convex shapes.
    /// Inspired by: http://www.dyn4j.org/2010/01/sat/
    /// </summary>
    public class Hitbox
    {
        /// <summary>
        /// This holds the non-modified normals (orientation vectors) for the Hitbox. May not be modified!
        /// </summary>
        private Vector2[] normalsSource = new Vector2[] { new Vector2(1, 0), new Vector2(0, 1) };
        /// <summary>
        /// This holds the current normals for the Hitbox. Changes with every call to Update(...).
        /// </summary>
        private Vector2[] normalsCurrent = new Vector2[] { new Vector2(1, 0), new Vector2(0, 1) };
        /// <summary>
        /// Temporary Vector instance for storing the MTV (minimum-translation-vector). The MTV indicates by how much the object would have to move in order to NOT collide anymore.
        /// </summary>
        private static Vector2 MTVTemp = new Vector2(0, 0);

        /// <summary>
        /// Holds the current information about the Hitbox's center point.
        /// </summary>
        private Vector2 vertexCenter = new Vector2(0, 0);

        /// <summary>
        /// Holds the non-modified (no scale, no rotation, no translation) coordinates of the Hitbox. 
        /// </summary>
        private Vector2[] verticesSource = new Vector2[] {
            new Vector2(-0.5f, 0.5f),
            new Vector2(+0.5f, 0.5f),
            new Vector2(+0.5f, -0.5f),
            new Vector2(-0.5f, -0.5f)
        };

        /// <summary>
        /// Holds the current information about the Hitbox's edges (vertices) after the Hitbox has been scaled, rotated and translated.
        /// </summary>
        private Vector2[] verticesCurrent = new Vector2[] {
            new Vector2(-0.5f, 0.5f),
            new Vector2(+0.5f, 0.5f),
            new Vector2(+0.5f, -0.5f),
            new Vector2(-0.5f, -0.5f)
        };

        /// <summary>
        /// Stores a reference to the Hitbox's owner object.
        /// </summary>
        public PhysicsObject PhysicsObject;

        /// <summary>
        /// Generates a new Hitbox instance
        /// </summary>
        /// <param name="parentObject">The Hitbox instance needs a parent object in order to later pass this information to the Intersection object.</param>
        public Hitbox(PhysicsObject parentObject)
        {
            if (parentObject == null)
                throw new Exception("Parent object needs to be a valid instance of a class.");
            PhysicsObject = parentObject;
        }

        /// <summary>
        /// Updates the Hitbox's normal vectors, corner points (vertices) according to the information passed in the parameters.
        /// </summary>
        /// <param name="centerX">horizontal center of the object</param>
        /// <param name="centerY">vertical center of the object</param>
        /// <param name="width">width (scale on x axis)</param>
        /// <param name="height">height (scale on y axis)</param>
        /// <param name="orientationDegrees">current orientation (will be converted to radians)</param>
        public void Update(Matrix4 matrix, double orientationDegrees, bool noMatrixRotation = false)
        {
            if (noMatrixRotation)
            {
                matrix = matrix.ClearRotation();
                orientationDegrees = 0;
            }

            // Rotate the Hitbox's normal vectors according to the current orientation:
            // TODO: Check if normals are normalized (i.e. their length must be 1 after rotation)...
            normalsCurrent[0] = RotateVector(normalsSource[0], orientationDegrees * Math.PI / 180);
            normalsCurrent[1] = RotateVector(normalsSource[1], orientationDegrees * Math.PI / 180);

            // Translate the corner points (vertices) for this Hitbox:
            for (int i = 0; i < verticesSource.Length; i++)
            {
                verticesCurrent[i] = (new Vector4(verticesSource[i].X, verticesSource[i].Y, 0, 1) *  matrix).Xy;
            }
            // Translate its center as well:
            vertexCenter = (new Vector4(0,0,0,1) * matrix).Xy;
        }

        /// <summary>
        /// Rotates a vector instance by the given degrees (in radians)
        /// </summary>
        /// <param name="vector">vector instance to be rotated</param>
        /// <param name="radians">orientation in radians</param>
        /// <returns></returns>
        private static Vector2 RotateVector(Vector2 vector, double radians)
        {
            return new Vector2((float)(vector.X * Math.Cos(radians) - vector.Y * Math.Sin(radians)),
                (float)(vector.X * Math.Sin(radians) + vector.Y * Math.Cos(radians)));
        }


        /// <summary>
        /// Main collision test method. It takes two hitbox instances as parameters. The first parameter is the caller's hitbox (the one who wants to know!). The second is the hitbox of the other object.
        /// </summary>
        /// <param name="caller">Caller's Hitbox</param>
        /// <param name="collider">Other Hitbox</param>
        /// <returns>Intersection instance with minimum-translation-vector (MTV) or NULL if the two Hitboxes do not collide</returns>
        public static bool TestIntersection(Hitbox caller, Hitbox collider, out OpenTK.Vector2 mtv)
        {
            float mtvDistance = float.MaxValue;
            float mtvDirection = 1;
            MTVTemp.X = 0; MTVTemp.Y = 0;
            mtv = new OpenTK.Vector2(MTVTemp.X, MTVTemp.Y);

            for (int i = 0; i < caller.normalsCurrent.Length; i++)
            {
                // Project both Hitboxes' vertices along the normals of the caller Hitbox:
                bool error = false;
                float shape1Min, shape1Max, shape2Min, shape2Max;
                SatTest(ref caller.normalsCurrent[i], ref caller.verticesCurrent, out shape1Min, out shape1Max);
                SatTest(ref caller.normalsCurrent[i], ref collider.verticesCurrent, out shape2Min, out shape2Max);
                if (!Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                {
                    return false;
                }
                else
                {
                    CalculateOverlap(ref caller.normalsCurrent[i], ref shape1Min, ref shape1Max, ref shape2Min, ref shape2Max,
                        out error, ref mtvDistance, ref MTVTemp, ref mtvDirection, ref caller.vertexCenter, ref collider.vertexCenter);
                }

                // Project both Hitboxes' vertices along the normals of the other Hitbox:
                SatTest(ref collider.normalsCurrent[i], ref caller.verticesCurrent, out shape1Min, out shape1Max);
                SatTest(ref collider.normalsCurrent[i], ref collider.verticesCurrent, out shape2Min, out shape2Max);
                if (!Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                {
                    return false;
                }
                else
                {
                    CalculateOverlap(ref collider.normalsCurrent[i], ref shape1Min, ref shape1Max, ref shape2Min, ref shape2Max,
                        out error, ref mtvDistance, ref MTVTemp, ref mtvDirection, ref caller.vertexCenter, ref collider.vertexCenter);
                }

            }

            if (MTVTemp == Vector2.Zero)
                return false;

            mtv = new OpenTK.Vector2(MTVTemp.X, MTVTemp.Y);

            return true;
        }

        /// <summary>
        /// Calculates the amount of projection overlap.
        /// </summary>
        /// <param name="axis">Current axis</param>
        /// <param name="shape1Min">leftmost projection of shape 1</param>
        /// <param name="shape1Max">rightmost projection of shape 1</param>
        /// <param name="shape2Min">lestmost projection of shape 2</param>
        /// <param name="shape2Max">rightmost projection of shape 2</param>
        /// <param name="error">turns true when there was a calculation error</param>
        /// <param name="mtvDistance">the MTV vector's length</param>
        /// <param name="mtv">the MTV</param>
        /// <param name="mtvDirection">the MTV's direction</param>
        /// <param name="posA">center position of Hitbox #1</param>
        /// <param name="posB">center position of Hitbox #2</param>
        private static void CalculateOverlap(ref Vector2 axis, ref float shape1Min, ref float shape1Max, ref float shape2Min, ref float shape2Max,
            out bool error, ref float mtvDistance, ref Vector2 mtv, ref float mtvDirection, ref Vector2 posA, ref Vector2 posB)
        {
            float intersectionDepthScaled = (shape1Min < shape2Min) ? (shape1Max - shape2Min) : (shape1Min - shape2Max);

            float axisLengthSquared = Vector2.Dot(axis, axis);
            if (axisLengthSquared < 1.0e-8f)
            {
                error = true;
                return;
            }
            error = false;
            float intersectionDepthSquared = (intersectionDepthScaled * intersectionDepthScaled) / axisLengthSquared;


            if (intersectionDepthSquared < mtvDistance || mtvDistance < 0)
            {
                mtvDistance = intersectionDepthSquared;
                mtv = axis * (intersectionDepthScaled / axisLengthSquared);
                float notSameDirection = Vector2.Dot(posA - posB, mtv);
                mtvDirection = notSameDirection < 0 ? -1.0f : 1.0f;
                mtv = mtv * mtvDirection;
            }

        }

        /// <summary>
        /// Checks if the projection limits of shape #1 and #2 overlap.
        /// </summary>
        /// <param name="min1">shape #1 left</param>
        /// <param name="max1">shape #1 right</param>
        /// <param name="min2">shape #2 left</param>
        /// <param name="max2">shape #2 right</param>
        /// <returns></returns>
        private static bool Overlaps(float min1, float max1, float min2, float max2)
        {
            return IsBetweenOrdered(min2, min1, max1) || IsBetweenOrdered(min1, min2, max2);
        }

        /// <summary>
        /// Checks if a value is inside the given bounds.
        /// </summary>
        /// <param name="val">value to be checked</param>
        /// <param name="lowerBound">lower bound</param>
        /// <param name="upperBound">upper bound</param>
        /// <returns>true if the value is inside the given bounds</returns>
        private static bool IsBetweenOrdered(float val, float lowerBound, float upperBound)
        {
            return lowerBound <= val && val <= upperBound;
        }

        /// <summary>
        /// Calculates the shape's shadow projection (leftmost/rightmost) along the given axis.
        /// </summary>
        /// <param name="axis">Projection axis</param>
        /// <param name="ptSet">Vertices that need to be projected</param>
        /// <param name="minAlong">leftmost projection edge</param>
        /// <param name="maxAlong">rightmost projection edge</param>
        private static void SatTest(ref Vector2 axis, ref Vector2[] ptSet, out float minAlong, out float maxAlong)
        {
            minAlong = float.MaxValue;
            maxAlong = float.MinValue;
            for (int i = 0; i < ptSet.Length; i++)
            {
                float dotVal = Vector2.Dot(ptSet[i], axis);
                if (dotVal < minAlong) minAlong = dotVal;
                if (dotVal > maxAlong) maxAlong = dotVal;
            }
        }
    }
} 