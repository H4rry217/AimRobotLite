using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.utils {
    public class GraphicsUtils {

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
        IntPtr hdcSrc, int nXSrc, int nYSrc, RasterOperation dwRop);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public enum RasterOperation : uint {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
        }

        public struct RECT {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static Bitmap GetScreenShot(IntPtr handle) {
            RECT windowRect;
            GetWindowRect(handle, out windowRect);

            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;

            Bitmap screenshot = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(screenshot)) {
                IntPtr hdcDest = graphics.GetHdc();
                IntPtr hdcSrc = GetDC(handle);
                IntPtr hdcCompatible = CreateCompatibleDC(hdcDest);
                IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
                IntPtr hOld = SelectObject(hdcCompatible, hBitmap);

                BitBlt(hdcCompatible, 0, 0, width, height, hdcSrc, 0, 0, RasterOperation.SRCCOPY);
                BitBlt(hdcDest, 0, 0, width, height, hdcCompatible, 0, 0, RasterOperation.SRCCOPY);

                // clean
                SelectObject(hdcCompatible, hOld);
                DeleteObject(hBitmap);
                ReleaseDC(handle, hdcSrc);
                ReleaseDC(handle, hdcCompatible);
                graphics.ReleaseHdc(hdcDest);
            }

            //Console.WriteLine("Screenshot capture");

            return screenshot;
        }


    }
}
