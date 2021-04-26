
using CinemaSuite.CinemaFaceCap.App.Core.Editor.UI;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.Layouts
{
    // Uncomment when there are 4+ available controls.
    //[CinemaFaceCapLayoutAttribute("Quad",5)]
    public class QuadMocapLayout : CinemaFaceCapLayout
    {
        public QuadMocapLayout()
        {
            base.viewerCount = 4;
        }

        public override List<UnityEngine.Rect> GetViewerRects(UnityEngine.Rect area)
        {
            List<Rect> viewerSpaces = new List<Rect>();

            Rect area1 = new Rect(0, 0, (area.width / 2) - GAP, (area.height / 2) - GAP);
            Rect area2 = new Rect((area.width / 2) + GAP, 0, (area.width / 2) - GAP, (area.height / 2) - GAP);
            Rect area3 = new Rect(0, (area.height / 2) + GAP, (area.width / 2) - GAP, (area.height / 2) - GAP);
            Rect area4 = new Rect((area.width / 2) + GAP, (area.height / 2) + GAP, (area.width / 2) - GAP, (area.height / 2) - GAP);

            viewerSpaces.Add(area1);
            viewerSpaces.Add(area2);
            viewerSpaces.Add(area3);
            viewerSpaces.Add(area4);

            return viewerSpaces;
        }
    }
}
