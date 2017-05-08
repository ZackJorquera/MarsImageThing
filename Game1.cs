using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Linq;
using System;


namespace MarsImageThing
{

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont font;// arial

        Texture2D whiteBox;//just one pixel

        Texture2D[] imagesInList = new Texture2D[8];
        string[] ImagesToEdit = new string[8];
        Texture2D[] imagesInList2 = new Texture2D[8];
        string[] ImagesToEdit2 = new string[8];
        Point ImageSize = Point.Zero;

        int cameraImagesAreFrom = -1;//-1 is left, 0 is both, 1 is right
        bool grabbingSlider = false;

        Texture2D BlankImage;
        int hoveringOver = 0;
        int openStackMaxValuePer = 100;
        int openStackValuePer = 0;

        bool hoveringOverNewPointButton = false;
        bool hoveringOverRemovePointButton = false;
        List<SpectralData> spectralDataPoints = new List<SpectralData>();
        Color[] colors = { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Gold, Color.Purple };
        int colorOn = 0;
        bool findingPoint = false;
        Point finding;
        Texture2D whiteDot;

        ClassifyImage ClassifyImage = new ClassifyImage();
        bool Classifying = false;
        bool Coloring = false;
        Texture2D classifyedImage;
        Stream OutputImageStream = null;
        bool hoveringOverClassifyButton = false;

        bool hoveringOverClearButton = false;

        bool hoveringOverColorImageButton = false;

        bool hoveringOverInfoButton = false;

        string OutputText = "";
        int ErrorTimeOut = 0;

        bool MouseCLickedDown = false;


        string x;
        string y;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.IsMouseVisible = true;
            this.Window.Title = "Mars Image Classification";

            graphics.PreferredBackBufferWidth = 1006;//855
            graphics.ApplyChanges();

            base.Initialize();


        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("FontArial");
            BlankImage = Content.Load<Texture2D>("BlankImage");
            whiteBox = Content.Load<Texture2D>("whiteImage");
            whiteDot = Content.Load<Texture2D>("White_dot");

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                Exit();
            MouseState mouseState = Mouse.GetState();

            if (!IsActive)
                return;

            if (Classifying || Coloring)
            {
                List<string> ImagesToClassify = new List<string>();
                ImagesToClassify = ImagesToEdit.ToList();
                if (cameraImagesAreFrom == 0)
                {
                    ImagesToClassify.Concat(ImagesToEdit2.ToList());
                }
                OutPutImageData outPutImagesData;
                if(Classifying)
                    outPutImagesData = ClassifyImage.Classify(ImagesToClassify.ToArray(), spectralDataPoints, cameraImagesAreFrom, ImageSize);
                else
                    outPutImagesData = ClassifyImage.ColorImage(ImagesToClassify.ToArray(), ImageSize);

                if (outPutImagesData.outPutImageStream != null)
                {
                    OutputImageStream = outPutImagesData.outPutImageStream;
                    classifyedImage = Texture2D.FromStream(GraphicsDevice, OutputImageStream);
                    OutputImageStream.Close();
                }
                else
                {
                    if (outPutImagesData.IOErrorException != null)
                    {
                        OutputText = "File input error: " + outPutImagesData.IOErrorException.Message;
                        OutputText = new string(OutputText.Select((j, i) => i > 0 && i % 60 == 0 ? new[] { '\n', j } : new[] { j }).SelectMany(j => j).ToArray());
                        ErrorTimeOut = 1000;
                    }
                }
                Classifying = false;
                Coloring = false;
            }


            if (findingPoint != true)
            {
                if (hoveringOver != 0)
                {
                    if (openStackValuePer < openStackMaxValuePer)
                        openStackValuePer += 15;
                    else if (openStackValuePer > openStackMaxValuePer)
                        openStackValuePer = openStackMaxValuePer;

                    if (mouseState.Y >= 25 && mouseState.Y < ((cameraImagesAreFrom == 0) ? 256 : 0) + 280 && mouseState.X < (imagesInList.Length - 1) * openStackValuePer + 25 + 255 && mouseState.X > 25)
                        hoveringOver = GetHoveringOverImageNumber(mouseState.X, mouseState.Y);
                    else
                        hoveringOver = 0;
                }
                else
                {
                    if (openStackValuePer > 0)
                        openStackValuePer -= 15;
                    else if (openStackValuePer < 0)
                        openStackValuePer = 0;

                    if (mouseState.Y >= 25 && mouseState.Y < 280 && mouseState.X < 25 + 255 + (imagesInList.Length - 1) * openStackValuePer && mouseState.X > 25 && grabbingSlider == false)
                        hoveringOver = GetHoveringOverImageNumber(mouseState.X, mouseState.Y);
                    else
                        hoveringOver = 0;
                }

                if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && hoveringOver != 0 && !MouseCLickedDown)
                    loadImages(hoveringOver);
            }

            x = mouseState.X.ToString();//for debug
            y = mouseState.Y.ToString();


            hoveringOverNewPointButton = false;
            hoveringOverRemovePointButton = false;
            hoveringOverClassifyButton = false;
            hoveringOverClearButton = false;
            hoveringOverColorImageButton = false;
            hoveringOverInfoButton = false;
            if (DoesImagesInListHaveMoreThan1Images() && hoveringOver == 0)
            {
                if (mouseState.X >= 300 && mouseState.X < 400 && mouseState.Y >= 215 && mouseState.Y < 215 + 25)
                {
                    hoveringOverNewPointButton = true;
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && MouseCLickedDown && colorOn < 6 && !findingPoint)
                    {
                        colorOn++;
                        findingPoint = true;
                    }
                }
                else if (mouseState.X >= 425 && mouseState.X < 425 + 125 && mouseState.Y >= 215 && mouseState.Y < 215 + 25)
                {
                    hoveringOverRemovePointButton = true;
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && MouseCLickedDown && colorOn > 0)
                    {
                        colorOn--;
                        if (findingPoint)
                            findingPoint = false;
                        else
                            spectralDataPoints.Remove(spectralDataPoints[spectralDataPoints.Count - 1]);
                    }
                }
                else if (mouseState.X >= 300 && mouseState.X < 300 + 100 && mouseState.Y >= 255 && mouseState.Y < 255 + 25)
                {
                    hoveringOverClassifyButton = true;
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && MouseCLickedDown && colorOn > 0)
                    {
                        Classifying = true;
                    }
                }
                else if (mouseState.X >= 425 && mouseState.X < 425 + 100 && mouseState.Y >= 255 && mouseState.Y < 255 + 25)
                {
                    hoveringOverClearButton = true;
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && MouseCLickedDown)
                    {
                        for (int i = 0; i < imagesInList.Length; i++)
                        {
                            imagesInList[i] = null;
                            ImagesToEdit[i] = null;
                            imagesInList2[i] = null;
                            ImagesToEdit2[i] = null;
                        }
                        cameraImagesAreFrom = -1;
                        spectralDataPoints = new List<SpectralData>();
                        classifyedImage = null;
                        colorOn = 0;
                        findingPoint = false;
                        ImageSize = Point.Zero;
                        OutputImageStream = null;

                    }
                }
                else if (mouseState.X >= 575 && mouseState.X < 575 + 100 && mouseState.Y >= 215 && mouseState.Y < 215 + 25)
                {
                    hoveringOverColorImageButton = true;
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && MouseCLickedDown && cameraImagesAreFrom == -1 && ((imagesInList[1] != null && imagesInList[4] != null && imagesInList[6] != null) || (imagesInList[2] != null && imagesInList[4] != null && imagesInList[5] != null)))
                    {
                        Coloring = true;
                    }
                }

                if (mouseState.X >= 90 && mouseState.X < 90 + 125 && mouseState.Y >= 290 && mouseState.Y < 290 + 25 && mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && !MouseCLickedDown && hoveringOver == 0)//center x = 153
                {
                    grabbingSlider = true;
                }

                if (mouseState.X >= graphics.PreferredBackBufferWidth - (256 + 25) && mouseState.X < graphics.PreferredBackBufferWidth - (256 + 25) + 256 && mouseState.Y >= 25 && mouseState.Y < 256 + 25)//graphics.PreferredBackBufferWidth - (256 + 25), 25, 256, 256
                {
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && MouseCLickedDown && OutputImageStream != null)
                    {
                        System.Diagnostics.Process.Start((OutputImageStream as FileStream).Name);
                    }
                }
            }

            if (mouseState.X >= 575 && mouseState.X < 575 + 100 && mouseState.Y >= 255 && mouseState.Y < 255 + 25)
            {
                hoveringOverInfoButton = true;
                if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && MouseCLickedDown)
                {
                    System.Diagnostics.Process.Start("https://github.com/ZackJorquera/MarsImageThing");
                }
            }

            if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && grabbingSlider == true)
                grabbingSlider = false;

            if (grabbingSlider)
            {
                if (mouseState.Position.X < 90 + 25 + 25 / 2)
                    cameraImagesAreFrom = -1;
                else if (mouseState.Position.X > 90 + 75 + 25 / 2)
                    cameraImagesAreFrom = 1;
                else
                    cameraImagesAreFrom = 0;
            }


            if (ErrorTimeOut-- < 0)
            {
                if (DoesImagesInListHaveMoreThan1Images())
                {
                    if (spectralDataPoints.Count > 1)
                    {
                        if (Classifying || Coloring)
                            OutputText = "Please wait While the images are classifying.";
                        else
                            OutputText = "Click on the 'Classify' button to made a new classified image.";
                    }
                    else
                        OutputText = "To add a point on the image to classify with click on the 'add point'\n" +
                                     "button and then click on the image where you want the point to be. \n" +
                                     "Or you can click on the color for the point at the bottom part of the page\n" +
                                     " to add spectral data in a .asc file. To remove or cancle the point selection\n" +
                                     "click the 'remove point' button. You can add up to 6 points and there must\n" +
                                     "be at least 2 point to classify with. The 'color' button creates a colored\n" +
                                     "image, It requires filters L2 L5 L7 or L3 L5 L6.";//L2 L5 L7 or L3 L5 L6
                }
                else
                    OutputText = "Click on the image/file icon to add or change the image slot.\n" +
                                 "Make sure at least 2 image slots are filled. If you plan to use\n" +
                                 "spectral data, make sure each image aligns up with the proper\n" +
                                 "image slot.";
                ErrorTimeOut = -1;
            }
            if (findingPoint)
            {

                finding.X = (mouseState.X - 25) * ImageSize.X / 256;
                finding.Y = (mouseState.Y - 25) * ImageSize.Y / 256;//-(mouseState.Y - 25 - 256) * ImageSize.Y / 256;

                if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && MouseCLickedDown && finding.X > 0 && finding.X <= ImageSize.X - 1 && finding.Y > 0 && finding.Y <= ImageSize.Y - 1)
                {
                    spectralDataPoints.Add(new SpectralData(finding, null, cameraImagesAreFrom));
                    findingPoint = false;
                }
                else if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && MouseCLickedDown && mouseState.X > 50 + (colorOn - 1) * (graphics.PreferredBackBufferWidth / 6) && mouseState.X <= 50 + (colorOn - 1) * (graphics.PreferredBackBufferWidth / 6) + 50 && mouseState.Y > 350 && mouseState.Y <= 400)//(40 + i * (graphics.PreferredBackBufferWidth/6), 350, 50,50)
                {
                    OpenFileDialog openFileDialog2 = new OpenFileDialog();
                    openFileDialog2.Filter = "Ascii (*.asc)|*.asc|All files (*.*)|*.*";
                    openFileDialog2.FilterIndex = 1;
                    openFileDialog2.RestoreDirectory = true;

                    try
                    {
                        Stream stream;
                        if (openFileDialog2.ShowDialog() == DialogResult.OK)
                        {
                            if ((stream = openFileDialog2.OpenFile()) != null)
                            {
                                spectralDataPoints.Add(new SpectralData(Point.Zero, stream, cameraImagesAreFrom));
                                findingPoint = false;
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        OutputText = "File input error: " + ex.Message;
                        OutputText = new string(OutputText.Select((j, i) => i > 0 && i % 60 == 0 ? new[] { '\n', j } : new[] { j }).SelectMany(j => j).ToArray());//??
                        ErrorTimeOut = 1000;
                    }
                }
            }
            else
            {
                finding.X = 0;
            }
            if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                MouseCLickedDown = true;
            else
                MouseCLickedDown = false;

            base.Update(gameTime);

        }

        int GetHoveringOverImageNumber(int mouseXpos, int mouseYpos)
        {
            if (mouseYpos <= 280)
            {
                if (mouseXpos >= 25 && mouseXpos < 25 + ((hoveringOver != 2 && hoveringOver != 3) ? 255 : 130))
                    return 1;
                else if (mouseXpos >= 25 + 100 + ((hoveringOver == 2) ? 0 : 125) && mouseXpos < 25 + 100 + ((hoveringOver != 3 && hoveringOver != 4) ? 255 : 140))
                    return 2;
                else if (mouseXpos >= 25 + 200 + ((hoveringOver == 3) ? 0 : 125) && mouseXpos < 25 + 200 + ((hoveringOver != 4 && hoveringOver != 5) ? 255 : 140))
                    return 3;
                else if (mouseXpos >= 25 + 300 + ((hoveringOver == 4) ? 0 : 125) && mouseXpos < 25 + 300 + ((hoveringOver != 5 && hoveringOver != 6) ? 255 : 140))
                    return 4;
                else if (mouseXpos >= 25 + 400 + ((hoveringOver == 5) ? 0 : 125) && mouseXpos < 25 + 400 + ((hoveringOver != 6 && hoveringOver != 7) ? 255 : 140))
                    return 5;
                else if (mouseXpos >= 25 + 500 + ((hoveringOver == 6) ? 0 : 125) && mouseXpos < 25 + 500 + ((hoveringOver != 7 && hoveringOver != 8) ? 255 : 140))
                    return 6;
                else if (mouseXpos >= 25 + 600 + ((hoveringOver == 7) ? 0 : 125) && mouseXpos < 25 + 600 + ((hoveringOver != 8) ? 255 : 140))
                    return 7;
                else if (mouseXpos >= 25 + 700 + ((hoveringOver == 8) ? 0 : 125) && mouseXpos < 25 + 700 + 255)
                    return 8;
                else
                    return 0;
            }
            else if (cameraImagesAreFrom == 0 && mouseYpos > 280)
            {
                if (mouseXpos >= 25 && mouseXpos < 25 + ((hoveringOver != 2 && hoveringOver != 3) ? 255 : 130))
                    return 9;
                else if (mouseXpos >= 25 + 100 + ((hoveringOver == 2) ? 0 : 125) && mouseXpos < 25 + 100 + ((hoveringOver != 3 && hoveringOver != 4) ? 255 : 140))
                    return 10;
                else if (mouseXpos >= 25 + 200 + ((hoveringOver == 3) ? 0 : 125) && mouseXpos < 25 + 200 + ((hoveringOver != 4 && hoveringOver != 5) ? 255 : 140))
                    return 11;
                else if (mouseXpos >= 25 + 300 + ((hoveringOver == 4) ? 0 : 125) && mouseXpos < 25 + 300 + ((hoveringOver != 5 && hoveringOver != 6) ? 255 : 140))
                    return 12;
                else if (mouseXpos >= 25 + 400 + ((hoveringOver == 5) ? 0 : 125) && mouseXpos < 25 + 400 + ((hoveringOver != 6 && hoveringOver != 7) ? 255 : 140))
                    return 13;
                else if (mouseXpos >= 25 + 500 + ((hoveringOver == 6) ? 0 : 125) && mouseXpos < 25 + 500 + ((hoveringOver != 7 && hoveringOver != 8) ? 255 : 140))
                    return 14;
                else if (mouseXpos >= 25 + 600 + ((hoveringOver == 7) ? 0 : 125) && mouseXpos < 25 + 600 + ((hoveringOver != 8) ? 255 : 140))
                    return 15;
                else if (mouseXpos >= 25 + 700 + ((hoveringOver == 8) ? 0 : 125) && mouseXpos < 25 + 700 + 255)
                    return 16;
                else
                    return 0;
            }
            else
                return 0;

        }

        void loadImages(int hovering)
        {
            bool forImagesInList2 = true;
            if (hovering < 9)
                forImagesInList2 = false;


            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image File (*.jpg,*.png,*.bmp,*.tif,*.dds)|*.jpg;*.png;*.bmp;*.tif;*.dds|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    char[] fileName = openFileDialog1.FileName.ToCharArray();
                    if (fileName[fileName.Length - 5] <= 57 && fileName[fileName.Length - 5] >= 48)
                    {
                        int startFileNumber = fileName[fileName.Length - 5] - 48;
                        for (int i = 0; i < imagesInList.Length - ((hovering < 9) ? hovering : hovering - 8) + 1 && i < imagesInList.Length - startFileNumber + 1; i++)//imagesInList.Length = imagesInList2.Length
                        {
                            fileName[fileName.Length - 5] = (char)(i + startFileNumber + 48);
                            var fname = new string(fileName);
                            FileInfo fI = new FileInfo(fname);
                            if (fI.Exists && (((hovering < 9) ? imagesInList[i + hovering - 1] : imagesInList2[i + hovering - 9]) == null || i == 0))
                            {
                                Stream stream = null;
                                stream = fI.Open(FileMode.Open);
                                if (forImagesInList2)
                                {
                                    imagesInList2[i + hovering - 9] = Texture2D.FromStream(GraphicsDevice, stream);
                                    ImagesToEdit2[i + hovering - 9] = fname;
                                }
                                else
                                {
                                    imagesInList[i + hovering - 1] = Texture2D.FromStream(GraphicsDevice, stream);
                                    ImagesToEdit[i + hovering - 1] = fname;
                                }
                                stream.Close();
                            }
                        }
                    }
                    else
                    {
                        if (forImagesInList2)
                        {
                            imagesInList2[hoveringOver - 9] = Texture2D.FromStream(GraphicsDevice, openFileDialog1.OpenFile());
                            ImagesToEdit2[hoveringOver - 9] = openFileDialog1.FileName;
                        }
                        else
                        {
                            imagesInList[hoveringOver - 1] = Texture2D.FromStream(GraphicsDevice, openFileDialog1.OpenFile());
                            ImagesToEdit[hoveringOver - 1] = openFileDialog1.FileName;
                        }

                    }
                }
                if (ImageSize == Point.Zero && imagesInList[hovering - 1] != null)
                {
                    ImageSize = new Point(imagesInList[hovering - 1].Width, imagesInList[hovering - 1].Height);
                }
            }
            catch (IOException ex)
            {
                OutputText = "File input error: " + ex.Message;
                OutputText = new string(OutputText.Select((j, i) => i > 0 && i % 60 == 0 ? new[] { '\n', j } : new[] { j }).SelectMany(j => j).ToArray());
                ErrorTimeOut = 1000;
            }
        }

        bool DoesImagesInListHaveMoreThan1Images()
        {
            int amountOfImages = 0;
            for (int i = 0; i < imagesInList.Length; i++)
            {
                if (imagesInList[i] != null)
                    amountOfImages++;
            }

            return (amountOfImages > 1) ? true : false;
        }


        protected override void Draw(GameTime gameTime)
        {
            bool BackgroundBlack = false;

            MouseState mouseState = Mouse.GetState();
            if (BackgroundBlack)
                GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
            else
                GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.White);

            spriteBatch.Begin();

            if (DoesImagesInListHaveMoreThan1Images() && colorOn < 6)
            {
                if (!hoveringOverNewPointButton)
                    spriteBatch.Draw(whiteBox, new Rectangle(304, 219, 100, 25), Color.LightGray);
                else
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                        spriteBatch.Draw(whiteBox, new Rectangle(303, 218, 100, 25), Color.LightGray);
                spriteBatch.Draw(whiteBox, new Rectangle(300, 215, 100, 25), colors[colorOn]);

                spriteBatch.DrawString(font, "Add Point", new Vector2(304, 219), Color.Black);
            }
            if (DoesImagesInListHaveMoreThan1Images() && colorOn > 0)
            {
                if (!hoveringOverRemovePointButton)
                    spriteBatch.Draw(whiteBox, new Rectangle(429, 219, 125, 25), Color.LightGray);
                else
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                        spriteBatch.Draw(whiteBox, new Rectangle(428, 218, 125, 25), Color.LightGray);
                spriteBatch.Draw(whiteBox, new Rectangle(425, 215, 125, 25), colors[colorOn - 1]);

                spriteBatch.DrawString(font, "Remove Point", new Vector2(429, 219), Color.Black);
            }
            if (spectralDataPoints.Count > 1)
            {
                if (!hoveringOverClassifyButton)
                    spriteBatch.Draw(whiteBox, new Rectangle(304, 259, 100, 25), Color.LightGray);
                else
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                        spriteBatch.Draw(whiteBox, new Rectangle(303, 258, 100, 25), Color.LightGray);
                spriteBatch.Draw(whiteBox, new Rectangle(300, 255, 100, 25), colors[colorOn - 1]);

                spriteBatch.DrawString(font, "Classify", new Vector2(304, 259), Color.Black);
            }

            if (cameraImagesAreFrom != 1 && ((imagesInList[1] != null && imagesInList[4] != null && imagesInList[6] != null) || (imagesInList[2] != null && imagesInList[4] != null && imagesInList[5] != null)))
            {
                if (!hoveringOverColorImageButton)//mouseState.X >= 575 && mouseState.X < 575 + 100 && mouseState.Y >= 215 && mouseState.Y < 215 + 25
                    spriteBatch.Draw(whiteBox, new Rectangle(579, 219, 100, 25), Color.LightGray);
                else
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                        spriteBatch.Draw(whiteBox, new Rectangle(578, 218, 100, 25), Color.LightGray);
                spriteBatch.Draw(whiteBox, new Rectangle(575, 215, 100, 25), Color.Red);

                spriteBatch.DrawString(font, "Color", new Vector2(579, 219), Color.Black);
            }

            if (DoesImagesInListHaveMoreThan1Images())
            {
                if (!hoveringOverClearButton)
                    spriteBatch.Draw(whiteBox, new Rectangle(429, 259, 100, 25), Color.LightGray);
                else
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                        spriteBatch.Draw(whiteBox, new Rectangle(428, 258, 100, 25), Color.LightGray);
                spriteBatch.Draw(whiteBox, new Rectangle(425, 255, 100, 25), Color.Beige);

                spriteBatch.DrawString(font, "Clear", new Vector2(429, 259), Color.Black);
            }

            if (true)
            {
                if (!hoveringOverInfoButton)
                    spriteBatch.Draw(whiteBox, new Rectangle(578, 259, 100, 25), Color.LightGray);
                else
                    if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                        spriteBatch.Draw(whiteBox, new Rectangle(578, 258, 100, 25), Color.LightGray);
                spriteBatch.Draw(whiteBox, new Rectangle(575, 255, 100, 25), Color.Beige);

                spriteBatch.DrawString(font, "</>", new Vector2(579, 259), Color.Black);
            }

            if (DoesImagesInListHaveMoreThan1Images())
            {
                spriteBatch.Draw(whiteBox, new Rectangle(90, 290, 125, 25), Color.LightGray); //center 256/2 + 25, 256+10
                spriteBatch.Draw(whiteDot, new Rectangle(153 + (cameraImagesAreFrom * 50) - 12, 290, 25, 25), Color.Blue);
                spriteBatch.DrawString(font, "Left     Both    Right", new Vector2(90, 290 + 25 + 10), Color.Black);


            }

            if (classifyedImage != null)
            {

                spriteBatch.Draw(classifyedImage, new Rectangle(graphics.PreferredBackBufferWidth - (256 + 25), 25, 256, 256), Color.White);
                if (ErrorTimeOut > 0)
                    spriteBatch.DrawString(font, OutputText, new Vector2(300, 75), Color.Black);
            }
            else
                spriteBatch.DrawString(font, OutputText, new Vector2(300, 75), Color.Black);

            for (int i = 0; i < spectralDataPoints.Count + ((findingPoint) ? 1 : 0) && i < colorOn; i++)
            {

                spriteBatch.Draw(whiteBox, new Rectangle(50 + i * (graphics.PreferredBackBufferWidth / 6), 350, 50, 50), ((!BackgroundBlack) ? Color.LightGray : Color.DarkGray));
                spriteBatch.Draw(whiteBox, new Rectangle(55 + i * (graphics.PreferredBackBufferWidth / 6), 355, 40, 40), colors[i]);
                if (spectralDataPoints.Count > i && spectralDataPoints[i].FileName != null)
                    spriteBatch.DrawString(font, spectralDataPoints[i].FileName, new Vector2(i * (graphics.PreferredBackBufferWidth / 6), 410 + ((i % 2 == 0) ? 0 : 15)), Color.Black);
                else
                {
                    spriteBatch.DrawString(font, "x=" + ((colorOn - 1 == i && findingPoint) ? finding.X.ToString() : ((spectralDataPoints[i].Point != Point.Zero) ? spectralDataPoints[i].Point.X.ToString() : "none")), new Vector2(50 + i * (graphics.PreferredBackBufferWidth / 6), 410), Color.Black);
                    spriteBatch.DrawString(font, "y=" + ((colorOn - 1 == i && findingPoint) ? finding.Y.ToString() : ((spectralDataPoints[i].Point != Point.Zero) ? spectralDataPoints[i].Point.Y.ToString() : "none")), new Vector2(50 + i * (graphics.PreferredBackBufferWidth / 6), 425), Color.Black);

                }

            }

            for (int i = imagesInList.Length + ((cameraImagesAreFrom == 0) ? imagesInList2.Length : 0); i > 0; i--)
            {
                if (DoesImagesInListHaveMoreThan1Images() && hoveringOver == 0 && ((i < 9) ? imagesInList[i - 1] : imagesInList2[i - 8 - 1]) == null)
                    continue;

                if (hoveringOver != i)
                {
                    if (i < 9)
                    {
                        spriteBatch.Draw((imagesInList[(i - 1)] == null) ? BlankImage : imagesInList[(i - 1)], new Rectangle(25 + (i - 1) * openStackValuePer, 25, 256, 256), Color.White);
                    }
                    else
                    {
                        spriteBatch.Draw((imagesInList2[(i - 9)] == null) ? BlankImage : imagesInList2[(i - 9)], new Rectangle(25 + (i - 9) * openStackValuePer, 25 + openStackValuePer * 256 / 100, 256, 256), Color.White);
                    }
                }
            }
            if (hoveringOver != 0)
            {
                if (hoveringOver < 9)
                    spriteBatch.Draw((imagesInList[(((hoveringOver == 0) ? 1 : hoveringOver) - 1)] == null) ? BlankImage : imagesInList[(((hoveringOver == 0) ? 1 : hoveringOver) - 1)], new Rectangle(25 + (((hoveringOver == 0) ? 1 : hoveringOver) - 1) * openStackValuePer - 10, 25 - 10, 256 + 20, 256 + 20), Microsoft.Xna.Framework.Color.White);
                else
                    spriteBatch.Draw((imagesInList2[hoveringOver - 9] == null) ? BlankImage : imagesInList2[hoveringOver - 9], new Rectangle(25 + (hoveringOver - 9) * openStackValuePer - 10, 25 - 10 + 256, 256 + 20, 256 + 20), Color.White);
                spriteBatch.DrawString(font, ((hoveringOver < 9) ? ((cameraImagesAreFrom == 1)? "R" : "L") : "R") + (hoveringOver - ((hoveringOver < 9) ? 0 : 8)), new Vector2(25 + (((hoveringOver == 0) ? 1 : ((hoveringOver < 9) ? hoveringOver : hoveringOver - 8)) - 1) * openStackValuePer - 10 + 50, ((hoveringOver < 9) ? 0 : 256)), Color.Black);
            }

            for (int i = 0; i < spectralDataPoints.Count + ((findingPoint) ? 1 : 0) && i < colorOn; i++)//DRAWS points on image
            {
                if (spectralDataPoints.Count > i && spectralDataPoints[i].Point != Point.Zero && hoveringOver == 0 && openStackValuePer <= 0)
                    spriteBatch.Draw(whiteDot, new Rectangle((spectralDataPoints[i].Point.X * 256 / ImageSize.X) + 25 - 5, (spectralDataPoints[i].Point.Y * 256 / ImageSize.Y) + 25 - 5, 10, 10), colors[i]);
            }

            spriteBatch.DrawString(font, "(" + x + "," + y + ")", new Vector2(0, 465), Color.Black);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
