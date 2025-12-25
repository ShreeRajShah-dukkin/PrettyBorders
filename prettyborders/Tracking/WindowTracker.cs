using System;
using System.Text;
using prettyborders.Win32;

namespace prettyborders.Tracking
{
  internal static class WindowTracker
  {

    public static IntPtr FindWindowByTitle(string containsTitle)
    {
      IntPtr foundHwnd = IntPtr.Zero;

      NativeMethods.EnumWindows((hWnd, lParam) =>
      {
        if (!NativeMethods.IsWindowVisible(hWnd))
          return true;

        var title = new StringBuilder(256);
        NativeMethods.GetWindowText(hWnd, title, title.Capacity);

        if (title.ToString().Contains(containsTitle, StringComparison.OrdinalIgnoreCase))
        {
          foundHwnd = hWnd;
          return false; // stop enumeration
        }

        return true;
      }, IntPtr.Zero);

      return foundHwnd;
    }


    public static void PrintAllVisibleWindows()
    {
      NativeMethods.EnumWindows((hWnd, lParam) =>
      {
        if (!NativeMethods.IsWindowVisible(hWnd))
          return true;

        var title = new StringBuilder(256);
        NativeMethods.GetWindowText(hWnd, title, title.Capacity);

        if (string.IsNullOrWhiteSpace(title.ToString()))
          return true;

        NativeMethods.GetWindowRect(hWnd, out RECT rect);

        Console.WriteLine(
                  $"HWND: {hWnd} | " +
                  $"Title: {title} | " +
                  $"X:{rect.Left} Y:{rect.Top} " +
                  $"W:{rect.Width} H:{rect.Height}"
              );

        return true;
      }, IntPtr.Zero);
    }
  }
}
