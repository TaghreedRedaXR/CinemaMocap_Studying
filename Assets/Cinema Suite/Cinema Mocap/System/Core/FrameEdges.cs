using System;

namespace CinemaSuite.CinemaMocap.System.Core
{
    [Serializable]
    public enum FrameEdges
    {
        None = 0,
        Right = 1,
        Left = 2,
        Top = 4,
        Bottom = 8,
    }
}
