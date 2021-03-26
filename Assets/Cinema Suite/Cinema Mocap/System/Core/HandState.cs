using System;

namespace CinemaSuite.CinemaMocap.System.Core
{
    [Serializable]
    public enum HandState
    {
        Unknown,
        NotTracked,
        Open,
        Closed,
        Lasso
    }
}
