using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.Layouts
{
    [CinemaMocapLayoutAttribute("Trio Vertical", 3)]
    public class VerticalTrioMocapLayout : CinemaMocapLayout
    {
        public VerticalTrioMocapLayout()
        {
            base.viewerCount = 3;
        }

        public override List<UnityEngine.Rect> GetViewerRects(UnityEngine.Rect area)
        {
            List<Rect> viewerSpaces = new List<Rect>();

            float tempHeight = AspectRatio / 2f;
            Rect area1 = new Rect(0, 0, area.width, (area.height * tempHeight) - GAP);
            Rect area2 = new Rect(0, (area.height * tempHeight) + GAP, (area.width / 2) - GAP, (area.height * (1-tempHeight)) - GAP);
            Rect area3 = new Rect((area.width / 2) + GAP, (area.height * tempHeight) + GAP, (area.width / 2) - GAP, (area.height * (1 - tempHeight)) - GAP);

            viewerSpaces.Add(area1);
            viewerSpaces.Add(area2);
            viewerSpaces.Add(area3);

            return viewerSpaces;
        }
    }
}
