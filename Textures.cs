
// The following Class is derived from Kerbal Alarm Clock mod. Which is licensed under:
// The MIT License(MIT) Copyright(c) 2014, David Tregoning
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using UnityEngine;
using RSTUtils;

namespace AY
{
    internal static class Textures
    {
        //Icons
        internal static Texture2D IconGreenOff = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D IconGreenOn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D IconRedOff = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D IconRedOn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D IconYellowOff = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D IconYellowOn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D IconGrayOff = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D IconGrayOn = new Texture2D(38, 38, TextureFormat.ARGB32, false);

        //Toolbar Icons
        internal static Texture2D ToolbariconGreenOff = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D ToolbariconGreenOn = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D ToolbariconRedOff = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D ToolbariconRedOn = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D ToolbariconYellowOff = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D ToolbariconYellowOn = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D ToolbariconGrayOff = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D ToolbariconGrayOn = new Texture2D(24, 24, TextureFormat.ARGB32, false);

        //Button Icons
        internal static Texture2D BtnPriority1 = new Texture2D(18, 18, TextureFormat.ARGB32, false);
        internal static Texture2D BtnPriority2 = new Texture2D(18, 18, TextureFormat.ARGB32, false);
        internal static Texture2D BtnPriority3 = new Texture2D(18, 18, TextureFormat.ARGB32, false);
        internal static Texture2D BtnIncInCalcs = new Texture2D(18, 18, TextureFormat.ARGB32, false);
        internal static Texture2D BtnEspInc = new Texture2D(18, 18, TextureFormat.ARGB32, false);
        internal static Texture2D TooltipBox = new Texture2D(10, 10, TextureFormat.ARGB32, false);
        internal static Texture2D BtnRedCross = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        internal static Texture2D BtnResize = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        internal static Texture2D BtnResizeHeight = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        internal static Texture2D BtnResizeWidth = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        internal static Texture2D BtnIS = new Texture2D(16, 16, TextureFormat.ARGB32, false);

        internal static String PathIconsPath = System.IO.Path.Combine(AmpYear.AssemblyFolder.Substring(0, AmpYear.AssemblyFolder.IndexOf("/AmpYear/") + 9), "Icons").Replace("\\", "/");
        internal static String PathToolbarIconsPath = PathIconsPath.Substring(PathIconsPath.ToLower().IndexOf("/gamedata/") + 10);
        

        internal static void LoadIconAssets()
        {
            try
            {
                LoadImageFromFile(ref IconGreenOff, "AYGreenOff.png", PathIconsPath);  
                LoadImageFromFile(ref IconGreenOn, "AYGreenOn.png", PathIconsPath);
                LoadImageFromFile(ref IconRedOff, "AYRedOff.png", PathIconsPath);
                LoadImageFromFile(ref IconRedOn, "AYRedOn.png", PathIconsPath);
                LoadImageFromFile(ref IconYellowOff, "AYYellowOff.png", PathIconsPath);
                LoadImageFromFile(ref IconYellowOn, "AYYellowOn.png", PathIconsPath);
                LoadImageFromFile(ref IconGrayOff, "AYGrayOff.png", PathIconsPath);
                LoadImageFromFile(ref IconGrayOn, "AYGrayOn.png", PathIconsPath);

                LoadImageFromFile(ref ToolbariconGreenOff, "AYGreenOffTB.png", PathIconsPath);
                LoadImageFromFile(ref ToolbariconGreenOn, "AYGreenOnTB.png", PathIconsPath);
                LoadImageFromFile(ref ToolbariconRedOff, "AYRedOffTB.png", PathIconsPath);
                LoadImageFromFile(ref ToolbariconRedOn, "AYRedOnTB.png", PathIconsPath);
                LoadImageFromFile(ref ToolbariconYellowOff, "AYYellowOffTB.png", PathIconsPath);
                LoadImageFromFile(ref ToolbariconYellowOn, "AYYellowOnTB.png", PathIconsPath);
                LoadImageFromFile(ref ToolbariconGrayOff, "AYGrayOffTB.png", PathIconsPath);
                LoadImageFromFile(ref ToolbariconGrayOn, "AYGrayOnTB.png", PathIconsPath);

                LoadImageFromFile(ref BtnPriority1, "AYbtnPriority1.png", PathIconsPath);
                LoadImageFromFile(ref BtnPriority2, "AYbtnPriority2.png", PathIconsPath);
                LoadImageFromFile(ref BtnPriority3, "AYbtnPriority3.png", PathIconsPath);
                LoadImageFromFile(ref BtnIncInCalcs, "AYbtnIncInCalcs.png", PathIconsPath);
                LoadImageFromFile(ref BtnEspInc, "AYbtnESPInc.png", PathIconsPath);
                LoadImageFromFile(ref TooltipBox, "AYToolTipBox.png", PathIconsPath);
                LoadImageFromFile(ref BtnRedCross, "AYbtnRedCross.png", PathIconsPath);
                LoadImageFromFile(ref BtnResize, "AYbtnResize.png", PathIconsPath);
                LoadImageFromFile(ref BtnResizeHeight, "AYbtnResizeHeight.png", PathIconsPath);
                LoadImageFromFile(ref BtnResizeWidth, "AYbtnResizeWidth.png", PathIconsPath);
                LoadImageFromFile(ref BtnIS, "AYbtnIS.png", PathIconsPath);
            }
            catch (Exception)
            {
                RSTUtils.Utilities.Log("AmpYear Failed to Load Textures - are you missing a file?");
            }
        }

        public static Boolean LoadImageFromFile(ref Texture2D tex, String fileName, String folderPath = "")
        {            
            Boolean blnReturn = false;
            try
            {
                if (folderPath == "") folderPath = PathIconsPath;

                //File Exists check
                if (System.IO.File.Exists(String.Format("{0}/{1}", folderPath, fileName)))
                {
                    try
                    {                        
                        tex.LoadImage(System.IO.File.ReadAllBytes(String.Format("{0}/{1}", folderPath, fileName)));
                        blnReturn = true;
                    }
                    catch (Exception ex)
                    {
                        RSTUtils.Utilities.Log("AmpYear Failed to load the texture:" + folderPath + "(" + fileName + ")");
                        RSTUtils.Utilities.Log(ex.Message);
                    }
                }
                else
                {
                    RSTUtils.Utilities.Log("AmpYear Cannot find texture to load:" + folderPath + "(" + fileName + ")");                    
                }


            }
            catch (Exception ex)
            {
                RSTUtils.Utilities.Log("AmpYear Failed to load (are you missing a file):" + folderPath + "(" + fileName + ")");
                RSTUtils.Utilities.Log(ex.Message);                
            }
            return blnReturn;
        }

        internal static GUIStyle SectionTitleStyle, SectionTitleStyleLeft, SubsystemButtonStyle, SubsystemConsumptionStyle, StatusStyle, WarningStyle,
            AlertStyle, PowerSinkStyle, PartListStyle, PartListpartHeadingStyle, PartListPartStyle, PartListPartRightStyle, PartListPartGrayStyle, PartListPartRightGrayStyle,
            ResizeStyle, StatusStyleLeft, WarningStyleLeft, AlertStyleLeft, PartListbtnStyle, PrioritybtnStyle;

        internal static bool StylesSet = false;

        internal static void SetupStyles()
        {
            GUI.skin = HighLogic.Skin;

            //Init styles

            Utilities._TooltipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                stretchHeight = true,
                wordWrap = true,
                border = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(4, 4, 6, 4),
                alignment = TextAnchor.MiddleCenter
            };
            Utilities._TooltipStyle.normal.background = TooltipBox;
            Utilities._TooltipStyle.normal.textColor = new Color32(207, 207, 207, 255);
            Utilities._TooltipStyle.hover.textColor = Color.blue;

            SectionTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                fontStyle = FontStyle.Bold
            };

            SectionTitleStyleLeft = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true,
                fontStyle = FontStyle.Bold
            };

            SubsystemConsumptionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerRight,
                stretchWidth = true
            };


            PowerSinkStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerLeft,
                stretchWidth = true
            };

            StatusStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                normal = { textColor = Color.white }
            };

            WarningStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.yellow }
            };

            AlertStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.red }
            };

            StatusStyleLeft = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true,
                normal = { textColor = Color.white }
            };

            WarningStyleLeft = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.yellow }
            };

            AlertStyleLeft = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.red }
            };

            SubsystemButtonStyle = new GUIStyle(GUI.skin.toggle)
            {
                margin =
                {
                    top = 0,
                    bottom = 0
                },
                padding =
                {
                    top = 0,
                    bottom = 0
                }
            };

            PartListStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                normal = { textColor = Color.yellow }
            };

            PartListpartHeadingStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true,
                normal = { textColor = Color.yellow }
            };

            PartListPartStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true,
                normal = { textColor = Color.white }
            };

            PartListPartRightStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
                stretchWidth = true,
                normal = { textColor = Color.white }
            };

            PartListPartGrayStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true,
                normal = { textColor = Color.black }
            };

            PartListPartRightGrayStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
                stretchWidth = true,
                normal = { textColor = Color.black }
            };

            PartListbtnStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 20,
                fixedHeight = 20,
                fontSize = 14,
                fontStyle = FontStyle.Normal
            };
            PartListbtnStyle.active.background = GUI.skin.toggle.onNormal.background;
            PartListbtnStyle.onActive.background = PartListbtnStyle.active.background;
            PartListbtnStyle.padding = Utilities.SetRectOffset(PartListbtnStyle.padding, 3);

            ResizeStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 20,
                fixedHeight = 20,
                fontSize = 14,
                fontStyle = FontStyle.Normal
            };
            ResizeStyle.onActive.background = PartListbtnStyle.active.background;
            ResizeStyle.padding = Utilities.SetRectOffset(PartListbtnStyle.padding, 3);

            PrioritybtnStyle = new GUIStyle(PartListbtnStyle);

            StylesSet = true;

        }
    }
}
