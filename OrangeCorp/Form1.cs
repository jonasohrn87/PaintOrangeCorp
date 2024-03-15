using Newtonsoft.Json;
using System.Drawing.Imaging;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Drawing;

namespace OrangeCorp
{
    public partial class wack_form : Form
    {
        private Pen pen = new Pen(Color.Black);
        private Color defaultShapeColor = Color.Black;
        private Color currentShapeColor;
        private bool colorChanged = false;
        public wack_form()
        {
            InitializeComponent();
            DoubleBuffered = true;
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private bool drawCircle = false;
        private bool drawSquare = false;
        private bool drawRectangle = false;
        private bool drawTriangle = false;

        private Stack<Shape> shapes = [];
        private Stack<Shape> redo = [];


        private void pb_box_Click(object sender, EventArgs e)
        {
            var mouse = (MouseEventArgs)e;
            Color shapeColor = currentShapeColor;

            if (drawCircle)
            {
                Circle circle = new Circle()
                {
                    Color = shapeColor,
                    Center = mouse.Location,
                    Radius = 100
                };
                shapes.Push(circle);
                redo.Clear();


            }
            else if (drawSquare)
            {
                Square square = new Square()
                {
                    Color = shapeColor,
                    Center = mouse.Location,
                    side = 20,
                };
                shapes.Push(square);
                redo.Clear();
            }

            else if (drawRectangle)
            {
                Rectangle rectangle = new Rectangle()
                {
                    Color = shapeColor,
                    height = 20,
                    width = 50,
                    Center = mouse.Location,
                };
                shapes.Push(rectangle);
                redo.Clear();
            }
            else if (drawTriangle)
            {
                int x = mouse.X;
                int y = mouse.Y;
                Triangle triangle = new Triangle()
                {
                    Color = shapeColor,
                    points = new Point[] { new(x + 100, y), new(x - 100, y), new(x, y - 200) },
                    Center = mouse.Location
                };
                shapes.Push(triangle);
                redo.Clear();
            }
            pb_box.Refresh();
        }

        private void pb_box_Paint(object sender, PaintEventArgs e)
        {
            
                foreach (var s in shapes)
                {
                    pen.Color = s.Color;

                    if (s is Circle c)
                    {
                        e.Graphics.DrawEllipse(pen, c.Center.X - c.Radius / 2, c.Center.Y - c.Radius / 2, c.Radius, c.Radius);
                    }
                    else if (s is Square sq)
                    {
                        e.Graphics.DrawRectangle(pen, sq.Center.X, sq.Center.Y, sq.side, sq.side);
                    }
                    else if (s is Rectangle r)
                    {
                        e.Graphics.DrawRectangle(pen, r.Center.X, r.Center.Y, r.width, r.height);
                    }
                    else if (s is Triangle t)
                    {
                        e.Graphics.DrawPolygon(pen, t.points);
                    }

                }   
            
        }


        private void btn_clear_Click(object sender, EventArgs e)
        {
            shapes.Clear();
            pb_box.Refresh();
        }

        private void cb_circle_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_circle.Checked)
            {
                drawCircle = true;
                cb_square.Checked = false;
                cb_Rectangle.Checked = false;
                cb_triangle.Checked = false;
            }
            else
            {
                drawCircle = false;
            }
        }

        private void cb_square_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_square.Checked)
            {
                drawSquare = true;
                cb_circle.Checked = false;
                cb_Rectangle.Checked = false;
                cb_triangle.Checked = false;
            }
            else
            {
                drawSquare = false;
            }
        }

        private void cb_Rectangle_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_Rectangle.Checked)
            {
                drawRectangle = true;
                cb_circle.Checked = false;
                cb_square.Checked = false;
                cb_triangle.Checked = false;
            }
            else
            {
                drawRectangle = false;
            }
        }

        private void cb_triangle_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_triangle.Checked)
            {
                drawTriangle = true;
                cb_circle.Checked = false;
                cb_Rectangle.Checked = false;
                cb_square.Checked = false;
            }
            else
            {
                drawTriangle = false;
            }
        }




        private void bt_save_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(pb_box.Width, pb_box.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                pb_box.Paint += new PaintEventHandler(pb_box_Paint);
                pb_box.Invalidate();
            }
                var result = saveFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    bitmap.Save(saveFileDialog1.FileName, ImageFormat.Png);
                    //var json = Newtonsoft.Json.JsonConvert.SerializeObject(shapes);
                    //File.WriteAllText(saveFileDialog1.FileName, json);
                }
        }

        private void bt_undo_Click(object sender, EventArgs e)
        {

            switchElement(shapes, redo);

        }

        private void bt_redo_Click(object sender, EventArgs e)
        {
            switchElement(redo, shapes);
        }

        private void switchElement(Stack<Shape> giver, Stack<Shape> receiver)
        {
            if (giver.Any())
            {
                receiver.Push(giver.Pop());
                pb_box.Refresh();
            }
        }

        private void bt_load_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                var file = openFileDialog1.FileName;
                var content = File.ReadAllText(file);

                var test = JsonConvert.DeserializeObject<Shape>(content);
            }
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            currentShapeColor = p.BackColor;
            pen.Color = currentShapeColor;
            pb_ChosenColor.BackColor = currentShapeColor;
            colorChanged = true;

        }

        //private void UpdatePenColor()
        //{
        //    pen.Color = colorChanged ? currentShapeColor : defaultShapeColor;
        //    colorChanged = false;
        //}
    }
}



//private void SaveAsImage(string fileName, ImageFormat format)
//{
//    // Skapa en ny bitmap med storleken av PictureBox
//    Bitmap bmp = new Bitmap(pb_box.Width, pb_box.Height);

//    // Rita PictureBox-innehållet på den nya bitmapen
//    pb_box.DrawToBitmap(bmp, new Rectangle(0, 0, pb_box.Width, pb_box.Height));

//    // Spara bitmapen till en fil i den valda formatet
//    bmp.Save(fileName, format);

//    // Frigör resurserna
//    bmp.Dispose();
//}

// Anropa denna metod när du vill spara bilden till en fil
// Exempel: SaveAsImage("ritad_bild.png", ImageFormat.Png);

//För att använda detta måste du inkludera System.Drawing.
//Imaging i ditt projekt och ange filnamnet och formatet
//(PNG, BMP eller JPG) när du anropar SaveAsImage-metoden.

//Till exempel:

//csharp

//SaveAsImage("ritad_bild.png", ImageFormat.Png);

//Detta sparar den ritade figuren som en PNG-fil med filnamnet "ritad_bild.png". Du kan byta ut
//filändelsen och filnamnet enligt dina önskemål.