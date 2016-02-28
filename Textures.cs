using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AY
{
    internal static class Textures
    {
        //Icons
        internal static Texture2D iconGreenOff = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D iconGreenOn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D iconRedOff = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D iconRedOn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D iconYellowOff = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D iconYellowOn = new Texture2D(38, 38, TextureFormat.ARGB32, false);

        //Toolbar Icons
        internal static Texture2D toolbariconGreenOff = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D toolbariconGreenOn = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D toolbariconRedOff = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D toolbariconRedOn = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D toolbariconYellowOff = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D toolbariconYellowOn = new Texture2D(24, 24, TextureFormat.ARGB32, false);              
        
        internal static String PathIconsPath = System.IO.Path.Combine(AmpYear._AssemblyFolder.Substring(0, AmpYear._AssemblyFolder.IndexOf("/AmpYear/") + 9), "Icons").Replace("\\", "/");
        internal static String PathToolbarIconsPath = PathIconsPath.Substring(PathIconsPath.ToLower().IndexOf("/gamedata/") + 10);
        

        internal static void loadIconAssets()
        {
            try
            {
                LoadImageFromFile(ref iconGreenOff, "AYGreenOff.png", PathIconsPath);  
                LoadImageFromFile(ref iconGreenOn, "AYGreenOn.png", PathIconsPath);
                LoadImageFromFile(ref iconRedOff, "AYRedOff.png", PathIconsPath);
                LoadImageFromFile(ref iconRedOn, "AYRedOn.png", PathIconsPath);
                LoadImageFromFile(ref iconYellowOff, "AYYellowOff.png", PathIconsPath);
                LoadImageFromFile(ref iconYellowOn, "AYYellowOn.png", PathIconsPath);

                LoadImageFromFile(ref toolbariconGreenOff, "AYGreenOffTB.png", PathIconsPath);
                LoadImageFromFile(ref toolbariconGreenOn, "AYGreenOnTB.png", PathIconsPath);
                LoadImageFromFile(ref toolbariconRedOff, "AYRedOffTB.png", PathIconsPath);
                LoadImageFromFile(ref toolbariconRedOn, "AYRedOnTB.png", PathIconsPath);
                LoadImageFromFile(ref toolbariconYellowOff, "AYYellowOffTB.png", PathIconsPath);
                LoadImageFromFile(ref toolbariconYellowOn, "AYYellowOnTB.png", PathIconsPath);
            }
            catch (Exception)
            {
                Utilities.Log("AmpYear","Failed to Load Textures - are you missing a file?");
            }
        }

        public static Boolean LoadImageFromFile(ref Texture2D tex, String FileName, String FolderPath = "")
        {            
            Boolean blnReturn = false;
            try
            {
                if (FolderPath == "") FolderPath = PathIconsPath;

                //File Exists check
                if (System.IO.File.Exists(String.Format("{0}/{1}", FolderPath, FileName)))
                {
                    try
                    {                        
                        tex.LoadImage(System.IO.File.ReadAllBytes(String.Format("{0}/{1}", FolderPath, FileName)));
                        blnReturn = true;
                    }
                    catch (Exception ex)
                    {
                        Utilities.Log("AmpYear", "Failed to load the texture:" + FolderPath + "(" + FileName + ")");
                        Utilities.Log("AmpYear", ex.Message);
                    }
                }
                else
                {
                    Utilities.Log("AmpYear", "Cannot find texture to load:" + FolderPath + "(" + FileName + ")");                    
                }


            }
            catch (Exception ex)
            {
                Utilities.Log("AmpYear", "Failed to load (are you missing a file):" + FolderPath + "(" + FileName + ")");
                Utilities.Log("AmpYear", ex.Message);                
            }
            return blnReturn;
        }
    }
}
