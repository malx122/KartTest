using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
using Hjg.Pngcs.Zlib;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;

namespace blend
{//150-2150
    class Program
    {
        static void Main(string[] args)
        {
            HotKeyManager.RegisterHotKey(Keys.A, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.D, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.W, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.S, KeyModifiers.Alt);

            HotKeyManager.RegisterHotKey(Keys.F, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.G, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.T, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.H, KeyModifiers.Alt);

            //Test
            HotKeyManager.RegisterHotKey(Keys.Y, KeyModifiers.Alt);

            HotKeyManager.RegisterHotKey(Keys.C, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.V, KeyModifiers.Alt);

            HotKeyManager.RegisterHotKey(Keys.Space, KeyModifiers.Alt);


            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyManager_HotKeyPressed);
            Console.ReadLine(); 
        }

        static Thread workthread;

        static Semaphore WaitForSpace = new Semaphore(0,1);
        static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            String map = "rip";
            int columns = 4;
            int rows = 8;
            int sleep = 30000; //time to sleep between moves (ms)
            
            String sweden_mapfile = String.Format(@"c:\map\sverige_{0}.png",map);
            String topo_mapfile = String.Format(@"c:\map\topo_{0}.png", map);
            String orto_mapfile = String.Format(@"c:\map\orto_{0}.png", map);
            String temp_mapfile = String.Format(@"c:\map\temp_{0}.png", map);
            String mapfile = String.Format(@"c:\map\{0}.png", map);

            switch (e.Key)
            {
                case Keys.A:
                    MoveLeft();
                    break;
                case Keys.D:
                    MoveRight();
                    break;
                case Keys.W:
                    MoveUp();
                    break;
                case Keys.S:
                    MoveDown();
                    break;
                case Keys.Space:
                  //  join(columns, rows, sweden_mapfile);
                            
                    WaitForSpace.Release(1);
                    break;
                case Keys.F:
                    if (workthread == null || !workthread.IsAlive)
                    {
                        workthread = new Thread(new ThreadStart(() =>
                        {
                            Console.Out.WriteLine("Skapar sverigekarta..");
                            TakeMultiSS(columns, rows, sleep);
                            join(columns, rows, sweden_mapfile);
                            if (IsOdd(columns))
                                Move(-columns + 1, -rows + 1);
                            else
                                Move(-columns + 1, 0);
                        }));
                        workthread.Start();
                    }
                    else
                        Console.Out.WriteLine("Already working!");
                    break;
                case Keys.G:
                    if (workthread == null || !workthread.IsAlive)
                    {
                        workthread = new Thread(new ThreadStart(()=> {
                            Console.Out.WriteLine("Skapar topokarta..");
                            TakeMultiSS(columns, rows, sleep);
                            join(columns, rows, topo_mapfile);
                            if (IsOdd(columns))
                                Move(-columns + 1, -rows + 1);
                            else
                                Move(-columns + 1, 0);
                         }));
                         workthread.Start();
                    }
                    else
                        Console.Out.WriteLine("Already working!");
                    break;
                case Keys.T:
                    if (workthread == null || !workthread.IsAlive)
                    {
                        workthread = new Thread(new ThreadStart(() =>
                        {
                            Console.Out.WriteLine("Skapar ortokarta..");
                            TakeMultiSS(columns, rows, sleep);
                            join(columns, rows, orto_mapfile);
                            if (IsOdd(columns))
                                Move(-columns + 1, -rows + 1);
                            else
                                Move(-columns + 1, 0);
                        }));
                        workthread.Start();
                    }
                    else
                        Console.Out.WriteLine("Already working!");
                    break;
                case Keys.H:
                    if (workthread == null || !workthread.IsAlive)
                    {
                        workthread = new Thread(new ThreadStart(() =>
                        {
                            Bumpify(sweden_mapfile, topo_mapfile, mapfile);
                        }));
                        workthread.Start();
                    }
                    else
                        Console.Out.WriteLine("Already working!");
                    break;
                case Keys.C:
                    if (workthread == null || !workthread.IsAlive)
                    {
                        workthread = new Thread(new ThreadStart(() =>
                        {
                            Blend(sweden_mapfile, orto_mapfile, temp_mapfile, 35);
                        }));
                        workthread.Start();
                    }
                    else
                        Console.Out.WriteLine("Already working!");
                    break;
                case Keys.V:
                    if (workthread == null || !workthread.IsAlive)
                    {
                        workthread = new Thread(new ThreadStart(() =>
                        {
                            Bumpify(temp_mapfile, topo_mapfile, mapfile);
                        }));
                        workthread.Start();
                    }
                    else
                        Console.Out.WriteLine("Already working!");
                    break;
                case Keys.Y:
                    Console.Out.WriteLine("Skapar ortokarta..");
                    join(columns, rows, orto_mapfile);
                    break;
            }
        }


        public static void Move(int columns, int rows)
        {
            Console.Out.WriteLine("Flyttar: {0} kolumner , {1} rader", columns, rows);
            bool negcols = columns < 0;
            for (int j = 0; j < Math.Abs(columns); j++)
            {
                if (negcols)
                    MoveLeft();
                else
                    MoveRight();
                Thread.Sleep(200);
            }
            bool negrows = rows < 0;
            for (int j = 0; j < Math.Abs(rows); j++)
            {
                if (negrows)
                    MoveUp();
                else
                    MoveDown();
                Thread.Sleep(200);
            }
        }

        public static void MoveDown()
        {
            MouseSimulation.MouseClickDrag(MouseSimulation.Button.Left, 1000, 1150, 1000, 250, 0);
        }

        public static void MoveUp()
        {
            MouseSimulation.MouseClickDrag(MouseSimulation.Button.Left, 1000, 250, 1000, 1150, 0);
        }

        public static void MoveLeft()
        {
            MouseSimulation.MouseClickDrag(MouseSimulation.Button.Left, 150, 500, 2150, 500, 0);
        }

        public static void MoveRight()
        {
            MouseSimulation.MouseClickDrag(MouseSimulation.Button.Left, 2150, 500, 150, 500, 0);
        }


        public static void SleepOrWait(int sleep)
        {
            if (sleep > 0)
                WaitForSpace.WaitOne(sleep);
            else
            {
                WaitForSpace.WaitOne();
            };
        }

        public static void TakeMultiSS(int cols, int rows, int sleep)
        {
            Console.Out.WriteLine("Tar multipla screenshots, Kolumner: {0}, Rader {1}, Sleep {2}", cols, rows, sleep); 
            int counter = 0;
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    Console.Write("{0}({1}), {2}({3}):", i, cols, j, rows);
                    TakeSS(counter++);
                    if (j != rows - 1)
                    {
                        if (!IsOdd(i))
                            MoveDown();
                        else
                            MoveUp();
                        SleepOrWait(sleep);
                    }
                }
                if (i != cols - 1)
                {
                    MoveRight();
                    SleepOrWait(sleep);
                }
            }
        }

        static void TakeSS(int counter)
        {
            Bitmap bmpScreenshot;
            Graphics gfxScreenshot;

            bmpScreenshot = new Bitmap(2000, 900, PixelFormat.Format32bppArgb);
            // Create a graphics object from the bitmap
            gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            // Take the screenshot from the upper left corner to the right bottom corner
            gfxScreenshot.CopyFromScreen(150, 
                250, 0, 0, bmpScreenshot.Size, CopyPixelOperation.SourceCopy);
            // Save the screenshot to the specified path that the user has chosen
            bmpScreenshot.Save(String.Format(@"c:\map\test{0}.png",counter++), ImageFormat.Png);
            Console.WriteLine(String.Format("SS{0} successful!", counter));
        }

        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        static void join(int cols, int rows, string endfilename)
        {
            int height = 900;
            int width = 2000;
            ImageInfo info = new ImageInfo(cols * width, rows * height, 8, false);
            PngWriter pngw = FileHelper.CreatePngWriter(endfilename, info, true); // idem

                for (int i = 0; i < rows; i++)
                {
                    Console.Write(String.Format("Row {0}:", i));
                    PngReader[] readers = new PngReader[cols];
                    for (int j = 0; j < cols; j++)
                    {
                        if (IsOdd(j))
                        {
                            String filename = String.Format(@"c:\map\test{0}.png", (j+1)*rows-i-1);
                            try
                            {
                                if (File.Exists(filename))
                                    readers[j] = FileHelper.CreatePngReader(filename);
                            }
                            catch
                            {
                                readers[j] = null;
                            }
                        }
                        else
                        {
                            String filename = String.Format(@"c:\map\test{0}.png", i+j*rows);
                            try
                            {
                                if (File.Exists(filename))
                                    readers[j] = FileHelper.CreatePngReader(filename);
                            }
                            catch
                            {
                                readers[j] = null;
                            }
                        }
                    }
                    int channels = readers[0].ImgInfo.Channels;
                    for (int k = 0; k < height; k++)
                    {
                        ImageLine line = new ImageLine(info);
                        for (int l = 0; l < cols; l++)
                        {
                            if (readers[l] != null)
                            {
                                ImageLine r = readers[l].ReadRowInt(k);
                                for (int m = 0; m < width; m++)
                                {
                                    line.Scanline[(m + (l*width))*3] = r.Scanline[m*channels];
                                    line.Scanline[(m + (l*width))*3 + 1] = r.Scanline[m * channels + 1];
                                    line.Scanline[(m + (l*width))*3 + 2] = r.Scanline[m * channels + 2];
                                }
                            }
                        }
                        pngw.WriteRow(line, k+i*height);
                        Console.Write(".");
                    }
                    foreach (PngReader reader in readers)
                    {
                        if (reader != null)
                            reader.End();
                    }
                    Console.Out.WriteLine("");
                    
                }
            pngw.End();

        }

        static void Bumpify(String flat, String topo, String outfile)
        {
            double middle = 0.8;
            double bumpifyfactor = 0.5;

            PngReader pngr = FileHelper.CreatePngReader(flat); // or you can use the constructor
            PngReader pngr2 = FileHelper.CreatePngReader(topo); // or you can use the constructor
            PngWriter pngw = FileHelper.CreatePngWriter(outfile, pngr.ImgInfo, true); // idem
            Console.WriteLine(pngr.ToString()); // just information
            int chunkBehav = ChunkCopyBehaviour.COPY_ALL_SAFE; // tell to copy all 'safe' chunks
            pngw.CopyChunksFirst(pngr, chunkBehav);          // copy some metadata from reader 
            int channels = pngr.ImgInfo.Channels;
            if (channels < 3)
                throw new Exception("This example works only with RGB/RGBA images");
            for (int row = 0; row < pngr.ImgInfo.Rows; row++) {
                ImageLine l1 = pngr.ReadRowInt(row); // format: RGBRGB... or RGBARGBA...
                ImageLine l2 = pngr2.ReadRowInt(row);
                int[] l1l = l1.Scanline;
                int[] l2l = l2.Scanline;
                for (int j = 0; j < pngr.ImgInfo.Cols; j++)
                {
                    //ImageLineHelper.
                    HSB hsb1 = ColorHelper.RGBtoHSB(l1l[j*channels], l1l[j*channels+1], l1l[j*channels+2]);
                    HSB hsb2 = ColorHelper.RGBtoHSB(l2l[j*channels], l2l[j*channels+1], l2l[j*channels+2]);
                    hsb1.Brightness = Math.Min(Math.Max(hsb1.Brightness + (hsb2.Brightness-middle)*bumpifyfactor, 0.0),1.0);
                    hsb1.Saturation = Math.Min(hsb1.Saturation + 0.1, 0.9);
                    RGB res = ColorHelper.HSBtoRGB(hsb1.Hue, hsb1.Saturation, hsb1.Brightness);
                    if (res.Green == 0 && res.Red == 0 && res.Blue == 0 && hsb1.Brightness > 0)
                        Console.Write("x");
                    ImageLineHelper.SetPixel(l1, j, res.Red, res.Green, res.Blue);
                }
                pngw.WriteRow(l1, row);
                Console.Write(".");
            }
            pngw.CopyChunksLast(pngr, chunkBehav); // metadata after the image pixels? can happen
            pngw.End(); // dont forget this
            pngr.End();
            pngr2.End();
        }

        static void Blend(String in1, String in2, String outfile, int percent)
        {
            Boolean skiphouses = true;

            PngReader pngr = FileHelper.CreatePngReader(in1); // or you can use the constructor
            PngReader pngr2 = FileHelper.CreatePngReader(in2); // or you can use the constructor
            PngWriter pngw = FileHelper.CreatePngWriter(outfile, pngr.ImgInfo, true); // idem
            Console.WriteLine(pngr.ToString()); // just information
            int chunkBehav = ChunkCopyBehaviour.COPY_ALL_SAFE; // tell to copy all 'safe' chunks
            pngw.CopyChunksFirst(pngr, chunkBehav);          // copy some metadata from reader 
            int channels = pngr.ImgInfo.Channels;
            if (channels < 3)
                throw new Exception("This example works only with RGB/RGBA images");
            for (int row = 0; row < pngr.ImgInfo.Rows; row++)
            {
                ImageLine l1 = pngr.ReadRowInt(row); // format: RGBRGB... or RGBARGBA...
                ImageLine l2 = pngr2.ReadRowInt(row);
                int[] l1l = l1.Scanline;
                int[] l2l = l2.Scanline;
                for (int j = 0; j < pngr.ImgInfo.Cols; j++)
                {
                    HSB hsb1 = ColorHelper.RGBtoHSB(l1l[j * channels], l1l[j * channels + 1], l1l[j * channels + 2]);
                    if ((hsb1.Saturation < 0.1 || hsb1.Saturation > 0.9) && (hsb1.Brightness < 0.91))
                    {
                        int red = l1l[j * channels];
                        int green = l1l[j * channels + 1];
                        int blue = l1l[j * channels + 2];
                        if (hsb1.Brightness < 0.5)
                            ImageLineHelper.SetPixel(l1, j, 0, 0, 0);
                        else
                            ImageLineHelper.SetPixel(l1, j, red, green, blue);
                    }
                    else if (hsb1.Brightness < 0.5)
                    {
                        ImageLineHelper.SetPixel(l1, j, 0, 0, 0);
                    }
                    else if (skiphouses &&
                        ((hsb1.Hue > 24 && hsb1.Hue < 28) ||
                        (hsb1.Hue > 40 && hsb1.Hue < 44)))
                    {
                        ImageLineHelper.SetPixel(l1, j, l2l[j * channels], l2l[j * channels + 1], l2l[j * channels + 2]);
                    }
                    else
                    {
                        int red = (percent * l1l[j * channels] + (100 - percent) * l2l[j * channels]) / 100;
                        int green = (percent * l1l[j * channels + 1] + (100 - percent) * l2l[j * channels + 1]) / 100;
                        int blue = (percent * l1l[j * channels + 2] + (100 - percent) * l2l[j * channels + 2]) / 100;
                        ImageLineHelper.SetPixel(l1, j, red, green, blue);
                    }
                }
                pngw.WriteRow(l1, row);
                Console.Write(".");
            }
            pngw.CopyChunksLast(pngr, chunkBehav); // metadata after the image pixels? can happen
            pngw.End(); // dont forget this
            pngr.End();
            pngr2.End();
        }
    }
}
    

