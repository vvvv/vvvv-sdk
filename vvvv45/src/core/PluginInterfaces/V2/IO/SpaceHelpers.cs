using System.Drawing;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2.IO
{
    public static class SpaceHelpers
    {
        public static Point DoMapPositionInNormalizedProjectionToPixels(this Vector2D normV, Size clientSize)
        {
            var clientArea = new Vector2D(clientSize.Width - 1, clientSize.Height - 1);
            var v = VMath.Map(normV, new Vector2D(-1, 1), new Vector2D(1, -1), Vector2D.Zero, clientArea, TMapMode.Float);
            return new Point((int)v.x, (int)v.y);
        }

        internal static void DoMapFromPixels(Point inPixels, Size clientArea, out Vector2D inNormalizedProjection, out Vector2D inProjection)
        {
            inNormalizedProjection = new Vector2D(
                -1 + 2 * ((float)inPixels.X / (clientArea.Width - 1)),
                 1 - 2 * ((float)inPixels.Y / (clientArea.Height - 1)));

            //let's not overdo it. let's return the normalized position as projeciton position in case interface is not implemented
            const bool autoAspect = false;

            if (autoAspect)
            {
                if (clientArea.Width < clientArea.Height)
                    inProjection = new Vector2D(inNormalizedProjection.x, inNormalizedProjection.y * clientArea.Height / clientArea.Width);
                else
                    inProjection = new Vector2D(inNormalizedProjection.x * clientArea.Width / clientArea.Height, inNormalizedProjection.y);
            }
            else
                inProjection = inNormalizedProjection;
        }

        public static void MapFromPixels(Point inPixels, object sender, Size clientArea, out Vector2D inNormalizedProjection, out Vector2D inProjection)
        {
            if (sender is IProjectionSpace)
            {
                (sender as IProjectionSpace).MapFromPixels(inPixels, out inNormalizedProjection, out inProjection);
                return;
            }

            if (sender is IProjectionSpace2)
            {
                double xInNormalizedProjection, yInNormalizedProjection, xInProjection, yInProjection;

                (sender as IProjectionSpace2).MapFromPixels(inPixels.X, inPixels.Y,
                    out xInNormalizedProjection, out yInNormalizedProjection, out xInProjection, out yInProjection);

                inNormalizedProjection = new Vector2D(xInNormalizedProjection, yInNormalizedProjection);
                inProjection = new Vector2D(xInProjection, yInProjection);
                return;
            }

            DoMapFromPixels(inPixels, clientArea, out inNormalizedProjection, out inProjection);
        }
    }
}
