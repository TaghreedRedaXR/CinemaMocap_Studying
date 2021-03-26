
//using UnityEngine;
//using UnityEditor;

//namespace CinemaSuite.CinemaMocap.System.Core.Editor
//{
//    public class CinemaMocapProductInfo : CinemaSuite.CinemaMocapBaseProductInfo
//    {
//        public CinemaMocapProductInfo()
//        {
//            name = "Cinema Mocap";
//            version = "2.0.0.6";
//            installed = true;

//            headerText = "<size=16>Cinema Mocap</size>";
//            header2Text = string.Format("<size=14><b>v{0}</b> detected.</size>", version);
//            bodyText = "Thank you for purchasing Cinema Mocap! We hope that you enjoy using the product and that it helps make your game dev project a success!\n\nIf you have a chance, please leave us a review on the Asset Store.";

//            string suffix = EditorGUIUtility.isProSkin ? "_Pro" : "_Personal";
//            resourceImage1 = Resources.Load("Cinema_Suite_Docs" + suffix) as Texture2D;
//            resourceImage2 = Resources.Load("Cinema_Suite_Forums" + suffix) as Texture2D;
//            resourceImage3 = Resources.Load("Cinema_Suite_Tips" + suffix) as Texture2D;
//            resourceImage4 = Resources.Load("Cinema_Suite_Video" + suffix) as Texture2D;

//            resourceImage1Link = "http://www.cinema-suite.com/Documentation/CinemaMoCap/Current/CinemaMoCapDocumentation.pdf";
//            resourceImage2Link = "http://cinema-suite.com/forum/viewforum.php?f=9";
//            resourceImage3Link = "https://www.youtube.com/watch?v=8NM4dvHT8ik";
//            resourceImage4Link = "https://www.youtube.com/playlist?list=PLkTFhf2jQXOnIFbvoysVpW1nOE69X0N-b";

//            resourceImage1Label = "Docs";
//            resourceImage2Label = "Forum";
//            resourceImage3Label = "FAQ";
//            resourceImage4Label = "Tutorial";

//            assetStorePage = "http://u3d.as/5PB";
//        }
//    }
//}