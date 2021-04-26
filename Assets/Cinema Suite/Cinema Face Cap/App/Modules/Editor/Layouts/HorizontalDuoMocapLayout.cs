
using CinemaSuite.CinemaFaceCap.App.Core.Editor.UI;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.Layouts
{
    [CinemaFaceCapLayoutAttribute("Duo Horizontal",2)]
    public class HorizontalDuoMocapLayout : CinemaFaceCapLayout
    {
        public HorizontalDuoMocapLayout()
        {
            base.viewerCount = 2;
        }

        public override List<UnityEngine.Rect> GetViewerRects(UnityEngine.Rect area)
        {
            List<Rect> viewerSpaces = new List<Rect>();

            Rect area1 = new Rect(0, 0, (area.width / 2) - GAP, area.height);
            Rect area2 = new Rect((area.width / 2) + GAP, 0, (area.width / 2), area.height);

            viewerSpaces.Add(area1);
            viewerSpaces.Add(area2);

            return viewerSpaces;
        }
    }
}
