using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.Layouts
{
    [CinemaMocapLayoutAttribute("Duo Vertical", 1)]
    public class VerticalDuoMocapLayout : CinemaMocapLayout
    {
        public VerticalDuoMocapLayout()
        {
            base.viewerCount = 2;
        }

        /// <summary>
        /// Get two viewer rects, splitting it in half vertically.
        /// </summary>
        /// <param name="area">The Area to be split up.</param>
        /// <returns>Two Rects, defining individual areas.</returns>
        public override List<UnityEngine.Rect> GetViewerRects(UnityEngine.Rect area)
        {
            List<Rect> viewerSpaces = new List<Rect>();

            Rect area1 = new Rect(0, 0, area.width, (area.height / 2) - GAP);
            Rect area2 = new Rect(0, (area.height / 2) + GAP, area.width, (area.height / 2) - GAP);

            viewerSpaces.Add(area1);
            viewerSpaces.Add(area2);

            return viewerSpaces;
        }
    }
}
