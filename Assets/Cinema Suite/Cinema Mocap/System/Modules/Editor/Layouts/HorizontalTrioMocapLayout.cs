using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.Layouts
{
    [CinemaMocapLayoutAttribute("Trio Horizontal",4)]
    public class HorizontalTrioMocapLayout : CinemaMocapLayout
    {
        public HorizontalTrioMocapLayout()
        {
            base.viewerCount = 3;
        }

        public override List<UnityEngine.Rect> GetViewerRects(UnityEngine.Rect area)
        {
            List<Rect> viewerSpaces = new List<Rect>();

            float tempWidth = AspectRatio / 2f;
            Rect area1 = new Rect(0, 0, (area.width * tempWidth) - GAP, area.height);
            Rect area2 = new Rect((area.width * tempWidth) + GAP, 0, (area.width * (1 - tempWidth)), (area.height / 2) - GAP);
            Rect area3 = new Rect((area.width * tempWidth) + GAP, (area.height / 2) + GAP, (area.width * (1 - tempWidth)), (area.height / 2) - GAP);

            viewerSpaces.Add(area1);
            viewerSpaces.Add(area2);
            viewerSpaces.Add(area3);

            return viewerSpaces;
        }
    }
}
